using BWReport.DAL.DAO;
using BWReport.DAL.Entities;
using CommonUtil.DAO;
using CommonUtil.DBSessionManager;
using CommonUtil.Entities;
using CommonUtil.Utilities;
using DAL.DAO;
using DAL.Entities;
using NHibernate;
using NHibernate.Engine;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("started");

            //new Program().UpdateFacilities();
            //  new Program().GenerateFacilityTargetCSV();


            new Program().GenerateFacilityCode();
            //new Program().UnprotectExcelFile();
          //  new Program().CopyAndPaste();

            Console.ReadLine();
        }
        
        public void GenerateFacilityCode()
        {
            YearlyPerformanceTargetDAO yptDAO = new YearlyPerformanceTargetDAO();

           // HealthFacilityDAO _sdfDao = new HealthFacilityDAO();
           // var IndexPeriods = ExcelHelper.GenerateIndexedPeriods();
            //var LGADictionary = new LGADao().RetrieveAll().ToDictionary(x => x.lga_code);

            //var ypts = yptDAO.GenerateYearlyTargetGroupedByLGA(2017);
           // var facilitiesGroupedByLGA = _sdfDao.RetrieveAll().GroupBy(x => x.LGA.lga_name).ToList();
             

            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\December 31 2016\December 31 2016\";// @"C:\Users\cmadubuko\Google Drive\MGIC\Project\ShieldPortal\Test\sample biweekly files\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.TopDirectoryOnly);

            var masterList = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\Reconciliation\FacilityWithCodes.csv";
            string[] linesInmasterList = File.ReadAllText(masterList).Split(new string[] { System.Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

            StringBuilder sb = new StringBuilder();
            foreach (var file in files)
            {
                string newTemplate = file.Replace(".xlsx", "_new.xlsx");
                string existingTemplate = file;
                using (ExcelPackage package = new ExcelPackage(new FileInfo(existingTemplate)))
                {
                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string lgaName = sheet.Name;
                        if (lgaName.ToLower().Contains("dashboard") || lgaName.Contains("LGA Level Dashboard"))
                            continue;

                        for (int row = 8; ; row++)
                        {
                            string fName = (string)sheet.Cells[row, 2].Value;
                            if (!string.IsNullOrEmpty(fName))
                            {
                                string code = SearchMasterList(linesInmasterList, fName, lgaName);
                                sheet.Cells[row, 1].Value = code;
                                if (code == "")
                                {
                                    sb.AppendLine(fName + "," + lgaName + "," + file);
                                }
                            }
                            else
                                break;
                        }
                    }
                    //foreach (var item in facilitiesGroupedByLGA)
                    //{
                    //    if (ypts.FirstOrDefault(x => x.lga_name == item.Key) == null)
                    //        continue;

                    //    string lgaName = item.Key;

                    //    ExcelWorksheet sheet = RetrieveMatchingWorkSheet(lgaName, package.Workbook.Worksheets, LGADictionary);
                    //    if (sheet == null)
                    //        continue;
                    //    var facilitiesInLGA = item.ToList();

                        //for (int row = 8; ; row++)  
                        //{
                        //    string fName = (string)sheet.Cells[row, 3].Value;
                        //    if (!string.IsNullOrEmpty(fName))
                        //    {
                        //        var sdf = facilitiesInLGA.FirstOrDefault(x => x.Name.ToLower().Trim() == fName);
                                

                        //        if (sdf != null)
                        //        {
                        //            sheet.Cells[row, 1].Value = sdf.FacilityCode;
                        //        }
                        //    }
                        //    else
                        //        break;
                        //}
                    //}
                    package.SaveAs(new FileInfo(newTemplate));
                }
            }
            File.WriteAllText(baseLocation + "_notFound.csv", sb.ToString());


        }


        private string SearchMasterList(string[] lines, string fname, string lgaName)
        {
            if (lgaName.ToLower().Contains("ifako"))
                lgaName = "Ifako Ijaiye";

            if (lgaName.ToLower().Contains("amac"))
                lgaName = "Municipal Area Council";

            foreach (var line in lines)
            {
                string[] items = line.Split(',');
                if (string.IsNullOrEmpty(items[0]))
                    break;
                
                string Sname = items[0].Trim().ToLower().Replace("-","").Replace(",","");
                string Slganame = items[6].Trim().ToLower();
                if (Sname == fname.Trim().ToLower().Replace("-", "").Replace(",", "") && lgaName.Trim().ToLower() ==Slganame )
                    return items[2];
            }
            return "";
        }

        private ExcelWorksheet RetrieveMatchingWorkSheet(string lgaName, ExcelWorksheets worksheets, Dictionary<string, LGA> lGADictionary)
        {
            ExcelWorksheet sheet = null;
            foreach (var sh in worksheets)
            {
                string sheet_lga = ExcelHelper.ReadCell(sh, 1, 1);

                LGA theLGA = null;
                lGADictionary.TryGetValue("NIE " + sheet_lga, out theLGA);
                if (theLGA == null)
                {
                    continue;
                }
                if (theLGA.lga_name == lgaName)
                    sheet = sh;
            }

            return sheet;
        }

        private void GenerateFacilityTargetCSV()
        {
            var hfDAO = new HealthFacilityDAO();
            var yptDAO = new YearlyPerformanceTargetDAO();
            StringBuilder sb = new StringBuilder();
            StringBuilder valid = new StringBuilder();

            List<YearlyPerformanceTarget> ypts = new List<YearlyPerformanceTarget>();

            valid.AppendLine("facilityName, facilityCode, HTC_TST, HTC_TST_pos, Tx_NEW");
            sb.AppendLine("facilityName, facilityCode, HTC_TST, HTC_TST_pos, Tx_NEW");

            string baseLocation = @"C:\Users\cmadubuko\Google Drive\MGIC\Documents\BWR\December 31 2016\December 31 2016\";
            string[] files = Directory.GetFiles(baseLocation, "*.xlsx", SearchOption.TopDirectoryOnly);

            //List<string> files = new List<string>
            //{
            //    "CCFN_Other_LGAs_sample.xlsx", "APIN_sample.xlsx", "CCFN_sample.xlsx","CIHP_sample.xlsx","IHVN_sample.xlsx",
            //};
            foreach (var file in files)
            {
                var existingFacilities = hfDAO.RetrieveAll().Where(x => file.Split('_')[0] == x.Organization.ShortName);//.ToDictionary(x => x.Name);
                valid.AppendLine(file);
                sb.AppendLine(file);

                using (ExcelPackage package = new ExcelPackage(new FileInfo(file)))
                {
                    var sheets = package.Workbook.Worksheets.Where(x => x.Hidden == eWorkSheetHidden.Visible).ToList();
                    foreach (var sheet in sheets)
                    {
                        string name = sheet.Name;
                        if (name.ToLower().Contains("dashboard") || name.Contains("LGA Level Dashboard"))
                            continue;

                        int row = 8;

                        while (true)
                        {
                            string facilityName = ExcelHelper.ReadCell(sheet, row, 2);
                            if (string.IsNullOrEmpty(facilityName))
                            {
                                break;
                            } 

                            HealthFacility theFacility = null;
                            theFacility = existingFacilities.FirstOrDefault(x => x.Name.Trim() == facilityName.Trim());

                            int HTC_TST = 0;
                            int HTC_TST_pos = 0;
                            int Tx_NEW = 0;
                            int.TryParse(ExcelHelper.ReadCell(sheet, row, 5), out HTC_TST);
                            int.TryParse(ExcelHelper.ReadCell(sheet, row, 7), out HTC_TST_pos);
                            int.TryParse(ExcelHelper.ReadCell(sheet, row, 9), out Tx_NEW);

                            if (theFacility == null)
                            {                                
                                sb.AppendLine(string.Format("{0},{1},{2},{3},{4}, {5}", facilityName, "", HTC_TST, HTC_TST_pos, Tx_NEW, sheet.Name));
                            }
                            //else
                            //{                                
                            //    valid.AppendLine(string.Format("{0},{1},{2},{3},{4}, {5}", facilityName, theFacility.FacilityCode, HTC_TST, HTC_TST_pos, Tx_NEW, theFacility.LGA.DisplayName));
                                 
                            //    ypts.Add(new YearlyPerformanceTarget
                            //    {
                            //        FiscalYear = 2017,
                            //        HealthFacilty = theFacility,
                            //        HTC_TST = HTC_TST,
                            //        HTC_TST_POS = HTC_TST_pos,
                            //        Tx_NEW = Tx_NEW,
                            //    });
                            //}
                            row++;
                        }
                    }
                }
            }
           // yptDAO.BulkInsert(ypts);
            File.WriteAllText(baseLocation + "_notFound.csv", sb.ToString());
           // File.WriteAllText(baseLocation + "_Found.csv", valid.ToString());
            Console.WriteLine("press enter to continue");
            Console.ReadLine();            
        }


        private async void UpdateFacilities()
        {
            HealthFacilityDAO hfdao = new HealthFacilityDAO();
            var hfs = hfdao.RetrieveAll();
            StringBuilder sb = new StringBuilder();
            var taskList = new List<Task<string>>();

            foreach (var hf in hfs)
            {
                string baseUrl = "https://www.datim.org/api/organisationUnits/" + hf.FacilityCode + "?fields=coordinates";
                taskList.Add(RetrieveDatimData(baseUrl, hf));
            }

            var result = await Task.WhenAll(taskList);
            string query = string.Join(";", result);
            Console.WriteLine("Finish fetching data. Press enter to update the db");
            //Console.ReadLine();
            DirectUpdateDB(query);
        }

        private async Task<string> RetrieveDatimData(string uri, HealthFacility hf)
        {
            string script = "";
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes("UMB_SHIELD:UMB@sh1eld")));
                httpClient.Timeout = TimeSpan.FromMinutes(2);

                await httpClient.GetAsync(uri)
                  .ContinueWith(x =>
                 {
                     Console.WriteLine("generated script for hf :{0}", uri);
                     if (x.IsCompleted && x.Status == TaskStatus.RanToCompletion)
                     {
                         var dt = x.Result.Content.ReadAsAsync<DatimResponse>().Result;
                         if (dt != null && !string.IsNullOrEmpty(dt.coordinates))
                         {
                             string[] lnglat = dt.coordinates.Split(new string[] { "[", "]", "," }, StringSplitOptions.RemoveEmptyEntries);
                             if (lnglat.Count() == 2)
                             {
                                 Console.WriteLine("generated script for hf :{0}", hf.Id);
                                 script = string.Format("Update[HealthFacility] set longitude = '{0}', latitude = '{1}' where id = {2};", lnglat[0], lnglat[1], hf.Id);
                             }
                         }
                     }
                 });
            }
            return script;
        }


        void DirectUpdateDB(string script)
        {
            ISessionFactory sessionFactory = NhibernateSessionManager.Instance.GetSession().SessionFactory;

            using (var connection = ((ISessionFactoryImplementor)sessionFactory).ConnectionProvider.GetConnection())
            {
                SqlConnection s = (SqlConnection)connection;
                SqlCommand command = new SqlCommand(script, s);
                int rows = command.ExecuteNonQuery();
                command.Dispose();
                Console.WriteLine("{0} rows affected", rows);
            }
        }

        public void InsertFacility()
        {
            StreamReader reader = new StreamReader(@"C:\Users\Somadina Mbadiwe\AppData\Roaming\Skype\My Skype Received Files\Facility_List(1).csv");
            string header = reader.ReadLine();
            string[] content = reader.ReadToEnd().Split(new string[] { "\n\r", System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            reader.Close();
            reader.Dispose();
            HealthFacilityDAO hfdao = new HealthFacilityDAO();
            IList<LGA> lgas = new LGADao().RetrieveAll();
            IList<Organizations> orgs = new OrganizationDAO().RetrieveAll();

            foreach (var item in content)
            {
                string[] theline = item.Split(',');

                LGA lga = lgas.FirstOrDefault(x => x.lga_name.Trim().ToUpper() == theline[2].Trim().ToUpper() && x.State.state_name.Trim().ToUpper() == theline[1].Trim().ToUpper());
                if (lga == null)
                    continue;

                HealthFacility hf = new HealthFacility
                {
                    FacilityCode = theline[4],
                    Name = theline[3],
                    LGA = lga,
                    Organization = orgs.FirstOrDefault(x => x.ShortName == theline[5]),
                    OrganizationType = CommonUtil.Enums.OrganizationType.HealthFacilty
                };
                hfdao.Save(hf);
            }
            hfdao.CommitChanges();
        }
        
        void CopyAndPaste()
        {
            string destinationDir = @"C:\Users\cmadubuko\Desktop\DQAs\";
            string baseLocation = @"C:\Users\cmadubuko\Desktop\Uploads\";
            string doneLocation = @"C:\Users\cmadubuko\Desktop\Unproted doc\";

            var files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            foreach (var file in files)
            {
                string newTemplate = destinationDir + file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries)[5];
                string existingTemplate = doneLocation + file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries)[5];
                

                bool exist = File.Exists(existingTemplate);
                if (exist)
                    continue;

                if (!File.Exists(newTemplate))
                {
                    FileInfo info = new FileInfo(existingTemplate);
                    info.CopyTo(newTemplate);
                }                                
            } 
        }

        void UnprotectExcelFile()
        {
            string destinationDir = @"C:\Users\cmadubuko\Desktop\DQAs\";
            string baseLocation = @"C:\Users\cmadubuko\Desktop\Uploads\";
            var files = Directory.GetFiles(baseLocation, "*.xlsm", SearchOption.TopDirectoryOnly);
            Application xApp = new Application();
            foreach (var file in files)
            {
                string newTemplate = destinationDir + file.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries)[5];
                string existingTemplate = file;

                bool exist = File.Exists(newTemplate);
                if (exist)
                    continue;

                var wkb = xApp.Workbooks.Open(file, 0, false, 5, "Pa55w0rd1", "", false, XlPlatform.xlWindows, "", true, false, 0, true);
                wkb.Unprotect("Pa55w0rd1");

                foreach (Worksheet sheet in wkb.Sheets)
                {
                    sheet.Unprotect("Pa55w0rd1");
                    sheet.Visible = XlSheetVisibility.xlSheetVisible;
                }
                wkb.SaveAs(newTemplate, wkb.FileFormat, "", "", false, false, XlSaveAsAccessMode.xlNoChange);
                wkb.Close();
            }
            xApp.Quit();
        }

        public void CreateNewDMP()
        {
            Guid guid = new Guid("CC16C80A-593F-4AB5-837C-A6F301107842");
            var tt = new ProfileDAO().Retrieve(guid);
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
                Version = "0.1",
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
            QualityAssurance = new QualityAssurance
            {
            },
            IntellectualPropertyCopyrightAndOwnership = new IntellectualPropertyCopyrightAndOwnership
            {
                ContractsAndAgreements = "None",
                Ownership = "Fully Us",
                UseOfThirdPartyDataSources = "None Needed"
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

    public class DatimResponse
    {
        public string coordinates { get; set; }
    }
}
