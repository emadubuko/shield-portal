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
            References(x => x.IP);
            References(x => x.UploadedBy);
            HasMany(x => x.HTS_Index).Inverse()
                 .KeyColumn("MetaDataId").Cascade.None();
            HasMany(x => x.LinkageToTreatment).Inverse()
                .KeyColumn("MetaDataId").Cascade.None();
            HasMany(x => x.ART).Inverse()
                .KeyColumn("MetaDataId").Cascade.None();
            HasMany(x => x.PITC).Inverse()
                .KeyColumn("MetaDataId").Cascade.None();
            HasMany(x => x.PMTCT).Inverse()
                .KeyColumn("MetaDataId").Cascade.None();
            HasMany(x => x.Pmtct_Viral_Load).Inverse()
                .KeyColumn("MetaDataId").Cascade.None();
        }
    }

    //public class UploadErrorMap : ClassMap<UploadError>
    //{
    //    public UploadErrorMap()
    //    {
    //        Table("mpm_UploadError");

    //        Id(x => x.Id);
    //        Map(x => x.ErrorMessage).CustomType(typeof(XmlType<List<string>>));
    //        References(x => x.MetaData).Column("MetaDataId");
    //    }
    //}

    public class LinkageToTreatmentMap : ClassMap<LinkageToTreatment>
    {
        public LinkageToTreatmentMap()
        {
            Table("mpm_LinkageToTreatment");

            Id(x => x.Id);
            Map(x => x.SiteName);
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
            Map(x => x.SiteName);
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
            Map(x => x.SiteName);
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
            Map(x => x.SiteName);
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
            Map(x => x.SiteName);
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
            Map(x => x.SiteName);
            Map(x => x.AgeGroup);
            Map(x => x.NewHIVPos);
            Map(x => x.NewOnART);
            Map(x => x.KnownHIVPos);
            Map(x => x.AlreadyOnART);
            Map(x => x.SampleCollected_between_0_to_2);
            Map(x => x.Pos_between_0_to_2);
            Map(x => x.ARTInitiation_between_0_to_2);

            Map(x => x.SampleCollected_between_2_to_12);
            Map(x => x.Pos_between_2_to_12);
            Map(x => x.ARTInitiation_between_2_to_12);
            References(x => x.MetaData).Column("MetaDataId");
        }
    }


}
