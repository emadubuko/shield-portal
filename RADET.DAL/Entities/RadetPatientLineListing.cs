using System;

namespace RADET.DAL.Entities
{
    public class RadetPatientLineListing
    {
        public virtual int Id { get; set; }
        public virtual RadetPatient RadetPatient { get; set; }
        public virtual DateTime? ARTStartDate { get; set; }
        public virtual DateTime? LastPickupDate { get; set; }
        public virtual int MonthsOfARVRefill { get; set; }
        public virtual string RegimenLineAtARTStart { get; set; }
        public virtual string RegimenAtStartOfART { get; set; }
        public virtual string CurrentRegimenLine { get; set; }
        public virtual string CurrentARTRegimen { get; set; }
        public virtual string PregnancyStatus { get; set; }
        public virtual string CurrentViralLoad { get; set; }
        public virtual DateTime? DateOfCurrentViralLoad { get; set; }
        public virtual string ViralLoadIndication { get; set; }
        public virtual string CurrentARTStatus { get; set; }
        public virtual bool SelectedForDQA { get; set; }
        public virtual string RadetPeriod { get; set; }
        public virtual RadetMetaData MetaData { get; set; }
    }
       
}
