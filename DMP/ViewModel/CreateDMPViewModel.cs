using DAL.Entities;
using System;
using System.Collections.Generic;

namespace DMP.ViewModel
{
    public class CreateDMPViewModel
    {
        public Organizations Organization { get; set; }
        public string DMPTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime DateCreated { get; set; }
        public Profile CreatedBy { get; set; }
        public ICollection<Organizations> organizations { get; set; }
    }
     
}