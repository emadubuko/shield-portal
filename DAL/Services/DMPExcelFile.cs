using CommonUtil.Utilities;
using DAL.Entities;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DAL.Services
{
    public class DMPExcelFile
    {
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
