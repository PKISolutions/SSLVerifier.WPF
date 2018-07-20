using System.Xml.Serialization;

namespace SSLVerifier.API.ModelObjects {
    [XmlType(AnonymousType = true), XmlRoot(Namespace = "", IsNullable = false)]
    public class XmlObject {
        [XmlElement("SERVER_ENTRY")]
        public ServerObject[] ServerObjects { get; set; }
    }
}
