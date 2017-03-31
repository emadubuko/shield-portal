using DAL.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml.Serialization;

namespace ShieldPortal.Services
{
    public class PDFUtilities
    {
        static BaseColor fadedBlue = new BaseColor(79, 129, 189);
        static BaseColor whiteColor = new BaseColor(255,255, 255);
        static BaseColor blackColor = new BaseColor(0, 0, 0);
        Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL, blackColor);

        public void GeneratePDFDocument(DMPDocument dmpDoc, ref Document doc)
        {
            WizardPage pageData = dmpDoc.Document;

            ProjectDetails ProjectDetails = dmpDoc.TheDMP.TheProject;

            Font pageFont = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.BOLD);

            Font istPageFont20 = new Font(Font.FontFamily.TIMES_ROMAN, 20, Font.BOLD, fadedBlue);
            Font istPageFont26 = new Font(Font.FontFamily.TIMES_ROMAN, 26, Font.NORMAL, fadedBlue);
            Font istPageFont16 = new Font(Font.FontFamily.TIMES_ROMAN, 16, Font.NORMAL, fadedBlue);
            Font istPageFont14 = new Font(Font.FontFamily.TIMES_ROMAN, 14, Font.NORMAL, new BaseColor(System.Drawing.Color.Black));
            Font istPageFont10 = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.NORMAL, new BaseColor(System.Drawing.Color.Black));


            //Paragraph istPage = new Paragraph("NOVEMBER 1, 2017", istPageFont20);
            //istPage.Alignment = Element.ALIGN_RIGHT;
            //istPage.IndentationRight = 55f;
            //doc.Add(istPage);

            WriteLines(8, ref doc);

            Image docLogo = GeneratePDFImage(dmpDoc.TheDMP.Organization.Logo, 0.5f);
            docLogo.Alignment = Element.ALIGN_CENTER;
            doc.Add(docLogo);

            Paragraph istPage = new Paragraph(dmpDoc.TheDMP.Organization.Name, istPageFont20);
            istPage.Alignment = Element.ALIGN_CENTER;
            doc.Add(istPage);

            WriteLines(3, ref doc);

            istPage = new Paragraph("DATA MANAGEMENT PLAN", istPageFont20);
            istPage.Alignment = Element.ALIGN_CENTER;
            doc.Add(istPage);
            
            WriteLines(6, ref doc);

            string leadActivityMgr = ProjectDetails.LeadActivityManager == null ? "" : ProjectDetails.LeadActivityManager.FullName;
            istPage = new Paragraph(leadActivityMgr, istPageFont14);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            istPage = new Paragraph(dmpDoc.TheDMP.Organization.ShortName, istPageFont10);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            istPage = new Paragraph(dmpDoc.TheDMP.Organization.Address, istPageFont10);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            doc.NewPage();
            doc.NewPage();
            Paragraph header = new Paragraph("Index page", new Font(Font.FontFamily.TIMES_ROMAN, 14, Font.BOLD));
            header.Alignment = Element.BODY;
            header.IndentationLeft = 55f;
            doc.Add(header);
             

            doc.NewPage();
            header = new Paragraph("Table of Content", pageFont);
            header.Alignment = Element.BODY;
            header.IndentationLeft = 55f;
            doc.Add(header);// add paragraph to the document
            doc.Add(CreateTableOfContent(pageData));
                       

            doc.NewPage();
            header = new Paragraph("1. PROJECT PROFILE", pageFont);
            header.Alignment = Element.BODY;
            header.IndentationLeft = 55f;
            doc.Add(header);// add paragraph to the document
            doc.Add(CreateProjectProfileTable(ProjectDetails)); //pageData.ProjectProfile));
            doc.Add(GenericPageTable(pageData.ProjectProfile.EthicalApproval, "Ethical Approval"));

            ////////revision
            doc.NewPage();
            var revisionTable = CreateDocumentRevisionPage(pageData.DocumentRevisions);
            foreach (var t in revisionTable)
            {
                doc.Add(t);
            }
            doc.NewPage();
            header = new Paragraph("2. Document revision", pageFont);
            header.Alignment = Element.BODY;
            header.IndentationLeft = 55f;
            header.SpacingAfter = 20f;
            doc.Add(header);
            doc.Add(CreateDocumentRevisionPage2(pageData.DocumentRevisions.LastOrDefault().Version));


            doc.NewPage();//Planning
            header = new Paragraph("3. Project Objectives", istPageFont16);
            header.IndentationLeft = 55;
            doc.Add(header);
            WriteLines(1, ref doc);
            var aparagraph = new Paragraph(pageData.Planning.Summary.ProjectObjectives, istPageFont14);
            aparagraph.IndentationLeft = 70f;
            aparagraph.IndentationRight = 50f;
            doc.Add(aparagraph); 

            doc.NewPage();// MonitoringAndEvaluationSystems
            header = new Paragraph("4. MONITORING AND EVALUATION SYSTEMS", istPageFont14);
            header.IndentationLeft = 55f;
            header.SpacingAfter = 10f;
            doc.Add(header);

            header = new Paragraph("Roles", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            var rls = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.People != null ? pageData.MonitoringAndEvaluationSystems.People.Roles : new List<StaffGrouping>();
            doc.Add(MultiColumn(rls));

            header = new Paragraph("Responsibilities", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            var rsps = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.People != null ? pageData.MonitoringAndEvaluationSystems.People.Responsibilities : new List<StaffGrouping>();
            doc.Add(MultiColumn(rsps));

            doc.NewPage();
            header = new Paragraph("Trainings", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            var trn = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.People != null ? pageData.MonitoringAndEvaluationSystems.People.Trainings : new List<Trainings>();
            doc.Add(MultiColumn(trn, 6));

            header = new Paragraph("Data Flow Chart", istPageFont10);
            header.Alignment = Element.ALIGN_LEFT;
            header.IndentationLeft = 70f;
            doc.Add(header);
            string dataflowchart = pageData.MonitoringAndEvaluationSystems != null && !string.IsNullOrEmpty(pageData.MonitoringAndEvaluationSystems.People.DataFlowChart) ? pageData.MonitoringAndEvaluationSystems.People.DataFlowChart.Split(',')[1] : null;
            if (dataflowchart != null)
            {
                byte[] imageByte = Convert.FromBase64String(dataflowchart);
                docLogo = GeneratePDFImage(imageByte, 0.85f);
                docLogo.Alignment = Element.ALIGN_CENTER;

                doc.Add(docLogo);
            }

            doc.NewPage();
            doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Process, "Process"));

            doc.NewPage();
            header = new Paragraph("Data collation", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            var dt = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.Process != null ? pageData.MonitoringAndEvaluationSystems.Process.DataCollation : new List<DataCollation>();
            doc.Add(MultiColumn(dt, 1));

            doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Equipment, "Equipment"));
            doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Environment, "Environment"));
            
            doc.NewPage();
            header = new Paragraph("5. Data Processes", istPageFont14);
            header.IndentationLeft = 55;
            doc.Add(header);
            WriteLines(1, ref doc);
            header = new Paragraph("Reporting levels", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            string concateddata = "";
            pageData.MonitoringAndEvaluationSystems.Process.ReportLevel.ForEach(x => {
                concateddata += x + " --> ";
            });
            concateddata = concateddata.TrimEnd(new char[] { '-', '>', ' ' });
            header = new Paragraph(concateddata, fontValue);
            header.IndentationLeft = 75f;
            doc.Add(header);

            WriteLines(1, ref doc);
            header = new Paragraph("Data", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataProcesses.DataCollection));
            //doc.Add(MultiColumn(pageData.DataProcesses.DataCollection, 1));

            doc.NewPage();
            header = new Paragraph("REPORTS", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            var reportList = pageData.DataProcesses.Reports != null ? pageData.DataProcesses.Reports.ReportData : new List<ReportData>();
            doc.Add(LateralMultiColumn(reportList));


            doc.NewPage();// QualityAssurance
            header = new Paragraph("6. Quality Assurance", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.QualityAssurance.DataVerification));
           
             
            doc.NewPage(); // Data Storage, Access & Sharing
            header = new Paragraph("7. Data Storage, Access & Sharing", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            WriteLines(1, ref doc);
            header = new Paragraph("Digital Data Storage", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.Digital));

            header = new Paragraph("Non Digital Data Storage", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.NonDigital));
            header = new Paragraph("Data Access and Sharing", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.DataAccessAndSharing));
            header = new Paragraph("Data Documentation Management and Entry", istPageFont10);
            header.IndentationLeft = 70f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.DataDocumentationManagementAndEntry));

            doc.NewPage();// IntellectualPropertyCopyrightAndOwnership
            header = new Paragraph("8. Intellectual Property, Copyright and Ownership", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(GenericPageTable(pageData.IntellectualPropertyCopyrightAndOwnership, "Intellectual Property, Copyright and Ownership"));
              
             
            doc.NewPage();// PostProjectDataRetentionSharingAndDestruction
            header = new Paragraph("9. Post Project Data Retention Sharing and Destruction", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(GenericPageTable(pageData.PostProjectDataRetentionSharingAndDestruction, "Post Project Data Retention Sharing and Destruction"));
            doc.Add(GenericPageTable(pageData.PostProjectDataRetentionSharingAndDestruction.DigitalDataRetention, "Digital Data Retention"));
            doc.Add(GenericPageTable(pageData.PostProjectDataRetentionSharingAndDestruction.NonDigitalRentention, "Non Digital Data Rentention"));

            #region ////******Correction*************
            /*
            doc.NewPage(); // DataStorage
            doc.Add(DigitalDataStorage(pageData.DataStorage));

            doc.NewPage(); // DataStorage
            doc.Add(NonDigitalDataStorage(pageData.DataStorage));

            doc.NewPage();// IntellectualPropertyCopyrightAndOwnership
            doc.Add(IntellectualPropertyCopyright(pageData.IntellectualPropertyCopyrightAndOwnership));

            doc.NewPage();// DataAccessAndSharing
            doc.Add(AccessAndSharing(pageData.DataAccessAndSharing));

            doc.NewPage();// DataDocumentationManagementAndEntry
            doc.Add(DataDocumentEntry(pageData.DataDocumentationManagementAndEntry));
                                    
            doc.NewPage();// PostProjectDataRetentionSharingAndDestruction
            doc.Add(PostDataRetention(pageData.PostProjectDataRetentionSharingAndDestruction));
            */
            #endregion
        }

        private IElement CreateTableOfContent(WizardPage pageData)
        {
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Regular, blackColor);

            Paragraph contents = new Paragraph();
            Paragraph pp = null;

            pp = new Paragraph(new Chunk("1. Project Profile", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            PropertyInfo[] infos = typeof(ProjectProfile).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }

            pp = new Paragraph(new Chunk("2. Document Revisions", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);

            pp = new Paragraph(new Chunk("3. Project Objectives", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);

            pp = new Paragraph(new Chunk("4. Monitoring and evaluation systems", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            infos = typeof(MonitoringAndEvaluationSystems).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }

            pp = new Paragraph(new Chunk("5. Data processes", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            infos = typeof(DataProcesses).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }

            pp = new Paragraph(new Chunk("6. Quality assurance", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            infos = typeof(QualityAssurance).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }

            pp = new Paragraph(new Chunk("7. Data Storage Access and Sharing", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            infos = typeof(DataStorage).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }

            pp = new Paragraph(new Chunk("8. Intellectual property copyright and ownership", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            infos = typeof(IntellectualPropertyCopyrightAndOwnership).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }

            pp = new Paragraph(new Chunk("9. Post project data retention sharing and destruction", font8));
            pp.IndentationLeft = 60f;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            contents.Add(pp);
            infos = typeof(PostProjectDataRetentionSharingAndDestruction).GetProperties();
            for (int i = 0; i < infos.Count(); i++)
            {
                pp = new Paragraph(new Chunk(ToRoman(i + 1) + ". " + CommonUtil.Utilities.Utilities.PasCaseConversion(infos[i].Name), font8));
                pp.IndentationLeft = 70f;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                contents.Add(pp);
            }
            return contents;
        }

        private void WriteLines(int count, ref Document doc)
        {
            for (int i = 0; i <= count; i++)
            {
                doc.Add(new Paragraph(" "));
            }
        }
        private Image GeneratePDFImage(byte[] imageBytes, float scale = 0)
        {
            string imgStr = Convert.ToBase64String(imageBytes);
            byte[] img = Convert.FromBase64String(imgStr);

            System.Drawing.Image drimage = null;
            Image pic = null;
            using (var ms = new MemoryStream(img, 0, img.Length))
            {
                drimage = System.Drawing.Image.FromStream(ms, true);
                pic = Image.GetInstance(drimage, whiteColor); //System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            

            //if (scale != 0)
            //{
            //    pic.ScalePercent(scale * 100);
            //}
            //else 
            if (pic.Height > pic.Width)
            {
                //Maximum height is 800 pixels.
                float percentage = 0.0f;
                percentage = 700 / pic.Height;
                pic.ScalePercent(percentage * 100);
            }
            else
            {
                // Maximum width is 600 pixels.
                float percentage = 0.0f;
                percentage = 540 / pic.Width;
                if (percentage < 1)
                    pic.ScalePercent(percentage * 100);

            }

            //pic.Border = iTextSharp.text.Rectangle.BOX;
            //pic.BorderColor = iTextSharp.text.BaseColor.BLACK;
            //pic.BorderWidth = 3f;
            return pic;
        }

        private PdfPTable PostDataRetention(PostProjectDataRetentionSharingAndDestruction postDataPolicy)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell("Post-program data retention, sharing and destruction");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            var tanColor = System.Drawing.Color.FromArgb(40, System.Drawing.Color.Tan);

            GenerateTable(postDataPolicy, ref body);
            GenerateTable(postDataPolicy.DigitalDataRetention, ref body);
            GenerateTable(postDataPolicy.NonDigitalRentention, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }



        PdfPTable MultiColumn<T>(List<T> dataList, int noOfFieldsToExclude = 0)
        {
            var infos = typeof(T).GetProperties().Where(x => !Attribute.IsDefined(x, typeof(XmlIgnoreAttribute)));

            PdfPTable table = new PdfPTable(infos.Count() - noOfFieldsToExclude);
            table.SpacingAfter = 10f;
            table.SpacingBefore = 10f;

            BaseColor whiteColor = new BaseColor(255, 255, 255);
            Font fontheader = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLDITALIC, whiteColor);

            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL, blackColor);
           

            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            foreach (var name in infos.Select(x => x.Name))
            {
                if (name == "Id" || name.Contains("StartDate") || name.Contains("EndDate"))
                    continue;

                string header = CommonUtil.Utilities.Utilities.PasCaseConversion(name);
                if (header.ToLower().Contains("timelines"))
                {
                    header = "Timelines";
                }
                else if (header.ToLower().Contains("frequency") || header.ToLower().Contains("fequency"))
                {
                    header = "Frequency";
                }
                else if (header.ToLower().Contains("duration"))
                {
                    header = "Duration (days)";
                }
                else if (header.ToLower().Contains("site display date"))
                {
                    header = "Site";
                }
                else if (header.ToLower().Contains("hqdisplay date"))
                {
                    header = "HQ";
                }
                else if (header.ToLower().Contains("region display date"))
                {
                    header = "Region/State";
                }
                else if (header.ToLower().Contains("site count"))
                {
                    header = "Site";
                }
                else if (header.ToLower().Contains("region count"))
                {
                    header = "Region/State";
                }
                else if (header.ToLower().Contains("hqcount"))
                {
                    header = "HQ";
                }


                PdfPCell hd = GenerateMultiTableHeaderCell(header);
                table.AddCell(hd);
            }

            foreach(var dt in dataList)
            {
                foreach (var info in infos)
                {
                    if (info.Name == "Id" || info.Name.Contains("StartDate") || info.Name.Contains("EndDate"))
                        continue;

                    string datavalue = "";
                    if (info.PropertyType == typeof(List<DateTime>))
                    {
                        List<DateTime> timelines = info.GetValue(dt) as List<DateTime>;
                        timelines.ForEach(x =>
                        {
                            datavalue += string.Format("{0:dd-MMM-yyyy} \n", x);
                        });
                    }
                    else
                    {
                        datavalue = Convert.ToString(info.GetValue(dt));  
                        //special line break
                        if(datavalue.Contains(" - ") && !datavalue.Contains("Bi - "))
                        {
                            datavalue = datavalue.Replace(" - ", "\n");
                        }
                        else if(datavalue == "0")
                        {
                            datavalue = " - ";
                        }                       
                    }
                     
                    pp = new Paragraph(new Chunk(datavalue, fontValue));
                    PdfPCell = new PdfPCell();
                    pp.IndentationLeft = 10;
                    PdfPCell = new PdfPCell();
                    PdfPCell.AddElement(pp);
                    PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
                    PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
                    table.AddCell(PdfPCell);
                }
            }
            return table;
        }

        PdfPTable LateralMultiColumn<T>(List<T> dataList)
        {
            string tableHeader = "";
            PdfPTable table = new PdfPTable(1);

            PropertyInfo reportytype = (typeof(T)).GetProperty("ReportsType");
            PropertyInfo reportlevel = (typeof(T)).GetProperty("ReportingLevel");
            PropertyInfo thematicinfo = (typeof(T)).GetProperty("ThematicArea");
            List<PropertyInfo> excludedinfo = new List<PropertyInfo> { reportlevel, thematicinfo, reportytype };

            for (int i=0; i < dataList.Count(); i++)
            {
                var data = dataList[i];
                if (typeof(T).Name == "ReportData")
                {
                    tableHeader = ToRoman(i + 1).ToLower() + ". " + reportlevel.GetValue(data) + " - " + reportytype.GetValue(data);
                }
                else if (thematicinfo != null)
                {
                    tableHeader = ToRoman(i + 1).ToLower() + ". " + reportlevel.GetValue(data) + " - " + thematicinfo.GetValue(data);
                }
                else
                {
                    tableHeader = ToRoman(i + 1).ToLower() + ". " + reportlevel.GetValue(data);
                 }

                PdfPCell hd = GenerateHeaderCell(tableHeader);
                hd.BackgroundColor = new BaseColor(System.Drawing.Color.FromArgb(130, System.Drawing.Color.SteelBlue)); 
                table.AddCell(hd); //add header

                PdfPTable body = new PdfPTable(2);
                GenerateTable(data, ref body, excludedinfo);

                PdfPCell bodyCell = new PdfPCell(body);
                bodyCell.BorderWidth = 0;
                table.AddCell(bodyCell);
            }
             
            table.SpacingAfter = 10f;
            table.SpacingBefore = 10f;
            return table;
             
        }


        List<PdfPTable> CreateDocumentRevisionPage(List<DAL.Entities.DocumentRevisions> documentRevisions)
        {
            List<PdfPTable> docTable = new List<PdfPTable>();

            bool firstVersion = true;
            foreach (DocumentRevisions revisions in documentRevisions)
            {
                docTable.Add(CreateDocumentRevision(revisions.Version, firstVersion));
                firstVersion = false;
            }

            return docTable;
        }

        PdfPTable CreateDocumentRevisionPage2(DAL.Entities.Version version)
        {
            PdfPTable docTable = new PdfPTable(2);

            PdfPCell PdfPCell = null;
            Paragraph pp = null;
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor);

            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.ITALIC, fadedBlue);

            pp = new Paragraph(new Chunk("Version date", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionMetadata.VersionDate, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Version Number", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionMetadata.VersionNumber, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Author", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionAuthor.DisplayName, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Job designation", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionAuthor.JobDesignation, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Phone number of author", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionAuthor.PhoneNumberOfAuthor, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);
            
            pp = new Paragraph(new Chunk("Email address of author", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionAuthor.EmailAddressOfAuthor, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            var approver = version.Approval;
            pp = new Paragraph(new Chunk("Approver", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(approver == null ? "": approver.DisplayName, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Job Designation", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            
            pp = new Paragraph(new Chunk(approver == null ? "" :approver.JobdesignationApprover, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Phone number of Approver", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(approver == null ? "" : approver.PhonenumberofApprover, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Email of Approver", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(approver == null ? "" : approver.EmailaddressofApprover, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);


            return docTable;
        }

        PdfPTable CreateDocumentRevision(DAL.Entities.Version version, bool firstVersion)
        {
            PdfPTable docTable = new PdfPTable(2);

            PdfPCell PdfPCell = null;
            Paragraph pp = null;
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor); 

            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.ITALIC, fadedBlue);

            string title = firstVersion ? "Initial date of ShieldPortal completion" : "Review date of ShieldPortal completion";

            pp = new Paragraph(new Chunk(title, font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionMetadata.VersionDate, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Version", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.VersionMetadata.VersionNumber, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Approval", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BorderColor = fadedBlue;
            docTable.AddCell(PdfPCell);


            PdfPTable innerTable = new PdfPTable(2);

            pp = new Paragraph(new Chunk("Director SI", fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.UseVariableBorders = true;
            PdfPCell.BorderColor = fadedBlue;
            PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            innerTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(version.Approval.JobdesignationApprover, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.UseVariableBorders = true;
            PdfPCell.BorderColor = fadedBlue;
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            innerTable.AddCell(PdfPCell);


            pp = new Paragraph(new Chunk("PI /CoP", fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.UseVariableBorders = true;
            PdfPCell.BorderColor = fadedBlue;
            PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.BOTTOM_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            innerTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("", fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.UseVariableBorders = true;
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.BOTTOM_BORDER);
            PdfPCell.BorderColor = fadedBlue;
            innerTable.AddCell(PdfPCell);

            innerTable.DefaultCell.BorderWidth = 0;
            innerTable.DefaultCell.BorderColor = fadedBlue;
            docTable.AddCell(innerTable);

            docTable.SpacingAfter = 20f;
            docTable.SpacingBefore = 10f;
            docTable.DefaultCell.BorderColor = fadedBlue;
            return docTable;
        }

      


        private PdfPTable CreateProjectProfileTable(ProjectDetails ProjectDetails)
        {
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor);

            BaseColor fadedBlue = new BaseColor(79, 129, 189);
            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL, fadedBlue);
            var tanColor = System.Drawing.Color.FromArgb(40, System.Drawing.Color.Tan);

            PdfPTable projectProfileTable = new PdfPTable(2);
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            PdfPTable bodyTable = new PdfPTable(1);
            bodyTable.AddCell(GenerateHeaderCell("Project Details")); 

            pp = new Paragraph(new Chunk("Mechanism Name", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.ProjectTitle, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Name of Implementing Partner", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.Organization.Name, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Abbreviation of Implementing Partner", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.Organization.ShortName, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Mission Partner", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.Organization.MissionPartner, fontValue));
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);


            pp = new Paragraph(new Chunk("Lead Activity Manager", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            string leadActivityMgr = ProjectDetails.LeadActivityManager != null ? ProjectDetails.LeadActivityManager.FullName : "";
            pp = new Paragraph(new Chunk(leadActivityMgr, fontValue));
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Address Of Organization", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.Organization.Address, fontValue));
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Phone Number", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.Organization.PhoneNumber, fontValue));
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);


            pp = new Paragraph(new Chunk("Project start date", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.ProjectStartDate, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Project end date", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.ProjectEndDate, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);
             

            pp = new Paragraph(new Chunk("Grant reference number", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(tanColor);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectDetails.GrantReferenceNumber, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);
             
            //projectProfileTable.SpacingAfter = 10f;
            //projectProfileTable.SpacingBefore = 10f; // Give some space after the text or it may overlap the table

           // bodyTable.AddCell(projectProfileTable);

            PdfPCell bodyCell = new PdfPCell(projectProfileTable);
            bodyCell.BorderWidth = 0;
            bodyTable.AddCell(bodyCell);

            bodyTable.SpacingAfter = 10f;
            bodyTable.SpacingBefore = 10f;

            return bodyTable;
        }

        public void GenerateTable<T>(T data, ref PdfPTable table, List<PropertyInfo> excludedProperty = null)
        {
            
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor);
            
            var tanColor = System.Drawing.Color.FromArgb(40, System.Drawing.Color.Tan);
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            IEnumerable<PropertyInfo> infos = typeof(T).GetProperties().Where(x => (x.PropertyType == typeof(string) || x.PropertyType == typeof(int) || x.PropertyType == typeof(List<DateTime>)) && !Attribute.IsDefined(x, typeof(XmlIgnoreAttribute)));
            
            foreach (var info in infos)
            {
                if(excludedProperty !=null && excludedProperty.Contains(info))
                {
                    continue;
                }
                if (info.Name == "DataFlowChart" || info.Name == "Id")
                {
                    continue;
                }

                if(info.Name == "ImplementingPartnerMEProcess")
                {
                    pp = new Paragraph(new Chunk("Implementing partner M&E process", font8));
                }
                else if (info.Name.ToLower().Contains("duration"))
                {
                    pp = new Paragraph(new Chunk("Duration (days)", font8));
                }
                else
                {
                    pp = new Paragraph(new Chunk(CommonUtil.Utilities.Utilities.PasCaseConversion(info.Name), font8));
                }
                pp.IndentationLeft = 20;
                pp.PaddingTop = 10f;
                pp.SpacingAfter = 5f;
                pp.SpacingBefore = 5f;
                PdfPCell = new PdfPCell();
                PdfPCell.AddElement(pp);
                PdfPCell.BackgroundColor = new BaseColor(tanColor);
                PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
                PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
                table.AddCell(PdfPCell);

                //add value
                string datavalue = "";
                if (info.PropertyType == typeof(List<DateTime>))
                {
                    List<DateTime> timelines = info.GetValue(data) as List<DateTime>;
                    timelines.ForEach(x =>
                    {
                        datavalue += string.Format("{0:dd-MMM-yyyy} \n", x);
                    });
                }
                else
                {
                    datavalue = Convert.ToString(info.GetValue(data));
                }                
                pp = new Paragraph(new Chunk(datavalue, fontValue));
                PdfPCell = new PdfPCell();
                pp.IndentationLeft = 10;
                PdfPCell = new PdfPCell();
                PdfPCell.AddElement(pp);
                PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
                PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
                table.AddCell(PdfPCell);
                //table.AddElement(PdfPCell);
            }

        }

        private PdfPCell GenerateMultiTableHeaderCell(string header)
        {
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            BaseColor whiteColor = new BaseColor(255, 255, 255);

            Font fontheader = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLDITALIC, whiteColor);
             
            pp = new Paragraph(new Chunk(header, fontheader));
            pp.IndentationLeft = 5f;// 20;
            pp.PaddingTop = 5f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = fadedBlue; // new BaseColor(System.Drawing.Color.FromArgb(130, System.Drawing.Color.DarkBlue));
            PdfPCell.DisableBorderSide(Rectangle.BOTTOM_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            //PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER); 

            return PdfPCell;
        }

        private PdfPCell GenerateHeaderCell(string header)
        {
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            BaseColor whiteColor = new BaseColor(255, 255, 255);

            Font fontheader = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLDITALIC, whiteColor);

            //PdfPTable hd = new PdfPTable(1);
            pp = new Paragraph(new Chunk(header, fontheader));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = fadedBlue;// new BaseColor(System.Drawing.Color.FromArgb(130, System.Drawing.Color.DarkBlue));
            PdfPCell.DisableBorderSide(Rectangle.BOTTOM_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            // hd.AddCell(PdfPCell);

            return PdfPCell;
        }


        [Obsolete]
        private PdfPTable GenerateHeader(string header)
        {
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            BaseColor whiteColor = new BaseColor(255, 255, 255);
            Font fontheader = FontFactory.GetFont(BaseFont.COURIER_BOLD, 11, (int)System.Drawing.FontStyle.Bold, whiteColor);

            PdfPTable hd = new PdfPTable(1);
            pp = new Paragraph(new Chunk(header, fontheader));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.BackgroundColor = new BaseColor(System.Drawing.Color.FromArgb(130, System.Drawing.Color.DarkBlue));
            PdfPCell.DisableBorderSide(Rectangle.BOTTOM_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            hd.AddCell(PdfPCell);

            return hd;
        }

        void GenerateInnerTable(string elementName, string value)
        {
            BaseColor fadedBlue = new BaseColor(79, 129, 189);
            Font fontValue = FontFactory.GetFont("Corbel", 11, fadedBlue);

            PdfPTable innerTable = new PdfPTable(2);
            Paragraph pp = null;
            PdfPCell PdfPCell = null;

            pp = new Paragraph(new Chunk(elementName, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.DisableBorderSide(Rectangle.LEFT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            innerTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(value));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            PdfPCell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            PdfPCell.DisableBorderSide(Rectangle.TOP_BORDER);
            innerTable.AddCell(PdfPCell);
        }


        /// <summary>
        /// not yet working
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataList"></param>
        /// <param name="tableHeader"></param>
        /// <returns></returns>
        private IElement GenericPageTable<T>(T data, string tableHeader)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell(tableHeader);
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            var tanColor = System.Drawing.Color.FromArgb(40, System.Drawing.Color.Tan);

            GenerateTable(data, ref body);

            PdfPCell bodyCell = new PdfPCell(body);
            bodyCell.BorderWidth = 0;
            table.AddCell(bodyCell);

            table.SpacingAfter = 10f;
            table.SpacingBefore = 10f;

            return table;
        }

        public string ToRoman(int number)
        {
            if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
            if (number < 1) return string.Empty;
            if (number >= 1000) return "m" + ToRoman(number - 1000);
            if (number >= 900) return "cm" + ToRoman(number - 900); //EDIT: i've typed 400 instead 900
            if (number >= 500) return "d" + ToRoman(number - 500);
            if (number >= 400) return "c" + ToRoman(number - 400);
            if (number >= 100) return "c" + ToRoman(number - 100);
            if (number >= 90) return "xc" + ToRoman(number - 90);
            if (number >= 50) return "l" + ToRoman(number - 50);
            if (number >= 40) return "xl" + ToRoman(number - 40);
            if (number >= 10) return "x" + ToRoman(number - 10);
            if (number >= 9) return "ix" + ToRoman(number - 9);
            if (number >= 5) return "v" + ToRoman(number - 5);
            if (number >= 4) return "iv" + ToRoman(number - 4);
            if (number >= 1) return "i" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }
    }
}