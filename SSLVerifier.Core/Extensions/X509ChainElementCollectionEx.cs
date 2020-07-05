using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace SSLVerifier.Core.Extensions {
	public static class X509ChainElementCollectionEx {
		public static Int32 IndexOf(this X509ChainElementCollection collection, X509ChainElement item) {
			if (collection == null) { return -1; }
			for (Int32 index = 0; index < collection.Count; index++) {
				if (collection[index].Certificate.Equals(item.Certificate)) { return index; }
			}
			return -1;
		}
		public static X509ChainElement Item(this X509ChainElementCollection collection, X509ChainElement item) {
			return collection.Count == 0
				? null
				: collection.Cast<X509ChainElement>().FirstOrDefault(x => x.Certificate.Thumbprint == item.Certificate.Thumbprint);
		}

		public static Boolean Contains(this X509ChainElementCollection collection, X509ChainElement item) {
			return collection.Cast<X509ChainElement>().Any(x => item.Certificate.Equals(x.Certificate));
		}
	}
}
