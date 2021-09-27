using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Giselle.DCConDownloader.Data
{
    public class DCConPackage
    {
        public string Referer { get; set; }
        public string Title { get; set; }

        public List<DCConImage> Images { get; }
        public List<string> Tags { get; }

        public DCConPackage()
        {
            this.Referer = null;
            this.Title = null;
            this.Images = new List<DCConImage>();
            this.Tags = new List<string>();
        }

        public void Read(JToken json)
        {
            var info = json["info"];
            this.Title = info.Value<string>("title").Replace(" ", "");

            var images = this.Images;
            images.Clear();
            var jdetail = json.Value<JArray>("detail");

            for (int i = 0; i < jdetail.Count; i++)
            {
                var jImage = jdetail[i];
                var image = new DCConImage();
                image.Read(jImage);
                images.Add(image);
            }

            var tags = this.Tags;
            tags.Clear();
            var jtags = json.Value<JArray>("tags");

            for (int i = 0; i < jtags.Count; i++)
            {
                var jtag = jtags[i];
                var tag = jtag.Value<string>("tag").Replace(" ", "");
                tags.Add(tag);
            }

        }

    }

}
