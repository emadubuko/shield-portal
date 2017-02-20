using CommonUtil.Mapping;
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
        public static int GetIpSubmitted(int partnerId,string month)
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


        public static List<StateSummary> GetStateIpStateSummary(int partnerId,string reporting_period)
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


        public static List<HealthFacility> GetSubmittedFacilities(int partnerId,string reporting_period)
        {
            var facility_ids= entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId  && e.ReportPeriod == reporting_period).Select(e=>e.Id).ToList();
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
            var lga = entity.lgas.FirstOrDefault(e=>e.lga_code==lgaId);
            return lga !=null? lga.lga_name : "";
        }

        public static string GetStateName(string stateId)
        {
            var state = entity.states.FirstOrDefault(e=>e.state_code==stateId);
            return state != null ? state.state_name : "";
        }

        public static decimal? GetDecimal(object value)
        {
            try
            {
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
            //command.CommandType = CommandType.StoredProcedure;
            var dataTable = new DataTable();
            try
            {
                connection.Open();

                // dataTable.Load(command.ExecuteReader());
                SqlDataAdapter da = new SqlDataAdapter(command);
                // this will query your database and return the result to your datatable
                da.Fill(dataTable);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                connection.Close();
            }


            return dataTable;

        }

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
            
            catch (Exception ex)
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
            catch (Exception ex)
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



    }
}