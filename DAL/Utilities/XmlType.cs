using NHibernate.SqlTypes;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace DAL.Utilities
{
    [Serializable]
    public class XmlType<T> : MutableType
    {
        public XmlType()
            : base(new XmlSqlType())
        {
        }


        public XmlType(SqlType sqlType)
            : base(sqlType)
        {
        }

        public override string Name
        {
            get { return "XmlOfT"; }
        }

        public override System.Type ReturnedClass
        {
            get { return typeof(T); }
        }

        public override void Set(IDbCommand cmd, object value, int index)
        {
            ((IDataParameter)cmd.Parameters[index]).Value = XMLUtil.ConvertToXml(value);
        }

        public override object Get(IDataReader rs, int index)
        {
            // according to documentation, GetValue should return a string, at list for MsSQL
            // hopefully all DataProvider has the same behaviour
            string xmlString = Convert.ToString(rs.GetValue(index));
            return FromStringValue(xmlString);
        }

        public override object Get(IDataReader rs, string name)
        {
            return Get(rs, rs.GetOrdinal(name));
        }

        public override string ToString(object val)
        {
            return val == null ? null : XMLUtil.ConvertToXml(val);
        }

        public override object FromStringValue(string xml)
        {
            if (xml != null)
            {
                return XMLUtil.FromXml<T>(xml);
            }
            return null;
        }

        public override object DeepCopyNotNull(object value)
        {           
            var original = (T)value;
            var copy = XMLUtil.FromXml<T>(XMLUtil.ConvertToXml(original));
            return copy;
        }

        public override bool IsEqual(object x, object y)
        {
            if (x == null && y == null)
            {
                return true;
            }
            if (x == null || y == null)
            {
                return false;
            }
            return XMLUtil.ConvertToXml(x) == XMLUtil.ConvertToXml(y);
        }
    }

    //the methods from this class are also available at: http://blog.nitriq.com/PutDownTheXmlNodeAndStepAwayFromTheStringBuilder.aspx
   
}
