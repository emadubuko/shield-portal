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
            return entity.dqa_facility.Where(e=>e.Partners==partnerId).Select(e=>e.State).Distinct().Count();
        }

        /// <summary>
        /// Get the number of lga a parnter is working in
        /// </summary>
        /// <param name="partnerId">the id of the partner</param>
        /// <returns>The number of lgas a partner has facilities in</returns>
        public static int GetIpLGACount(int partnerId)
        {
            return entity.dqa_facility.Where(e => e.Partners == partnerId).Select(e => e.LGA).Distinct().Count();
        }

        /// <summary>
        /// Get the number of facilities reporting for a partner for the reporting period
        /// </summary>
        /// <param name="partnerId">Id of the partner</param>
        /// <param name="month">Reporting period</param>
        /// <returns>Number of facilities</returns>
        public static int GetIpSubmitted(int partnerId,string month)
        {
            return entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.Month == month).Count();
        }

        /// <summary>
        /// Get the number of facilities for a parner
        /// </summary>
        /// <param name="partnerId">Id of the partner</param>
        /// <returns>Number of facilities</returns>
        public static int GetIpFacilitycount(int partnerId)
        {
            return entity.dqa_facility.Count(e => e.Partners == partnerId);
        }


        public static List<StateSummary> GetStateIpStateSummary(int partnerId,string reporting_period)
        {
            var states= entity.dqa_facility.Where(e => e.Partners == partnerId).Select(e => e.State).Distinct();
            var summaries = new List<StateSummary>();
            foreach (var stateId in states)
            {
                var summary = new StateSummary();
                var state = entity.dqa_states.FirstOrDefault(e => e.id == stateId);
                if (state == null) continue;
                summary.Id = state.id;
                summary.Name = state.state_name;

                //get the facilities submitted for the state
                summary.Submitted = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.StateId == stateId && e.Month == reporting_period).Count();
                var total_state_facilities = entity.dqa_facility.Count(e => e.Partners == partnerId && e.State == stateId);
                summary.Pending = total_state_facilities - summary.Submitted;
                if (total_state_facilities > 0)
                {
                    var value= (float.Parse(summary.Submitted.ToString()) / float.Parse(total_state_facilities.ToString())) * 100;

                    summary.Percentage =Convert.ToInt32(value);//((summary.Submitted / total_state_facilities) * 100);
                }
                summaries.Add(summary);
            }
            return summaries;
        }


        public static List<dqa_facility> GetSubmittedFacilities(int partnerId,string reporting_period)
        {
            var facility_ids= entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId  && e.Month == reporting_period).Select(e=>e.Id).ToList();
            return entity.dqa_facility.Where(x => facility_ids.Contains(x.Id)).ToList();
        }


        public static List<dqa_facility> GetPendingFacilities(int partnerId, string reporting_period)
        {
            var facility_ids = entity.dqa_report_metadata.Where(e => e.ImplementingPartner == partnerId && e.Month == reporting_period).Select(e => e.Id).ToList();
            return entity.dqa_facility.Where(x => !facility_ids.Contains(x.Id)).ToList();
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

        public static dqa_lga GetLga(int lgaId)
        {
            return entity.dqa_lga.Find(lgaId);
        }

        public static dqa_states GetState(int stateId)
        {
            return entity.dqa_states.Find(stateId);
        }

        public static string GetFacilityLevel(int levelId)
        {
            try
            {
                return entity.dqa_facility_level.FirstOrDefault(e => e.Id == levelId).FacilityLevel;
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
                return entity.dqa_facility_type.FirstOrDefault(e => e.Id == typeId).TypeName;
            }
            catch(Exception ex)
            {
                return "";
            }
            
        }
    }
}