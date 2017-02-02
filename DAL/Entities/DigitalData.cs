namespace DAL.Entities
{
    public class DigitalData
    {
        public virtual string VolumeOfDigitalData { get; set; }
        public virtual string DataStorageFormat { get; set; }
        public virtual string StorageLocation { get; set; }
        public virtual string Backup { get; set; }
        public virtual string DataSecurity { get; set; }
        public virtual string PatientConfidentialityPolicies { get; set; }

        //Storageofpre-existingdata
        public virtual string StorageOfPreExistingData { get; set; }


    }
}
