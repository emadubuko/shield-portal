using CommonUtil.Entities;
using System;
using System.Collections.Generic;

namespace RADET.DAL.Entities
{
    public class RadetUpload
    {
        public virtual int Id { get; set; }
        public virtual Organizations IP { get; set; }
        public virtual Profile UploadedBy { get; set; } 
        public virtual IList<RadetMetaData> RadetMetaData { get; set; }
        public virtual DateTime DateUploaded { get; set; }        
    }

    
}
