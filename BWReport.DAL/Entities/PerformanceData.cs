using CommonUtil.Enums;
using System;
using System.Collections.Generic;

namespace BWReport.DAL.Entities
{
    public class PerformanceData
    {
        public virtual long Id { get; set; }
        public virtual int HTC_TST { get; set; }
        public virtual int HTC_TST_POS { get; set; }
        public virtual int Tx_NEW { get; set; }

        public virtual string ReportPeriod { get; set; }
        public virtual int FY { get; set; }

        public virtual string ReportPeriodDisplayName
        {
            get
            {
                return ReportPeriod + " " + FY;
            }
        }

        public virtual HealthFacility HealthFacility { get; set; }

        public virtual ReportUploads ReportUpload { get; set; }

    }


    public class LGALevelAchievementPerTarget
    {
        public string lga_name { get; set; }
        public string lga_code { get; set; }

        public decimal LinkageRate
        {
            get
            {
                if (TOTAL_HTC_TST_POS == 0)
                    return 0;
                return (Convert.ToDecimal(Total_Tx_New) / Convert.ToDecimal(TOTAL_HTC_TST_POS)) * 100;
            }
        }

        public int TOTAL_HTC_TST { get; set; }
        public decimal HTC_TST_Target { get; set; }
        public decimal HTC_TST_Percentage_Achievement
        {
            get
            {
                if (HTC_TST_Target == 0)
                    return 0;
                return (Convert.ToDecimal(TOTAL_HTC_TST) / Convert.ToDecimal(HTC_TST_Target)) * 100;
            }
        }


        public int TOTAL_HTC_TST_POS { get; set; }
        public decimal HTC_TST_POS_Target { get; set; }
        public decimal HTC_TST_POS_Percentage_Achievement
        {
            get
            {
                if (HTC_TST_POS_Target == 0)
                    return 0;
                return (Convert.ToDecimal(TOTAL_HTC_TST_POS) / Convert.ToDecimal(HTC_TST_POS_Target)) * 100;
            }
        }

        public int Total_Tx_New { get; set; }
        public decimal Tx_New_Target { get; set; }
        public decimal Tx_New_Acheivement
        {
            get
            {
                if (Tx_New_Target == 0)
                    return 0;
                return (Convert.ToDecimal(Total_Tx_New) / Convert.ToDecimal(Tx_New_Target)) * 100;
            }
        }
    }

    public class Facility_Community_Postivity
    {
        public string lga_name { get; set; }
        public string lga_code { get; set; }

        public int Tested { get; set; }
        public int Positive { get; set; }
        public decimal Positivity
        {
            get
            {
                if (Tested == 0)
                    return 0;
                return (Convert.ToDecimal(Positive) / Convert.ToDecimal(Tested)) * 100;
            }
        }
        public OrganizationType FacilityType { get; set; }
    }

    public class IPUploadReport
    {
        public string IPName { get; set; }
        public string ReportPeriod { get; set; }
    }
}
