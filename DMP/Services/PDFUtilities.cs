using DAL.Entities;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace DMP.Services
{
    public class PDFUtilities
    {
        BaseColor fadedBlue = new BaseColor(79, 129, 189);
        public void GeneratePDFDocument(WizardPage pageData, ref Document doc)
        {
            Font pageFont = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.BOLD);
            // FontFactory.GetFont("Corbel", 7);

            Font istPageFont20 = new Font(Font.FontFamily.TIMES_ROMAN, 20, Font.BOLD, fadedBlue);
            Font istPageFont26 = new Font(Font.FontFamily.TIMES_ROMAN, 26, Font.NORMAL, fadedBlue);
            Font istPageFont18 = new Font(Font.FontFamily.TIMES_ROMAN, 18, Font.NORMAL, new BaseColor(System.Drawing.Color.DarkBlue));
            Font istPageFont14 = new Font(Font.FontFamily.TIMES_ROMAN, 14, Font.NORMAL, new BaseColor(System.Drawing.Color.Black));
            Font istPageFont10 = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.NORMAL, new BaseColor(System.Drawing.Color.Black));
            

            Paragraph istPage = new Paragraph("NOVEMBER 1, 2017", istPageFont20);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            for(int i=0;i<=25; i++)
            {
                doc.Add(new Paragraph("\n"));
            }
            
            istPage = new Paragraph("IP DATA MANAGEMENT PLAN", istPageFont26);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            istPage = new Paragraph("PROJECT", istPageFont18);
            istPage.Alignment = Element.ALIGN_RIGHT;
            istPage.IndentationRight = 55f;
            doc.Add(istPage);

            for (int i = 0; i <= 3; i++)
            {
                doc.Add(new Paragraph("\n"));
            }
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


            doc.NewPage();
            Paragraph projectHeader = new Paragraph("PROJECT PROFILE", pageFont);
            projectHeader.Alignment = Element.BODY;
            projectHeader.IndentationLeft = 55f;
            doc.Add(projectHeader);// add paragraph to the document

            doc.Add(CreateProjectProfileTable(pageData.ProjectProfile)); // add pdf table to the document

            doc.NewPage();
            var revisionTable =CreateDocumentRevisionPage(pageData.DocumentRevisions);
            foreach (var t in revisionTable)
            {
                doc.Add(t);
            }

            doc.NewPage();
            doc.Add(DataCollection(pageData.DataCollectionProcesses, pageData.DataCollection, pageData.QualityAssurance));

            doc.NewPage();
            doc.Add(DigitalDataStorage(pageData.DataStorage));

            doc.NewPage();
            doc.Add(NonDigitalDataStorage(pageData.DataStorage));

            doc.NewPage();
            doc.Add(IntellectualPropertyCopyright(pageData.IntellectualPropertyCopyrightAndOwnership));

            doc.NewPage();
            doc.Add(DataDocumentEntry(pageData.DataDocumentationManagementAndEntry));

            doc.NewPage();
            doc.Add(AccessAndSharing(pageData.DataAccessAndSharing));

            doc.NewPage();
            doc.Add(PostDataRetention(pageData.PostProjectDataRetentionSharingAndDestruction));

            //List<dynamic> dataList = new List<dynamic> { postDataPolicy, postDataPolicy.DigitalDataRetention, postDataPolicy.NonDigitalRentention };
            //doc.Add(CreatePageTable(dataList, "Some test"));

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

        private PdfPTable AccessAndSharing(DataAccessAndSharing dataSharing)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell("Access and sharing");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            GenerateTable(dataSharing, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }

        PdfPTable DataDocumentEntry(DataDocumentationManagementAndEntry DataDocumentMgt)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell(" Data documentation and management");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            GenerateTable(DataDocumentMgt, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }


        PdfPTable IntellectualPropertyCopyright(IntellectualPropertyCopyrightAndOwnership intelllectualProperty)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell(" Intellectual Property, Copyright and Ownership");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            GenerateTable(intelllectualProperty, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }


        PdfPTable DigitalDataStorage(DataStorage storage)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell(" Data Storage – Digital Data");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            GenerateTable(storage.Digital, ref body);
            //GenerateTable(storage.NonDigital, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }

        PdfPTable NonDigitalDataStorage(DataStorage storage)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell(" Data Storage – Non Digital Data");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            GenerateTable(storage.NonDigital, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }


        PdfPTable DataCollection(DataCollectionProcesses DCP, DataCollection dCollection, QualityAssurance QA)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell("Data Collection and Analysis");
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);

            GenerateTable(DCP, ref body);
            GenerateTable(dCollection.Report.ReportData, ref body);
            GenerateTable(dCollection.Report.RoleAndResponsibilities, ref body);
            GenerateTable(QA, ref body);

            PdfPCell bodyCell = new PdfPCell(body);
            bodyCell.BorderWidth = 0;
            table.AddCell(bodyCell);
            return table;
        }

        List<PdfPTable> CreateDocumentRevisionPage(List<DAL.Entities.DocumentRevisions> documentRevisions)
        {
            List<PdfPTable> docTable = new List<PdfPTable>();

            bool firstVersion = true;
            foreach(DocumentRevisions revisions in documentRevisions)
            {
                docTable.Add(CreateDocumentRevision(revisions.Version, firstVersion));
                firstVersion = false;
            }

            return docTable;
        }

        PdfPTable CreateDocumentRevision(DAL.Entities.Version version,  bool firstVersion)
        {
            PdfPTable docTable = new PdfPTable(2);            

            PdfPCell PdfPCell = null;
            Paragraph pp = null;
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor); 
            //FontFactory.GetFont("Corbel", 11, (int)System.Drawing.FontStyle.Italic, blackColor);
            
            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11,Font.ITALIC, fadedBlue);

            string title = firstVersion ? "Initial date of DMP completion" : "Review date of DMP completion";

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


        private PdfPTable CreateProjectProfileTable(ProjectProfile ProjectProfile)
        {
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor);

            BaseColor fadedBlue = new BaseColor(79, 129, 189);
            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL, fadedBlue);

            PdfPTable projectProfileTable = new PdfPTable(2);
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            pp = new Paragraph(new Chunk("Programme title", font8));
            pp.IndentationLeft = 20;
            pp.PaddingTop = 10f;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.ProjectTitle, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Implementing Org", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.NameOfImplementingPartner, fontValue));
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
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.MissionPartner, fontValue));
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
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.ProjectStartDate, fontValue));
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
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.ProjectEndDate, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Project summary", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.ProjectSummary, fontValue));
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
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk(ProjectProfile.ProjectDetails.GrantReferenceNumber, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            pp = new Paragraph(new Chunk("Ethics Approval", font8));
            pp.IndentationLeft = 20;
            pp.SpacingAfter = 5f;
            pp.SpacingBefore = 5f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);


            pp = new Paragraph(new Chunk(ProjectProfile.EthicalApproval.EthicalApprovalForTheProject, fontValue));
            PdfPCell = new PdfPCell();
            pp.IndentationLeft = 10;
            pp.SpacingAfter = 0.1f;
            pp.SpacingBefore = 1f;
            PdfPCell = new PdfPCell();
            PdfPCell.AddElement(pp);
            projectProfileTable.AddCell(PdfPCell);

            projectProfileTable.SpacingAfter = 10f;
            projectProfileTable.SpacingBefore = 10f; // Give some space after the text or it may overlap the table

            return projectProfileTable;
        }

        public void GenerateTable<T>(T data, ref PdfPTable table)
        {
            BaseColor blackColor = new BaseColor(0, 0, 0);
            Font font8 = new Font(Font.FontFamily.TIMES_ROMAN, 11, (int)System.Drawing.FontStyle.Italic, blackColor);
            Font fontValue = new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL, blackColor);
            var tanColor = System.Drawing.Color.FromArgb(40, System.Drawing.Color.Tan);
            PdfPCell PdfPCell = null;
            Paragraph pp = null;

            var infos = typeof(T).GetProperties().Where(x => x.PropertyType == typeof(string));

            foreach (var info in infos)
            {
                pp = new Paragraph(new Chunk(PasCaseConversion(info.Name), font8));
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
                //table.AddElement(PdfPCell);

                pp = new Paragraph(new Chunk(Convert.ToString(info.GetValue(data)), fontValue));
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
            PdfPCell.BackgroundColor = new BaseColor(System.Drawing.Color.FromArgb(130, System.Drawing.Color.DarkBlue));
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

        public string PasCaseConversion(string PascalWord)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("([A-Z]+[a-z]+)");
            string result = _regex.Replace(PascalWord, m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ");
            return result;
        }

        /// <summary>
        /// not yet working
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataList"></param>
        /// <param name="tableHeader"></param>
        /// <returns></returns>
        private IElement GenericPageTable<T>(List<T> dataList, string tableHeader)
        {
            PdfPTable table = new PdfPTable(1);

            PdfPCell hd = GenerateHeaderCell(tableHeader);
            table.AddCell(hd); //add header

            PdfPTable body = new PdfPTable(2);
            var tanColor = System.Drawing.Color.FromArgb(40, System.Drawing.Color.Tan);

            foreach (var data in dataList)
            {
                GenerateTable(data, ref body);
            }
            //pdfUtil.GenerateTable(postDataPolicy, ref body);
            //pdfUtil.GenerateTable(postDataPolicy.DigitalDataRetention, ref body);
            //pdfUtil.GenerateTable(postDataPolicy.NonDigitalRentention, ref body);

            PdfPCell bodyCell = new PdfPCell(body); bodyCell.BorderWidth = 0; table.AddCell(bodyCell);
            return table;
        }
    }
}