using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Giselle.DCConDownloader.Utils;
using Newtonsoft.Json.Linq;

namespace Giselle.DCConDownloader.Data
{
    public class DCConImage
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string Ext { get; set; }

        public DCConImage()
        {
            this.Title = null;
            this.Path = null;
            this.Ext = null;
        }

        public void Read(JToken json)
        {
            this.Title = PathUtils.FilterInvalids(json.Value<string>("title"));
            this.Path = PathUtils.FilterInvalids(json.Value<string>("path"));
            this.Ext = json.Value<string>("ext");
        }

    }

}
