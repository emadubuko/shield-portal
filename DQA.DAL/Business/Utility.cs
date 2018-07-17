using CommonUtil.DAO;
using CommonUtil.Mapping;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using DQA.DAL.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace DQA.DAL.Business
{
    public static class Utility
    {
        readonly static shield_dmpEntities entity = new shield_dmpEntities();

        /// <summary>
        /// Get the number of states a parnter is working in
        /// </summary>
        /// <param name="partnerId">the id of the partner</param>
        /// <returns>The number of states a partner has facilities in</returns>
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
            //return entity.HealthFacilities.Where(e => e.ImplementingPartnerId == partnerId).Select(e => e.LGA).Distinct().Count();

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

            //var states= entity.dqa_facility.Where(e => e.Partners == partnerId).Select(e => e.State).Distinct();
            //var summaries = new List<StateSummary>();
            //foreach (var stateId in states)
            //{
            //    var summary = new StateSummary();
            //    var state = entity.dqa_states.FirstOrDefault(e => e.id == stateId);
            //    if (state == null) continue;
            //    summary.Id = state.id;
            //    summary.Name = state.state_name;

            //    //get the facilities submitted for the state
            //    summary.Submitted = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.StateId == stateId && e.ReportPeriod == reporting_period).Count();
            //    var total_state_facilities = entity.dqa_facility.Count(e => e.Partners == partnerId && e.State == stateId);
            //    summary.Pending = total_state_facilities - summary.Submitted;
            //    if (total_state_facilities > 0)
            //    {
            //        var value= (float.Parse(summary.Submitted.ToString()) / float.Parse(total_state_facilities.ToString())) * 100;

            //        summary.Percentage =Convert.ToInt32(value);//((summary.Submitted / total_state_facilities) * 100);
            //    }
            //    summaries.Add(summary);
            //}
            return summaries;
        }


        public static List<HealthFacility> GetSubmittedFacilities(int partnerId, string reporting_period)
        {
            var facility_ids = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.ReportPeriod == reporting_period).Select(e => e.Id).ToList();
            return entity.HealthFacilities.Where(x => facility_ids.Contains((int)x.Id)).ToList();
        }


        public static List<HealthFacility> GetPendingFacilities(int partnerId, string reporting_period)
        {
            var facility_ids = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.ReportPeriod == reporting_period).Select(e => e.Id).ToList();
            return entity.HealthFacilities.Where(x => !facility_ids.Contains((int)x.Id)).ToList();
        }

        public static HealthFacility GetFacility(int facilityId)
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
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
            }
            finally
            {
                connection.Close();
            }
            return dataTable;
        }

        /*
        public static lga GetLga(string lgaId)
        {
            return entity.lgas.FirstOrDefault(e=>e.lga_code==lgaId);
        }

        public static state GetState(int stateId)
        {
            return entity.states.Find(stateId);
        }

        public static string GetFacilityLevel(int levelId)
        {
            try
            {
                return "";// entity.dqa_facility_level.FirstOrDefault(e => e.Id == levelId).FacilityLevel;
            }
            
            catch (Exception)
            {
                return "";
            }
        }

        public static string GetFacilityType(int typeId)
        {
            try
            {
                return "";//entity.dqa_facility_type.FirstOrDefault(e => e.Id == typeId).TypeName;
            }
            catch(Exception ex)
            {
                return "";
            }
            
        }

        public static string GetStringValue(SqlCommand command)
        {
            var connection = (SqlConnection)entity.Database.Connection;
            command.Connection = connection;
            //command.CommandType = CommandType.StoredProcedure;
            var result = "";
            try
            {
                connection.Open();

                result = (string)command.ExecuteScalar();


            }
            catch (Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }

            return result;

        }
        */

        public static int GetNumberValue(SqlCommand command)
        {
            var connection = (SqlConnection)entity.Database.Connection;
            command.Connection = connection;
            //command.CommandType = CommandType.StoredProcedure;
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
            //SqlCommand cmd = conn.CreateCommand();
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
            // string radetPeriod = System.Configuration.ConfigurationManager.AppSettings["ReportPeriod"];

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
        //public static DataTable GetQ1FY18Analysis(string IP_id, bool get_partner_report)
        //{
        //    var cmd = new SqlCommand();
        //    cmd.CommandText = "get_q1_FY18_analysis_report";
        //    cmd.CommandType = CommandType.StoredProcedure;
        //    cmd.Parameters.AddWithValue("@ip", IP_id);
        //    cmd.Parameters.AddWithValue("@get_partner_report", get_partner_report);
        //    var dataTable = GetDatable(cmd);

        //    return dataTable;
        //}
        public static DataTable GetFY18Analysis(string IP_id,string reportPeriod, bool get_partner_report)
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
    }
}