using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.ViewModel
{
    public class DMPViewModel
    {
        public string Title { get; set; }
        public string  ProjectTitle { get; set; }
        public string Owner { get; set; }
        public string CreatedBy { get; set; } 
        public string Status { get; set; }
        public string DateCreated { get; set; }
        public int Id { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }


    public class ScoreCardModel
    {
        public string IP { get; set; }
        public bool ProjectProfile { get; set; }
        public bool ProgramObjectives { get; set; }
        public bool MandE { get; set; }
        public bool DataProcesses { get; set; }
        public bool QA { get; set; }
        public bool DataStorage { get; set; }
        public bool CopyRight { get; set; }
        public bool DataRetention { get; set; }
        public int Status
        {
            get
            {
                int r = 0;
                if (ProjectProfile)
                    r += 10;
                if (ProgramObjectives)
                    r += 10;
                if (MandE)
                    r += 20;
                if (DataProcesses)
                    r += 10;
                if (QA)
                    r += 10;
                if (DataStorage)
                    r += 20;
                if (CopyRight)
                    r += 10;
                if (DataRetention)
                    r += 10;

                return r;
            }
        }
    }
}