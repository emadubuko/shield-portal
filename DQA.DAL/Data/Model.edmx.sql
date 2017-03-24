
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 03/23/2017 21:54:49
-- Generated from EDMX file: C:\Users\cmadubuko\Google Drive\MGIC\Project\ShieldPortal\DQA.DAL\Data\Model.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [shield_db_test];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_LGA_states]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[lga] DROP CONSTRAINT [FK_LGA_states];
GO
IF OBJECT_ID(N'[dbo].[FK_LINK_VERSION]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[dqa_lnk_version_indicators] DROP CONSTRAINT [FK_LINK_VERSION];
GO
IF OBJECT_ID(N'[dbo].[FK_FK19DCEB72794DD3B7]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[lga] DROP CONSTRAINT [FK_FK19DCEB72794DD3B7];
GO
IF OBJECT_ID(N'[dbo].[FK_FK6ECB7A12794DD3B7]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[lga] DROP CONSTRAINT [FK_FK6ECB7A12794DD3B7];
GO
IF OBJECT_ID(N'[dbo].[FK_FKA9A8FFC7C3F5A779]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HealthFacility] DROP CONSTRAINT [FK_FKA9A8FFC7C3F5A779];
GO
IF OBJECT_ID(N'[dbo].[FK_FKA9A8FFC7CC15123B]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HealthFacility] DROP CONSTRAINT [FK_FKA9A8FFC7CC15123B];
GO
IF OBJECT_ID(N'[dbo].[FK_FKDAADEEE960D0DD2E]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[lga] DROP CONSTRAINT [FK_FKDAADEEE960D0DD2E];
GO
IF OBJECT_ID(N'[dbo].[FK_FKE287F3F135201EB8]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HealthFacility] DROP CONSTRAINT [FK_FKE287F3F135201EB8];
GO
IF OBJECT_ID(N'[dbo].[FK_FKE287F3F1C04CA587]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[HealthFacility] DROP CONSTRAINT [FK_FKE287F3F1C04CA587];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[dqa_funder]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_funder];
GO
IF OBJECT_ID(N'[dbo].[dqa_indicator]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_indicator];
GO
IF OBJECT_ID(N'[dbo].[dqa_lnk_version_indicators]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_lnk_version_indicators];
GO
IF OBJECT_ID(N'[dbo].[dqa_report_metadata]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_report_metadata];
GO
IF OBJECT_ID(N'[dbo].[dqa_report_value]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_report_value];
GO
IF OBJECT_ID(N'[dbo].[dqa_summary_indicators]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_summary_indicators];
GO
IF OBJECT_ID(N'[dbo].[dqa_summary_value]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_summary_value];
GO
IF OBJECT_ID(N'[dbo].[dqa_versions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[dqa_versions];
GO
IF OBJECT_ID(N'[dbo].[HealthFacility]', 'U') IS NOT NULL
    DROP TABLE [dbo].[HealthFacility];
GO
IF OBJECT_ID(N'[dbo].[ImplementingPartners]', 'U') IS NOT NULL
    DROP TABLE [dbo].[ImplementingPartners];
GO
IF OBJECT_ID(N'[dbo].[lga]', 'U') IS NOT NULL
    DROP TABLE [dbo].[lga];
GO
IF OBJECT_ID(N'[dbo].[states]', 'U') IS NOT NULL
    DROP TABLE [dbo].[states];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'dqa_funder'
CREATE TABLE [dbo].[dqa_funder] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Funder] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'dqa_indicator'
CREATE TABLE [dbo].[dqa_indicator] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [IndicatorCode] nvarchar(15)  NOT NULL,
    [IndicatorName] nvarchar(1000)  NOT NULL,
    [ThematicArea] nvarchar(50)  NOT NULL,
    [Readonly] nvarchar(1)  NULL
);
GO

-- Creating table 'dqa_report_metadata'
CREATE TABLE [dbo].[dqa_report_metadata] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SiteId] int  NOT NULL,
    [LgaId] varchar(50)  NOT NULL,
    [StateId] varchar(50)  NOT NULL,
    [LgaLevel] int  NOT NULL,
    [FundingAgency] int  NOT NULL,
    [ImplementingPartner] int  NOT NULL,
    [FiscalYear] nvarchar(4)  NOT NULL,
    [AssessmentWeek] int  NOT NULL,
    [CreateDate] datetime  NOT NULL,
    [CreatedBy] nvarchar(50)  NOT NULL,
    [ReportPeriod] nvarchar(20)  NOT NULL
);
GO

-- Creating table 'dqa_report_value'
CREATE TABLE [dbo].[dqa_report_value] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [MetadataId] int  NOT NULL,
    [IndicatorId] int  NOT NULL,
    [IndicatorValueMonth1] decimal(18,0)  NULL,
    [IndicatorValueMonth2] decimal(18,0)  NULL,
    [IndicatorValueMonth3] decimal(18,0)  NULL
);
GO

-- Creating table 'HealthFacilities'
CREATE TABLE [dbo].[HealthFacilities] (
    [Id] bigint IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(255)  NULL,
    [FacilityCode] nvarchar(255)  NULL,
    [Longitude] nvarchar(255)  NULL,
    [Latitude] nvarchar(255)  NULL,
    [LGAId] nvarchar(50)  NULL,
    [OrganizationType] nvarchar(255)  NULL,
    [ImplementingPartnerId] int  NULL
);
GO

-- Creating table 'ImplementingPartners'
CREATE TABLE [dbo].[ImplementingPartners] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(255)  NULL,
    [ShortName] nvarchar(255)  NULL,
    [Address] nvarchar(255)  NULL,
    [OrganizationType] nvarchar(255)  NULL,
    [MissionPartner] nvarchar(255)  NULL,
    [Logo] varbinary(max)  NULL,
    [WebSite] nvarchar(255)  NULL,
    [Fax] nvarchar(255)  NULL,
    [PhoneNumber] nvarchar(255)  NULL
);
GO

-- Creating table 'lgas'
CREATE TABLE [dbo].[lgas] (
    [lga_code] nvarchar(50)  NOT NULL,
    [state_code] nvarchar(50)  NULL,
    [lga_name] nvarchar(250)  NULL,
    [lga_hm_longcode] nvarchar(50)  NULL
);
GO

-- Creating table 'states'
CREATE TABLE [dbo].[states] (
    [state_code] nvarchar(50)  NOT NULL,
    [geo_polictical_region] nvarchar(50)  NULL,
    [state_name] nvarchar(250)  NULL
);
GO

-- Creating table 'dqa_lnk_version_indicators'
CREATE TABLE [dbo].[dqa_lnk_version_indicators] (
    [id] int IDENTITY(1,1) NOT NULL,
    [version_id] int  NOT NULL,
    [indicator_code] nvarchar(15)  NOT NULL
);
GO

-- Creating table 'dqa_summary_indicators'
CREATE TABLE [dbo].[dqa_summary_indicators] (
    [id] int IDENTITY(1,1) NOT NULL,
    [summary_code] nvarchar(10)  NOT NULL,
    [thematic_area] nvarchar(50)  NOT NULL,
    [indicator_element] nvarchar(1000)  NOT NULL
);
GO

-- Creating table 'dqa_versions'
CREATE TABLE [dbo].[dqa_versions] (
    [id] int IDENTITY(1,1) NOT NULL,
    [version_number] nvarchar(50)  NOT NULL
);
GO

-- Creating table 'dqa_summary_value'
CREATE TABLE [dbo].[dqa_summary_value] (
    [id] int IDENTITY(1,1) NOT NULL,
    [metadata_id] int  NOT NULL,
    [summary_object] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'DQADimensions'
CREATE TABLE [dbo].[DQADimensions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [FacilityName] nvarchar(max)  NOT NULL,
    [FacilityCode] nvarchar(max)  NOT NULL,
    [HTC_Charts] nvarchar(max)  NOT NULL,
    [PMTCT_STAT_charts] nvarchar(max)  NOT NULL,
    [PMTCT_EID_charts] nvarchar(max)  NOT NULL,
    [PMTCT_ARV_Charts] nvarchar(max)  NOT NULL,
    [TX_NEW_charts] nvarchar(max)  NOT NULL,
    [TX_CURR_charts] nvarchar(max)  NOT NULL,
    [HTC_Charts_Precisions] nvarchar(max)  NOT NULL,
    [PMTCT_STAT_Charts_Precisions] nvarchar(max)  NOT NULL,
    [PMTCT_EID_Charts_Precisions] nvarchar(max)  NOT NULL,
    [PMTCT_ARV_Charts_Precisions] nvarchar(max)  NOT NULL,
    [TX_NEW_Charts_Precisions] nvarchar(max)  NOT NULL,
    [TX_CURR_Charts_Precisions] nvarchar(max)  NOT NULL,
    [Total_Completeness_HTC_TST] nvarchar(max)  NOT NULL,
    [Total_Completeness_PMTCT_STAT] nvarchar(max)  NOT NULL,
    [Total_completeness_PMTCT_EID] nvarchar(max)  NOT NULL,
    [Total_completeness_PMTCT_ARV] nvarchar(max)  NOT NULL,
    [Total_completeness_TX_NEW] nvarchar(max)  NOT NULL,
    [Total_completeness_TX_CURR] nvarchar(max)  NOT NULL,
    [Total_consistency_HTC_TST] nvarchar(max)  NOT NULL,
    [Total_consistency_PMTCT_STAT] nvarchar(max)  NOT NULL,
    [Total_consistency_PMTCT_EID] nvarchar(max)  NOT NULL,
    [Total_consistency_PMTCT_ART] nvarchar(max)  NOT NULL,
    [Total_consistency_TX_NEW] nvarchar(max)  NOT NULL,
    [Total_consistency_TX_Curr] nvarchar(max)  NOT NULL,
    [Total_precision_HTC_TST] nvarchar(max)  NOT NULL,
    [Total_precision_PMTCT_STAT] nvarchar(max)  NOT NULL,
    [Total_precision_PMTCT_EID] nvarchar(max)  NOT NULL,
    [Total_precision_PMTCT_ARV] nvarchar(max)  NOT NULL,
    [Total_precision_TX_NEW] nvarchar(max)  NOT NULL,
    [Total_precision_TX_CURR] nvarchar(max)  NOT NULL,
    [Total_integrity_HTC_TST] nvarchar(max)  NOT NULL,
    [Total_integrity_PMTCT_STAT] nvarchar(max)  NOT NULL,
    [Total_integrity_PMTCT_EID] nvarchar(max)  NOT NULL,
    [Total_integrity_PMTCT_ART] nvarchar(max)  NOT NULL,
    [Total_integrity_TX_NEW] nvarchar(max)  NOT NULL,
    [Total_integrity_TX_Curr] nvarchar(max)  NOT NULL,
    [Total_Validity_HTC_TST] nvarchar(max)  NOT NULL,
    [Total_Validity_PMTCT_STAT] nvarchar(max)  NOT NULL,
    [Total_Validity_PMTCT_EID] nvarchar(max)  NOT NULL,
    [Total_Validity_PMTCT_ART] nvarchar(max)  NOT NULL,
    [Total_Validity_TX_NEW] nvarchar(max)  NOT NULL,
    [Total_Validity_TX_Curr] nvarchar(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'dqa_funder'
ALTER TABLE [dbo].[dqa_funder]
ADD CONSTRAINT [PK_dqa_funder]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'dqa_indicator'
ALTER TABLE [dbo].[dqa_indicator]
ADD CONSTRAINT [PK_dqa_indicator]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'dqa_report_metadata'
ALTER TABLE [dbo].[dqa_report_metadata]
ADD CONSTRAINT [PK_dqa_report_metadata]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'dqa_report_value'
ALTER TABLE [dbo].[dqa_report_value]
ADD CONSTRAINT [PK_dqa_report_value]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'HealthFacilities'
ALTER TABLE [dbo].[HealthFacilities]
ADD CONSTRAINT [PK_HealthFacilities]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'ImplementingPartners'
ALTER TABLE [dbo].[ImplementingPartners]
ADD CONSTRAINT [PK_ImplementingPartners]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [lga_code] in table 'lgas'
ALTER TABLE [dbo].[lgas]
ADD CONSTRAINT [PK_lgas]
    PRIMARY KEY CLUSTERED ([lga_code] ASC);
GO

-- Creating primary key on [state_code] in table 'states'
ALTER TABLE [dbo].[states]
ADD CONSTRAINT [PK_states]
    PRIMARY KEY CLUSTERED ([state_code] ASC);
GO

-- Creating primary key on [id] in table 'dqa_lnk_version_indicators'
ALTER TABLE [dbo].[dqa_lnk_version_indicators]
ADD CONSTRAINT [PK_dqa_lnk_version_indicators]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'dqa_summary_indicators'
ALTER TABLE [dbo].[dqa_summary_indicators]
ADD CONSTRAINT [PK_dqa_summary_indicators]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'dqa_versions'
ALTER TABLE [dbo].[dqa_versions]
ADD CONSTRAINT [PK_dqa_versions]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'dqa_summary_value'
ALTER TABLE [dbo].[dqa_summary_value]
ADD CONSTRAINT [PK_dqa_summary_value]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [Id] in table 'DQADimensions'
ALTER TABLE [dbo].[DQADimensions]
ADD CONSTRAINT [PK_DQADimensions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [IndicatorId] in table 'dqa_report_value'
ALTER TABLE [dbo].[dqa_report_value]
ADD CONSTRAINT [FK_dqa_report_value_dqa_indicator]
    FOREIGN KEY ([IndicatorId])
    REFERENCES [dbo].[dqa_indicator]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dqa_report_value_dqa_indicator'
CREATE INDEX [IX_FK_dqa_report_value_dqa_indicator]
ON [dbo].[dqa_report_value]
    ([IndicatorId]);
GO

-- Creating foreign key on [ImplementingPartner] in table 'dqa_report_metadata'
ALTER TABLE [dbo].[dqa_report_metadata]
ADD CONSTRAINT [FK_dqa_report_metadata_ImplementingPartners]
    FOREIGN KEY ([ImplementingPartner])
    REFERENCES [dbo].[ImplementingPartners]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dqa_report_metadata_ImplementingPartners'
CREATE INDEX [IX_FK_dqa_report_metadata_ImplementingPartners]
ON [dbo].[dqa_report_metadata]
    ([ImplementingPartner]);
GO

-- Creating foreign key on [MetadataId] in table 'dqa_report_value'
ALTER TABLE [dbo].[dqa_report_value]
ADD CONSTRAINT [FK_dqa_report_value_dqa_report_metadata]
    FOREIGN KEY ([MetadataId])
    REFERENCES [dbo].[dqa_report_metadata]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_dqa_report_value_dqa_report_metadata'
CREATE INDEX [IX_FK_dqa_report_value_dqa_report_metadata]
ON [dbo].[dqa_report_value]
    ([MetadataId]);
GO

-- Creating foreign key on [LGAId] in table 'HealthFacilities'
ALTER TABLE [dbo].[HealthFacilities]
ADD CONSTRAINT [FK_FKA9A8FFC7C3F5A779]
    FOREIGN KEY ([LGAId])
    REFERENCES [dbo].[lgas]
        ([lga_code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FKA9A8FFC7C3F5A779'
CREATE INDEX [IX_FK_FKA9A8FFC7C3F5A779]
ON [dbo].[HealthFacilities]
    ([LGAId]);
GO

-- Creating foreign key on [ImplementingPartnerId] in table 'HealthFacilities'
ALTER TABLE [dbo].[HealthFacilities]
ADD CONSTRAINT [FK_FKA9A8FFC7CC15123B]
    FOREIGN KEY ([ImplementingPartnerId])
    REFERENCES [dbo].[ImplementingPartners]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FKA9A8FFC7CC15123B'
CREATE INDEX [IX_FK_FKA9A8FFC7CC15123B]
ON [dbo].[HealthFacilities]
    ([ImplementingPartnerId]);
GO

-- Creating foreign key on [LGAId] in table 'HealthFacilities'
ALTER TABLE [dbo].[HealthFacilities]
ADD CONSTRAINT [FK_FKE287F3F135201EB8]
    FOREIGN KEY ([LGAId])
    REFERENCES [dbo].[lgas]
        ([lga_code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FKE287F3F135201EB8'
CREATE INDEX [IX_FK_FKE287F3F135201EB8]
ON [dbo].[HealthFacilities]
    ([LGAId]);
GO

-- Creating foreign key on [ImplementingPartnerId] in table 'HealthFacilities'
ALTER TABLE [dbo].[HealthFacilities]
ADD CONSTRAINT [FK_FKE287F3F1C04CA587]
    FOREIGN KEY ([ImplementingPartnerId])
    REFERENCES [dbo].[ImplementingPartners]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FKE287F3F1C04CA587'
CREATE INDEX [IX_FK_FKE287F3F1C04CA587]
ON [dbo].[HealthFacilities]
    ([ImplementingPartnerId]);
GO

-- Creating foreign key on [state_code] in table 'lgas'
ALTER TABLE [dbo].[lgas]
ADD CONSTRAINT [FK_LGA_states]
    FOREIGN KEY ([state_code])
    REFERENCES [dbo].[states]
        ([state_code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LGA_states'
CREATE INDEX [IX_FK_LGA_states]
ON [dbo].[lgas]
    ([state_code]);
GO

-- Creating foreign key on [state_code] in table 'lgas'
ALTER TABLE [dbo].[lgas]
ADD CONSTRAINT [FK_FK19DCEB72794DD3B7]
    FOREIGN KEY ([state_code])
    REFERENCES [dbo].[states]
        ([state_code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FK19DCEB72794DD3B7'
CREATE INDEX [IX_FK_FK19DCEB72794DD3B7]
ON [dbo].[lgas]
    ([state_code]);
GO

-- Creating foreign key on [state_code] in table 'lgas'
ALTER TABLE [dbo].[lgas]
ADD CONSTRAINT [FK_FK6ECB7A12794DD3B7]
    FOREIGN KEY ([state_code])
    REFERENCES [dbo].[states]
        ([state_code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FK6ECB7A12794DD3B7'
CREATE INDEX [IX_FK_FK6ECB7A12794DD3B7]
ON [dbo].[lgas]
    ([state_code]);
GO

-- Creating foreign key on [state_code] in table 'lgas'
ALTER TABLE [dbo].[lgas]
ADD CONSTRAINT [FK_FKDAADEEE960D0DD2E]
    FOREIGN KEY ([state_code])
    REFERENCES [dbo].[states]
        ([state_code])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FKDAADEEE960D0DD2E'
CREATE INDEX [IX_FK_FKDAADEEE960D0DD2E]
ON [dbo].[lgas]
    ([state_code]);
GO

-- Creating foreign key on [version_id] in table 'dqa_lnk_version_indicators'
ALTER TABLE [dbo].[dqa_lnk_version_indicators]
ADD CONSTRAINT [FK_LINK_VERSION]
    FOREIGN KEY ([version_id])
    REFERENCES [dbo].[dqa_versions]
        ([id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_LINK_VERSION'
CREATE INDEX [IX_FK_LINK_VERSION]
ON [dbo].[dqa_lnk_version_indicators]
    ([version_id]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------