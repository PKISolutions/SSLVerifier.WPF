using System;
using System.IO;
using System.Xml.Serialization;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.MainLogic {
	static class XmlRoutine {
		public static XmlObject Deserialize(String file) {
			FileStream fs = new FileStream(file, FileMode.Open);
			try {
				XmlSerializer serializer = new XmlSerializer(typeof(XmlObject));
				return (XmlObject)serializer.Deserialize(fs);
			} finally {
				fs.Close();
			}
		}
	}
}
