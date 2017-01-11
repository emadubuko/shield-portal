using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMP.ViewModel
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

    }

    public class EditDocumentViewModel2
    {
        public virtual ICollection<Comment> Comments { get; set; }
      
        public string documentID { get; set; }
        public DMPStatus status { get; set; }

        public Organizations Organization { get; internal set; }
        public Profile Initiator { get; internal set; }

        public Approval approval { get; set; }
        public VersionAuthor versionAuthor { get; set; }
        public VersionMetadata versionMetadata { get; set; }

        public Summary summary { get; set; }
        public ReportData reportData { get; set; }
        public RolesAndResponsiblities roleNresp { get; set; }
        public DataVerificaton dataVerification { get; set; }
        public DataCollectionProcesses datacollectionProcesses { get; set; }
        public DigitalData digital { get; set; }
        public NonDigitalData nonDigital { get; set; }
        public IntellectualPropertyCopyrightAndOwnership intelProp { get; set; }
        public DataAccessAndSharing dataSharing { get; set; }
        public DataDocumentationManagementAndEntry dataDocMgt { get; set; }
        public PostProjectDataRetentionSharingAndDestruction ppData { get; set; }
        public DigitalDataRetention digitalDataRetention { get; set; }
        public NonDigitalDataRetention nonDigitalRetention { get; set; }
        public EthicsApproval ethicsApproval { get; set; }
        public ProjectDetails projectDetails { get; set; }


        public ICollection<string> ethicalApprovalTypes
        {
            get
            {
                return new string[] { "Temporary", "Global" };
            }
        }
        public ICollection<string> thematicAreas
        {
            get
            {
                return new string[] { "Research", "Data Validation" };
            }
        }
        public ICollection<string> reportDataType
        {
            get
            {
                return new string[] { "freeText", "digital" };
            }
        }
        public ICollection<string> formsOfDataVerification
        {
            get
            {
                return new string[] { "Visual", "others" };
            }
        }

        public ICollection<string> typesOfDataVerification
        {
            get
            {
                return new string[] { "Manual Comparison", "DQA" };
            }
        }
        public ICollection<string> storagetype
        {
            get
            {
                return new string[] { "Database", "others" };
            }
        }
        public ICollection<string> storageLocation
        {
            get
            {
                return new string[] { "On premise", "Cloud store" };
            }
        }
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