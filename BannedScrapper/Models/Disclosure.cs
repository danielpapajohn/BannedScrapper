using System;

namespace BannedScrapper
{
    public class Disclosure
    {
        public string eventDate { get; set; }
        /*
         * "Regulatory"
         * "Customer Dispute"
         * "Employment Separation After Allegations"
         * "Judgment / Lien"
         * "Investigation"
         * 
         **/
        public string disclosureType { get; set; }
        public string disclosureResolution { get; set; }
        public DisclosureDetail disclosureDetail { get; set; }

        public DateTime eventDateAsDateTime { get; set; }

        public void init()
        {
            //date format is mm/dd/yyyy
            string[] dateParts = eventDate.Split('/');
            int year = Int32.Parse(dateParts[2]);
            int month = Int32.Parse(dateParts[0]);
            int day = Int32.Parse(dateParts[1]);
            eventDateAsDateTime = new DateTime(year, month, day);
        }
    }
}