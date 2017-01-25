using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace DMP.ViewModel
{
    public class AutomaticViewModel<T> where T : class
    {
        private Dictionary<string, object> _fieldCollection;
        public Dictionary<string, object> FieldCollection
        {
            get
            {
                if (_fieldCollection != null && _fieldCollection.Count() != 0)
                    return _fieldCollection;

                else
                {
                    _fieldCollection = new Dictionary<string, object>();
                    foreach (var info in typeof(T).GetProperties().Where(x =>x.CanWrite && !Attribute.IsDefined(x, typeof(XmlIgnoreAttribute))).ToArray())
                    {
                        _fieldCollection.Add(CommonUtil.Utilities.Utilities.PasCaseConversion(info.Name), info.PropertyType.Name);
                    }
                    return _fieldCollection;
                }
            }
        }
 
    }
}