using CommonUtil.Entities;
using System;
using System.Collections.Generic;

namespace MPM.DAL.DTO
{
    public class MetaData
    {
        public virtual int Id { get; set; }
        public virtual string ReportingPeriod { get; set; }
        public virtual Profile UploadedBy { get; set; }
        public virtual DateTime DateUploaded { get; set; }
        public virtual Organizations IP { get; set; }
        public virtual ReportLevel ReportLevel { get; set; }
        public virtual string ReportLevelValue { get; set; }

        public virtual string FilePath { get; set; }

        public virtual IList<HTS_Index> HTS_Index { get; set; }
        public virtual IList<LinkageToTreatment> LinkageToTreatment { get; set; }
        public virtual IList<ART> ART { get; set; }
        public virtual IList<PMTCT_Viral_Load> Pmtct_Viral_Load { get; set; }
        public virtual IList<HTS_Other_PITC> PITC { get; set; }
        public virtual IList<PMTCT> PMTCT { get; set; }
        public virtual IList<PMTCT_EID> PMTCT_EID { get; set; }

        public virtual IList<TB_TPT_Eligible> TB_TPT_Eligible { get; set; }
        public virtual IList<TB_Screened> TB_Screened { get; set; }
        public virtual IList<TB_Presumptive> TB_Presumptives { get; set; }
        public virtual IList<TB_Bacteriology_Diagnosis> TB_Bacteriology_Diagnosis { get; set; }
        public virtual IList<TB_Diagnosed> TB_Diagnosed { get; set; }
         
        public virtual IList<TB_Treatment_Started> TB_Treatment_Started { get; set; }

        public virtual IList<TB_New_Relapsed> TB_Relapsed { get; set; }
        public virtual IList<TB_New_Relapsed_Known_Status> TB_New_Relapsed_Known_Status { get; set; }
        public virtual IList<TB_New_Relapsed_Known_Pos> TB_New_Relapsed_Known_Pos { get; set; }
        public virtual IList<TB_ART> TB_ART { get; set; }
    }

    public class UploadError
    {
        public virtual int Id { get; set; }
        public virtual IList<string> ErrorMessage { get; set; }
        public virtual MetaData MetaData { get; set; }
    }


    public class LinkageToTreatment : BaseT
    {
        public virtual MetaData MetaData { get; set; }
        public virtual int? POS { get; set; }
        public virtual int? Tx_NEW { get; set; }
    }

    public class HTS_Index : BaseT
    {
        public virtual MetaData MetaData { get; set; }
        public virtual int? POS { get; set; }
        public virtual int? NEG { get; set; }
        public virtual string TestingType { get; set; }
    }

    public class HTS_Other_PITC : BaseT
    {
        public virtual MetaData MetaData { get; set; }
        public virtual int? POS { get; set; }
        public virtual int? NEG { get; set; }
        public virtual string SDP { get; set; }
    }

    public class ART : BaseT
    {
        public virtual MetaData MetaData { get; set; }
        public virtual int? Denominator { get; set; }
        public virtual int? Numerator { get; set; }
        public virtual ART_Indicator_Type IndicatorType { get; set; }
    }

    public class PMTCT_Viral_Load
    {
        public virtual int Id { get; set; }
        public virtual HealthFacility Site { get; set; }
        public virtual string AgeGroup { get; set; }
        public virtual int? _less_than_1000 { get; set; }
        public virtual int? _greater_than_1000 { get; set; }
        public virtual PMTCT_Category Category { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class PMTCT
    {
        public virtual int Id { get; set; }
        public virtual HealthFacility Site { get; set; }
        public virtual string AgeGroup { get; set; }
        public virtual string Description { get; set; }
        public virtual int? NewClient { get; set; }
        public virtual int? KnownStatus { get; set; }
        //public virtual int? NewlyTested { get; set; }
        public virtual int? KnownHIVPos { get; set; }
        public virtual int? NewHIVPos { get; set; }
        public virtual int? AlreadyOnART { get; set; }
        public virtual int? NewOnART { get; set; }      
       

        //public virtual int? SampleCollected_between_0_to_2 { get; set; }
        //public virtual int? Pos_between_0_to_2 { get; set; }
        //public virtual int? ARTInitiation_between_0_to_2 { get; set; }
        //public virtual int? SampleCollected_between_2_to_12 { get; set; }
        //public virtual int? Pos_between_2_to_12 { get; set; }
        //public virtual int? ARTInitiation_between_2_to_12 { get; set; }

        public virtual MetaData MetaData { get; set; }
       
    }

    public class PMTCT_EID
    {
        public virtual int Id { get; set; }
        public virtual HealthFacility Site { get; set; }
        public virtual string AgeGroup { get; set; }
        public virtual int? SampleCollected { get; set; }
        public virtual int? Pos { get; set; }
        public virtual int? ARTInitiation { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_Screened : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_Presumptive : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_Bacteriology_Diagnosis : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_Diagnosed : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_Treatment_Started : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual int? Tx_TB { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }     

    public class TB_TPT_Eligible : BaseT
    {
        public virtual int? PLHIV_eligible_for_TPT { get; set; }
        public virtual int? Started_on_TPT { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_New_Relapsed : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_New_Relapsed_Known_Status : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_New_Relapsed_Known_Pos : BaseT
    {
        public virtual int? Known_Pos { get; set; }
        public virtual int? New_Pos { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    public class TB_ART : BaseT
    {
        public virtual int? Number { get; set; }
        public virtual string Description { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

    //public class TB_Treatment_Started : BaseT
    //{
    //    public virtual int? New_TB_Cases { get; set; }
    //    public virtual int? Tx_TB { get; set; }
    //    public virtual string Description { get; set; }
    //    public virtual MetaData MetaData { get; set; }
    //}

    public abstract class BaseT
    {
        public virtual int Id { get; set; }
        public virtual HealthFacility Site { get; set; }
        public virtual Sex Sex { get; set; }
        public virtual string AgeGroup { get; set; }
    }

    public enum Sex
    {
        F = 1, M = 2
    }

    public enum ART_Indicator_Type
    {
        Tx_RET = 1,
        Tx_VLA = 2
    }

    public enum PMTCT_Category
    {
        Newly_Identified = 1,
        Already_HIV_Positive = 2
    }

    public enum ReportLevel
    {
        State, LGA, Facility, IP
    }
}
