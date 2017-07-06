using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Linq;
using RADET.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace RADET.DAL.DAO
{
    public class RadetMetaDataDAO : BaseDAO<RadetMetaData, int>
    {
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


        public IList<RadetPatientLineListing> RetrievePatientListingByMetaDataId(int metadataId)
        {
            ISession session = BuildSession();
            var result = session.Query<RadetPatientLineListing>().Where(x => x.MetaData.Id == metadataId).ToList();
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
            ISessionFactory sessionFactory = BuildSession().SessionFactory;

            using (IStatelessSession session = sessionFactory.OpenStatelessSession())
            using (ITransaction transaction = session.BeginTransaction())
            {
                foreach (var entry in RadetData)
                {
                    var previousReport = CheckPreviousUpload(session, entry.IP, entry.LGA, entry.RadetPeriod, entry.Facility);
                    if (!previousReport)
                    {
                        if (entry.PatientLineListing != null)
                        {
                            session.Insert(entry);
                            foreach (var item in entry.PatientLineListing)
                            {
                                var patientExist = CheckIfPatientExists(session, item.RadetPatient);
                                if (patientExist == null)
                                {
                                    session.Insert(item.RadetPatient);
                                }
                                else
                                {
                                    item.RadetPatient = patientExist;
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
                int i = StartSQL(query);
                CommitSQL();
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

        private RadetPatient CheckIfPatientExists(IStatelessSession session, RadetPatient patient)
        {
            return session.Query<RadetPatient>().FirstOrDefault(f => f.PatientId == patient.PatientId && f.HospitalNo == patient.HospitalNo && f.Sex == patient.Sex && f.IP == patient.IP && f.FacilityName == patient.FacilityName);
            //string queryString = string.Format("select * from radet_patient where PatientId='{0}'and HospitalNo='{1}' and Sex='{2}' and IP='{3}' and FacilityName='{4}'", patient.PatientId, patient.HospitalNo, patient.Sex, patient.IP.Id, patient.FacilityName);
            //var result = session.CreateSQLQuery(queryString).AddEntity(typeof(RadetPatient)).UniqueResult<RadetPatient>();
            //return result;
        }

        private bool CheckPreviousUpload(IStatelessSession session, CommonUtil.Entities.Organizations iP, CommonUtil.Entities.LGA lGA, string radetPeriod, string facilityName)
        {
            string queryString = string.Format(" SELECT COUNT(*) FROM [radet_MetaData] where RadetPeriod = '{0}' and Facility = '{1}' and IP = '{2}' and LGA = '{3}'", radetPeriod, facilityName, iP.Id, lGA.lga_code);
            var result = session.CreateSQLQuery(queryString).UniqueResult<int>();

            return result != 0;
        }

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
                                DateOfCurrentViralLoad =  string.Format("{0:dd-MM-yyyy}", item.DateOfCurrentViralLoad),
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
