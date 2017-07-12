using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Linq;
using NHibernate.Transform;
using RADET.DAL.Entities;
using RADET.DAL.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace RADET.DAL.DAO
{
    public class RadetMetaDataDAO : BaseDAO<RadetMetaData, int>
    {
        public List<RadetReportModel2> RetrieveUsingPaging(RadetMetaDataSearchModel search, int startIndex, int maxRows, bool order_Asc, out int totalCount)
        {
            IStatelessSession session = BuildSession().SessionFactory.OpenStatelessSession();
            ICriteria criteria = session.CreateCriteria<RadetMetaData>("pt")
                .CreateAlias("pt.RadetUpload", "rup")
                .CreateAlias("pt.IP", "ip")
                .CreateAlias("rup.UploadedBy", "upl")
                .CreateAlias("pt.LGA", "lga")
                .SetProjection(Projections.ProjectionList()
                .Add(Projections.Property("pt.Facility"), "Facility")
                    .Add(Projections.Property("ip.ShortName"), "IP")
                    .Add(Projections.Property("pt.RadetPeriod"), "RadetPeriod")
                     .Add(Projections.SqlFunction("concat",
                            NHibernateUtil.String,
                            Projections.Property("upl.FirstName"),
                            Projections.Constant(" "),
                           Projections.Property("upl.Surname")), "UploadedBy")
                   .Add(Projections.Property("rup.DateUploaded"), "DateUploaded")
                    .Add(Projections.Property("pt.Id"), "Id")
                   .Add(Projections.Property("pt.Id"), "DT_RowId")
                   .Add(Projections.Property("lga.lga_code"), "LGA_code")
                   )
                .SetResultTransformer(Transformers.AliasToBean<RadetReportModel2>());
                        

            if (search != null)
            {
                if (search.IPs != null && search.IPs.Count > 0)
                {
                    criteria.Add(Restrictions.In("ip.ShortName", search.IPs));
                }
                if (search.lga_codes != null && search.lga_codes.Count > 0)
                {
                    criteria.Add(Restrictions.In("pt.LGA.lga_code", search.lga_codes));
                }
                if (search.facilities != null && search.facilities.Count > 0)
                {
                    criteria.Add(Restrictions.In("pt.Facility", search.facilities));
                }
                if (!string.IsNullOrEmpty(search.RadetPeriod))
                {
                    criteria.Add(Restrictions.Eq("pt.RadetPeriod", search.RadetPeriod));
                }
            }
            criteria.SetFirstResult(startIndex);
            if (maxRows > 0)
            {
                criteria.SetMaxResults(maxRows);
            }
            
            var result = criteria.List<RadetReportModel2>() as List<RadetReportModel2>;
            totalCount = result.Count;
            return result;

            //ICriteria countCriteria = CriteriaTransformer.Clone(criteria).SetProjection(Projections.RowCount());
            //ICriteria listCriteria = CriteriaTransformer.Clone(criteria).SetFirstResult(startIndex).SetMaxResults(maxRows);
            //IList allResults = session.CreateMultiCriteria().Add<T>(listCriteria).Add(countCriteria).List();
            //totalCount = Convert.ToInt32(((IList)allResults[1])[0]);
            //return allResults[0] as List<T>;
        }


        public IQueryable<RadetMetaData> RetrieveRadetUpload(string RadetPeriod, int IP = 0, string facility = null)
        {
            IQueryable<RadetMetaData> result = null;
            ISession session = BuildSession();
            result = session.Query<RadetMetaData>().Where(x => x.PatientLineListing.Count > 0 && x.RadetPeriod == RadetPeriod);

            if (IP != 0)
            {
                result = result.Where(x => x.IP.Id == IP);
            }
            if (!string.IsNullOrEmpty(facility))
            {
                result = result.Where(x => x.Facility == facility);
            }
            return result;
        }

        public IList<RadetPatientLineListing> RetrieveRadetLineListingForDQA(string RadetPeriod, int IP, string facility)
        {
            ISession session = BuildSession();
            var result = session.Query<RadetPatientLineListing>().Where(x => x.SelectedForDQA && x.MetaData.RadetPeriod == RadetPeriod && x.MetaData.IP.Id == IP && x.MetaData.Facility == facility);
            return result.ToList();
        }

        public List<T> RetrieveRadetList<T>(List<int> radetIds, bool selectedForDQAOnly) where T : class
        {
            IStatelessSession session = BuildSession().SessionFactory.OpenStatelessSession();

            ICriteria criteria = session.CreateCriteria<RadetPatientLineListing>("pt")
                .Add(Restrictions.In("MetaData.Id", radetIds))
                .CreateAlias("RadetPatient", "rpt")
                .CreateAlias("rpt.IP", "ip")
                .SetProjection(Projections.ProjectionList()
                .Add(Projections.Property("pt.ARTStartDate"), "ARTStartDate")
                    .Add(Projections.Property("pt.LastPickupDate"), "LastPickupDate")
                    .Add(Projections.Property("pt.MonthsOfARVRefill"), "MonthsOfARVRefill")
                   .Add(Projections.Property("pt.RegimenLineAtARTStart"), "RegimenLineAtARTStart")
                   .Add(Projections.Property("pt.RegimenAtStartOfART"), "RegimenAtStartOfART")
                   .Add(Projections.Property("pt.CurrentRegimenLine"), "CurrentRegimenLine")
                   .Add(Projections.Property("pt.CurrentARTRegimen"), "CurrentARTRegimen")
                   .Add(Projections.Property("pt.PregnancyStatus"), "PregnancyStatus")
                   .Add(Projections.Property("pt.CurrentViralLoad"), "CurrentViralLoad")
                    .Add(Projections.Property("pt.DateOfCurrentViralLoad"), "DateOfCurrentViralLoad")
                   .Add(Projections.Property("pt.ViralLoadIndication"), "ViralLoadIndication")
                   .Add(Projections.Property("pt.CurrentARTStatus"), "CurrentARTStatus")
                   .Add(Projections.Property("rpt.PatientId"), "PatientId")
                   .Add(Projections.Property("rpt.HospitalNo"), "HospitalNo")
                   .Add(Projections.Property("rpt.Sex"), "Sex")
                   .Add(Projections.Property("rpt.Age_at_start_of_ART_in_years"), "AgeInYears")
                   .Add(Projections.Property("rpt.Age_at_start_of_ART_in_months"), "AgeInMonths")
                   .Add(Projections.Property("rpt.FacilityName"), "Facility")
                   .Add(Projections.Property("ip.ShortName"), "IPShortName")
                   .Add(Projections.Property("pt.SelectedForDQA"), "SelectedForDQA"))
                .SetResultTransformer(Transformers.AliasToBean<T>());

            if (selectedForDQAOnly)
            {
                criteria.Add(Restrictions.Eq("pt.SelectedForDQA", selectedForDQAOnly));
            }

            var result = criteria.List<T>() as List<T>;
            return result;
        }

        public IList<RadetMetaData> SearchRadetData(List<string> IPs, List<string> lga_codes, List<string> facilities, string RadetPeriod = "")
        {
            ISession session = BuildSession();
            ICriteria criteria = session.CreateCriteria<RadetMetaData>("rmt");
            if (IPs != null)
            {
                criteria.CreateAlias("IP", "ip");
                criteria.Add(Restrictions.In("ip.ShortName", IPs));
            }
            if (lga_codes != null)
            {
                //criteria.CreateAlias("LGA", "lga");
                criteria.Add(Restrictions.In("rmt.lga.lga_code", lga_codes));
            }
            if (facilities != null)
            {
                criteria.Add(Restrictions.In("FacilityName", facilities));
            }
            if (RadetPeriod != "")
            {
                criteria.Add(Restrictions.Eq("RadetPeriod", RadetPeriod));
            }
            return criteria.List<RadetMetaData>();
        }


        public IList<RandomizationUpdateModel> SearchPatientLineListing(List<string> IPs, List<string> lga_codes, List<string> facilities, List<string> states, string RadetPeriod = "")
        {
            IStatelessSession session = BuildSession().SessionFactory.OpenStatelessSession();
            ICriteria criteria = session.CreateCriteria<RadetPatientLineListing>("rpt");
            criteria.CreateAlias("MetaData", "rmt");
            criteria.CreateAlias("rmt.LGA", "lga");
            criteria.CreateAlias("rmt.IP", "ip");
            if (IPs != null && IPs.Count > 0)
            { 
                criteria.Add(Restrictions.In("ip.ShortName", IPs));
            }
            if (lga_codes != null && lga_codes.Count > 0)
            {
                criteria.Add(Restrictions.In("rmt.LGA.lga_code", lga_codes));
            }
            if(states !=null && states.Count > 0)
            {                
                criteria.CreateAlias("lga.State", "st");
                criteria.Add(Restrictions.In("st.state_name", states));
            }
            if (facilities != null && facilities.Count > 0)
            {
                criteria.Add(Restrictions.In("rmt.Facility", facilities));
            }
            if (RadetPeriod != "")
            {
                criteria.Add(Restrictions.Eq("rmt.RadetPeriod", RadetPeriod));
            }
            return criteria.SetProjection(Projections.ProjectionList()
                    .Add(Projections.Property("rpt.Id"), "Id")
                    .Add(Projections.Property("rmt.Id"), "MetadataId")
                    .Add(Projections.Property("rpt.CurrentARTStatus"), "CurrentARTStatus")
                    .Add(Projections.Property("ip.ShortName"), "IP")
                    .Add(Projections.Property("rmt.Facility"), "FacilityName")
                    .Add(Projections.Property("rpt.SelectedForDQA"), "SelectedForDQA"))
                .SetResultTransformer(Transformers.AliasToBean<RandomizationUpdateModel>())
                .List<RandomizationUpdateModel>() as List<RandomizationUpdateModel>;
        }

        public IQueryable<RadetPatientLineListing> SearchPatientLineListing()
        {
            ISession session = BuildSession();
            var result = session.Query<RadetPatientLineListing>();//.Where(x => x.MetaData.Id == metadataId);
            return result;
        }

        /// <summary>
        /// saves the metadata plus the patient line listing plus the patients entries
        /// </summary>
        /// <param name="RadetData"></param>
        /// <returns></returns>
        public TimeSpan BulkSave(IList<RadetMetaData> RadetData)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            int totalCount = 0;
            RadetMetaDataSearchModel search = new Models.RadetMetaDataSearchModel
            {
                facilities = RadetData.Select(x => x.Facility).Distinct().ToList(),
                lga_codes = RadetData.Select(x => x.LGA.lga_code).Distinct().ToList(),
                IPs = RadetData.Select(x => x.IP.ShortName).Distinct().ToList(),
            };
            List<RadetReportModel2> previously = RetrieveUsingPaging(search, 0, 0, false, out totalCount);

            
            ISessionFactory sessionFactory = BuildSession().SessionFactory;

            using (IStatelessSession session = sessionFactory.OpenStatelessSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                var patients = RetrievePatients(session, RadetData.Select(x => x.IP.Id).Distinct().ToList());

                foreach (var entry in RadetData)
                {
                    var previousReport = previously.FirstOrDefault(x => x.IP == entry.IP.ShortName && x.LGA_code == entry.LGA.lga_code && x.RadetPeriod == entry.RadetPeriod && x.Facility == entry.Facility); // CheckPreviousUpload(session, entry.IP, entry.LGA, entry.RadetPeriod, entry.Facility);
                    if (previousReport == null)
                    {
                        if (entry.PatientLineListing != null)
                        {
                            session.Insert(entry);
                            foreach (var item in entry.PatientLineListing)
                            {
                                var _patient_exist = patients.FirstOrDefault(f => f.PatientId == item.RadetPatient.PatientId && f.HospitalNo == item.RadetPatient.HospitalNo && f.Sex == item.RadetPatient.Sex); //CheckIfPatientExists(session, item.RadetPatient);
                                if (_patient_exist == null)
                                {
                                    session.Insert(item.RadetPatient);
                                }
                                else
                                {
                                    item.RadetPatient = _patient_exist;
                                }
                                item.MetaData = entry;
                                session.Insert(item);
                            }
                        }
                    }
                }
                transaction.Commit();
            }

            stopwatch.Stop();
            var time = stopwatch.Elapsed;
            return time;
        }

        public void DeleteRecord(int id)
        {
            try
            {
                string query = string.Format("delete from radet_patient_line_listing where RadetMetaDataId = {0}; delete from radet_MetaData where Id ={0};  ", id);
                int i = RunSQL(query);
                //CommitSQL();
            }
            catch (Exception ex)
            {
                RollbackSQL();
                throw ex;
            }
        }

        public bool BulkUpdate(List<RadetPatientLineListing> patientsLineListing)
        {
            ISessionFactory sessionFactory = BuildSession().SessionFactory;

            using (IStatelessSession session = sessionFactory.OpenStatelessSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var entry in patientsLineListing)
                {
                    session.Update(entry);
                }
                transaction.Commit();
            }
            return true;
        }


        private int CheckIfPatientExists(IStatelessSession session, RadetPatient patient)
        {
            //return session.Query<RadetPatient>().FirstOrDefault(f => f.PatientId == patient.PatientId && f.HospitalNo == patient.HospitalNo && f.Sex == patient.Sex && f.IP == patient.IP && f.FacilityName == patient.FacilityName);
            var id = (from f in session.Query<RadetPatient>()
                     where f.PatientId == patient.PatientId && f.HospitalNo == patient.HospitalNo && f.Sex == patient.Sex && f.IP == patient.IP && f.FacilityName == patient.FacilityName
                     select f.Id).FirstOrDefault();
            return id;
        }

        private IList<RadetPatient> RetrievePatients(IStatelessSession session, List<int> Ip)
        {
            string queryString = string.Format("select * from dbo.radet_patient where IP in ('{0}')", string.Join(",", Ip));
            var result = session.CreateSQLQuery(queryString).AddEntity(typeof(RadetPatient)).List<RadetPatient>();
            return result;
        }

        private bool CheckPreviousUpload(IStatelessSession session, CommonUtil.Entities.Organizations iP, CommonUtil.Entities.LGA lGA, string radetPeriod, string facilityName)
        {
            string queryString = string.Format(" SELECT COUNT(*) FROM [radet_MetaData] where RadetPeriod = '{0}' and Facility = '{1}' and IP = '{2}' and LGA = '{3}'", radetPeriod, facilityName.Replace("'","''"), iP.Id, lGA.lga_code);
            var result = session.CreateSQLQuery(queryString).UniqueResult<int>();

            return result != 0;
        }

        /// <summary>
        /// formatted for the pop up page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string RetrieveRadetData(int id)
        {
            ISession session = BuildSession();
            var Current_year_tx_new = session.Query<RadetPatientLineListing>().Count(c => c.MetaData.Id == id && c.ARTStartDate.HasValue && c.ARTStartDate.Value.Year == DateTime.Now.Year && c.CurrentARTStatus == "Active");
            var SelectedForDQA = session.Query<RadetPatientLineListing>().Count(x => x.MetaData.Id == id && x.SelectedForDQA);
            var Tx_Current = session.Query<RadetPatientLineListing>().Count(c => c.MetaData.Id == id && c.CurrentARTStatus == "Active");


            var lineItems = from item in session.Query<RadetPatientLineListing>()
                            .Where(x => x.MetaData.Id == id)
                            select new
                            {
                                item.RadetPatient.PatientId,
                                item.RadetPatient.HospitalNo,
                                item.RadetPatient.Sex,
                                item.RadetPatient.Age_at_start_of_ART_in_months,
                                item.RadetPatient.Age_at_start_of_ART_in_years,
                                ARTStartDate = string.Format("{0:dd-MM-yyyy}", item.ARTStartDate),
                                LastPickupDate = string.Format("{0:dd-MM-yyyy}", item.LastPickupDate),
                                item.MonthsOfARVRefill,
                                item.RegimenLineAtARTStart,
                                item.RegimenAtStartOfART,
                                item.CurrentRegimenLine,
                                item.CurrentARTRegimen,
                                item.PregnancyStatus,
                                item.CurrentViralLoad,
                                DateOfCurrentViralLoad = string.Format("{0:dd-MM-yyyy}", item.DateOfCurrentViralLoad),
                                item.ViralLoadIndication,
                                item.CurrentARTStatus,
                                item.SelectedForDQA,
                                item.RadetYear,
                            };


            var processData = new
            {
                data = lineItems,
                Current_year_tx_new = Current_year_tx_new,
                SelectedForDQA = SelectedForDQA,
                Tx_Current = Tx_Current
            };

            string result = Newtonsoft.Json.JsonConvert.SerializeObject(processData);
            return result;
        }

        /*
     public IList<RadetPatientLineListing> SearchPatientLineListing(List<string> IPs, List<string> lga_codes, List<string> facilities, string RadetPeriod = "")
     {
         ISession session = BuildSession();
         ICriteria criteria = session.CreateCriteria<RadetPatientLineListing>("rpt");
         criteria.CreateAlias("MetaData", "rmt");
         if (IPs != null)
         {
             criteria.CreateAlias("rmt.IP", "ip");
             criteria.Add(Restrictions.In("ip.ShortName", IPs));
         }
         if (lga_codes != null)
         {
             criteria.Add(Restrictions.In("rmt.LGA.lga_code", lga_codes));
         }
         if (facilities != null)
         {
             criteria.Add(Restrictions.In("rmt.Facility", facilities));
         }
         if (RadetPeriod != "")
         {
             criteria.Add(Restrictions.Eq("rmt.RadetPeriod", RadetPeriod));
         }
         return criteria.List<RadetPatientLineListing>();
     }
     */

        /* 
        /// <summary>
        /// bulk copy using ado.net
        /// </summary>
        /// <param name="RadetMetaData"></param>
        public void BulkSave(List<RadetMetaData> RadetMetaData)
        {
            //save RadetPatientLineListing
            List<RadetPatientLineListing> listing = new List<RadetPatientLineListing>();
            RadetMetaData.ForEach(x => listing.AddRange(x.PatientLineListing));

            //CopyLineListing(listing);

            //save radet patient,
            List<RadetPatient> patients = new List<RadetPatient>();
            listing.ForEach(x => patients.Add(x.RadetPatient));

            //CopyPatientRecord(patients);

            //save RadetMetaData
            CopyRadetMetaData(RadetMetaData);
        }
       
        private void CopyRadetMetaData(IList<RadetMetaData> RadetMetaData)
        {
            string tableName = "radet_MetaData";
            var dt = new DataTable(tableName);

            dt.Columns.Add(new DataColumn("RadetPeriod", typeof(string)));
            dt.Columns.Add(new DataColumn("Facility", typeof(string)));
            dt.Columns.Add(new DataColumn("LGA", typeof(string)));
            dt.Columns.Add(new DataColumn("IP", typeof(int)));
            dt.Columns.Add(new DataColumn("RadetUploadId", typeof(int)));

            try
            {
                foreach (var tx in RadetMetaData)
                {
                    var row = dt.NewRow();

                    row["RadetPeriod"] = GetDBValue(tx.RadetPeriod);
                    row["Facility"] = GetDBValue(tx.Facility);
                    row["LGA"] = GetDBValue(tx.LGA);
                    row["IP"] = GetDBValue(tx.IP);
                    row["RadetUploadId"] = GetDBValue(tx.RadetUpload.Id);
                    
                    dt.Rows.Add(row);
                }
                DirectDBPostNew(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CopyLineListing(List<RadetPatientLineListing> table)
        {
            string tableName = "radet_patient_line_listing";

            var dt = new DataTable(tableName);
             
            dt.Columns.Add(new DataColumn("ARTStartDate", typeof(string)));
            dt.Columns.Add(new DataColumn("LastPickupDate", typeof(string)));
            dt.Columns.Add(new DataColumn("MonthsOfARVRefill", typeof(string)));
            dt.Columns.Add(new DataColumn("RegimenLineAtARTStart", typeof(string)));
            dt.Columns.Add(new DataColumn("RegimenAtStartOfART", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentRegimenLine", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentARTRegimen", typeof(string)));
            dt.Columns.Add(new DataColumn("PregnancyStatus", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentViralLoad", typeof(string)));
            dt.Columns.Add(new DataColumn("DateOfCurrentViralLoad", typeof(string)));
            dt.Columns.Add(new DataColumn("ViralLoadIndication", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentARTStatus", typeof(string)));
            dt.Columns.Add(new DataColumn("SelectedForDQA", typeof(bool)));
            dt.Columns.Add(new DataColumn("RadetYear", typeof(string)));
            dt.Columns.Add(new DataColumn("RadetMetaDataId", typeof(int)));
            dt.Columns.Add(new DataColumn("RadetPatientId", typeof(int)));
            try
            {
                foreach (var tx in table)
                {
                    try
                    {
                        var row = dt.NewRow();

                        row["ARTStartDate"] = GetDBValue(tx.ARTStartDate);
                        row["LastPickupDate"] = GetDBValue(tx.LastPickupDate);
                        row["MonthsOfARVRefill"] = GetDBValue(tx.MonthsOfARVRefill);
                        row["RegimenLineAtARTStart"] = GetDBValue(tx.RegimenLineAtARTStart);
                        row["RegimenAtStartOfART"] = GetDBValue(tx.RegimenAtStartOfART);
                        row["CurrentRegimenLine"] = GetDBValue(tx.CurrentRegimenLine);
                        row["CurrentARTRegimen"] = GetDBValue(tx.CurrentARTRegimen);
                        row["PregnancyStatus"] = GetDBValue(tx.PregnancyStatus);
                        row["CurrentViralLoad"] = GetDBValue(tx.CurrentViralLoad);
                        row["DateOfCurrentViralLoad"] = GetDBValue(tx.DateOfCurrentViralLoad);
                        row["ViralLoadIndication"] = GetDBValue(tx.ViralLoadIndication);
                        row["CurrentARTStatus"] = GetDBValue(tx.CurrentARTStatus);
                        row["SelectedForDQA"] = GetDBValue(tx.SelectedForDQA);
                        row["RadetYear"] = GetDBValue(tx.RadetReportPeriod);
                        row["RadetMetaDataId"] = GetDBValue(tx.MetaData.Id);
                        row["RadetPatientId"] = GetDBValue(tx.RadetPatient.Id);

                        dt.Rows.Add(row);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                DirectDBPostNew(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void CopyPatientRecord(List<RadetPatient> table)
        {
            string tableName = "radet_patient";

            var dt = new DataTable(tableName);
            dt.Columns.Add(new DataColumn("PatientId", typeof(string)));
            dt.Columns.Add(new DataColumn("HospitalNo", typeof(string)));
            dt.Columns.Add(new DataColumn("Sex", typeof(string)));
            dt.Columns.Add(new DataColumn("Age_at_start_of_ART_in_years", typeof(string)));
            dt.Columns.Add(new DataColumn("Age_at_start_of_ART_in_months", typeof(string)));
            try
            {
                foreach (var tx in table)
                {
                    try
                    {
                        var row = dt.NewRow();

                        row["PatientId"] = GetDBValue(tx.PatientId);
                        row["HospitalNo"] = GetDBValue(tx.HospitalNo);
                        row["Sex"] = GetDBValue(tx.Sex);
                        row["Age_at_start_of_ART_in_years"] = GetDBValue(tx.Age_at_start_of_ART_in_years);
                        row["Age_at_start_of_ART_in_months"] = GetDBValue(tx.Age_at_start_of_ART_in_months);

                        dt.Rows.Add(row);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                DirectDBPostNew(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

 */
    }
}
