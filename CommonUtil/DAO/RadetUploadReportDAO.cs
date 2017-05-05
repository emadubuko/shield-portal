using System.Collections.Generic;
using System.IO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using NHibernate;
using NHibernate.Criterion;
using OfficeOpenXml;
using CommonUtil.Utilities;
using System;
using System.Data;
using System.Linq;
using System.Dynamic;
using System.Text;

namespace CommonUtil.DAO
{
    public class RadetUploadReportDAO : BaseDAO<RadetUploadReport, int>
    {
        public bool DeleteRecord(int id)
        {
            // ISession session = BuildSession();
            try
            {
                StartSQL("delete from dbo.dqa_radet where [UploadReportId] =" + id);
                ContinueSQL("DELETE FROM dbo.dqa_radet_upload_report WHERE Id =" + id);
                CommitSQL();
            }
            catch (Exception ex)
            {
                RollbackSQL();
                return false;
            }
            return true;
        }

        public List<RadetUploadReport> RetrieveRadetUpload(int IP, int year, string quarter, string facility = null)
        {
            List<RadetUploadReport> result = null;
            ISession session = BuildSession();

            ICriteria criteria = session.CreateCriteria<RadetUploadReport>()
                .Add(Restrictions.Eq("dqa_year", year))
                 .Add(Restrictions.Eq("dqa_quarter", quarter));
            if (!string.IsNullOrEmpty(facility))
            {
                criteria.Add(Restrictions.Eq("Facility", facility));
            }
            if (IP != 0)
            {
                criteria.Add(Restrictions.Eq("IP.Id", IP));
            }
            result = criteria.List<RadetUploadReport>() as List<RadetUploadReport>;
            return result;
        }

        public bool ReadRadetFile(Stream uploadedFile, string selectedQuater, int selectedYear, Profile loggedinProfile, out string result)
        { 
            List<RadetTable> table = new List<RadetTable>();
            result = "";
            string facilityName = "";

            var Ips = new OrganizationDAO().RetrieveAll().ToDictionary(s => s.ShortName);
            Organizations org = null;
            using (ExcelPackage package = new ExcelPackage(uploadedFile))
            {
                var worksheets = package.Workbook.Worksheets;
                foreach (var worksheet in worksheets)
                {
                    if (worksheet.Name == "MainPage")
                    {
                        facilityName = ExcelHelper.ReadCell(worksheet, 20, 19);
                        if (!string.IsNullOrEmpty(facilityName) && facilityName.Count() > 4)
                        {
                            facilityName = facilityName.Substring(3);
                        }
                        else
                        {
                            result = "Could not read Facility Name";
                            return false;
                        }
                        string ipshortname = ExcelHelper.ReadCell(worksheet, 24, 19);
                        
                        if(Ips.TryGetValue(ipshortname, out org) == false)
                        {
                            if(ipshortname == "CCRN")
                            {
                                ipshortname = "CCCRN";
                            }
                            if (Ips.TryGetValue(ipshortname, out org) == false)
                            {

                                result = "Ip [" + ipshortname + "] not configured";
                            } 
                        }

                        var t = RetrieveRadetUpload(loggedinProfile.Organization.Id, selectedYear, selectedQuater, facilityName);
                        if (t != null && t.Count() != 0)
                        {
                            result = "Result already exist";
                            return false;
                        }
                    }

                    var istEntry = ExcelHelper.ReadCell(worksheet, 1, 7);
                    //if the sheet does not have a column named Patiend Id, then skip
                    if (string.IsNullOrEmpty(istEntry) || !istEntry.ToLower().Contains("patient unique id/art"))
                    {
                        continue;
                    }
                    int row = 2;
                    while (true)
                    {
                        string patientId = ExcelHelper.ReadCell(worksheet, row, 7);
                        if (string.IsNullOrEmpty(patientId))
                        {
                            break;
                        }

                        table.Add(new RadetTable
                        {
                            PatientId = patientId,
                            HospitalNo = ExcelHelper.ReadCellText(worksheet, row, 8),
                            Sex = ExcelHelper.ReadCellText(worksheet, row, 9),
                            Age_at_start_of_ART_in_years = ExcelHelper.ReadCellText(worksheet, row, 10),
                            Age_at_start_of_ART_in_months = ExcelHelper.ReadCellText(worksheet, row, 11),
                            ARTStartDate = ExcelHelper.ReadCellText(worksheet, row, 12),
                            LastPickupDate = ExcelHelper.ReadCellText(worksheet, row, 13),
                            MonthsOfARVRefill = ExcelHelper.ReadCellText(worksheet, row, 14),
                            RegimenLineAtARTStart = ExcelHelper.ReadCellText(worksheet, row, 15),
                            RegimenAtStartOfART = ExcelHelper.ReadCellText(worksheet, row, 16),
                            CurrentRegimenLine = ExcelHelper.ReadCellText(worksheet, row, 17),
                            CurrentARTRegimen = ExcelHelper.ReadCellText(worksheet, row, 18),
                            Pregnancy_Status = ExcelHelper.ReadCellText(worksheet, row, 19),
                            Current_Viral_Load = ExcelHelper.ReadCellText(worksheet, row, 20),
                            Date_of_Current_Viral_Load = ExcelHelper.ReadCellText(worksheet, row, 21),
                            Viral_Load_Indication = ExcelHelper.ReadCellText(worksheet, row, 22),
                            CurrentARTStatus = ExcelHelper.ReadCellText(worksheet, row, 23),
                            RadetYear = worksheet.Name,
                            IP = loggedinProfile.Organization
                        });
                        row += 1;
                    }
                }
            }
            MarkSelectedItems(ref table);
            RadetUploadReport report = new Entities.RadetUploadReport()
            {
                Facility = facilityName,
                IP = org, // loggedinProfile.Organization,
                UploadedBy = loggedinProfile,
                dqa_year = selectedYear,
                dqa_quarter = selectedQuater,
                DateUploaded = DateTime.Now,
                Uploads = table,
                CurrentYearTx_New = table.Count(y => y.RadetYear == DateTime.Now.Year.ToString())
            };
            foreach (var item in table)
            {
                item.UploadReport = report;
            }
            Save(report);
            CommitChanges();

            result = Newtonsoft.Json.JsonConvert.SerializeObject(
                new
                {
                    data = from item in report.Uploads
                           select new
                           {
                               item.PatientId,
                               item.HospitalNo,
                               item.Sex,
                               item.Age_at_start_of_ART_in_months,
                               item.Age_at_start_of_ART_in_years,
                               item.ARTStartDate,
                               item.LastPickupDate,
                               item.MonthsOfARVRefill,
                               item.RegimenLineAtARTStart,
                               item.RegimenAtStartOfART,
                               item.CurrentRegimenLine,
                               item.CurrentARTRegimen,
                               item.Pregnancy_Status,
                               item.Current_Viral_Load,
                               item.Date_of_Current_Viral_Load,
                               item.Viral_Load_Indication,
                               item.CurrentARTStatus,
                               item.SelectedForDQA,
                               item.RadetYear,
                           },
                    Current_year_tx_new = report.CurrentYearTx_New
                }
                );

            return true;
        }

