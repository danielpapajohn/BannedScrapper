using System.Collections.Generic;

namespace BannedScrapper
{
    public class SanctionDetail
    {
        public string category { get; set; }
        public string regulator { get; set; }
        public List<string> messages { get; set; }
        public List<object> detail { get; set; }
        public List<string> capacity { get; set; }
    }
}