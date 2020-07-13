using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SSLVerifier.Core.Extensions;
using SSLVerifier.Core.Models;
using SSLVerifier.Core.Processor;

namespace SSLVerifier.Core.Data {
    public class HtmlProcessor {
        readonly IList<IServerObject> _serverList = new List<IServerObject>();
        readonly ICertProcessorConfig _config;

        public HtmlProcessor(ICertProcessorConfig config) {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public String GenerateReport(IServerObject[] servers) {
            if (servers == null) {
                throw new ArgumentNullException(nameof(servers));
            }
            _serverList.Clear();
            foreach (IServerObject server in servers) {
                _serverList.Add(server);
            }

            return String.Format(HtmlTemplate.HTML_REPORT,
                buildMetadata(),
                buildMainRows(),
                buildCountSummary(),
                buildTopDetails());
        }

        String buildMetadata() {
            return String.Format(HtmlTemplate.HTML_REPORT_META,
                DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"),
                _config.AllowUserTrust.ToYesNo(),
                _config.SslProtocolsToUse.ToString(),
                _config.CheckWeakPubKey.ToYesNo(),
                _config.MinimumRsaPubKeyLength,
                _config.StrictUsage,
                _config.Threshold,
                _config.CheckWeakPubKey.ToYesNo(),
                String.Join("<br/>",_config.WeakAlgorithms.Cast<Oid>().Select(x => x.Format())));
        }

        String buildMainRows() {
            var sb = new StringBuilder();
            foreach (IServerObject server in _serverList) {
                sb.AppendLine(String.Format(HtmlTemplate.HTML_MAIN_ROW,
                    getCssClassFromStatus(server.ItemStatus),
                    getIconFromStatus(server.ItemStatus),
                    server.ServerAddress,
                    server.Port,
                    server.Certificate?.Subject,
                    server.Certificate?.NotBefore.ToString("dd MMM yyyy"),
                    server.Certificate?.NotAfter.ToString("dd MMM yyyy"),
                    server.Certificate == null ? 0 : (server.Certificate.NotAfter - DateTime.Now).Days
                ));
            }

            return sb.ToString();
        }
        String buildCountSummary() {
            return String.Format(HtmlTemplate.HTML_COUNT_SUMMARY,
                _serverList.Count,
                _serverList.Count(x => x.ItemStatus == ServerStatusEnum.Unknown),
                _serverList.Count(x => x.ItemStatus == ServerStatusEnum.Valid),
                _serverList.Count(x => x.ItemStatus == ServerStatusEnum.Pending),
                _serverList.Count(x => x.ItemStatus == ServerStatusEnum.Failed));
        }
        String buildTopDetails() {
            var sb = new StringBuilder();

            for (Int32 i = 0; i < _serverList.Count; i++) {
                sb.AppendLine(String.Format(HtmlTemplate.HTML_ENTRY_CERT_ALL,
                    $"{i + 1}. {_serverList[i].ServerAddress}:{_serverList[i].Port}",
                    buildChains(_serverList[i].Tree)));
            }

            return sb.ToString();
        }
        String buildChains(IEnumerable<TreeNode<IChainElement>> tree) {
            var sb = new StringBuilder();
            foreach (TreeNode<IChainElement> subTree in tree) {
                sb.AppendLine(buildChain(subTree));
            }

            return sb.ToString();
        }
        String buildChain(TreeNode<IChainElement> tree) {
            List<IChainElement> list = tree.Flatten().Reverse().ToList();
            if (list.Count == 0) {
                return String.Empty;
            }
            var sb = new StringBuilder();

            for (Int32 i = 0; i < list.Count - 1; i++) {
                sb.AppendLine(buildCertEntry(i, list[i]));
            }

            return String.Format(HtmlTemplate.HTML_CERT_CHAIN,
                tree.Value.Name,
                sb);
        }
        String buildCertEntry(Int32 id, IChainElement chainElement) {
            X509Certificate2 cert = chainElement.Certificate;
            return String.Format(HtmlTemplate.HTML_CERT_DUMP,
                id + 1,
                cert.Subject,
                cert.Extensions["2.5.29.17"]?.Format(false),
                cert.SerialNumber,
                cert.NotBefore.ToString("ddd, dd MMM yyyy HH:mm:ss"),
                cert.NotAfter.ToString("ddd, dd MMM yyyy HH:mm:ss"),
                getPublicKeyString(cert.PublicKey),
                cert.Issuer,
                cert.SignatureAlgorithm.Format(),
                cert.Thumbprint,
                chainElement.NativeErrors.ToString(),
                cert.ToPEM());
        }
        static String getPublicKeyString(PublicKey key) {
            StringBuilder sb = new StringBuilder(key.Oid.Format());
            sb.Append(", ");
            try {
                sb.Append(key.Key.KeySize + " bits");
            } catch {
                var bytes = new List<Byte>(new Byte[]{48, (Byte)key.EncodedParameters.RawData.Length});
                bytes.AddRange(key.EncodedParameters.RawData);
                var asn = new AsnEncodedData(bytes.ToArray());
                var eku = new X509EnhancedKeyUsageExtension(asn, false);
                sb.Append(eku.EnhancedKeyUsages[0].Format());
            }
            return sb.ToString();
        }

        static String getIconFromStatus(ServerStatusEnum status) {
            switch (status) {
                case ServerStatusEnum.Unknown:
                    return HtmlTemplate.IMG_BLANK;
                case ServerStatusEnum.Valid:
                    return HtmlTemplate.IMG_OK;
                case ServerStatusEnum.Pending:
                    return HtmlTemplate.IMG_WARN;
                case ServerStatusEnum.Failed:
                    return HtmlTemplate.IMG_BAD;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
        static String getCssClassFromStatus(ServerStatusEnum status) {
            switch (status) {
                case ServerStatusEnum.Unknown:
                    return "default";
                case ServerStatusEnum.Valid:
                    return "success";
                case ServerStatusEnum.Pending:
                    return "warning";
                case ServerStatusEnum.Failed:
                    return "danger";
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}