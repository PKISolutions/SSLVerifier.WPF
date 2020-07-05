using System;
using System.Collections.ObjectModel;

namespace SSLVerifier.API.ModelObjects {
	class BackgroundObject {
		public ObservableCollection<ServerObject> Servers { get; set; }
		public StatusCounter Counters { get; set; }
		public Int32 Threshold { get; set; }
		public Boolean SingleScan { get; set; }
	}
}
