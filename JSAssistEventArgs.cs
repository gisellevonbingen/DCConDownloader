using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Giselle.DCConDownloader
{
    public class JSAssistEventArgs : EventArgs
    {
        public string KeywordPrefix { get; }

        public List<string> CommonTags { get; }

        public JSAssistEventArgs(string keywordPrefix, List<string> commonTags)
        {
            this.KeywordPrefix = keywordPrefix;
            this.CommonTags = commonTags;
        }

    }

}
