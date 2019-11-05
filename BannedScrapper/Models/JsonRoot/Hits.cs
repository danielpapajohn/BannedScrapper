using System;
using System.Collections.Generic;
using System.Text;

namespace BannedScrapper.Models.JsonRoot
{
    class Hits
    {
        public int total { get; set; }
        public List<Hit> hits { get; set; }
    }
}
