using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Giselle.DCConDownloader
{
    public class CellData
    {
        public List<string> Keywords { get; }
        public List<string> Tags { get; }
        public string Ext { get; set; }
        public Texture Texture { get; set; }

        public CellData()
        {
            this.Keywords = new List<string>();
            this.Tags = new List<string>();
            this.Ext = null;
            this.Texture = null;
        }

    }

}
