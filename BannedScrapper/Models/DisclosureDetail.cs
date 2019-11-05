using System.Collections.Generic;

namespace BannedScrapper
{
    public class DisclosureDetail
    {
        public string DocketNumberFDA { get; set; }
        public string DocketNumberAAO { get; set; }
        public string initiatedBy { get; set; }
        public string Allegations { get; set; }
        public string Resolution { get; set; }
        public List<SanctionDetail> SanctionDetails { get; set; }
        public string Sanctions { get; set; }
        public string sanctionDetails { get; set; }
        public string damageAmountRequested { get; set; }
        public string damagesGranted { get; set; }
        public string DisplayAAOLinkIfExists { get; set; }
        public string arbitrationClaimFiledDetail { get; set; }
        public string arbitrationDocketNumber { get; set; }
        public string brokerComment { get; set; }
    }
}