using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using SSLVerifier.API.ModelObjects;

namespace SSLVerifier.API.MainLogic {
	static class XmlRoutine {
		public static void Serialize(IEnumerable<ServerObject> servers, String file) {
			XmlObject root = new XmlObject {ServerObjects = servers.ToArray()};
			FileStream fs = new FileStream(file, FileMode.Create);
			try {
				XmlSerializer serializer = new XmlSerializer(typeof(XmlObject));
				serializer.Serialize(fs, root);
			} finally {
				fs.Close();
			}
		}
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
