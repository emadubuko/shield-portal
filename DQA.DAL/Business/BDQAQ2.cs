using CommonUtil.Entities;
using CommonUtil.Utilities;
using DQA.DAL.Data;
using DQA.DAL.Model;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using CommonUtil.DAO;

namespace DQA.DAL.Business
{
    public class BDQAQ2
    {
        shield_dmpEntities entity;
        public BDQAQ2()
        {
            entity = new shield_dmpEntities();
        }

        public async Task<string> GenerateDQA(List<PivotUpload> facilities, Stream template, Organizations ip)
        {
            string fileName = "SHIELD_DQA_Q2_" + ip.ShortName + ".zip";
            string directory = System.Web.Hosting.HostingEnvironment.MapPath("~/Report/Template/DQA FY2017 Q2/SHIELD_DQA_Q2_" + ip.ShortName);

            string zippedFile = directory + "/" + fileName;
            
            if(Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }
            else
            {
                try
                {
                    string[] filenames = Directory.GetFiles(directory);
                    foreach (var file in filenames)
                    {
                        File.Delete(file);
                    }
                }
                catch (Exception ex)
                {

                } 
            }
             
            foreach (var site in facilities)
            { 
                using (ExcelPackage package = new ExcelPackage(template))
                {
                    var radetfile = new RadetUploadReportDAO().RetrieveRadetUpload(ip.Id, 2017, "Q2(Jan-Mar)", site.FacilityName);
                    var radet = radetfile != null && radetfile.FirstOrDefault() != null ? radetfile.FirstOrDefault().Uploads.Where(x => x.SelectedForDQA).ToList() : null;
                    if (radet != null)
                    {
                        var sheet = package.Workbook.Worksheets["TX_CURR"];
                        int row = 1;
                        for (int i = 0; i < radet.Count(); i++)
                        {
                            row++;
                            sheet.Cells["A" + row].Value = radet[i].PatientId;
                            sheet.Cells["B" + row].Value = radet[i].HospitalNo;
                            sheet.Cells["C" + row].Value = radet[i].Sex;
                            sheet.Cells["D" + row].Value = radet[i].Age_at_start_of_ART_in_years;
                            sheet.Cells["E" + row].Value = radet[i].Age_at_start_of_ART_in_months;
                            sheet.Cells["F" + row].Value = radet[i].ARTStartDate;
                            sheet.Cells["G" + row].Value = radet[i].LastPickupDate;
                            
                            sheet.Cells["J" + row].Value = radet[i].MonthsOfARVRefill;
                            
                            sheet.Cells["M" + row].Value = radet[i].RegimenLineAtARTStart;
                            sheet.Cells["N" + row].Value = radet[i].RegimenAtStartOfART;
                            
                            sheet.Cells["Q" + row].Value = radet[i].CurrentRegimenLine;
                            sheet.Cells["R" + row].Value = radet[i].CurrentARTRegimen;
                            
                            sheet.Cells["U" + row].Value = radet[i].Pregnancy_Status;
                            sheet.Cells["V" + row].Value = radet[i].Current_Viral_Load;
                            sheet.Cells["W" + row].Value = radet[i].Date_of_Current_Viral_Load;
                            sheet.Cells["X" + row].Value = radet[i].Viral_Load_Indication;
                            sheet.Cells["Y" + row].Value = radet[i].CurrentARTStatus;
                        }
                    }

                    var sheetn = package.Workbook.Worksheets["Worksheet"];
                    sheetn.Cells["P2"].Value = site.IP;
                    sheetn.Cells["R2"].Value = site.State;
                    sheetn.Cells["T2"].Value = site.Lga;
                    sheetn.Cells["V2"].Value = site.FacilityName;
                    sheetn.Cells["AA2"].Value = site.FacilityCode;


                    package.SaveAs(new FileInfo(directory + "/" + site.FacilityName + ".xlsm"));
                }
            }

            await ZipFolder(directory);
            return fileName;
        }

        private async Task ZipFolder(string filepath)
        {
            string[] filenames = Directory.GetFiles(filepath);
             
            using (ZipOutputStream s = new
            ZipOutputStream(File.Create(filepath + ".zip")))
            {
                s.SetLevel(9); // 0-9, 9 being the highest compression

                byte[] buffer = new byte[4096];

                foreach (string file in filenames)
                {

                    ZipEntry entry = new
                    ZipEntry(Path.GetFileName(file));

                    entry.DateTime = DateTime.Now;
                    s.PutNextEntry(entry);

                    using (FileStream fs = File.OpenRead(file))
                    {
                        int sourceBytes;
                        do
                        {
                            sourceBytes = await fs.ReadAsync(buffer, 0,
                            buffer.Length);

                            s.Write(buffer, 0, sourceBytes);

                        } while (sourceBytes > 0);
                    }
                }
                s.Finish();
                s.Close();
            }
        }

        private async Task ZipFiles(Dictionary<string, MemoryStream> tools, string filepath)
        {
            MemoryStream outputMemStream = new MemoryStream();
            ZipOutputStream zipStream = new ZipOutputStream(outputMemStream);
            zipStream.SetLevel(9); //0-9, 9 being the highest level of compression
            foreach (var t in tools)
            {
                ZipEntry entry = new ZipEntry(t.Key + ".xlsm");
                entry.DateTime = DateTime.Now;
                zipStream.PutNextEntry(entry);
                StreamUtils.Copy(t.Value, zipStream, new byte[4096]);
                zipStream.CloseEntry();
            }

            zipStream.IsStreamOwner = false;
            zipStream.Close();
            outputMemStream.Position = 0;

            using (var fileStream = File.Create(filepath))
            {
                await outputMemStream.CopyToAsync(fileStream);
            }
        }
    }
}
