using CommonUtil.DBSessionManager;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MPM.DAL.DTO;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System.Linq;

namespace MPM.DAL.DAO
{
    public class MPMDAO : BaseDAO<MetaData, int>
    {

        public void BulkInsertWithStatelessSession(MetaData mt)
        {
            using (var session = BuildSession().SessionFactory.OpenStatelessSession())
            using (var tx = session.BeginTransaction())
            {
                session.Insert(mt);
                foreach (var a in mt.ART)
                {
                    session.Insert(a);
                }
                foreach (var o in mt.HTS_Index)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.LinkageToTreatment)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.PITC)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.Pmtct_Viral_Load)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.PMTCT)
                {
                    session.Insert(o);
                }
                tx.Commit();
            }
        }

        //public MetaData SearchForPreviousUpload(string reportingPeriod, int IpId)
        //{
        //    var session = BuildSession().SessionFactory.OpenStatelessSession();
        //    var result = session.Query<MetaData>().Where(x => x.ReportingPeriod == reportingPeriod && x.IP.Id == IpId).FirstOrDefault();
        //    return result;
        //}

        public IList<IPUploadReport> GenerateIPUploadReports(int OrgId, string reportingPeriod)
        {
            ICriteria criteria = BuildSession()
                .CreateCriteria<MetaData>("pd")          
                .CreateCriteria("IP", "org", NHibernate.SqlCommand.JoinType.InnerJoin);

            if (OrgId != 0)
            {
                criteria.Add(Restrictions.Eq("org.Id", OrgId));
            }
            if (!string.IsNullOrEmpty(reportingPeriod))
            {
                criteria.Add(Restrictions.Eq("pd.ReportingPeriod", reportingPeriod));
            }


            criteria.SetProjection(
                Projections.Alias(Projections.GroupProperty("org.ShortName"), "IPName"),
                 Projections.Alias(Projections.GroupProperty("pd.ReportingPeriod"), "ReportPeriod")
                );

            IList<IPUploadReport> reports = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(IPUploadReport))).List<IPUploadReport>();
            return reports;
        }


        public List<MPMFacilityListing> GetPivotTableFromFacility(string IP)
        { 
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_generate_MPM_facility_list";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IP", IP);

            var dataTable = GetDatable(cmd);

            var list = new List<MPMFacilityListing>();
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(new MPMFacilityListing
                {
                    ART = row.Field<bool>("ART"),
                    PMTCT = row.Field<bool>("PMTCT"),
                    HTS = row.Field<bool>("HTS"),
                    DATIMCode = row.Field<string>("DatimCode"),
                    Facility = row.Field<string>("Facility"),
                    IP = row.Field<string>("IP")
                });
            }
            return list;
        }
    }
}
