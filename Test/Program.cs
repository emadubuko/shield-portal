using DAL.DAO;
using DAL.Entities;
using DAL.Utilities;
using System;
using System.Collections.Generic;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press enter to begin");

            new Program().CreateNewDMP();
            //new Program().QuerySystem();

            Console.ReadLine();

        }

        public void CreateNewDMP()
        {
            Guid guid = new Guid("CC16C80A-593F-4AB5-837C-A6F301107842");
           var tt=  new ProfileDAO().Retrieve(guid);
          //  profile.Username = "John doe ";
            new ProfileDAO().Save(profile);
            var dmpDao = new DMPDAO();
            var dmpDocumentDao = new DMPDocumentDAO();

            Organizations org = new BaseDAO<Organizations, int>().Retrieve(1);
            //    new Organizations
            //{
            //    Address = "Jahi District",
            //    Name = "Centre for Clinical Care and Research Nigeria",
            //    OrganizationType = OrganizationType.ImplemetingPartner,
            //    ShortName = "CCCRN"
            //};
            //new BaseDAO<Organizations, int>().Save(org);

            page.ProjectProfile.ProjectDetails.Organization = org;
            new BaseDAO<ProjectDetails, int>().Save(page.ProjectProfile.ProjectDetails);

            DMP dmp = new DMP
            {
                CreatedBy = profile,
                DateCreated = DateTime.Now,
                DMPTitle = "CCCRN SEEDS DMP",
                EndDate = DateTime.Now.AddMonths(9),
                StartDate = DateTime.Now.AddMonths(1),
                TheProject = page.ProjectProfile.ProjectDetails,
                Organization = org
            };

            DMPDocument dmpDocument = new DMPDocument
            {
                Document = page,
                PageNumber = 1,
                Initiator = profile,
                InitiatorUsername = profile.Username,
                LastModifiedDate = DateTime.Now,
                ReferralCount = 0,
                Status = DMPStatus.New,
                CreationDate = DateTime.Now,
                TheDMP = dmp,
                Version ="0.1", 
            };

            try
            {
                dmpDao.Save(dmp);
                dmpDocumentDao.Save(dmpDocument);
                dmpDao.CommitChanges();
            }
            catch
            {
                dmpDao.RollbackChanges();
            }

        }

        public void QuerySystem()
        {
            var dmpDao = new DMPDocumentDAO();
            dmpDao.GenericSearch("maryland global");
            //var yy = dmpDao.Retrieve(1);

            Console.ReadLine();
        }

        WizardPage page = new WizardPage
        {
            ProjectProfile = new ProjectProfile
            {
                EthicalApproval = new EthicsApproval
                {
                    EthicalApprovalForTheProject = "Yes there is",
                    TypeOfEthicalApproval = "General",
                },
                ProjectDetails = new ProjectDetails
                {
                    AbreviationOfImplementingPartner = "CCCRN",
                    AddressOfOrganization = "Jahi district",
                    NameOfImplementingPartner = "Center for clinical research nigeria",
                    DocumentTitle = "SEED DMP for CCCRN",
                    ProjectTitle = "SEEDS Evaluation",
                    ProjectEndDate = string.Format("{0:dd-MMM-yyyy}", DateTime.Now.AddMonths(7)),
                    ProjectStartDate = string.Format("{0:dd-MMM-yyyy}", DateTime.Now.AddMonths(-2)),
                },
            },
            DocumentRevisions = new List<DocumentRevisions>
            {
                new DocumentRevisions{
                Version = new DAL.Entities.Version
                {
                    Approval = new Approval
                    {
                        SurnameApprover = "madubuko",
                        FirstnameofApprover = "Emeka"
                    },
                    VersionAuthor = new VersionAuthor
                    {
                        SurnameAuthor = "Madubuko",
                        FirstNameOfAuthor = "Christian"
                    },
                    VersionMetadata = new VersionMetadata
                    {
                        VersionDate = DateTime.Now.ToShortDateString(),
                        VersionNumber = "1.0.0"
                    },
                }
                }
            },
            Planning = new Planning
            {
                Summary = new Summary
                { ProjectObjectives = "TL;DR. Too long dont read" }
            },
            //DataCollection = new DataCollection
            //{
            //    Report = new Report
            //    {
            //        ReportData = new ReportData
            //        {
            //            NameOfReport = "Test report",
            //            DataType = "dont know"
            //        },
            //        RoleAndResponsibilities = new RolesAndResponsiblities
            //        {
            //            CDC = "Determines the report",
            //            FMoH = "Archives",
            //            HealthFacility = "Generates the report",
            //            ImplementingPartner = "Mgic",
            //            LGA = "AMAC",
            //            StateMoH = "Non involved"
            //        }
            //    },
            //},
            QualityAssurance = new QualityAssurance
            {
                //DataVerification = new DataVerificaton
                //{
                //    FormsOfDataVerification = "Manual",
                //    TypesOfDataVerification = "DQA"
                //},
            },
            DataCollectionProcesses = new DataCollectionProcesses
            {
                DataCollectionProcessess = "Collected by hand"
            },
            DataStorage = new DataStorage
            {
                Digital = new DigitalData
                {
                    Backup = "None",
                    Storagetype = "DBs"
                },
                NonDigital = new NonDigitalData
                {
                    NonDigitalDataTypes = "Registers",
                    StorageLocation = "File Cabinet"
                }
            },
            IntellectualPropertyCopyrightAndOwnership = new IntellectualPropertyCopyrightAndOwnership
            {
                ContractsAndAgreements = "None",
                Ownership = "Fully Us",
                UseOfThirdPartyDataSources = "None Needed"
            },
            DataAccessAndSharing = new DataAccessAndSharing
            {
                DataAccess = "Everyone with Login",
                DataSharingPolicies = "Only staff",
                DataTransmissionPolicies = "SSL Secured",
                SharingPlatForms = "Mobile"
            },
            DataDocumentationManagementAndEntry = new DataDocumentationManagementAndEntry
            {
                NamingStructureAndFilingStructures = "camel Case Name, arranged Alphabetical order",
                StoredDocumentationAndDataDescriptors = "Dont know"
            },
            PostProjectDataRetentionSharingAndDestruction = new PostProjectDataRetentionSharingAndDestruction
            {
                DataToRetain = "None",
                DigitalDataRetention = new DigitalDataRetention
                {
                    DataRetention = "Yes, anticipated"
                },
                Licensing = "MIT",
                NonDigitalRentention = new NonDigitalDataRetention
                {
                    DataRention = "In place"
                },
                Duration = "As long as relevant",
                PreExistingData = "Nope"
            }
        };

        Profile profile = new Profile
        {
            ContactEmailAddress = "emadubuko@mgic-nigeria.org",
            ContactPhoneNumber = "08068627544",
            FirstName = "John",
            JobDesignation = "Software developer",
            Password = "password",
            Surname = "Doe",
            Username = "johndoe@missingPlace.org",  
        };

    }
}
