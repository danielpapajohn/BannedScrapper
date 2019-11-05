using System;
using System.Collections.Generic;
using System.Text;

namespace BannedScrapper.Models.JsonRoot
{
    class RootObject
    {
        public Hits hits { get; set; }

        public bool hasData()
        {
            if (hits.hits.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}