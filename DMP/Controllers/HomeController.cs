using CommonUtil.DAO;
using DAL.DAO;
using DMP.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DMP.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        DMPDAO dmpDAO = null;
        DMPDocumentDAO dmpDocDAO = null;
        OrganizationDAO orgDAO = null;
        ProjectDetailsDAO projDAO = null;
         
        public HomeController()
        {
            dmpDAO = new DMPDAO();
            dmpDocDAO = new DMPDocumentDAO();
            orgDAO = new OrganizationDAO();
            projDAO = new ProjectDetailsDAO();

        }

        public ActionResult Index()
        {
            return RedirectToAction("index", "DMP");
        }


        public ActionResult ViewAllTracker()
        {
            List<TrackerViewModel> trackableDMP = new List<TrackerViewModel>();

            //dmpDocDAO.SearchMostRecentByDMP

            return View();
        }


        public ActionResult Tracker(Guid documnentId)
        {
            var doc = dmpDocDAO.Retrieve(documnentId);

            if (doc.Document.MonitoringAndEvaluationSystems == null || doc.Document.QualityAssurance == null || doc.Document.DataProcesses.Reports == null)
            {
                return new HttpStatusCodeResult(400, "DMP document is not yet completed");
            }
            var trainings = doc.Document.MonitoringAndEvaluationSystems.People.Trainings;
            var dataCollection = doc.Document.DataProcesses.DataCollection;
            var dataVerification = doc.Document.QualityAssurance.DataVerification;
            var reports = doc.Document.DataProcesses.Reports.ReportData;


            List<GanttChartData> chartData = new List<GanttChartData>();
            List<ChartValues> values = new List<ChartValues>();
            foreach (var tr in trainings)
            {
                tr.TimelinesForTrainings.ForEach(z =>
                {
                    if (z > DateTime.MinValue)
                    {
                        values.Add(new ChartValues
                        {
                            customClass = Labels.trainingLabelClass,
                            from = z,
                            to = z.AddDays(tr.DurationOfTrainings),
                            label = Labels.trainingLabelName,
                            dataObj = tr.NameOfTraining,
                        });
                    }
                });
            }
            chartData.Add(
                   new GanttChartData
                   {
                       name = "Training",
                       values = values
                   });

            values = new List<ChartValues>();
            //foreach (var dt in dataCollection)
            //{
            //    dt.DataCollectionTimelines.ForEach(z =>
            //    {
            //        if (z > DateTime.MinValue)
            //        {
            //            values.Add(new ChartValues
            //            {
            //                customClass = Labels.dataCollectionLabelClass,
            //                from = z,
            //                to = z.AddDays(dt.DurationOfDataCollection),
            //                label = Labels.dataCollectionLabelName,
            //                dataObj = dt.DataType,
            //            });
            //        }
            //    });
            //}
            chartData.Add(
                    new GanttChartData
                    {
                        name = "Data Collection",
                        values = values
                    });

            values = new List<ChartValues>();
            foreach (var rpt in reports)
            {
                rpt.TimelinesForReporting.ForEach(z =>
                {
                    if (z > DateTime.MinValue)
                    {
                        values.Add(new ChartValues
                        {
                            customClass = Labels.reportLabelClass,
                            from = z,
                            to = z.AddDays(rpt.DurationOfReporting),
                            label = Labels.reportLabelName,
                            dataObj = rpt.NameOfReport,
                        });
                    }
                });
            }
            chartData.Add(
                    new GanttChartData
                    {
                        name = "Report",
                        values = values
                    });

            values = new List<ChartValues>();
            foreach (var dv in dataVerification)
            {
                dv.TimelinesForDataVerification.ForEach(z =>
                {
                    if (z > DateTime.MinValue)
                    {
                        values.Add(new ChartValues
                        {
                            customClass = Labels.dataVerificationLabelClass,
                            from = z,
                            to = z.AddDays(dv.DurationOfDataVerificaion),
                            label = Labels.dataVerificationLabelName,
                            dataObj = dv.TypesOfDataVerification + "\n " + dv.DataVerificationApproach,
                        });
                    }
                });
            }
            chartData.Add(
                    new GanttChartData
                    {
                        name = "Data Verification",
                        values = values
                    });


            TrackerViewModel vM = new TrackerViewModel
            {
                data = chartData,
                DocumentTitle = doc.DocumentTitle
            };

            return View(vM);
        }

        [AllowAnonymous]
        public ActionResult DynamicTable()
        {
            List<MockReportData> data = new List<MockReportData>();
            data.Add(new MockReportData
            {
                DurationOfReporting = "ij",
                FrequencyOfReporting = "jb",
                Id = 1,
                NameOfReport = "hjbk",
                ThematicArea = "tehre",
                TimelinesForReporting = new List<DateTime> { DateTime.Now, DateTime.Now.AddMonths(1) }
            });
            return View(data);
        }
        [HttpPost]
        public ActionResult DynamicTableAdd(List<MockReportData> data)
        {
            return Json(data.Count());
        }

        #region - obsolete
        /*
        public ActionResult CreateNewDMP()
        { 
            //System.Threading.Thread.Sleep(5000);             

            return View(OrgsRepo());
        }

        [HttpPost]
        public ActionResult DocumentWizardPage(DAL.Entities.DMP newDMP)
        {
            if(newDMP == null || string.IsNullOrEmpty(newDMP.DMPTitle))
            {
                return new HttpStatusCodeResult(400, "Please provide a title");
            }
            MyDMP = newDMP;
            MyDMP.DateCreated = DateTime.Now;
            MyDMP.CreatedBy = initiator;

            bool result = SaveOrUpdateDMP(true);
            if (result)
            {
                return Json(MyDMP.Id);
            }
            else
            {
                return new HttpStatusCodeResult(400, "DMP with this title already exists");
            }
        }

        public ActionResult DocumentWizardPage(int? dmpId)
        {
            if ((!dmpId.HasValue || dmpId.Value == 0))
            {
                return RedirectToAction("CreateNewDMP");
            }

            MyDMP = dmpDAO.Retrieve(dmpId.Value);
            var previousDoc = dmpDocDAO.SearchByDMP(dmpId.Value);
            if (previousDoc != null && previousDoc.Count() > 0)
            {
                Dictionary<string, object> routeObj = new Dictionary<string, object> { { "dmpId", MyDMP.Id }, { "documnentId", previousDoc.LastOrDefault().Id } };
                return RedirectToAction("EditDocumentWizardPage", new System.Web.Routing.RouteValueDictionary(routeObj));
            }

            CreateDocumentViewModel docVM = new CreateDocumentViewModel
            {
                Organization = MyDMP.Organization,
                Initiator = initiator,
            };
            return View(docVM);
        }

        [HttpPost]
        public ActionResult SaveNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF,
            Summary summary, RolesAndResponsiblities roleNresp, DataCollection DataCollection,
            ReportData reportData, DataVerificaton dataVerification, DigitalData digital, NonDigitalData nonDigital,
            DataCollectionProcesses datacollectionProcesses, IntellectualPropertyCopyrightAndOwnership intelProp,
            DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, Trainings Trainings,
            DigitalDataRetention digitalDataRetention, NonDigitalDataRetention nonDigitalRetention)
        {
            //incase session has timed out
            if (MyDMP == null)
            {
                var dmpid = Convert.ToInt32(Request.UrlReferrer.Query.Split(new string[] { "?dmpId=" }, StringSplitOptions.RemoveEmptyEntries)[0]);
                MyDMP = dmpDAO.Retrieve(dmpid);
                if (MyDMP == null)
                {
                    return RedirectToAction("CreateNewDMP");
                }
            }

            ProjectDetails ProjDetails = projDTF;
            ProjDetails.Organization = MyDMP.Organization;
            ProjDetails.AbreviationOfImplementingPartner = MyDMP.Organization.ShortName;
            ProjDetails.NameOfImplementingPartner = MyDMP.Organization.Name;


            WizardPage page = new WizardPage
            {
                ProjectProfile = new ProjectProfile
                {
                    EthicalApproval = ethicsApproval,
                    ProjectDetails = ProjDetails,
                },
                Planning = new Planning { Summary = summary },
                DataCollection = DataCollection,
                MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
                {
                    DataFlowChart = doc.DataFlowChart,
                    RoleAndResponsibilities = roleNresp,
                    Trainings = Trainings,
                },
                Reports = new Report
                {
                    ReportData = reportData
                },
                //DataCollection = new DataCollection
                //{
                //    Report = new Report
                //    {
                //        ReportData = reportData,
                //        RoleAndResponsibilities = roleNresp
                //    },
                //},
                QualityAssurance = new QualityAssurance { DataVerification = dataVerification },
                DataCollectionProcesses = datacollectionProcesses,
                DataStorage = new DataStorage
                {
                    Digital = digital,
                    NonDigital = nonDigital
                },
                IntellectualPropertyCopyrightAndOwnership = intelProp,
                DataAccessAndSharing = dataSharing,
                DataDocumentationManagementAndEntry = dataDocMgt,
                PostProjectDataRetentionSharingAndDestruction = new PostProjectDataRetentionSharingAndDestruction
                {
                    DataToRetain = doc.DataToRetain,
                    DigitalDataRetention = digitalDataRetention,
                    Licensing = doc.Licensing,
                    NonDigitalRentention = nonDigitalRetention,
                    Duration = doc.Duration,
                    PreExistingData = doc.PreExistingData
                }
            };


            try
            {
                bool saved = SaveProject(ProjDetails);
                if (saved || MyDMP.TheProject != null)
                {
                    projDAO.CommitChanges();

                    MyDMP.TheProject = ProjDetails;
                    dmpDAO.Update(MyDMP);
                    //SaveOrUpdateDMP(false);
                    var theDoc = SaveDMPDocument(page, MyDMP.Id);
                    dmpDocDAO.CommitChanges();

                  var data=   new { documentId = theDoc.Id, projectId = ProjDetails.Id };

                    return Json(data, JsonRequestBehavior.AllowGet);
                    //return Json(theDoc, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("project with the same name already exist", JsonRequestBehavior.AllowGet);
                }

            }
            catch (Exception ex)
            {
                projDAO.RollbackChanges();
                throw ex;
            }


        }


        public ActionResult EditDocumentWizardPage(int? dmpId, string documnentId = null)
        {
            if ((documnentId == null))
            {
                return RedirectToAction("CreateNewDMP", "Home");
            }
            Guid dGuid = new Guid(documnentId);
            DMPDocument Doc = new DMPDocumentDAO().Retrieve(dGuid);

            if (Doc == null)
            {
                return new HttpStatusCodeResult(400, "Bad request");
            }
            MyDMP = Doc.TheDMP;

            var thepageDoc = Doc.Document;
            EditDocumentViewModel2 docVM = new EditDocumentViewModel2
            {
                //versionAuthor = thepageDoc.DocumentRevisions.LastOrDefault().Version.VersionAuthor,
                datacollectionProcesses = thepageDoc.DataCollectionProcesses,
                dataDocMgt = thepageDoc.DataDocumentationManagementAndEntry,
                dataSharing = thepageDoc.DataAccessAndSharing,
                dataVerification = thepageDoc.QualityAssurance.DataVerification,
                digital = thepageDoc.DataStorage.Digital,
                nonDigital = thepageDoc.DataStorage.NonDigital,
                digitalDataRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.DigitalDataRetention,
                nonDigitalRetention = thepageDoc.PostProjectDataRetentionSharingAndDestruction.NonDigitalRentention,
                ethicsApproval = thepageDoc.ProjectProfile.EthicalApproval,
                intelProp = thepageDoc.IntellectualPropertyCopyrightAndOwnership,
                ppData = thepageDoc.PostProjectDataRetentionSharingAndDestruction,
                projectDetails = thepageDoc.ProjectProfile.ProjectDetails,
                summary = thepageDoc.Planning.Summary,
                reportData = thepageDoc.Reports.ReportData,
                 Trainings = thepageDoc.MonitoringAndEvaluationSystems.Trainings,
                roleNresp = thepageDoc.MonitoringAndEvaluationSystems.RoleAndResponsibilities,
                documentID = Doc.Id.ToString(),
            };

            return View(docVM);
        }

        [HttpPost]
        public ActionResult EditDocumentNext(EditDocumentViewModel doc, EthicsApproval ethicsApproval, ProjectDetails projDTF, Approval approval,
            VersionAuthor versionAuthor, VersionMetadata versionMetadata, Summary summary, RolesAndResponsiblities roleNresp,
            ReportData reportData, DataVerificaton dataVerification, DigitalData digital, NonDigitalData nonDigital,
            DataCollectionProcesses datacollectionProcesses, IntellectualPropertyCopyrightAndOwnership intelProp,
            DataAccessAndSharing dataSharing, DataDocumentationManagementAndEntry dataDocMgt, DataCollection DataCollection,
            DigitalDataRetention digitalDataRetention, Trainings Trainings)
        {

            ProjectDetails ProjDetails = projDTF; ;
            Guid dGuid = new Guid(doc.documentID);
            var previousDoc = dmpDocDAO.Retrieve(dGuid);

            if (previousDoc.TheDMP.TheProject == null)
            {
                ProjDetails = projDTF;
                SaveProject(ProjDetails);
            }
            else
            {
                ProjDetails.Id = previousDoc.TheDMP.TheProject.Id;
                projDAO.Update(ProjDetails);
            }




            WizardPage page = new WizardPage
            {
                ProjectProfile = new ProjectProfile
                {
                    EthicalApproval = ethicsApproval,
                    ProjectDetails = ProjDetails,
                },
                DocumentRevisions = new List<DocumentRevisions>
                {
                    new DocumentRevisions{
                        Version = new DAL.Entities.Version
                        {
                            Approval = approval,
                            VersionAuthor = versionAuthor,
                            VersionMetadata = versionMetadata,
                        }
                    }
                },
                Planning = new Planning { Summary = summary },
                DataCollection = DataCollection,
                MonitoringAndEvaluationSystems = new MonitoringAndEvaluationSystems
                {
                    DataFlowChart = doc.DataFlowChart,
                    RoleAndResponsibilities = roleNresp,
                    Trainings = Trainings,
                },
                Reports = new Report
                {
                    ReportData = reportData
                },
                QualityAssurance = new QualityAssurance { DataVerification = dataVerification },
                DataCollectionProcesses = datacollectionProcesses,
                DataStorage = new DataStorage
                {
                    Digital = digital,
                    NonDigital = nonDigital
                },
                IntellectualPropertyCopyrightAndOwnership = intelProp,
                DataAccessAndSharing = dataSharing,
                DataDocumentationManagementAndEntry = dataDocMgt,
                PostProjectDataRetentionSharingAndDestruction = new PostProjectDataRetentionSharingAndDestruction
                {
                    DataToRetain = doc.DataToRetain,
                    DigitalDataRetention = digitalDataRetention,
                    Licensing = doc.Licensing,
                    NonDigitalRentention = new NonDigitalDataRetention
                    {
                        DataRention = doc.nonDigitalDataRetention
                    },
                    Duration = doc.Duration,
                    PreExistingData = doc.PreExistingData
                }
            };

            var theDoc = SaveDMPDocument(page, previousDoc.TheDMP.Id);
            projDAO.CommitChanges();

            var data = new { documentId = theDoc.Id, projectId = ProjDetails.Id };
            return Json(data, JsonRequestBehavior.AllowGet);

            //return Json(theDoc, JsonRequestBehavior.AllowGet);
        }


        public DMPDocument SaveDMPDocument(WizardPage documentPages, int dmpId)
        {
            DMPDocument Doc = dmpDocDAO.SearchMostRecentByDMP(dmpId);
            if (Doc == null)
            {
                Doc = new DMPDocument
                {
                    CreationDate = DateTime.Now,
                    Initiator = initiator,
                    InitiatorUsername = initiator.Username,
                    LastModifiedDate = DateTime.Now,
                    Status = DMPStatus.New,
                    Version = 0,
                    TempVersion = 1,
                    TheDMP = MyDMP,
                    ReferralCount = 0,
                    PageNumber = 0,
                    Document = documentPages,
                    ApprovedBy = null,
                };
                dmpDocDAO.Save(Doc);
            }
            else
            {
                Doc.LastModifiedDate = DateTime.Now;
                Doc.Document = documentPages;
                dmpDocDAO.Update(Doc);
            }
            return Doc;
        }

        public bool SaveProject(ProjectDetails theproject)
        {
            bool saved = false;
            if (projDAO.SearchByName(theproject.ProjectTitle) == null)
            {
                projDAO.Save(theproject);
                saved = true;
            }
            return saved;
        }

        public bool SaveOrUpdateDMP(bool save)
        {
            var tmpDMP = dmpDAO.SearchByName(MyDMP.DMPTitle);
            if (save && tmpDMP == null)
            {
                dmpDAO.Save(MyDMP);
                dmpDAO.CommitChanges();
                return true;
            }
            else if (!save)
            {
                dmpDAO.Update(MyDMP);
                return true;
            }
            else
            {
                return false;
            }
        }

    
        */
        #endregion
    }

    public class MockReportData
    {
        public virtual int Id { get; set; }
        public virtual string NameOfReport { get; set; }
        public virtual string ThematicArea { get; set; }
        public virtual List<DateTime> TimelinesForReporting { get; set; }
        public virtual string FrequencyOfReporting { get; set; }
        public virtual string DurationOfReporting { get; set; }
    }

    
}
