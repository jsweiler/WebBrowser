using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebBrowserTest
{
    public sealed class UrlSplitter
    {
        public UrlSplitter(string data)
        {
            Splits = new List<UrlSplit>();
            if (string.IsNullOrWhiteSpace(data))
            {
                return;
            }

            var splits = data.Split('&');
            foreach (var item in splits)
            {
                var equalIndex = item.IndexOf('=');
                if (equalIndex < 0)
                    continue;
                var name = item.Substring(0, equalIndex);
                var value = item.Substring(equalIndex + 1, item.Length - equalIndex - 1);
                Splits.Add(new UrlSplit(name, value));
            }
        }

        public object Query(string name)
        {
            var split = Splits.FirstOrDefault(c => c.Name == name);
            return split?.Value;
        }

        public List<UrlSplit> Splits { get; set; }

    }

    public class UrlSplit
    {
        public UrlSplit(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public object Value { get; private set; }
    }
}
