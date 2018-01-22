using CommonUtil.Utilities;
using DAL.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace DAL.Services
{
    public class DMPExcelFile
    {
        public void ExtractRolesFromFile(Stream ReportStream, out List<StaffGrouping> roles, out List<StaffGrouping> responsibility, out List<Trainings> trainings)
        {
            Dictionary<string, StaffGrouping> role_location = new Dictionary<string, StaffGrouping>();
            Dictionary<string, StaffGrouping> resp_location = new Dictionary<string, StaffGrouping>();
            Dictionary<string, Trainings> training = new Dictionary<string, Trainings>();

            using (ExcelPackage package = new ExcelPackage(ReportStream))
            {
                ExcelWorksheet roleSheet = package.Workbook.Worksheets.FirstOrDefault();

                int countColumn = 1;
                int roleColumn = 2;
                int responsibilityColumn = 3;
                int locationColumn = 4;
                int trainingColumn = 5;
                int training_st_dt_column = 6;
                int training_ed_dt_column = 7;

                for (int i = 2; ; i++) //this is for the rows
                {

                    string location = ExcelHelper.ReadCell(roleSheet, i, locationColumn);

                    if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(location.Trim()))
                    {
                        break;
                    }
                    else
                    {
                        location = location.Trim();

                        string roleName = ExcelHelper.ReadCell(roleSheet, i, roleColumn);
                        string responsibilityName = ExcelHelper.ReadCell(roleSheet, i, responsibilityColumn);

                        string count = ExcelHelper.ReadCell(roleSheet, i, countColumn);

                        string trainingName = ExcelHelper.ReadCell(roleSheet, i, trainingColumn);
                        string TrainingStartDate = ExcelHelper.ReadCellText(roleSheet, i, training_st_dt_column);
                        string TrainingEndDate = ExcelHelper.ReadCellText(roleSheet, i, training_ed_dt_column);

                        StaffGrouping previously = null;
                        try
                        {
                            if (role_location.TryGetValue(roleName, out previously))
                            {
                                if (location.ToLower() == "site")
                                {
                                    role_location[roleName].SiteCount += count.ToInt();
                                }
                                else if (location.ToLower() == "region")
                                {
                                    role_location[roleName].RegionCount += count.ToInt();
                                }
                                else if (location.ToLower() == "hq")
                                {
                                    role_location[roleName].HQCount += count.ToInt();
                                }
                            }
                            else
                            {
                                role_location.Add(roleName, new StaffGrouping
                                {
                                    Name = roleName,
                                    HQCount = location.ToLower() == "hq" ? count.ToInt() : 0,
                                    RegionCount = location.ToLower() == "region" ? count.ToInt() : 0,
                                    SiteCount = location.ToLower() == "site" ? count.ToInt() : 0,
                                });
                            }

                            if (resp_location.TryGetValue(responsibilityName, out previously))
                            {
                                if (location.ToLower() == "site")
                                {
                                    resp_location[responsibilityName].SiteCount += count.ToInt();
                                }
                                else if (location.ToLower() == "region")
                                {
                                    resp_location[responsibilityName].RegionCount += count.ToInt();
                                }
                                else if (location.ToLower() == "hq")
                                {
                                    resp_location[responsibilityName].HQCount += count.ToInt();
                                }
                            }
                            else
                            {
                                resp_location.Add(responsibilityName, new StaffGrouping
                                {
                                    Name = responsibilityName,
                                    HQCount = location.ToLower() == "hq" ? count.ToInt() : 0,
                                    RegionCount = location.ToLower() == "region" ? count.ToInt() : 0,
                                    SiteCount = location.ToLower() == "site" ? count.ToInt() : 0,
                                });
                            }

                            if (!string.IsNullOrEmpty(trainingName))
                            {
                                DateTime edt = new DateTime();
                                DateTime stdt = new DateTime();

                                DateTime.TryParseExact(TrainingStartDate, "d-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out stdt);
                                DateTime.TryParseExact(TrainingEndDate, "d-MMM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out edt);

                                string startDateString = stdt > DateTime.MinValue ? string.Format("{0:d-MMM-yyyy}", stdt) : "invalid";
                                string EndDateString = stdt > DateTime.MinValue ? string.Format("{0:d-MMM-yyyy}", edt) : "invalid";

                                Trainings tr = null;
                                if (training.TryGetValue(trainingName, out tr))
                                {
                                    if (location.ToLower() == "hq")
                                    {
                                        training[trainingName].HQStartDate = startDateString;
                                        training[trainingName].HQEndDate = EndDateString;
                                    }
                                    else if (location.ToLower() == "region")
                                    {
                                        training[trainingName].RegionStartDate = startDateString;
                                        training[trainingName].RegionEndDate = EndDateString;
                                    }
                                    else if (location.ToLower() == "site")
                                    {
                                        training[trainingName].SiteStartDate = startDateString;
                                        training[trainingName].SiteEndDate = EndDateString;
                                    }
                                }
                                else
                                {
                                    training.Add(trainingName, new Trainings
                                    {
                                        NameOfTraining = trainingName,
                                        HQStartDate = location.ToLower() == "hq" ? startDateString : "",
                                        HQEndDate = location.ToLower() == "hq" ? EndDateString : "",

                                        RegionStartDate = location.ToLower() == "region" ? startDateString : "",
                                        RegionEndDate = location.ToLower() == "region" ? EndDateString : "",

                                        SiteStartDate = location.ToLower() == "site" ? startDateString : "",
                                        SiteEndDate = location.ToLower() == "site" ? EndDateString : "",
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }

            roles = role_location.Values.ToList();
            responsibility = resp_location.Values.ToList();
            trainings = training.Values.ToList();
        }


        [Obsolete]
        public void ExtractRoles(Stream ReportStream, out List<StaffGrouping> roles, out List<StaffGrouping> responsibility, out List<Trainings> trainings)
        {
            Dictionary<string, StaffGrouping> role_location = new Dictionary<string, StaffGrouping>();
            Dictionary<string, StaffGrouping> resp_location = new Dictionary<string, StaffGrouping>();
            Dictionary<string, Trainings> training = new Dictionary<string, Trainings>();

            using (ExcelPackage package = new ExcelPackage(ReportStream))
            {
                ExcelWorksheet roleSheet = package.Workbook.Worksheets.FirstOrDefault();

                int responsibilityColumn = 12;
                int roleColumn = 2;
                int locationColumn = 13;
                int trainingColumn = 14;
                int training_st_dt_column = 15;
                int training_ed_dt_column = 16;

                for (int i = 2; ; i++) //this is for the rows
                {
                    string roleName = ExcelHelper.ReadCell(roleSheet, i, roleColumn);
                    string responsibilityName = ExcelHelper.ReadCell(roleSheet, i, responsibilityColumn);
                    string location = ExcelHelper.ReadCell(roleSheet, i, locationColumn);

                    string trainingName = ExcelHelper.ReadCell(roleSheet, i, trainingColumn);
                    string TrainingStartDate = ExcelHelper.ReadCell(roleSheet, i, training_st_dt_column);
                    string TrainingEndDate = ExcelHelper.ReadCell(roleSheet, i, training_ed_dt_column);

                    if (string.IsNullOrEmpty(location))
                    {
                        break;
                    }
                    else
                    {
                        string roleKey = roleName;

                        StaffGrouping previously = null;
                        bool exist = role_location.TryGetValue(roleKey, out previously);
                        if (exist)
                        {
                            if (location.ToLower().Contains("site"))
                            {
                                role_location[roleKey].SiteCount += 1;
                            }
                            else if (location.ToLower().Contains("region"))
                            {
                                role_location[roleKey].RegionCount += 1;
                            }
                            else if (location.ToLower().Contains("hq"))
                            {
                                role_location[roleKey].HQCount += 1;
                            }
                        }
                        else
                        {
                            role_location.Add(roleKey, new StaffGrouping
                            {
                                Name = roleName,
                                HQCount = location.ToLower().Contains("hq") ? 1 : 0,
                                RegionCount = location.ToLower().Contains("region") ? 1 : 0,
                                SiteCount = location.ToLower().Contains("site") ? 1 : 0,
                            });
                        }

                        string resp_key = responsibilityName;
                        if (resp_location.TryGetValue(resp_key, out previously))
                        {
                            if (location.ToLower().Contains("site"))
                            {
                                resp_location[resp_key].SiteCount += 1;
                            }
                            else if (location.ToLower().Contains("region"))
                            {
                                resp_location[resp_key].RegionCount += 1;
                            }
                            else if (location.ToLower().Contains("hq"))
                            {
                                resp_location[resp_key].HQCount += 1;
                            }
                        }
                        else
                        {
                            resp_location.Add(resp_key, new StaffGrouping
                            {
                                Name = responsibilityName,
                                HQCount = location.ToLower().Contains("hq") ? 1 : 0,
                                RegionCount = location.ToLower().Contains("region") ? 1 : 0,
                                SiteCount = location.ToLower().Contains("site") ? 1 : 0,
                            });
                        }

                        Trainings tr = null;
                        DateTime edt = new DateTime();
                        DateTime stdt = new DateTime();

                        DateTime.TryParse(TrainingStartDate, out stdt);
                        DateTime.TryParse(TrainingEndDate, out edt);

                        string startDateString = stdt > DateTime.MinValue ? string.Format("{0:dd-MMM-yyyy}", stdt) : "invalid";
                        string EndDateString = stdt > DateTime.MinValue ? string.Format("{0:dd-MMM-yyyy}", edt) : "invalid";

                        if (!string.IsNullOrEmpty(trainingName))
                        {
                            string training_key = trainingName;
                            if (training.TryGetValue(training_key, out tr))
                            {
                                if (location.ToLower().Contains("hq"))
                                {
                                    training[training_key].HQStartDate = startDateString;
                                    training[training_key].HQEndDate = EndDateString;
                                }
                                else if (location.ToLower().Contains("region"))
                                {
                                    training[training_key].RegionStartDate = startDateString;
                                    training[training_key].RegionEndDate = EndDateString;
                                }
                                else if (location.ToLower().Contains("site"))
                                {
                                    training[training_key].SiteStartDate = startDateString;
                                    training[training_key].SiteEndDate = EndDateString;
                                }
                            }
                            else
                            {
                                training.Add(training_key, new Trainings
                                {
                                    NameOfTraining = trainingName,
                                    HQStartDate = location.ToLower().Contains("hq") ? startDateString : "",
                                    HQEndDate = location.ToLower().Contains("hq") ? EndDateString : "",

                                    RegionStartDate = location.ToLower().Contains("region") ? startDateString : "",
                                    RegionEndDate = location.ToLower().Contains("region") ? EndDateString : "",

                                    SiteStartDate = location.ToLower().Contains("site") ? startDateString : "",
                                    SiteEndDate = location.ToLower().Contains("site") ? EndDateString : "",
                                });
                            }
                        }
                    }
                }
            }

            roles = role_location.Values.ToList();
            responsibility = resp_location.Values.ToList();
            trainings = training.Values.ToList();
        }
    }
}
