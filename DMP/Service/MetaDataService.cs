
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

        public List<dqa_report_metadata> SearchIpMetaData(int ip,string period,string lga,string state,string site_name)
        {
            return entity.dqa_report_metadata.Where(e => e.ImplementingPartner == ip && (e.ReportPeriod == period || e.LgaId==lga || e.StateId==state)).ToList();
        }

        public List<dqa_report_metadata> GetAllMetadata()
        {
            return entity.dqa_report_metadata.ToList();
        }
    }
}