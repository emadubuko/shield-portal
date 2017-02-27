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

            Font pageFont = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.BOLD);

            Font istPageFont20 = new Font(Font.FontFamily.TIMES_ROMAN, 20, Font.BOLD, fadedBlue);
            Font istPageFont26 = new Font(Font.FontFamily.TIMES_ROMAN, 26, Font.NORMAL, fadedBlue);
            Font istPageFont16 = new Font(Font.FontFamily.TIMES_ROMAN, 16, Font.NORMAL, fadedBlue);
            Font istPageFont14 = new Font(Font.FontFamily.TIMES_ROMAN, 14, Font.NORMAL, new BaseColor(System.Drawing.Color.Black));
            Font istPageFont10 = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.NORMAL, new BaseColor(System.Drawing.Color.Black));


            Paragraph istPage = new Paragraph("NOVEMBER 1, 2017", istPageFont20);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            WriteLines(8, ref doc);

            Image docLogo = GeneratePDFImage(dmpDoc.TheDMP.Organization.Logo, 0.5f);
            docLogo.Alignment = Element.ALIGN_CENTER;
            doc.Add(docLogo);

            istPage = new Paragraph(dmpDoc.TheDMP.Organization.Name, istPageFont20);
            istPage.Alignment = Element.ALIGN_CENTER;
            doc.Add(istPage);

            WriteLines(3, ref doc);

            istPage = new Paragraph("DATA MANAGEMENT PLAN", istPageFont20);
            istPage.Alignment = Element.ALIGN_CENTER;
            doc.Add(istPage);
            
            WriteLines(6, ref doc);

            istPage = new Paragraph("SI LEAD", istPageFont14);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            istPage = new Paragraph("[Company Name]", istPageFont10);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            istPage = new Paragraph("[Company Address]", istPageFont10);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            ////////revision
            doc.NewPage();
            var revisionTable = CreateDocumentRevisionPage(pageData.DocumentRevisions);
            foreach (var t in revisionTable)
            {
                doc.Add(t);
            }


            doc.NewPage();
            Paragraph header = new Paragraph("PROJECT PROFILE", pageFont);
            header.Alignment = Element.BODY;
            header.IndentationLeft = 55f;
            doc.Add(header);// add paragraph to the document
            doc.Add(CreateProjectProfileTable(dmpDoc.TheDMP.TheProject)); //pageData.ProjectProfile));
            doc.Add(GenericPageTable(pageData.ProjectProfile.EthicalApproval, "Ethical Approval"));
            
            doc.NewPage();//Planning
            header = new Paragraph("Project Objectives", istPageFont16);
            header.IndentationLeft = 55;
            doc.Add(header);
            WriteLines(1, ref doc);
            var aparagraph = new Paragraph(pageData.Planning.Summary.ProjectObjectives, istPageFont14);
            aparagraph.IndentationLeft = 70f;
            doc.Add(aparagraph); 

            doc.NewPage();// MonitoringAndEvaluationSystems
            header = new Paragraph("MONITORING AND EVALUATION SYSTEMS", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            WriteLines(1, ref doc);

            header = new Paragraph("Data Flow Chart", pageFont);
            header.Alignment = Element.ALIGN_LEFT;
            header.IndentationLeft = 55f;
            doc.Add(header);
            string dataflowchart = pageData.MonitoringAndEvaluationSystems != null && !string.IsNullOrEmpty(pageData.MonitoringAndEvaluationSystems.People.DataFlowChart) ? pageData.MonitoringAndEvaluationSystems.People.DataFlowChart.Split(',')[1] : null;
            if (dataflowchart != null)
            {
                byte[] imageByte = Convert.FromBase64String(dataflowchart);
                docLogo = GeneratePDFImage(imageByte, 0.85f);
                docLogo.Alignment = Element.ALIGN_CENTER;

                doc.Add(docLogo);
            }

            //doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.People, "People"));
            //WriteLines(1, ref doc);
            header = new Paragraph("Roles", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            var rls = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.People != null ? pageData.MonitoringAndEvaluationSystems.People.Roles : new List<StaffGrouping>();
            doc.Add(MultiColumn(rls));

            header = new Paragraph("Responsibilities", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            var rsps = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.People != null ? pageData.MonitoringAndEvaluationSystems.People.Responsibilities : new List<StaffGrouping>();
            doc.Add(MultiColumn(rsps));

            doc.NewPage();
            header = new Paragraph("Trainings", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            var trn = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.People != null ? pageData.MonitoringAndEvaluationSystems.People.Trainings : new List<Trainings>();
            doc.Add(MultiColumn(trn, 6));             

            doc.NewPage();
            doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Process, "Process"));
            header = new Paragraph("Data collation", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            var dt = pageData.MonitoringAndEvaluationSystems != null && pageData.MonitoringAndEvaluationSystems.Process != null ? pageData.MonitoringAndEvaluationSystems.Process.DataCollation : new List<DataCollation>();
            doc.Add(MultiColumn(dt, 1));

            doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Equipment, "Equipment"));
            doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Environment, "Environment"));
            //doc.NewPage();
            //doc.Add(GenericPageTable(pageData.MonitoringAndEvaluationSystems.Organization, "Organization"));


            doc.NewPage();
            header = new Paragraph("Data Processes", istPageFont14);
            header.IndentationLeft = 55;
            doc.Add(header);
            WriteLines(1, ref doc);
            header = new Paragraph("Reporting levels", istPageFont14);
            header.IndentationLeft = 55;
            doc.Add(header);
            string concateddata = "";
            pageData.MonitoringAndEvaluationSystems.Process.ReportLevel.ForEach(x => {
                concateddata += x + " --> ";
            });
            concateddata = concateddata.TrimEnd(new char[] { '-', '>', ' ' });
            header = new Paragraph(concateddata, fontValue);
            header.IndentationLeft = 55;
            doc.Add(header);

            WriteLines(1, ref doc);
            header = new Paragraph("Data", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(MultiColumn(pageData.DataProcesses.DataCollection, 1));
           
            header = new Paragraph("REPORTS", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            var reportList = pageData.DataProcesses.Reports != null ? pageData.DataProcesses.Reports.ReportData : new List<ReportData>();
            doc.Add(LateralMultiColumn(reportList));


            doc.NewPage();// QualityAssurance
            header = new Paragraph("Quality Assurance", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.QualityAssurance.DataVerification));
           
             
            doc.NewPage(); // Data Storage, Access & Sharing
            header = new Paragraph("Data Storage, Access & Sharing", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            WriteLines(1, ref doc);
            header = new Paragraph("Digital Data Storage", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.Digital));

            header = new Paragraph("Non Digital Data Storage", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.NonDigital));
            header = new Paragraph("Data Access and Sharing", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.DataAccessAndSharing));
            header = new Paragraph("Data Documentation Management and Entry", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(LateralMultiColumn(pageData.DataStorageAccessAndSharing.DataDocumentationManagementAndEntry));

            doc.NewPage();// IntellectualPropertyCopyrightAndOwnership
            header = new Paragraph("Intellectual Property, Copyright and Ownership", istPageFont14);
            header.IndentationLeft = 55f;
            doc.Add(header);
            doc.Add(GenericPageTable(pageData.IntellectualPropertyCopyrightAndOwnership, "Intellectual Property, Copyright and Ownership"));
              
             
            doc.NewPage();// PostProjectDataRetentionSharingAndDestruction
            header = new Paragraph("Post Project Data Retention Sharing and Destruction", istPageFont14);
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
                    tableHeader = ToRoman(i+1).ToLower() + ". " + reportlevel.GetValue(data) + " - " + reportytype.GetValue(data);
                }
                else
                {
                    tableHeader = ToRoman(i+1).ToLower() + ". " + reportlevel.GetValue(data) + " - " + thematicinfo.GetValue(data);
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

            pp = new Paragraph(new Chunk("Programme title", font8));
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

            pp = new Paragraph(new Chunk(ProjectDetails.NameOfImplementingPartner, fontValue));
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

            pp = new Paragraph(new Chunk(ProjectDetails.AbreviationOfImplementingPartner, fontValue));
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

            pp = new Paragraph(new Chunk(ProjectDetails.MissionPartner, fontValue));
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

            pp = new Paragraph(new Chunk(ProjectDetails.AddressOfOrganization, fontValue));
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

            pp = new Paragraph(new Chunk(ProjectDetails.PhoneNumber, fontValue));
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
            if (number >= 1000) return "M" + ToRoman(number - 1000);
            if (number >= 900) return "CM" + ToRoman(number - 900); //EDIT: i've typed 400 instead 900
            if (number >= 500) return "D" + ToRoman(number - 500);
            if (number >= 400) return "CD" + ToRoman(number - 400);
            if (number >= 100) return "C" + ToRoman(number - 100);
            if (number >= 90) return "XC" + ToRoman(number - 90);
            if (number >= 50) return "L" + ToRoman(number - 50);
            if (number >= 40) return "XL" + ToRoman(number - 40);
            if (number >= 10) return "X" + ToRoman(number - 10);
            if (number >= 9) return "IX" + ToRoman(number - 9);
            if (number >= 5) return "V" + ToRoman(number - 5);
            if (number >= 4) return "IV" + ToRoman(number - 4);
            if (number >= 1) return "I" + ToRoman(number - 1);
            throw new ArgumentOutOfRangeException("something bad happened");
        }
    }
}