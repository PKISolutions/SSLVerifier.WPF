using System;
using SSLVerifier.Core.Models;

namespace SSLVerifier.API.ModelObjects {
	class ReportObject {
		public String Action { get; set; }
		public Int32 Index { get; set; }
		public TreeNode<ChainElement> NewTree { get; set; }
	}
}
