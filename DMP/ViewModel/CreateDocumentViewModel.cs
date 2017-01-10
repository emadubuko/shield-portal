using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DMP.ViewModel
{
    public class CreateDocumentViewModel
    {
        public Organizations Organization { get; set; }
        public Profile Initiator { get; set; }

        public DateTime versionDate
        {
            get
            {
                return DateTime.Now;
            }
        }
        public ICollection<string> ethicalApprovalTypes
        {
            get
            {
                return new string[] { "Temporary", "Global" };
            }
        }
        public ICollection<string> thematicAreas
        {
            get
            {
                return new string[] { "Research", "Data Validation" };
            }
        }
        public ICollection<string> reportDataType
        {
            get
            {
                return new string[] { "freeText", "digital" };
            }
        }
        public ICollection<string> formsOfDataVerification
        {
            get
            {
                return new string[] { "Visual", "others" };
            }
        }

        public ICollection<string> typesOfDataVerification
        {
            get
            {
                return new string[] { "Manual Comparison", "DQA" };
            }
        }
        public ICollection<string> storagetype
        {
            get
            {
                return new string[] { "Database", "others" };
            }
        }
        public ICollection<string> storageLocation
        {
            get
            {
                return new string[] { "On premise", "Cloud store" };
            }
        }
        public ICollection<string> nonDigitalDataTypes
        {
            get
            {
                return new string[] { "Files", "Registers" };
            }
        }
        public ICollection<string> nonDigitalStorageLocation
        {
            get
            {
                return new string[] { "Cabinet", "Drawers" };
            }
        }
    }
}