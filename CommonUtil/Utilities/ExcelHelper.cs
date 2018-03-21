using CommonUtil.DBSessionManager;
using NHibernate;
using NHibernate.Engine;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using OfficeOpenXml.FormulaParsing.Exceptions;

namespace CommonUtil.Utilities
{
    public class ExcelHelper
    {
        public static string ReadCell(ExcelWorksheet sheet, int row, int column)
        {
            var range = sheet.Cells[row, column] as ExcelRange;
            if (!string.IsNullOrEmpty(range.Formula))
            {
                try
                {
                    var calculateOptions = new ExcelCalculationOption();
                    calculateOptions.AllowCirculareReferences = true;
                    range.Calculate(calculateOptions);
                }
                catch (CircularReferenceException ex)
                {
                    throw ex;
                }
            }
            if (range.Value != null)
            {
                return range.Value.ToString();
            }
            return "";
        }

        public static string ReadCellText(ExcelWorksheet sheet, int row, int column)
        {
            var range = sheet.Cells[row, column] as ExcelRange;            
            if (range.Text != null)
            {
                return range.Text.ToString();
            }
            return "";
        }

        public static string ReadCell(ExcelWorksheet sheet, string address)
        {
            var range = sheet.Cells[address] as ExcelRange;
            if (!string.IsNullOrEmpty(range.Formula))
            {
                try
                {
                    var calculateOptions = new ExcelCalculationOption();
                    calculateOptions.AllowCirculareReferences = true;
                    range.Calculate(calculateOptions);
                }
                catch (CircularReferenceException ex)
                {
                    throw ex;
                }
            }
            if (range.Value != null)
            {
                return range.Value.ToString();
            }
            return "";
        }

        public static string GetRandomizeChartNUmber(string value)
        {
            int x = Convert.ToInt32(value);
            int result = 0;
            if (x >= 24 && x <= 30)
                result = 24;
            else if (x >= 31 && x <= 40)
                result = 30;
            else if (x >= 41 && x <= 50)
                result = 35;
            else if (x >= 51 && x <= 60)
                result = 39;
            else if (x >= 61 && x <= 70)
                result = 43;
            else if (x >= 71 && x <= 80)
                result = 46;
            else if (x >= 81 && x <= 90)
                result = 49;
            else if (x >= 91 && x <= 100)
                result = 52;
            else if (x >= 101 && x <= 119)
                result = 57;
            else if (x >= 120 && x <= 139)
                result = 61;
            else if (x >= 140 && x <= 159)
                result = 64;
            else if (x >= 160 && x <= 179)
                result = 67;
            else if (x >= 180 && x <= 199)
                result = 70;
            else if (x >= 200 && x <= 249)
                result = 75;
            else if (x >= 250 && x <= 299)
                result = 79;
            else if (x >= 300 && x <= 349)
                result = 82;
            else if (x >= 350 && x <= 399)
                result = 85;
            else if (x >= 400 && x <= 449)
                result = 87;
            else if (x >= 450 && x <= 499)
                result = 88;
            else if (x >= 500 && x <= 749)
                result = 94;
            else if (x >= 750 && x <= 999)
                result = 97;
            else if (x >= 1000 && x <= 4999)
                result = 105;
            else if (x >= 5000)
                result = 107;
            else
                result = x;

            return result.ToString();
        }

        public static Dictionary<string, int> GenerateIndexedPeriods()
        {
            Dictionary<string, int> indexedPeriod = new Dictionary<string, int>();
            string script = "Select ReportPeriod, ExcelColumn from ReportPeriodToDateMapper";

            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(script, s);
                SqlDataReader reader = null;
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    indexedPeriod.Add(reader[0] as string, Convert.ToInt32(reader[1]));
                }
                reader.Dispose();
            }

            return indexedPeriod;
        }

        public static List<string> RetrieveStatesName()
        {
            List<string> states = new List<string>();
            string script = "select [state_name] from [states]";

            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand selectCommand = new SqlCommand(script, s);
                SqlDataReader reader = null;
                reader = selectCommand.ExecuteReader();

                while (reader.Read())
                {
                    states.Add(reader[0] as string);
                }
                reader.Dispose();
            }
            return states;
        }
    }
}