        private void MarkSelectedItems(ref List<RadetTable> table)
        {
            string no_to_select = ExcelHelper.GetRandomizeChartNUmber(table.Count.ToString());
            table.Shuffle();
            foreach (var item in table.Take(Convert.ToInt32(no_to_select)))
            {
                item.SelectedForDQA = true;
            }
        }

        private void BulkInser(List<RadetTable> table)
        {
            string tableName = "dqa_radet";

            var dt = new DataTable(tableName);
            dt.Columns.Add(new DataColumn("PatientId", typeof(string)));
            dt.Columns.Add(new DataColumn("HospitalNo", typeof(string)));
            dt.Columns.Add(new DataColumn("Sex", typeof(string)));
            dt.Columns.Add(new DataColumn("Age_at_start_of_ART_in_years", typeof(string)));
            dt.Columns.Add(new DataColumn("Age_at_start_of_ART_in_months", typeof(string)));
            dt.Columns.Add(new DataColumn("ARTStartDate", typeof(string)));
            dt.Columns.Add(new DataColumn("LastPickupDate", typeof(string)));
            dt.Columns.Add(new DataColumn("MonthsOfARVRefill", typeof(string)));
            dt.Columns.Add(new DataColumn("RegimenLineAtARTStart", typeof(string)));
            dt.Columns.Add(new DataColumn("RegimenAtStartOfART", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentRegimenLine", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentARTRegimen", typeof(string)));
            dt.Columns.Add(new DataColumn("Pregnancy_Status", typeof(string)));
            dt.Columns.Add(new DataColumn("Current_Viral_Load", typeof(string)));
            dt.Columns.Add(new DataColumn("Date_of_Current_Viral_Load", typeof(string)));
            dt.Columns.Add(new DataColumn("Viral_Load_Indication", typeof(string)));
            dt.Columns.Add(new DataColumn("CurrentARTStatus", typeof(string)));
            dt.Columns.Add(new DataColumn("SelectedForDQA", typeof(bool)));
            dt.Columns.Add(new DataColumn("RadetYear", typeof(string)));
            dt.Columns.Add(new DataColumn("IP", typeof(int)));

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
                        row["ARTStartDate"] = GetDBValue(tx.ARTStartDate);
                        row["LastPickupDate"] = GetDBValue(tx.LastPickupDate);
                        row["MonthsOfARVRefill"] = GetDBValue(tx.MonthsOfARVRefill);
                        row["RegimenLineAtARTStart"] = GetDBValue(tx.RegimenLineAtARTStart);
                        row["RegimenAtStartOfART"] = GetDBValue(tx.RegimenAtStartOfART);
                        row["CurrentRegimenLine"] = GetDBValue(tx.CurrentRegimenLine);
                        row["CurrentARTRegimen"] = GetDBValue(tx.CurrentARTRegimen);
                        row["Pregnancy_Status"] = GetDBValue(tx.Pregnancy_Status);
                        row["Current_Viral_Load"] = GetDBValue(tx.Current_Viral_Load);
                        row["Date_of_Current_Viral_Load"] = GetDBValue(tx.Date_of_Current_Viral_Load);
                        row["Viral_Load_Indication"] = GetDBValue(tx.Viral_Load_Indication);
                        row["CurrentARTStatus"] = GetDBValue(tx.CurrentARTStatus);
                        row["SelectedForDQA"] = GetDBValue(tx.SelectedForDQA);
                        row["RadetYear"] = GetDBValue(tx.RadetYear);
                        row["IP"] = GetDBValue(tx.IP.Id);

                        dt.Rows.Add(row);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                DirectDBPost(dt, tableName);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
