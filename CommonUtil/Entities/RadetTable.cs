namespace CommonUtil.Entities
{
    public class RadetTable
    {
        public virtual int Id { get; set; }
        public virtual string PatientId { get; set; }
        public virtual string HospitalNo { get; set; }
        public virtual string Sex { get; set; }
        public virtual string Age_at_start_of_ART_in_years { get; set; }
        public virtual string Age_at_start_of_ART_in_months { get; set; }
        public virtual string ARTStartDate { get; set; }
        public virtual string LastPickupDate { get; set; }
        public virtual string MonthsOfARVRefill { get; set; }
        public virtual string RegimenLineAtARTStart { get; set; }
        public virtual string RegimenAtStartOfART { get; set; }
        public virtual string CurrentRegimenLine { get; set; }
        public virtual string CurrentARTRegimen { get; set; }
        public virtual string Pregnancy_Status { get; set; }
        public virtual string Current_Viral_Load { get; set; }
        public virtual string Date_of_Current_Viral_Load { get; set; }
        public virtual string Viral_Load_Indication { get; set; }
        public virtual string CurrentARTStatus { get; set; }

        public virtual bool SelectedForDQA { get; set; } 
        public virtual string RadetYear { get; set; } 
        public virtual Organizations IP { get; set; } 
        public virtual RadetUploadReport UploadReport { get; set; }

    }
}
