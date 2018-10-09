using CommonUtil.DAO;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using DQA.DAL.Model;
using OfficeOpenXml;
using RADET.DAL.DAO;
using RADET.DAL.Entities;
using RADET.DAL.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DQA.DAL.Business
{
    public class Utility
    {
        readonly static shield_dmpEntities entity = new shield_dmpEntities();

        public static int GetIpStateCount(int partnerId)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = string.Format("select count(distinct(state_code)) from [dbo].[lga] where lga_code in (select lgaId from [dbo].[HealthFacility]  where [ImplementingPartnerId]={0})", partnerId);
            cmd.CommandType = CommandType.Text;
            var result = GetNumberValue(cmd);
            return result; //.Select(e=>e.State).Distinct().Count();
        }

        /// <summary>
        /// Get the number of lga a parnter is working in
        /// </summary>
        /// <param name="partnerId">the id of the partner</param>
        /// <returns>The number of lgas a partner has facilities in</returns>
        public static int GetIpLGACount(int partnerId)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = string.Format("select count(distinct(lga_code)) from [dbo].[lga] where lga_code in (select lgaId from [dbo].[HealthFacility]  where [ImplementingPartnerId]={0})", partnerId);
            cmd.CommandType = CommandType.Text;
            var result = GetNumberValue(cmd);
            return result;
        }

        /// <summary>
        /// Get the number of facilities reporting for a partner for the reporting period
        /// </summary>
        /// <param name="partnerId">Id of the partner</param>
        /// <param name="month">Reporting period</param>
        /// <returns>Number of facilities</returns>
        public static int GetIpSubmitted(int partnerId, string month)
        {
            return entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.ReportPeriod == month).Count();
        }

        /// <summary>
        /// Get the number of facilities for a parner
        /// </summary>
        /// <param name="partnerId">Id of the partner</param>
        /// <returns>Number of facilities</returns>
        public static int GetIpFacilitycount(int partnerId)
        {
            return entity.HealthFacilities.Count(e => e.ImplementingPartnerId == partnerId);
        }


        public static List<StateSummary> GetStateIpStateSummary(int partnerId, string reporting_period)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_get_IP_state_summary";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip_id", partnerId);
            cmd.Parameters.AddWithValue("@period", reporting_period);
            var dataTable = GetDatable(cmd);

            var summaries = new List<StateSummary>();
            foreach (DataRow row in dataTable.Rows)
            {
                var summary = new StateSummary();
                summary.Name = row.ItemArray[1].ToString();
                summary.Submitted = Convert.ToInt32(row.ItemArray[2].ToString());
                summary.Pending = Convert.ToInt32(row.ItemArray[3].ToString());
                if (Convert.ToInt32(row.ItemArray[4].ToString()) > 0)
                {
                    var value = (float.Parse(summary.Submitted.ToString()) / float.Parse(row.ItemArray[4].ToString())) * 100;
                }
                summaries.Add(summary);
            }
            return summaries;
        }

        public static List<Data.HealthFacility> GetSubmittedFacilities(int partnerId, string reporting_period)
        {
            var facility_ids = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.ReportPeriod == reporting_period).Select(e => e.Id).ToList();
            return entity.HealthFacilities.Where(x => facility_ids.Contains((int)x.Id)).ToList();
        }


        public static List<Data.HealthFacility> GetPendingFacilities(int partnerId, string reporting_period)
        {
            var facility_ids = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.ReportPeriod == reporting_period).Select(e => e.Id).ToList();
            return entity.HealthFacilities.Where(x => !facility_ids.Contains((int)x.Id)).ToList();
        }

        public static Data.HealthFacility GetFacility(int facilityId)
        {
            return entity.HealthFacilities.FirstOrDefault(e => e.Id == facilityId);
        }

        public static string GetLgaName(string lgaId)
        {
            var lga = entity.lgas.FirstOrDefault(e => e.lga_code == lgaId);
            return lga != null ? lga.lga_name : "";
        }

        public static string GetStateName(string stateId)
        {
            var state = entity.states.FirstOrDefault(e => e.state_code == stateId);
            return state != null ? state.state_name : "";
        }

        public static decimal? GetDecimal(object value)
        {
            try
            {
                if (value == null)
                    return null;
                if (!string.IsNullOrEmpty(Convert.ToString(value)) && Convert.ToString(value).StartsWith("?"))
                {
                    throw new ApplicationException("invalid value in upload");
                }
                return Convert.ToDecimal(value);
            }
            catch
            {
                return null;
            }
        }

        public static DataTable GetDatable(SqlCommand command)
        {
            var connection = (SqlConnection)entity.Database.Connection;
            command.Connection = connection;
            var dataTable = new DataTable();
            try
            {
                connection.Open();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(dataTable);
            }
            finally
            {
                connection.Close();
            }
            return dataTable;
        }

        public static int GetNumberValue(SqlCommand command)
        {
            var connection = (SqlConnection)entity.Database.Connection;
            command.Connection = connection;
            var result = 0;
            try
            {
                connection.Open();
                result = (int)command.ExecuteScalar();
            }
            catch (Exception)
            {

            }
            finally
            {
                connection.Close();
            }
            return result;
        }

        public static DataSet GetDataSet(SqlCommand cmd)
        {
            var conn = (SqlConnection)entity.Database.Connection;
            SqlDataAdapter da = new SqlDataAdapter();
            cmd.Connection = conn;
            da.SelectCommand = cmd;
            DataSet ds = new DataSet();

            if (conn.State == ConnectionState.Closed)
                conn.Open();

            da.Fill(ds);
            conn.Close();

            return ds;
        }

        public static List<PivotTableModel> RetrievePivotTablesForDQATool(int ip_id, string quarter)
        {
            List<dqa_pivot_table_upload> pivotTableUpload = null;
            if (ip_id != 0)
            {
                pivotTableUpload = entity.dqa_pivot_table_upload.Where(x => x.IP == ip_id && x.Quarter == quarter).ToList();
            }
            else
            {
                pivotTableUpload = entity.dqa_pivot_table_upload.Where(x => x.Quarter == quarter).ToList();
            }

            if (pivotTableUpload != null)
            {
                List<PivotTableModel> result = new List<PivotTableModel>();
                foreach (var item in pivotTableUpload)
                {
                    result.AddRange((from table in item.dqa_pivot_table
                                     .OrderByDescending(x => x.SelectedForDQA)

                                     select new PivotTableModel
                                     {
                                         Id = table.Id,
                                         State = table.HealthFacility.lga.state.state_name,
                                         Lga = table.HealthFacility.lga.lga_name,
                                         IP = table.HealthFacility.ImplementingPartner.ShortName,
                                         FacilityName = table.HealthFacility.Name, //table.FacilityName,
                                         FacilityCode = table.HealthFacility.FacilityCode,
                                         OVC_Total = table.OVC.HasValue && table.OVC_NotReported.HasValue ? table.OVC.Value + table.OVC_NotReported.Value : 0,

                                         PMTCT_ART = table.PMTCT_ART,
                                         TX_CURR = table.TX_CURR,
                                         HTC_Only = table.HTC_Only,
                                         HTC_Only_POS = table.HTC_Only_POS,
                                         HTS_TST = table.HTS_TST,
                                         PMTCT_EID = table.PMTCT_EID,
                                         PMTCT_STAT = table.PMTCT_STAT,
                                         PMTCT_STAT_NEW = table.PMTCT_STAT_NEW,
                                         PMTCT_STAT_PREV = table.PMTCT_STAT_PREV,
                                         TX_NEW = table.TX_NEW,

                                         PMTCT_FO = table.PMTCT_FO,
                                         TB_STAT = table.TB_STAT,
                                         TX_PVLS = table.TX_PVLS,
                                         TX_RET = table.TX_RET,
                                         TX_TB = table.TX_TB,

                                         TB_ART = table.TB_ART,
                                         PMTCT_HEI_POS = table.PMTCT_HEI_POS,

                                         SelectedForDQA = table.SelectedForDQA,
                                         SelectedReason = table.SelectedReason
                                     }).ToList());
                }
                return result;
            }
            //return null if nothing is found
            return null;
        }

        public static List<UploadList> RetrievePivotTables(int ip_id, string reportPeriod)
        {
            var result = entity.dqa_pivot_table_upload.Where(x => x.Quarter == reportPeriod);
            if (ip_id != 0)
            {
                result = result.Where(x => x.IP == ip_id);
            }

            IEnumerable<UploadList> list = (from item in result.ToList()
                                            select new UploadList
                                            {
                                                IP = item.ImplementingPartner.ShortName,
                                                Period = item.Quarter,
                                                DateUploaded = item.DateUploaded,
                                                UploadedBy = new ProfileDAO().Retrieve(item.UploadedBy).FullName,
                                                id = item.Id.ToString(),
                                                Tables = from table in item.dqa_pivot_table
                                                         select new PivotTableModel
                                                         {
                                                             IP = table.IP.ToString(),
                                                             State = table.HealthFacility.lga.state.state_name,
                                                             Lga = table.HealthFacility.lga.lga_name,
                                                             FacilityName = table.HealthFacility.Name,
                                                             FacilityCode = table.HealthFacility.FacilityCode,
                                                             OVC_Total = table.OVC.HasValue && table.OVC_NotReported.HasValue ? table.OVC.Value + table.OVC_NotReported.Value : 0,
                                                             PMTCT_ART = table.PMTCT_ART,
                                                             TB_ART = table.TB_ART,
                                                             TX_CURR = table.TX_CURR,
                                                             PMTCT_STAT = table.PMTCT_STAT,
                                                             PMTCT_EID = table.PMTCT_EID,
                                                             PMTCT_FO = table.PMTCT_FO,
                                                             PMTCT_HEI_POS = table.PMTCT_HEI_POS,
                                                             PMTCT_STAT_NEW = table.PMTCT_STAT_NEW,
                                                             PMTCT_STAT_PREV = table.PMTCT_STAT_PREV,
                                                             HTC_Only = table.HTC_Only,
                                                             HTC_Only_POS = table.HTC_Only_POS,
                                                             HTS_TST = table.HTS_TST,
                                                             TX_NEW = table.TX_NEW,
                                                             TX_RET = table.TX_RET,
                                                             TX_PVLS = table.TX_PVLS,
                                                             TB_STAT = table.TB_STAT,
                                                             TX_TB = table.TX_TB,
                                                             SelectedForDQA = table.SelectedForDQA,
                                                             SelectedReason = table.SelectedReason
                                                         }
                                            });

            return list.ToList();
        }

        public static List<PivotTableModel> RetrievePivotTablesForComparison(List<string> ip, string quarter, List<string> state_code = null, List<string> lga_code = null, List<string> facilityName = null)
        {
            var list = (from item in entity.dqa_pivot_table_upload.Where(x => x.Quarter == quarter)
                        from table in item.dqa_pivot_table
                        where table.TX_CURR > 0
                        select new PivotTableModel
                        {
                            IP = item.ImplementingPartner.ShortName,
                            FacilityName = table.HealthFacility.Name,
                            FacilityCode = table.HealthFacility.FacilityCode,
                            TX_CURR = table.TX_CURR,
                            TX_NEW = table.TX_NEW,
                            TheLGA = table.HealthFacility.lga,
                        });
            if (ip != null && ip.Count > 0)
            {
                if (ip.Count() == 1 && string.IsNullOrEmpty(ip.FirstOrDefault()))
                {
                    //do nothing
                }
                else
                    list = list.Where(x => ip.Contains(x.IP));
            }

            if (state_code != null && state_code.Count > 0)
            {
                list = list.Where(x => state_code.Contains(x.TheLGA.state_code));
            }
            if (lga_code != null && lga_code.Count > 0)
            {
                list = list.Where(x => lga_code.Contains(x.TheLGA.lga_code));
            }
            if (facilityName != null && facilityName.Count > 0)
            {
                list = list.Where(x => facilityName.Contains(x.FacilityName));
            }

            return list.ToList();
        }

        public static List<PivotTableModel> RetrievePivotTablesForNDR(List<string> ip, string quarter, List<string> state_code = null, List<string> lga_code = null, List<string> facilityName = null)
        {
            var list = (from item in entity.dqa_pivot_table_upload.Where(x => x.Quarter == quarter)
                        from table in item.dqa_pivot_table
                        where table.TX_CURR > 0
                        select new PivotTableModel
                        {
                            IP = item.ImplementingPartner.ShortName,
                            FacilityName = table.HealthFacility.Name,
                            FacilityCode = table.HealthFacility.FacilityCode,
                            TX_CURR = table.TX_CURR,
                            TX_NEW = table.TX_NEW,
                            TX_PVLS = table.TX_PVLS,
                            TX_RET = table.TX_RET,
                            Lga = table.HealthFacility.lga.lga_name,
                            State = table.HealthFacility.lga.state.state_name,
                        });
            if (ip != null && ip.Count > 0)
            {
                if (ip.Count() == 1 && string.IsNullOrEmpty(ip.FirstOrDefault()))
                {
                    //do nothing
                }
                else
                    list = list.Where(x => ip.Contains(x.IP));
            }

            if (state_code != null && state_code.Count > 0)
            {
                list = list.Where(x => state_code.Contains(x.TheLGA.state_code));
            }
            if (lga_code != null && lga_code.Count > 0)
            {
                list = list.Where(x => lga_code.Contains(x.TheLGA.lga_code));
            }
            if (facilityName != null && facilityName.Count > 0)
            {
                list = list.Where(x => facilityName.Contains(x.FacilityName));
            }

            return list.ToList();
        }

        public static DataTable GetRADETNumbers(string partnerShortName, string startQuarterDate, string endQuarterDate, string radetPeriod)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "sp_aggregate_radet";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@radetperiod", radetPeriod);
            cmd.Parameters.AddWithValue("@startdate", startQuarterDate);
            cmd.Parameters.AddWithValue("@enddate", endQuarterDate);
            cmd.Parameters.AddWithValue("@ip", partnerShortName);
            var dataTable = GetDatable(cmd);

            return dataTable;
        }


        //gets dqa_upload report
        public static DataTable GetUploadReport(string reportPeriod, string IP_id, List<string> state_code = null, List<string> lga_code = null, List<string> facilityName = null)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "[sp_get_DQA_upload_report]";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@period", reportPeriod);
            cmd.Parameters.AddWithValue("@ip", IP_id);
            var dataTable = GetDatable(cmd);

            return dataTable;
        }

        public static DataTable GetQ3Analysis(string IP_id, bool get_partner_report)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "get_q3_FY17_analysis_report";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip", IP_id);
            cmd.Parameters.AddWithValue("@get_partner_report", get_partner_report);
            var dataTable = GetDatable(cmd);

            return dataTable;
        }
        public static DataTable GetQ4Analysis(string IP_id, bool get_partner_report)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "get_q4_FY17_analysis_report";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@ip", IP_id);
            cmd.Parameters.AddWithValue("@get_partner_report", get_partner_report);
            var dataTable = GetDatable(cmd);

            return dataTable;
        }

        public static DataTable GetFY18Analysis(string IP_id, string reportPeriod, bool get_partner_report)
        {
            var cmd = new SqlCommand();
            cmd.CommandText = "get_FY18_analysis_report_by_quarter";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@reportPeriod", reportPeriod);
            cmd.Parameters.AddWithValue("@ip", IP_id);
            cmd.Parameters.AddWithValue("@get_partner_report", get_partner_report);
            var dataTable = GetDatable(cmd);

            return dataTable;
        }

        public static DataSet GetDashboardStatistic(string IP, string reportPeriod)
        {
            //TODO: seperate period into year and quarter
            var cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "sp_get_dqa_dashboard_by_period_and_ip"; //"sp_get_q3_FY17_dashboard";
            cmd.Parameters.AddWithValue("@ip", IP);
            cmd.Parameters.AddWithValue("@reportperiod", reportPeriod);
            try
            {
                return GetDataSet(cmd);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return null;
            }

        }

        public static void DeletePivotTable(int id)
        {
            var data = entity.dqa_pivot_table_upload.First(x => x.Id == id);
            entity.dqa_pivot_table.RemoveRange(data.dqa_pivot_table).ToList();
            entity.dqa_pivot_table_upload.Remove(data);
            entity.SaveChanges();
        }

        public static string GetReportDetails(int metadataid)
        {
            var report_value = new dqa_report_value();
            var result = (from item in entity.dqa_report_value.Where(x => x.MetadataId == metadataid)
                          select new
                          {
                              item.dqa_indicator.ThematicArea,
                              item.dqa_indicator.IndicatorName,
                              item.IndicatorValueMonth1,
                              item.IndicatorValueMonth2,
                              item.IndicatorValueMonth3
                          }).ToList();
            string Processed_result = Newtonsoft.Json.JsonConvert.SerializeObject(result);
            return Processed_result;
        }

        public async Task<string> GenerateDQA(List<PivotTableModel> facilities, Organizations ip, string fileName, string directory, string template, string reportingPeriod)
        {
            string zippedFile = directory + "\\" + fileName;

            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                try
                {
                    string[] filenames = Directory.GetFiles(directory);
                    foreach (var file in filenames)//incase the folder is not empty
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex); 
                }
            }
            var artSites = Utilities.GetARTSiteWithDATIMCode();
            Logger.LogInfo("generate dqa", " got art sites");

            IList<RadetPatientLineListing> radet = null;

            //site name come from pivot table and names match the data in health facility in the database
            foreach (var site in facilities)
            {
                artSites.TryGetValue(site.FacilityCode, out string radetSite);

                if (!string.IsNullOrEmpty(radetSite))
                {
                    radet = new RadetMetaDataDAO().RetrieveRadetLineListingForDQA(reportingPeriod, site.IP, radetSite);
                }
                if(radet == null || radet.Count == 0)
                {
                    radet = new RadetMetaDataDAO().RetrieveRadetLineListingForDQA(reportingPeriod, site.IP, site.FacilityName);
                }

                using (ExcelPackage package = new ExcelPackage(new FileInfo(template)))
                {
                    int tx_current_count = site.TX_CURR;// 0;

                    if (radet != null && radet.Count() > 0)
                    {
                        radet = radet.Where(x => !x.MetaData.Supplementary).ToList();
                        if (radet.Count > 107)
                        {
                            radet.Shuffle();
                            radet = radet.Take(107).ToList();
                        }
                        tx_current_count = radet.Count;

                        int viral_load_count = radet.Count(x => !string.IsNullOrEmpty(x.CurrentViralLoad));
                        int viral_load_count_suppression = 0;

                        if (radet != null)
                        {
                            try
                            {
                                var sheet = package.Workbook.Worksheets["TX_CURR"];

                                int row = 1;
                                for (int i = 0; i < radet.Count(); i++)
                                {
                                    row++;

                                    sheet.Cells["A" + row].Value = radet[i].RadetPatient.PatientId;
                                    sheet.Cells["B" + row].Value = radet[i].RadetPatient.HospitalNo;
                                    sheet.Cells["C" + row].Value = radet[i].RadetPatient.Sex;
                                    sheet.Cells["D" + row].Value = radet[i].RadetPatient.Age_at_start_of_ART_in_years == 0 ? "" : string.Format("{0}", radet[i].RadetPatient.Age_at_start_of_ART_in_years);
                                    sheet.Cells["E" + row].Value = radet[i].RadetPatient.Age_at_start_of_ART_in_months == 0 ? "" : string.Format("{0}", radet[i].RadetPatient.Age_at_start_of_ART_in_months);
                                    sheet.Cells["F" + row].Value = radet[i].ARTStartDate;// string.Format("{0:d-MMM-yyyy}", radet[i].ARTStartDate);
                                    sheet.Cells["F" + row].Style.Numberformat.Format = "d-MMM-yyyy";
                                    sheet.Cells["G" + row].Value = radet[i].LastPickupDate; // string.Format("{0:d-MMM-yyyy}", radet[i].LastPickupDate);
                                    sheet.Cells["G" + row].Style.Numberformat.Format = "d-MMM-yyyy";

                                    sheet.Cells["J" + row].Value = radet[i].MonthsOfARVRefill;

                                    sheet.Cells["M" + row].Value = radet[i].RegimenLineAtARTStart;
                                    sheet.Cells["N" + row].Value = radet[i].RegimenAtStartOfART;

                                    sheet.Cells["Q" + row].Value = radet[i].CurrentRegimenLine;
                                    sheet.Cells["R" + row].Value = radet[i].CurrentARTRegimen;
                                    sheet.Cells["U" + row].Value = radet[i].PregnancyStatus;

                                    string currentViralLoad = radet[i].CurrentViralLoad;
                                    sheet.Cells["V" + row].Value = currentViralLoad;
                                    sheet.Cells["W" + row].Value = radet[i].DateOfCurrentViralLoad; // string.Format("{0:d-MMM-yyyy}", radet[i].DateOfCurrentViralLoad);
                                    sheet.Cells["W" + row].Style.Numberformat.Format = "d-MMM-yyyy";
                                    sheet.Cells["X" + row].Value = radet[i].ViralLoadIndication;
                                    sheet.Cells["Y" + row].Value = radet[i].CurrentARTStatus;

                                    if (!string.IsNullOrEmpty(currentViralLoad) && !string.IsNullOrEmpty(currentViralLoad.Trim()))
                                    {
                                        int.TryParse(currentViralLoad, out int cvl);
                                        if (cvl <= 1000)
                                        {
                                            viral_load_count_suppression += 1;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.LogInfo("package ", package.File.FullName);
                                Logger.LogInfo("package.Workbook.Worksheets.Count() ", package.Workbook.Worksheets.Count());

                                Logger.LogInfo("generateDQA try_catch", string.Format("{0},{1},{2}", reportingPeriod, site.IP, radetSite));
                                Logger.LogError(ex);
                                throw ex;
                            }
                        }
                    }
                    else
                    {
                        Logger.LogInfo("Genrate DQA", "No Radet file for facility " + radetSite);
                    }
                    var sheetn = package.Workbook.Worksheets["Worksheet"];
                    sheetn.Cells["P2"].Value = site.IP;
                    sheetn.Cells["R2"].Value = site.State;
                    sheetn.Cells["T2"].Value = site.Lga;
                    sheetn.Cells["V2"].Value = site.FacilityName;
                    sheetn.Cells["AA2"].Value = site.FacilityCode;
                    sheetn.Cells["Y2"].Value = reportingPeriod;

                    sheetn.Cells["N10"].Value = tx_current_count;

                    var sheet__all_Q = package.Workbook.Worksheets["All Questions"];
                    //hts_tst
                    sheet__all_Q.Cells["F2"].Value = site.HTS_TST;
                    sheet__all_Q.Cells["F3"].Value = site.HTC_Only_POS + site.PMTCT_STAT_NEW;
                    sheet__all_Q.Cells["F4"].Value = site.HTC_Only;
                    sheet__all_Q.Cells["F5"].Value = site.HTC_Only_POS;
                    //pmtct_stat
                    sheet__all_Q.Cells["F12"].Value = site.PMTCT_STAT;
                    sheet__all_Q.Cells["F13"].Value = site.PMTCT_STAT_NEW;
                    sheet__all_Q.Cells["F14"].Value = site.PMTCT_STAT_PREV;
                    //pmtct_art
                    sheet__all_Q.Cells["F23"].Value = site.PMTCT_ART;
                    //pmtct_eid
                    sheet__all_Q.Cells["F28"].Value = site.PMTCT_EID;
                    //PMTCT_HEI_POS
                    sheet__all_Q.Cells["F33"].Value = site.PMTCT_HEI_POS;
                    //TX_NEW
                    sheet__all_Q.Cells["F38"].Value = site.TX_NEW;

                    //TB_STAT
                    sheet__all_Q.Cells["F44"].Value = site.TB_STAT;
                    //TB_ART
                    sheet__all_Q.Cells["F50"].Value = site.TB_ART;
                    //TX_TB
                    sheet__all_Q.Cells["F55"].Value = site.TX_TB;
                    //PMTCT_FO
                    sheet__all_Q.Cells["F65"].Value = site.PMTCT_FO;

                    var sheet__all_Summary = package.Workbook.Worksheets["DQA Summary (Map to Quest Ans)"];
                    sheet__all_Summary.Cells["E12"].Value = tx_current_count;

                    package.SaveAs(new FileInfo(directory + "/" + site.FacilityName + ".xlsm"));
                }
            }
            await new Utilities().ZipFolder(directory);
            return fileName;
        }


        public List<dynamic> RadetForValidationData(RadetMetaDataSearchModel searchModel, string ip, string period, string startDate, string endDate)
        {
            var radet_data = Utility.GetRADETNumbers(ip, startDate, endDate, period);
            var pivot_data = Utility.RetrievePivotTablesForComparison(new List<string> { ip }, period, searchModel.state_codes, searchModel.lga_codes, searchModel.facilities);
            var artSites = Utilities.GetARTSiteWithDATIMCode();

            List<dynamic> mydata = new List<dynamic>();
            List<dynamic> mydata2 = new List<dynamic>();

            foreach (DataRow dr in radet_data.Rows)
            {
                var dtt = new
                {
                    ShortName = dr[0],
                    Facility = dr[1],
                    Tx_New = dr[2],
                    Tx_Curr = dr[3],
                };
                mydata.Add(dtt);
            }
            foreach (var item in pivot_data)
            {
                string radetSite;
                artSites.TryGetValue(item.FacilityCode, out radetSite);
                if (string.IsNullOrEmpty(radetSite))
                {
                    radetSite = item.FacilityName;
                }
                var r_data = mydata.FirstOrDefault(x => x.ShortName == item.IP && x.Facility == radetSite);
                if (r_data != null)
                {
                    int tx_new = item.TX_NEW.HasValue ? item.TX_NEW.Value : 0;
                    mydata2.Add(new
                    {
                        ShortName = item.IP,
                        State = item.TheLGA.state.state_name,
                        LGA = $"{item.TheLGA.lga_name}",
                        Facility = item.FacilityName,
                        r_data.Tx_New,
                        p_Tx_New = item.TX_NEW,
                        Tx_New_difference = Math.Abs((int)r_data.Tx_New - tx_new),
                        Tx_New_concurrency = 100 * Math.Abs((int)r_data.Tx_New - tx_new) / (int)r_data.Tx_New,

                        r_data.Tx_Curr,
                        p_Tx_Curr = item.TX_CURR,
                        Tx_Curr_difference = Math.Abs((int)r_data.Tx_Curr - item.TX_CURR),
                        Tx_Curr_concurrency = 100 * Math.Abs((int)r_data.Tx_Curr - item.TX_CURR) / (int)r_data.Tx_Curr,
                    });
                }
            }
            return mydata2;
        }
    }
}