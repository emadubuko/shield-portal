using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class DigitalData
    {
        public virtual string VolumeOfdigitalData { get; set; }
        public virtual string Storagetype { get; set; }
        public virtual string Storagelocation { get; set; }
        public virtual string Backup { get; set; }
        public virtual string Datasecurity { get; set; }
        public virtual string Patientconfidentialitypolicies { get; set; }

        //Storageofpre-existingdata
        public virtual string StorageOfPreExistingData { get; set; }


    }
}
