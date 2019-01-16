using CommonUtil.DBSessionManager;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MPM.DAL.DTO;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using System.Linq;
using System;
using NHibernate.Engine;

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
                foreach(var h in mt.HTS_TST)
                {
                    session.Insert(h);
                }
                foreach (var o in mt.HTS_Index)
                {
                    session.Insert(o);
                }
                //foreach (var o in mt.LinkageToTreatment)
                //{
                //    session.Insert(o);
                //}
                foreach (var o in mt.PITC)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.Pmtct_Viral_Load)
                {
                    session.Insert(o);
                }
                //foreach (var o in mt.Viral_Load)
                //{
                //    session.Insert(o);
                //}

                foreach (var o in mt.PMTCT)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.PMTCT_EID)
                {
                    session.Insert(o);
                }
                //
                foreach (var o in mt.TB_Treatment_Started)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Presumptives)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Bacteriology_Diagnosis)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Screened)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Diagnosed)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Relapsed)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_New_Relapsed_Known_Pos)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_New_Relapsed_Known_Status)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_TPT_Eligible)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_ART)
                {
                    session.Insert(o);
                }
                tx.Commit();
            }
        }

        internal void UpdateRecord(MetaData mt)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_delete_MPM_data_list";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@metadataId", mt.Id);
            GetDatable(cmd);
            cmd.Dispose();

            using (var session = BuildSession().SessionFactory.OpenStatelessSession())
            using (var tx = session.BeginTransaction())
            {
                session.Update(mt);
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
                foreach (var o in mt.PMTCT_EID)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Treatment_Started)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Presumptives)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Bacteriology_Diagnosis)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Screened)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_Relapsed)
                {
                    session.Insert(o);
                }
                foreach (var o in mt.TB_TPT_Eligible)
                {
                    session.Insert(o);
                }
                tx.Commit();
            }
        }

        public IList<IPUploadReport> GenerateIPUploadReports(int OrgId, string reportingPeriod, string reportlevelvaue, ReportLevel? reportLevel)
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
            if (reportLevel.HasValue)
            {
                criteria.Add(Restrictions.Eq("pd.ReportLevel", reportLevel.Value));
            }
            if (!string.IsNullOrEmpty(reportlevelvaue))
            {
                criteria.Add(Restrictions.Eq("pd.ReportLevelValue", reportlevelvaue));
            }

            criteria.SetProjection(
                Projections.Alias(Projections.GroupProperty("pd.ReportLevelValue"), "ReportingLevelValue"),
                Projections.Alias(Projections.GroupProperty("org.ShortName"), "IPName"),
                 Projections.Alias(Projections.GroupProperty("pd.ReportingPeriod"), "ReportPeriod"),
                 Projections.Alias(Projections.GroupProperty("pd.Id"), "Id"),
                 Projections.Alias(Projections.GroupProperty("pd.FilePath"), "FilePath")
                );

            IList<IPUploadReport> reports = criteria.SetResultTransformer(new NHibernate.Transform.AliasToBeanResultTransformer(typeof(IPUploadReport))).List<IPUploadReport>();
            return reports;
        }


        public List<MPMFacilityListing> GetPivotTableFromFacility(string IP, string state)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_generate_MPM_facility_list";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@IP", IP);
            cmd.Parameters.AddWithValue("@state", state);

            var dataTable = GetDatable(cmd);

            var list = new List<MPMFacilityListing>();
            foreach (DataRow row in dataTable.Rows)
            {
                list.Add(new MPMFacilityListing
                {
                    ART = row.Field<bool>("ART"),
                    PMTCT = row.Field<bool>("PMTCT"),
                    HTS = row.Field<bool>("HTS"),
                    TB = row.Field<bool>("TB"),
                    DATIMCode = row.Field<string>("DatimCode"),
                    Facility = row.Field<string>("Facility"),
                    IP = row.Field<string>("IP")
                });
            }
            return list;
        }

        public DataTable GetUploadReport(string reportPeriod)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "[sp_mpm_report]";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@reportPeriod", reportPeriod);
            var dataTable = GetDatable(cmd);

            return dataTable;
        }

        public string GetLastReport(int ip_id = 0)
        {
            //return DateTime.Now.ToString("MMM yy");
            string sql = "select top 1 ReportingPeriod from (select top 1 ReportingPeriod from [dbo].[mpm_MetaData] where ReportLevel != 'IP' ";
            if (ip_id != 0)
            {
                sql += "and Ip =" + ip_id;
            }
            sql += ") as dt order by cast('01'+'-'+ReportingPeriod as datetime) desc";
            var conn = (SqlConnection)((ISessionFactoryImplementor)BuildSession().SessionFactory).ConnectionProvider.GetConnection();
            var cmd = new SqlCommand(sql, conn);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            try
            {
                var result = cmd.ExecuteScalar();
                if (result == null)
                    return string.Empty;

                return result.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }


        public string GetGSMLastReport(int ip_id = 0)
        {
             string sql = "select top 1 ReportingPeriod from (select top 1 ReportingPeriod from [dbo].[mpm_MetaData] where ReportLevel = 'IP' ";
            if (ip_id != 0)
            {
                sql += "and Ip =" + ip_id;
            }
            sql += ") as dt order by cast(ReportingPeriod as datetime) desc";
            var conn = (SqlConnection)((ISessionFactoryImplementor)BuildSession().SessionFactory).ConnectionProvider.GetConnection();
            var cmd = new SqlCommand(sql, conn);
            if (conn.State != ConnectionState.Open)
            {
                conn.Open();
            }
            try
            {
                var result = cmd.ExecuteScalar();
                if (result == null)
                    return string.Empty;

                return result.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                cmd.Dispose();
            }
        }

        public DataTable RetriveDataAsDataTables(string stored_procedure, MPMDataSearchModel searchModel)//, string IPfilter="")
        {
            var cmd = new SqlCommand();
            cmd.CommandText = stored_procedure;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 180;
              
            if(searchModel != null)
            {
                cmd.Parameters.AddWithValue("reportPeriod", searchModel.ReportPeriod);

                if (searchModel.IPs !=null && searchModel.IPs.Count > 0)
                {
                    cmd.Parameters.AddWithValue("IP", string.Join("','", searchModel.IPs));
                }
                if(searchModel.state_codes !=null && searchModel.state_codes.Count > 0)
                {
                    cmd.Parameters.AddWithValue("Statecode", string.Join("','", searchModel.state_codes));
                }
                if (searchModel.lga_codes != null && searchModel.lga_codes.Count > 0)
                {
                    cmd.Parameters.AddWithValue("LGA_code", string.Join("','", searchModel.lga_codes));
                }
                if (searchModel.facilities != null && searchModel.facilities.Count > 0)
                {
                    cmd.Parameters.AddWithValue("facility", string.Join("','", searchModel.facilities));
                }
                if(!string.IsNullOrEmpty(searchModel.Sex))
                {
                    cmd.Parameters.AddWithValue("sex", searchModel.Sex);
                }
                if (!string.IsNullOrEmpty(searchModel.Agegroup))
                {
                    cmd.Parameters.AddWithValue("agegroup", searchModel.Agegroup);
                }
                if (!string.IsNullOrEmpty(searchModel.PopulationGroup))
                {
                    cmd.Parameters.AddWithValue("populationgroup", searchModel.PopulationGroup);
                }
            }

            var dataTable = GetDatable(cmd);
            return dataTable;
        }
    }
         
}
