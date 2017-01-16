using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace DMP.ViewModel
{
    public class CreateIPViewModel : AutomaticViewModel<Organizations>
    {
        private List<string> _organizationType;
        public List<string> OrganizationType
        {
            get
            {
                if (_organizationType != null)
                    return _organizationType;

                _organizationType = new List<string>();
                foreach (var name in Enum.GetNames(typeof(OrganizationType)))
                {
                    _organizationType.Add(DAL.Utilities.Utilities.PasCaseConversion(name));
                }
                return _organizationType;
            }
        }
    }
}