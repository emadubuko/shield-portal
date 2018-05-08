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

        public virtual IList<HTS_Index> HTS_Index { get; set; }
        public virtual IList<LinkageToTreatment> LinkageToTreatment { get; set; }
        public virtual IList<ART> ART { get; set; }
        public virtual  IList<PMTCT_Viral_Load> Pmtct_Viral_Load { get; set; }
        public virtual IList<HTS_Other_PITC> PITC { get; set; }
        public virtual IList<PMTCT> PMTCT { get; set; }
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
        public virtual int? NewHIVPos { get; set; }
        public virtual int? NewOnART { get; set; }
        public virtual int? KnownHIVPos { get; set; }
        public virtual int? AlreadyOnART { get; set; }

        public virtual int? SampleCollected_between_0_to_2 { get; set; }
        public virtual int? Pos_between_0_to_2 { get; set; }
        public virtual int? ARTInitiation_between_0_to_2 { get; set; }
        public virtual int? SampleCollected_between_2_to_12 { get; set; }
        public virtual int? Pos_between_2_to_12 { get; set; }
        public virtual int? ARTInitiation_between_2_to_12 { get; set; }
        public virtual MetaData MetaData { get; set; }
    }

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
     
}
