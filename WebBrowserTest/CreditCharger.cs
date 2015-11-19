using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WebBrowserTest
{

    public sealed class CardCharger : NotifyObject
    {

        public string Otk { get; private set; }

        public bool HandleCharge(MainViewModel vm)
        {
            var sourceUrl = this.GetSource(vm);
            if (sourceUrl == null)
                return false;
            vm.Source = sourceUrl;

            return true;
        }

        public string GetSource(MainViewModel vm)
        {
            Otk = "";
            var gatewayResponse = GetOtk(vm.Amount, vm);
            if (string.IsNullOrWhiteSpace(gatewayResponse))
            {
                return null;
            }
            Otk = gatewayResponse;
            var hpf = GetHpf(Otk);
            return hpf;

        }

        private string GetOtk(decimal amount, MainViewModel vm)
        {
            var dictionary = new Dictionary<string, string>
            {
                {"TerminalID", vm.TerminalId.Trim()},
                {"AuthKey", vm.AuthKey.Trim()},
                {"Industry", vm.Industry.Trim()},
                {"SpecVersion", "XWebSecure3.5"},
                {"TransactionType", "CreditSaleTransaction"},
                {"CreateAlias", "TRUE"},
                {"DuplicateMode", "CHECKING_OFF"},
                {"Amount", Math.Abs(amount).ToString(CultureInfo.CurrentCulture)},
                {"AmountLocked", "TRUE"},
                {"DeviceType", "EMV"},

            };

            //var url = creditUrl();
            var httpsUrl = "https://test.t3secure.net/x-chargeweb.dll";
            var xmlPost = "?XWebID=" + vm.XWebId.Trim();
            xmlPost = dictionary.Aggregate(xmlPost,
                (current, item) => current + "&" + item.Key + "=" + item.Value);

            var fullUrl = httpsUrl + xmlPost;

            try
            {
                var client = new WebClient();
                using (var r = client.OpenRead(fullUrl))
                using (var sr = new StreamReader(r))
                {
                    var result = sr.ReadToEnd();
                    var splits = result.Split('&');
                    foreach (var item in splits)
                    {
                        var name = item.Substring(0, item.IndexOf('='));
                        if (name == "OTK")
                        {
                            var equalIndex = item.IndexOf('=');
                            var value = item.Substring(equalIndex + 1, item.Length - equalIndex - 1);
                            return value;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return null;
        }

        private string GetHpf(string otk)
        {
            var baseUrl = "https://integrator.t3secure.net/hpf/hpf.aspx" + "?otk=" + otk;

            return baseUrl;
        }

    }

}
