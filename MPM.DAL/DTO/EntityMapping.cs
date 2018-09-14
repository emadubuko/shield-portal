using CommonUtil.Utilities;
using FluentNHibernate.Mapping;
using System.Collections.Generic;

namespace MPM.DAL.DTO
{
    public class MetaDataMap : ClassMap<MetaData>
    {
        public MetaDataMap()
        {
            Table("mpm_MetaData");

            Id(x => x.Id);
            Map(x => x.ReportingPeriod);
            Map(x => x.DateUploaded);
            Map(x => x.ReportLevel);
            Map(x => x.ReportLevelValue);
            References(x => x.IP).Column("IP");
            References(x => x.UploadedBy).Column("UserId");
            HasMany(x => x.HTS_Index).Inverse()
                 .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.LinkageToTreatment).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.ART).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.PITC).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.PMTCT).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.Pmtct_Viral_Load).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.PMTCT_EID).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            //
            HasMany(x => x.TB_HIV_Treatment).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.TB_Presumptives).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.TB_Presumptives_Diagnosis).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.TB_Screened).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.TB_TPT_Completed).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();
            HasMany(x => x.TB_TPT_Eligible).Inverse()
                .KeyColumn("MetaDataId").Cascade.None().ExtraLazyLoad();

        }
    }
     

    public class LinkageToTreatmentMap : ClassMap<LinkageToTreatment>
    {
        public LinkageToTreatmentMap()
        {
            Table("mpm_LinkageToTreatment");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Sex);
            Map(x => x.POS);
            Map(x => x.Tx_NEW);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }

    public class HTS_IndexMap : ClassMap<HTS_Index>
    {
        public HTS_IndexMap()
        {
            Table("mpm_HTS_Index");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Sex);
            Map(x => x.POS);
            Map(x => x.NEG);
            Map(x => x.TestingType);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }

    public class HTS_Other_PITCMap : ClassMap<HTS_Other_PITC>
    {
        public HTS_Other_PITCMap()
        {
            Table("mpm_HTS_Other_PITC");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Sex);
            Map(x => x.POS);
            Map(x => x.NEG);
            Map(x => x.SDP);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }


    public class ARTMap : ClassMap<ART>
    {
        public ARTMap()
        {
            Table("mpm_ART");
            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Sex);
            Map(x => x.Denominator);
            Map(x => x.Numerator);
            Map(x => x.IndicatorType);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }


    public class PMTCT_Viral_LoadMap : ClassMap<PMTCT_Viral_Load>
    {
        public PMTCT_Viral_LoadMap()
        {
            Table("mpm_PMTCT_Viral_Load");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Category); 
            Map(x => x._less_than_1000);
            Map(x => x._greater_than_1000);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }

    public class PMTCTMap : ClassMap<PMTCT>
    {
        public PMTCTMap()
        {
            Table("mpm_PMTCT");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.NewClient);
            Map(x => x.NewlyTested);
            Map(x => x.NewHIVPos);
            Map(x => x.NewOnART);
            Map(x => x.KnownHIVPos);
            Map(x => x.AlreadyOnART);
            //Map(x => x.SampleCollected_between_0_to_2);
            //Map(x => x.Pos_between_0_to_2);
            //Map(x => x.ARTInitiation_between_0_to_2);

            //Map(x => x.SampleCollected_between_2_to_12);
            //Map(x => x.Pos_between_2_to_12);
            //Map(x => x.ARTInitiation_between_2_to_12);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }

    public class PMTCT_EIDMap : ClassMap<PMTCT_EID>
    {
        public PMTCT_EIDMap()
        {
            Table("mpm_PMTCT_EID");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);

            Map(x => x.SampleCollected);
            Map(x => x.Pos);
            Map(x => x.ARTInitiation);
             
            References(x => x.MetaData).Column("MetaDataId");
        }
    }

    //,,,,,

    public class TB_ScreenedMap : ClassMap<TB_Screened>
    {
        public TB_ScreenedMap()
        {
            Table("mpm_TB_Screened");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);

            Map(x => x.Sex);
            Map(x => x.Number);
            Map(x => x.Description);

            References(x => x.MetaData).Column("MetaDataId");
        }
    }
    public class TB_PresumptiveMap : ClassMap<TB_Presumptive>
    {
        public TB_PresumptiveMap()
        {
            Table("mpm_TB_Presumptive");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);

            Map(x => x.Sex);
            Map(x => x.Number);
            Map(x => x.Description);

            References(x => x.MetaData).Column("MetaDataId");
        }
    }
    public class TB_Presumptive_DiagnosedMap : ClassMap<TB_Presumptive_Diagnosed>
    {
        public TB_Presumptive_DiagnosedMap()
        {
            Table("mpm_TB_Presumptive_Diagnosed");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Sex);
            Map(x => x.Number);
            Map(x => x.Description);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }
    public class TB_TPT_CompletedMap : ClassMap<TB_TPT_Completed>
    {
        public TB_TPT_CompletedMap()
        {
            Table("mpm_TB_TPT_Completed");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);

            Map(x => x.Sex);
            Map(x => x.Number);
            Map(x => x.Description);

            References(x => x.MetaData).Column("MetaDataId");
        }
    }
    public class TB_TPT_EligibleMap : ClassMap<TB_TPT_Eligible>
    {
        public TB_TPT_EligibleMap()
        {
            Table("mpm_TB_TPT_Eligible");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);
            Map(x => x.Description);
            Map(x => x.Sex);
            Map(x => x.Started_on_TPT);
            Map(x => x.PLHIV_eligible_for_TPT);

            References(x => x.MetaData).Column("MetaDataId");
        }
    }

    public class TB_HIV_TreatmentMap : ClassMap<TB_HIV_Treatment>
    {
        public TB_HIV_TreatmentMap()
        {
            Table("mpm_TB_HIV_Treatment");

            Id(x => x.Id);
            References(x => x.Site).Column("SiteId");
            Map(x => x.AgeGroup);

            Map(x => x.Description);
            Map(x => x.New_TB_Cases);
            Map(x => x.Sex);
            Map(x => x.Tx_TB);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }

}
