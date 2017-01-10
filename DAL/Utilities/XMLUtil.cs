using DAL.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DAL.Utilities
{
    public class XMLUtil
    {
        public static string ConvertToXml(object item)
        {
            XmlSerializer xmlser = new XmlSerializer(item.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                xmlser.Serialize(ms, item);
                UTF8Encoding textconverter = new UTF8Encoding();
                return textconverter.GetString(ms.ToArray());
            }
        }

        public static T FromXml<T>(string xml)
        {
            XmlSerializer xmlser = new XmlSerializer(typeof(T));
            using (StringReader sr = new StringReader(xml))
            {
                return (T)xmlser.Deserialize(sr);
            }
        }


    }
}
