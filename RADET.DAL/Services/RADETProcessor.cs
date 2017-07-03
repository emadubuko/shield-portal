using CommonUtil.Entities;
using CommonUtil.Utilities;
using OfficeOpenXml;
using RADET.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RADET.DAL.Services
{
    public class RADETProcessor
    {
        public bool ReadRadetFile(Stream RadetFile, string fileName, Organizations IP, IList<LGA> LGAs, out RadetMetaData metadata, out List<ErrorDetails> error)
        {
            metadata = new RadetMetaData();
            List<RadetPatientLineListing> lineItems = new List<RadetPatientLineListing>();

            error = new List<ErrorDetails>();
            string facilityName = "";

            List<string> validRegimenLine = System.Configuration.ConfigurationSettings.AppSettings["validRegimenLine"].Split(',').ToList();
            List<string> validRegimen = new List<string> { "AZT-3TC-NVP", "AZT-3TC-EFV", "D4T-3TC-NVP", "D4T-3TC-EFV", "TDF-3TC-NVP", "TDF-3TC-EFV", "TDF-FTC-NVP", "TDF-FTC-EFV", "AZT-3TC-ABC", "AZT-3TC-TDF", "Other" };
            List<string> validPregnancyStatus = new List<string> { "Not Pregnant", "Pregnant", "Breastfeeding" };
            List<string> validSex = new List<string> { "Female", "Male" };
            List<string> validARTStatus = new List<string> { "Active", "Dead", "LTFU", "Stopped", "Transferred Out" };


            LGA _lga = null;

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

                _lga = LGAs.FirstOrDefault(x => x.lga_name == lga.Substring(3).Replace(" Local Government Area", "") && x.State.state_name == state.Substring(3).Replace(" State", ""));
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
                else if (!string.IsNullOrEmpty(RadetPeriod) && !string.IsNullOrEmpty(facilityName))
                {
                    var previousReport = CheckPreviousUpload(IP, RadetPeriod, facilityName);

                    if (previousReport)
                    {
                        error.Add(new ErrorDetails
                        {
                            ErrorMessage = "Result already exist",
                            FileName = fileName,
                            FileTab = "Main page",
                            LineNo = "",
                            PatientNo = ""
                        });
                    }
                }


                List<string> skipPages = new List<string>() { "MainPage", "StateLGA", "SOP", "Summary", "Historic" }; //confirm the names
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

                                RegimenLineAtARTStart = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 15), "Regimen Line At ART Start", fileName, worksheet.Name, row, patientId, validRegimenLine, false, true, ref error),
                                RegimenAtStartOfART = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 16), "Regimen At Start of ART", fileName, worksheet.Name, row, patientId, validRegimen, false, true, ref error),
                                CurrentRegimenLine = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 17), "Current Regimen Line", fileName, worksheet.Name, row, patientId, validRegimenLine, false, true, ref error),
                                CurrentARTRegimen = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 18), "Current ART Regimen", fileName, worksheet.Name, row, patientId, validRegimen, false, true, ref error),

                                PregnancyStatus = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 19), "Pregnancy Status", fileName, worksheet.Name, row, patientId, validPregnancyStatus, true, false, ref error),
                                CurrentViralLoad = ExcelHelper.ReadCellText(worksheet, row, 20),
                                //DateOfCurrentViralLoad = ExcelHelper.ReadCellText(worksheet, row, 21), //ValidateDateTime(ExcelHelper.ReadCellText(worksheet, row, 21), "Date Of Current Viral Load", fileName, worksheet.Name, row, patientId, ref error),
                                ViralLoadIndication = ExcelHelper.ReadCellText(worksheet, row, 22),
                                CurrentARTStatus = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 23), "Current ART Status", fileName, worksheet.Name, row, patientId, validARTStatus, false, true, ref error),
                                RadetPatient = new RadetPatient
                                {
                                    PatientId = patientId,
                                    HospitalNo = ExcelHelper.ReadCellText(worksheet, row, 8),
                                    Sex = ValidateGenerics(ExcelHelper.ReadCellText(worksheet, row, 9), "Sex", fileName, worksheet.Name, row, patientId, validSex, false, false, ref error),
                                    Age_at_start_of_ART_in_years = ValidateNumber(ExcelHelper.ReadCellText(worksheet, row, 10), "Age at start of ART in years", fileName, worksheet.Name, row, patientId, 120, ref error),
                                    Age_at_start_of_ART_in_months = ValidateNumber(ExcelHelper.ReadCellText(worksheet, row, 11), "Age at start of ART in months", fileName, worksheet.Name, row, patientId, 60, ref error)
                                },
                                RadetPeriod = worksheet.Name,
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
                                LineNo = row.ToString(),
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


        private DateTime? ValidateDateTime(string input, string fieldName, string fileName, string fileTab, int LineNo, string PatientId, ref List<ErrorDetails> error)
        {
            DateTime output;
            if (DateTime.TryParse(input, out output) == false)
            {
                error.Add(new ErrorDetails
                {
                    ErrorMessage = "Invalid Date supplied for '" + fieldName + "' ( <span style='color:red'>" + input + "</span>)",
                    FileName = fileName,
                    FileTab = fileTab,
                    LineNo = Convert.ToString(LineNo),
                    PatientNo = PatientId
                });
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
                    LineNo = Convert.ToString(LineNo),
                    PatientNo = PatientId
                });
            }
            if (output > maxNo)
            {
                error.Add(new ErrorDetails
                {
                    ErrorMessage = "Maximum value exceed for '" + fieldName + "' (<span style='color:red'>" + input + " </span>)",
                    FileName = fileName,
                    FileTab = fileTab,
                    LineNo = Convert.ToString(LineNo),
                    PatientNo = PatientId
                });
            }
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
                    LineNo = Convert.ToString(LineNo),
                    PatientNo = PatientId
                });
            }
            return output;
        }

        private bool CheckPreviousUpload(Organizations iP, string radetPeriod, string facilityName)
        {
            return false;
            //throw new NotImplementedException();
        }
    }
}
