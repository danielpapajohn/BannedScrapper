using System.Collections.Generic;

namespace BannedScrapper
{
    public class Sanctions
    {
        public string permanentBar { get; set; }
        public List<SanctionDetail> sanctionDetails { get; set; }
    }
}