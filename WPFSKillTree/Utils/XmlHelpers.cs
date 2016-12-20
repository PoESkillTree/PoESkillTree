using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace POESKillTree.Utils
{
    public class XmlHelpers
    {
        public static T DeserializeXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var strm = new StringReader(xml))
            using (var reader = XmlReader.Create(strm))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
    }
}
