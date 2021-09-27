using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Giselle.Commons;
using Giselle.Commons.Net;
using Giselle.Commons.Net.Http;
using Giselle.DCConDownloader.Data;
using Newtonsoft.Json.Linq;

namespace Giselle.DCConDownloader
{
    public class DCConCrawler
    {
        private readonly WebExplorer Explorer;

        public DCConCrawler()
        {
            this.Explorer = new WebExplorer();
        }

        public Stream DownloadAsStream(string referer, string path)
        {
            var req = new WebRequestParameter
            {
                Uri = this.GetImageURL(path),
                Method = "GET",
                Referer = referer,
            };
            req.Headers["Origin"] = "http://dccon.dcinside.com";

            using (var res = this.Explorer.Request(req))
            {
                using (var stream = res.ReadAsStream())
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    ms.Position = 0;

                    return ms;
                }

            }

        }

        public string GetImageURL(string path)
        {
            return $"http://dcimg5.dcinside.com/dccon.php?no={path}";
        }

        public DCConPackage GetPackage(string url)
        {
            var fragment = new Uri(url).Fragment;
            var prefix = HttpUtility2.FragmentSeparator;

            if (fragment.StartsWith(prefix) == true)
            {
                fragment = fragment.RemovePrefix(prefix);
            }
            else
            {
                return null;
            }

            var req = new WebRequestParameter
            {
                Uri = "https://dccon.dcinside.com/index/package_detail",
                Method = "POST",
                Referer = "http://dccon.dcinside.com/",
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                WriteParameter = $"package_idx={fragment}&code=",
            };
            req.Headers["Origin"] = "http://dccon.dcinside.com";
            req.Headers["X-Requested-With"] = "XMLHttpRequest";

            using (var res = this.Explorer.Request(req))
            {
                var json = res.ReadAsString();
                var jobject = JObject.Parse(json);

                var package = new DCConPackage { Referer = url };
                package.Read(jobject);

                return package;
            }

        }

    }

}
