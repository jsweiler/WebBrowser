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
            var gatewayResponse = GetOtk(vm);
            if (string.IsNullOrWhiteSpace(gatewayResponse.OTK))
            {
                vm.Message = gatewayResponse.ResponseDescription;
                return null;
            }
            Otk = gatewayResponse.OTK;
            var hpf = GetHpf(Otk);
            return hpf;

        }

        private CardResponse GetOtk(MainViewModel vm)
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
                {"Amount", Math.Abs(vm.Amount).ToString(CultureInfo.CurrentCulture)},
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
                var result = client.DownloadString(fullUrl);

                var cardResponse = new CardResponse();

                var splitter = new UrlSplitter(result);
                cardResponse.OTK = (string) splitter.Query(nameof(cardResponse.OTK));
                cardResponse.ResponseCode = (string) splitter.Query(nameof(cardResponse.ResponseCode));
                cardResponse.ResponseDescription = (string) splitter.Query(nameof(cardResponse.ResponseDescription));
                return cardResponse;
            }
            catch (Exception e)
            {
                vm.Message = e.Message;
                return null;
            }
        }

        private string GetHpf(string otk)
        {
            var baseUrl = "https://integrator.t3secure.net/hpf/hpf.aspx" + "?otk=" + otk;

            return baseUrl;
        }

    }


    public class CardResponse
    {
        public string OTK { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
    }

}
