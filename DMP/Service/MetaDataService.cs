
using DQA.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShieldPortal.Service
{
    public class MetaDataService
    {
        readonly shield_dmpEntities entity = new shield_dmpEntities();

        public List<dqa_report_metadata> GetIpMetaData(int ip)
        {
            return entity.dqa_report_metadata.Where(e => e.ImplementingPartner == ip).ToList();
        }
    }
}