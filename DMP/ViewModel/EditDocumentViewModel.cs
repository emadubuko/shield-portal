using CommonUtil.Entities;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class EditDocumentViewModel
    {
        public string ProjectTitle { get; set; }
        public string ProjectLogo { get; set; }
        public string DocumentTitle { get; set; }
        public string organization { get; set; }
        public string ProjectSummary { get; set; }
        public string MissionPartner { get; set; }
        public string ProjectStartDate { get; set; }
        public string ProjectEndDate { get; set; }
        public string GrantReferenceNumber { get; set; }
        public string EthicalApprovalForTheProject { get; set; }
        public string TypeOfEthicalApproval { get; set; }
        public string VersionDate { get; set; }
        public string TitleOfAuthor { get; set; }
        public string SurnameAuthor { get; set; }
        public string FirstNameOfAuthor { get; set; }
        public string OtherNamesOfAuthor { get; set; }
        public string JobDesignation { get; set; }
        public string SignatureOfAuthor { get; set; }
        public string PhoneNumberOfAuthor { get; set; }
        public string EmailAddressOfAuthor { get; set; }
        public string TitleofApprover { get; set; }
        public string SurnameApprover { get; set; }
        public string FirstnameofApprover { get; set; }
        public string OthernamesofApprover { get; set; }
        public string JobdesignationApprover { get; set; }
        public string SignatureofApprover { get; set; }
        public string PhonenumberofApprover { get; set; }
        public string EmailaddressofApprover { get; set; }
        public string AddressofAuthor { get; set; }
        public string NameOfReport { get; set; }
        public string ThematicArea { get; set; }
        public string FrequencyOfDataCollection { get; set; }
        public string Datatype { get; set; }
        public string Dataformat { get; set; }
        public string DataCollectionAndReportingTools { get; set; }
        public string DataFlowChart { get; set; }
        public string Staffing { get; set; }
        public string StaffingInformation { get; set; }
        public string RoleAndResponsibilities { get; set; }
        public string DataHandlingAndEntry { get; set; }

        public string HealthFacility { get; set; }
        public string ImplementingPartner { get; set; }
        public string LGA { get; set; }
        public string StateMoH { get; set; }
        public string CDC { get; set; }
        public string FMoH { get; set; }
        public string FormsOfDataVerification { get; set; }
        public string TypesOfDataVerification { get; set; }
        public string DataCollectionProcessess { get; set; }
        public string VolumeOfdigitalData { get; set; }
        public string Storagetype { get; set; }
        public string Storagelocation { get; set; }
        public string Backup { get; set; }
        public string Datasecurity { get; set; }
        public string Patientconfidentialitypolicies { get; set; }
        public string StorageOfPreExistingData { get; set; }
        public string NonDigitalDataTypes { get; set; }
        public string NonDigitalStorageLocation { get; set; }
        public string SafeguardsAndRequirements { get; set; }
        public string ContractsAndAgreements { get; set; }
        public string Ownership { get; set; }
        public string UseOfThirdPartyDataSources { get; set; }
        public string DataAccess { get; set; }
        public string DataSharingPolicies { get; set; }
        public string DataTransmissionPolicies { get; set; }
        public string SharingPlatForms { get; set; }
        public string StoredDocumentationAndDataDescriptors { get; set; }
        public string NamingStructureAndFilingStructures { get; set; }

        public string DataRetention { get; set; }
        public string nonDigitalDataRetention { get; set; }

        public string DataToRetain { get; set; }
        public string PreExistingData { get; set; }
        public string Duration { get; set; }
        public string Licensing { get; set; }
        public string VersionNumber { get; set; }

        public string documentID { get; set; }
        public Guid leadactivitymanagerId { get; set; }
        public string AdditionalInformation { get; set; }
        public string StatesCoveredByImplementingPartners { get; set; }
    }

    public class EditDocumentViewModel2
    {
        public List<DocumentRevisions> documentRevisions { get; set; }
        public List<DataCollection> dataCollection { get; set; }

         public List<StaffGrouping> roles { get; set; }
        public List<StaffGrouping> responsibilities { get; set; }
        public List<string> reportingLevel { get; set; }

        public List<DataCollation> dataCollation { get; set; }

        public string DataFlowChart { get; set; }
        public string AdditionalInformation { get; set; }
        public List<Trainings> Trainings { get; set; }

        public bool EditMode { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }

        public string documentID { get; set; }
        public int dmpId { get; set; }
        public DMPStatus status { get; set; }

        public Organizations Organization { get; internal set; }
        public Profile Initiator { get; internal set; }

        public string ProjectSummary { get; set; }
        public Approval approval { get; set; }
        public VersionAuthor versionAuthor { get; set; }
        public VersionMetadata versionMetadata { get; set; }

        public Summary summary { get; set; }
        public ReportData reportData { get; set; }
        public List<ReportData> reportDataList { get; set; }
        public RolesAndResponsiblities roleNresp { get; set; }
        public List<DataVerificaton> dataVerification { get; set; }
        public Processes processes { get; set; }
        public People People { get; set; }
        public Processes Process { get; set; }
        public Equipment Equipment { get; set; }
        public DAL.Entities.Environment Environment { get; set; }
        
        public DataProcesses dataProcess { get; set; }
        public List<DigitalData> digital { get; set; }
        public List<NonDigitalData> nonDigital { get; set; }
        public IntellectualPropertyCopyrightAndOwnership intelProp { get; set; }
        public List<DataAccessAndSharing> dataSharing { get; set; }
        public List<DataDocumentationManagementAndEntry> dataDocMgt { get; set; }
        public PostProjectDataRetentionSharingAndDestruction ppData { get; set; }
        public DigitalDataRetention digitalDataRetention { get; set; }
        public NonDigitalDataRetention nonDigitalRetention { get; set; }
        public EthicsApproval ethicsApproval { get; set; }
        public ProjectDetails projectDetails { get; set; }

        public IDictionary<Guid, Profile> Profiles { get; set; }

        public List<string> YesNoOption
        {
            get
            {
                return new List<string> { "No", "Yes" };
            }
        }

        public List<string> storageLoation
        {
            get
            {
                return new List<string> { "Online", "Offline", "Compact disk", "Hard drives", "Vault" };
            }
        }

        public List<string> Duration
        {
            get
            {
                return new List<string> { "Days", "Weeks", "Months", "Years" };
            }
        }

        public List<string> equipment
        {
            get
            {
                return new List<string> { "IT equipments", "Tools", "IT solutions", "Centralized server", "Mobile computer Lab", "Others" };
            }
        }

        public List<string> reportingTools
        {
            get
            {
                return new List<string> { "Community enrollment form","Household enumeration forms","Household enumeration sticker",
                    "Mobile ART card","Referral forms","HIV request result form","Mobile appointment diary","Others(Please List)"  };
            }
        }
        public ICollection<string> ethicalApprovalTypes
        {
            get
            {
                return new string[] { "Non Research Determination", "Non- human subject research", "Human subject research", "Other" };
            }
        }

        public ICollection<string> rational
        {
            get
            {
                return new string[] { " Project", "Publish", "Other" };
            }
        }

        public ICollection<string> reviewBoard
        {
            get
            {
                return new string[] { "CDC Atlanta", "NHREC", "Others" };
            }
        }

        public ICollection<string> programArea
        {
            get
            { 
                return new string[] { "Prevention", "Treatment" };
            }
        }

        public ICollection<string> dataSourcesTypes
        {
            get
            {
                return new string[] { "EMR", "Registers", "Monthly Summary Forms", "Client intake forms", "Hand card", "Community enrollment form", "Household enumeration forms", "Household enumeration sticker", "Mobile ART card", "Referral forms", "HIV request result form", "Mobile appointment diary", "Others(Please List)" };
            }
        }
           

        public ICollection<string> dataFrequency
        {
            get
            {
                return new string[] { "Daily", "Weekly", "Bi - Weekly", "Monthly", "Bi - Monthly", "Quarterly", "Bi - Annually", "Annually" };
            }
        }
        public ICollection<string> reportDataType
        {
            get
            {
                return new string[] { "Qualitative", "Quantitative" };
            }
        }

        public ICollection<string> DataVerificationApproach
        {
            get
            {
                return new string[] { "Data Qualiity Assurance", "Data Quality Assessments", "Data Quality Audits" };
            }
        }
       
         public ICollection<string> reportTypes
        {
            get
            {
                return new string[] { "ART", "PMTCT", "HTC", "OVC", "RADET", "Bi-Weekly for scale-up LGAs", "OTHERS" };
            }
        }

        public ICollection<string> thematicAreas
        {
            get
            {
                return new string[] { "ART", "PMTCT", "HTC" };
            }
        }
        public ICollection<string> datareported
        {
            get
            {
                return new string[] { "GON", "PEPFAR", "OTHERS" };
            }
        }

        public ICollection<string> typesOfDataVerification
        {
            get
            { 
                return new string[] { "Soft - checks", "Cross - checks of data sources", "Document reviews", "Trace verification",
                "Rountine Data Quality Assessment","Data Quality Audit","Others" };
            }
        }

        public ICollection<string> storageformats
        {
            get
            {
                return new string[] {
                "Comma - separated values(CSV) file(.csv)",
                "Tab - delimited file(.tab)",
                "SQL data definition",
                "SPSS portable format (.por)",
                "eXtensible Mark - up Language(XML)",
                "Rich Text Format(.rtf)",
                "plain text data, UTF - 8(Unicode; .txt)",
                "TIFF version 6 uncompressed(.tif)",
                "Free Lossless Audio Codec (FLAC)(.flac)",
                "Waveform Audio Format(WAV)(.wav)",
                "MPEG - 1 Audio Layer 3(.mp3)",
                "MPEG - 4 High Profile(.mp4)",
                "motion JPEG 2000(.jp2)",
                "Open Document Text(.odt)",
                "HTML(.htm, .html)" };                 
            }
        }
        public ICollection<string> states { get; set; }

        public ICollection<string> nonDigitalDataTypes
        {
            get
            {
                return new string[] { "Files", "Registers" };
            }
        }
        public ICollection<string> nonDigitalStorageLocation
        {
            get
            {
                return new string[] { "Cabinet", "Drawers" };
            }
        }

       
    }
}