using System;
using System.Collections.ObjectModel;
using SSLVerifier.Core;

namespace SSLVerifier.API.ModelObjects {
	class BackgroundObject {
		public ObservableCollection<ServerObject> Servers { get; set; }
		public IStatusCounter Counters { get; set; }
		public Boolean SingleScan { get; set; }
	}
}
