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

            if (Directory.Exists(directory) == false)
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

        public List<dqaQ1Fy17Analysis> GetAnalysisReport()
        {
            var _analysis = entity.dqa_FY17Q1_Analysyis.ToList();
            List<dqaQ1Fy17Analysis> t = (from item in _analysis
                                         select new dqaQ1Fy17Analysis
                                         {
                                             IP = item.IP,
                                             Facility = item.Facility,

                                             DQA_FY17Q1_HTC_TST = item.DQA_FY17Q1_HTC_TST,
                                             Validated_HTC_TST = item.Validated_HTC_TST,
                                             CR_HTC_TST = item.CR_HTC_TST.HasValue ? (item.CR_HTC_TST.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_PMTCT_STAT = item.DQA_FY17Q1_PMTCT_STAT,
                                             Validated_PMTCT_STAT = item.Validated_PMTCT_STAT,
                                             CR_PMTCT_STAT = item.CR_PMTCT_STAT.HasValue ? (item.CR_PMTCT_STAT.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_PMTCT_EID = item.DQA_FY17Q1_PMTCT_EID,
                                             Validated_PMTCT_EID = item.Validated_PMTCT_EID,
                                             CR_PMTCT_EID = item.CR_PMTCT_EID.HasValue ? (item.CR_PMTCT_EID.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_PMTCT_ARV = item.DQA_FY17Q1_PMTCT_ARV,
                                             Validated_PMTCT_ARV = item.Validated_PMTCT_ARV,
                                             CR_PMTCT_ARV = item.CR_PMTCT_ARV.HasValue ? (item.CR_PMTCT_ARV.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_TX_NEW = item.DQA_FY17Q1_TX_NEW,
                                             Validated_TX_NEW = item.Validated_TX_NEW,
                                             CR_TX_NEW = item.CR_TX_NEW.HasValue ? (item.CR_TX_NEW.Value * 100).ToString("N1") : " ",

                                             DQA_FY17Q1_TX_CURR = item.DQA_FY17Q1_TX_CURR,
                                             Validated_TX_CURR = item.Validated_TX_CURR,
                                             CR_TX_CURR = item.CR_TX_CURR.HasValue ? (item.CR_TX_CURR.Value * 100).ToString("N1") : " ",

                                             Count_Fails = item.Count_Fails
                                         }).ToList();
            return t;
        }

        public bool ReadPivotTable(Stream datimFile, string quarter, int year, Profile profile, out string result)
        {
            var hfs = entity.HealthFacilities.ToDictionary(x => x.FacilityCode);

            List<dqa_pivot_table> pivotTable = new List<dqa_pivot_table>();
            try
            {
                using (ExcelPackage package = new ExcelPackage(datimFile))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    int row = 2;

                    while (true)
                    {
                        Data.HealthFacility hf = null;
                        string fCode = ExcelHelper.ReadCell(worksheet, row, 1);
                        if (string.IsNullOrEmpty(fCode))
                        {
                            break;
                        }
                        if (hfs.TryGetValue(fCode, out hf) == false)
                        {
                            throw new ApplicationException("unknown facility with code [" + fCode + "] uploaded ");
                        }
                        if (hf.ImplementingPartner.Id != profile.Organization.Id)
                        {
                            throw new ApplicationException("Facility with code [" + fCode + "] does not belong to the your IP [" + profile.Organization.ShortName + "]. Please correct and try again");
                        }

                        string fName = ExcelHelper.ReadCell(worksheet, row, 2);
                        int tb_art, pmtct_art, tx_curr, ovc = 0;
                        int.TryParse(ExcelHelper.ReadCell(worksheet, row, 3), out pmtct_art);
                        int.TryParse(ExcelHelper.ReadCell(worksheet, row, 4), out tx_curr);
                        int.TryParse(ExcelHelper.ReadCell(worksheet, row, 5), out tb_art);
                        int.TryParse(ExcelHelper.ReadCell(worksheet, row, 6), out ovc);



                        pivotTable.Add(new dqa_pivot_table
                        {
                            HealthFacility = hf,
                            TB_ART = tb_art,
                            PMTCT_ART = pmtct_art,
                            TX_CURR = tx_curr,
                            OVC = ovc,
                            Year = year,
                            Quarter = quarter,
                            ImplementingPartner = hf.ImplementingPartner,
                        });
                        row += 1;
                    }
                }

                List<dqa_pivot_table> selectedList = new List<dqa_pivot_table>();

                if (Convert.ToBoolean(System.Configuration.ConfigurationManager.AppSettings["Use_top_90_for_dqa_site_selection"]))
                {
                    #region top 90%
                    int total_tx = pivotTable.Sum(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART));
                    double _90_percent_of_Total_tx = total_tx * 0.9;
                    pivotTable = pivotTable.OrderByDescending(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART)).ToList();
                    foreach (var item in pivotTable)
                    {
                        selectedList.Add(item);
                        if (selectedList.Sum(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART)) < _90_percent_of_Total_tx)
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "Based on 90% of Tx";
                        }
                        else if (item.OVC > 0)//all ovc sites are selected
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "OVC Site";
                        }
                    }

                    #endregion
                }
                else
                {
                    #region top 80% and randomized 50% of remaining 

                    //calclaute 80% of tx_curr
                    int total_tx_curr = pivotTable.Sum(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART));
                    double _80_percent_of_Total_tx_curr = total_tx_curr * 0.8;
                    pivotTable = pivotTable.OrderByDescending(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART)).ToList();
                    foreach (var item in pivotTable)
                    {
                        selectedList.Add(item);
                        if (selectedList.Sum(x => (x.TX_CURR + x.PMTCT_ART + x.TB_ART)) < _80_percent_of_Total_tx_curr)
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "Based on 80% of Tx_curr";
                        }
                        else if (item.OVC > 0)
                        {
                            item.SelectedForDQA = true;
                            item.SelectedReason = "OVC Site";
                        }
                    }

                    //Randomize and select 50%
                    var remaining = selectedList.Where(x => !x.SelectedForDQA).ToList();
                    remaining.Shuffle();
                    for (int i = 0; i <= remaining.Count() / 2; i++)
                    {
                        if ((remaining[i].TX_CURR + remaining[i].PMTCT_ART + remaining[i].TB_ART) > 0)
                        {
                            selectedList.FirstOrDefault(x => x == remaining[i]).SelectedForDQA = true;
                            selectedList.FirstOrDefault(x => x == remaining[i]).SelectedReason = "Based on randomization";
                        }
                    }
                    #endregion
                }

                //return result
                var previously = entity.dqa_pivot_table_upload.FirstOrDefault(x => x.Year == year && x.Quarter == quarter.Trim());
                if (previously != null)
                {
                    entity.dqa_pivot_table_upload.Remove(previously);
                }

                entity.dqa_pivot_table_upload.Add(
                    new dqa_pivot_table_upload
                    {
                        DateUploaded = DateTime.Now,
                        dqa_pivot_table = selectedList,
                        IP = profile.Organization.Id,
                        Quarter = quarter,
                        Year = year,
                        UploadedBy = profile.Id
                    });
                entity.dqa_pivot_table.RemoveRange(entity.dqa_pivot_table.Where(x => x.Year == year && x.Quarter == quarter.Trim()).ToList());
                entity.dqa_pivot_table.AddRange(selectedList);
                entity.SaveChanges();

                result = Newtonsoft.Json.JsonConvert.SerializeObject(
                    from item in selectedList
                    select new
                    {
                        FacilityName = item.HealthFacility.Name,
                        item.OVC,
                        item.PMTCT_ART,
                        item.TB_ART,
                        item.TX_CURR,
                        item.SelectedForDQA,
                        item.SelectedReason
                    }
                 );
            }
            catch (Exception ex)
            {
                result = ex.Message;
                return false;
            }
            return true;
        }
    }
}
