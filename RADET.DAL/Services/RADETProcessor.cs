﻿using CommonUtil.Utilities;
using OfficeOpenXml;
using RADET.DAL.DAO;
using RADET.DAL.Entities;
using RADET.DAL.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace RADET.DAL.Services
{
    public class RADETProcessor
    {
        public List<RandomizationUpdateModel> Randomizetems(IList<RandomizationUpdateModel> table, int percent_of_active, int percent_inactive)
        {
            List<RandomizationUpdateModel> newTable = new List<RandomizationUpdateModel>();
            RadetMetaDataDAO _dao = new RadetMetaDataDAO();
            //IList<RadetPatientLineListing> table = _dao.RetrievePatientListingByMetaDataId(MetadataId);

            foreach (var item in table) //reset everything
            {
                item.SelectedForDQA = false;
            }

            var perIp = table.GroupBy(x => x.IP);
            foreach (var ip in perIp)
            {
                var perFacility = ip.ToList().GroupBy(x => x.FacilityName);
                foreach (var f in perFacility)
                {
                    var active = f.ToList().Where(x => x.CurrentARTStatus.Trim() == "Active").ToList(); //table.Where(x => x.CurrentARTStatus.Trim() == "Active").ToList();
                    var inactive = f.Where(x => x.CurrentARTStatus.Trim() != "Active").ToList(); //table.Where(x => x.CurrentARTStatus.Trim() != "Active").ToList();

                    int no_of_active_to_select = (int)(active.Count * (double)percent_of_active / 100);
                    int no_of_inactive_to_select = (int)(inactive.Count * (double)percent_inactive / 100);

                    active.Shuffle();
                    inactive.Shuffle();


                    foreach (var item in active.Take(no_of_active_to_select))
                    {
                        item.SelectedForDQA = true;
                    }
                    foreach (var item in inactive.Take(no_of_inactive_to_select))
                    {
                        item.SelectedForDQA = true;
                    }

                    newTable.AddRange(active);
                    newTable.AddRange(inactive);
                }
            }
             

            StringBuilder sb = new StringBuilder();
            foreach (var item in newTable)
            {
                sb.AppendLine(string.Format("Update radet_patient_line_listing set SelectedForDQA = '{0}' where Id = '{1}';", item.SelectedForDQA ? 1 : 0, item.Id));
            }

           int i = _dao.RunSQL(sb.ToString()); //.BulkUpdate(newTable);
            if (i != newTable.Count)
                throw new ApplicationException("an error occured");

            return newTable;
        }



        public bool ReadRadetFile(Stream RadetFile, string fileName, CommonUtil.Entities.Organizations IP, IList<CommonUtil.Entities.LGA> LGAs, out RadetMetaData metadata, out List<ErrorDetails> error)
        {
            metadata = new RadetMetaData();
            List<RadetPatientLineListing> lineItems = new List<RadetPatientLineListing>();

            error = new List<ErrorDetails>();
            string facilityName = "";

            List<string> validRegimenLine = System.Configuration.ConfigurationManager.AppSettings["validRegimenLine"].Split(',').ToList();
            List<string> validRegimen = System.Configuration.ConfigurationManager.AppSettings["validRegimen"].Split(',').ToList();
            List<string> validPregnancyStatus = System.Configuration.ConfigurationManager.AppSettings["validPregnancyStatus"].Split(',').ToList();
            List<string> validSex = System.Configuration.ConfigurationManager.AppSettings["validSex"].Split(',').ToList();
            List<string> validARTStatus = System.Configuration.ConfigurationManager.AppSettings["validARTStatus"].Split(',').ToList();


            CommonUtil.Entities.LGA _lga = null;

            using (ExcelPackage package = new ExcelPackage(RadetFile))
            {
                var mainWorksheet = package.Workbook.Worksheets["MainPage"];
                var worksheets = package.Workbook.Worksheets;

                if (worksheets.Count < 6) //some file just had the main page, summary and co but not tabs for the years
                {
                    return true;//empty file
                }
                if (mainWorksheet == null)
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "No main page found",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                }

                facilityName = ExcelHelper.ReadCell(mainWorksheet, 20, 19);

                if (string.IsNullOrEmpty(facilityName))
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "Could not read Facility Name in file ",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                }
                else
                {
                    facilityName = facilityName.Substring(3);
                }

                string ipshortname = ExcelHelper.ReadCell(mainWorksheet, 24, 19);
                if (ipshortname == "CCRN")
                {
                    ipshortname = "CCCRN";
                }
                if (IP.ShortName.ToLower() != ipshortname.ToLower())
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "IP selected in File did not match with the specified IP",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                }
                string state = ExcelHelper.ReadCell(mainWorksheet, 14, 19);
                string lga = ExcelHelper.ReadCell(mainWorksheet, 17, 19);
                if (string.IsNullOrEmpty(lga))
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "No Lga selected",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                }
                if (string.IsNullOrEmpty(state))
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "no state selected",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                }

               // _lga = LGAs.FirstOrDefault(x => x.lga_name.ToLower() == lga.ToLower().Substring(3).Replace(" local government area", "") && x.State.state_name.ToLower() == state.ToLower().Substring(3).Replace(" state", ""));
                _lga = FindLGA(LGAs, lga, state);
                if (_lga == null)
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "invalid Lga selected",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                }

                string RadetPeriod = ExcelHelper.ReadCell(mainWorksheet, 7, 19);
                if (string.IsNullOrEmpty(RadetPeriod))
                {
                    error.Add(new ErrorDetails
                    {
                        ErrorMessage = "RADET period not indicated",
                        FileName = fileName,
                        FileTab = "Main page",
                        LineNo = "",
                        PatientNo = ""
                    });
                } 

                List<string> skipPages = new List<string>() { "MainPage", "StateLGA", "SOP", "Summary", "Historic", "Sheet1" }; //confirm the names
                foreach (var worksheet in worksheets)
                {
                    if (skipPages.Any(a => a == worksheet.Name))
                        continue;

                    //read first header, the first header should be patient unique id/art
                    var patientIdHeader = ExcelHelper.ReadCell(worksheet, 1, 7);
                    if (string.IsNullOrEmpty(patientIdHeader) || !patientIdHeader.ToLower().Contains("patient unique id/art")) //add aditional check for tab name before flagging it
                    {
                        error.Add(new ErrorDetails
                        {
                            ErrorMessage = "invalid tab",
                            FileName = fileName,
                            FileTab = worksheet.Name,
                            PatientNo = "",
                            LineNo = ""
                        });
                    }

                    int row = 2;
                    while (true)
                    {
                        string patientId = ExcelHelper.ReadCell(worksheet, row, 7);
                        if (string.IsNullOrEmpty(patientId)) //end of file
                        {
                            break;
                        }

                        try
                        {
                            var lineItem = new RadetPatientLineListing
                            {
                                ARTStartDate = ValidateDateTime(ExcelHelper.ReadCellText(worksheet, row, 12), "ART Start Date", fileName, worksheet.Name, row, patientId, ref error),
                                LastPickupDate = ValidateDateTime(ExcelHelper.ReadCellText(worksheet, row, 13), "Last Pick up Date", fileName, worksheet.Name, row, patientId, ref error),
                                MonthsOfARVRefill = ValidateNumber(ExcelHelper.ReadCellText(worksheet, row, 14), "Month of ARV Refil", fileName, worksheet.Name, row, patientId, 10, ref error),

                                RegimenLineAtARTStart = ExcelHelper.ReadCellText(worksheet, row, 15), // ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 15), "Regimen Line At ART Start", fileName, worksheet.Name, row, patientId, validRegimenLine, false, true, ref error),
                                RegimenAtStartOfART = ExcelHelper.ReadCellText(worksheet, row, 16), // ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 16), "Regimen At Start of ART", fileName, worksheet.Name, row, patientId, validRegimen, false, true, ref error),
                                CurrentRegimenLine = ExcelHelper.ReadCellText(worksheet, row, 17), // ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 17), "Current Regimen Line", fileName, worksheet.Name, row, patientId, validRegimenLine, false, true, ref error),
                                CurrentARTRegimen = ExcelHelper.ReadCellText(worksheet, row, 18), // ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 18), "Current ART Regimen", fileName, worksheet.Name, row, patientId, validRegimen, false, true, ref error),

                                PregnancyStatus = ExcelHelper.ReadCellText(worksheet, row, 19), // ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 19), "Pregnancy Status", fileName, worksheet.Name, row, patientId, validPregnancyStatus, true, false, ref error),
                                CurrentViralLoad = ExcelHelper.ReadCellText(worksheet, row, 20),
                                ViralLoadIndication = ExcelHelper.ReadCellText(worksheet, row, 22),
                                CurrentARTStatus = ExcelHelper.ReadCellText(worksheet, row, 23), //ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 23), "Current ART Status", fileName, worksheet.Name, row, patientId, validARTStatus, false, true, ref error),
                                RadetPatient = new RadetPatient
                                {
                                    PatientId = patientId,
                                    HospitalNo = ExcelHelper.ReadCellText(worksheet, row, 8),
                                    Sex = ExcelHelper.ReadCellText(worksheet, row, 9), //ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 9), "Sex", fileName, worksheet.Name, row, patientId, validSex, false, false, ref error),
                                    Age_at_start_of_ART_in_years = ValidateNumber(ExcelHelper.ReadCellText(worksheet, row, 10), "Age at start of ART in years", fileName, worksheet.Name, row, patientId, 120, ref error),
                                    Age_at_start_of_ART_in_months = ValidateNumber(ExcelHelper.ReadCellText(worksheet, row, 11), "Age at start of ART in months", fileName, worksheet.Name, row, patientId, 60, ref error),
                                    FacilityName = facilityName,
                                    IP = IP
                                },
                                RadetYear = worksheet.Name,
                            };

                            string _dateOfCurrentViralLoad = ExcelHelper.ReadCellText(worksheet, row, 21);
                            if (!string.IsNullOrEmpty(lineItem.CurrentViralLoad) && !string.IsNullOrEmpty(lineItem.ViralLoadIndication))
                            {
                                lineItem.DateOfCurrentViralLoad = ValidateDateTime(ExcelHelper.ReadCellText(worksheet, row, 21), "Date Of Current Viral Load", fileName, worksheet.Name, row, patientId, ref error);
                            }

                            lineItems.Add(lineItem);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError(ex);
                            error.Add(new ErrorDetails
                            {
                                ErrorMessage = ex.Message,
                                FileName = fileName,
                                FileTab = worksheet.Name,
                                LineNo = (row - 1).ToString(),
                            });
                        }

                        row += 1;
                    }
                }

                metadata = new RadetMetaData
                {
                    Facility = facilityName,
                    IP = IP,
                    LGA = _lga,
                    PatientLineListing = lineItems,
                    RadetPeriod = RadetPeriod
                };
            }

            return error.Count == 0;
        }

        static CommonUtil.Entities.LGA FindLGA(IList<CommonUtil.Entities.LGA> lgas, string lgaName, string state)
        {
            CommonUtil.Entities.LGA lga = null;
            string _lga_name = lgaName.ToLower().Replace(" local government area", ""); //.Substring(3);
            string _state_name = state.ToLower().Replace(" state", ""); //.Substring(3);
            _lga_name = _lga_name.Length > 3 ? _lga_name.Substring(3) : _lga_name;
            _state_name = _state_name.Length > 3 ? _state_name.Substring(3) : _state_name;

            lga = lgas.FirstOrDefault(x => x.lga_name.ToLower() == _lga_name && x.State.state_name.ToLower() == _state_name);
            if (lga == null)
            {
                lga = lgas.FirstOrDefault(x => !string.IsNullOrEmpty(x.alternative_name)
                && x.alternative_name.ToLower() == _lga_name && x.State.state_name.ToLower() == _state_name);
            }
            return lga;
        }


        private DateTime? ValidateDateTime(string input, string fieldName, string fileName, string fileTab, int LineNo, string PatientId, ref List<ErrorDetails> error)
        {
            if (string.IsNullOrEmpty(input))
                return null;

            DateTime output;
            if (DateTime.TryParse(input, out output) == false)
            {
                string[] formats = { "dd/MM/yyyy" };
                if(DateTime.TryParseExact(input, formats, new CultureInfo("en-US"), DateTimeStyles.None, out output) == false)
                {
                    if(DateTime.TryParseExact(input, "dd-MM-yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out output) == false)
                    {
                        error.Add(new ErrorDetails
                        {
                            ErrorMessage = "Invalid Date supplied for '" + fieldName + "' ( <span style='color:red'>" + input + "</span>)",
                            FileName = fileName,
                            FileTab = fileTab,
                            LineNo = Convert.ToString(LineNo - 1),
                            PatientNo = PatientId
                        });
                    }                    
                }                
                return null;
            }
            return output;
        }

        private int ValidateNumber(string input, string fieldName, string fileName, string fileTab, int LineNo, string PatientId, int maxNo, ref List<ErrorDetails> error)
        {
            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(input.Trim()))
                return 0;

            int output;
            if (int.TryParse(input, out output) == false)
            {
                error.Add(new ErrorDetails
                {
                    ErrorMessage = "Invalid number supplied for '" + fieldName + "' (<span style='color:red'>" + input + " </span>)",
                    FileName = fileName,
                    FileTab = fileTab,
                    LineNo = Convert.ToString(LineNo - 1),
                    PatientNo = PatientId
                });
            }
            //if (output > maxNo)
            //{
            //    error.Add(new ErrorDetails
            //    {
            //        ErrorMessage = "Maximum value exceed for '" + fieldName + "' (<span style='color:red'>" + input + " </span>)",
            //        FileName = fileName,
            //        FileTab = fileTab,
            //        LineNo = Convert.ToString(LineNo - 1),
            //        PatientNo = PatientId
            //    });
            //}
            return output;
        }
        
        private string ValidateGenerics(string input, string fieldName, string fileName, string fileTab, int LineNo, string PatientId, List<string> valids, bool allowEmpty, bool caseSensitive, ref List<ErrorDetails> error)
        {
            if(allowEmpty && (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(input.Trim())))
            {
                return input;
            }

            if (!caseSensitive)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                input = textInfo.ToTitleCase(input.ToLower());
            }

            string output = "";
            if (!string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(input.Trim()) && valids.Any(x => x == input.Trim()))
            {
                output = input;
            }
            else
            {
                error.Add(new ErrorDetails
                {
                    ErrorMessage = "Invalid value supplied for '" + fieldName + "' (<span style='color:red'>" + input + "</span>)",
                    FileName = fileName,
                    FileTab = fileTab,
                    LineNo = Convert.ToString(LineNo - 1),
                    PatientNo = PatientId
                });
            }
            return output;
        }
  
    }
}