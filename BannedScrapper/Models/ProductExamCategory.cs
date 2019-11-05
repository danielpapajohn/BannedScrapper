using System;

namespace BannedScrapper
{
    public class ProductExamCategory
    {
        public string examCategory { get; set; }
        public string examName { get; set; }
        public string examTakenDate { get; set; }

        public DateTime examDateAsDateTime { get; set; }

        public void init()
        {
            //date format is mm/dd/yyyy
            string[] dateParts = examTakenDate.Split('/');
            int year = Int32.Parse(dateParts[2]);
            int month = Int32.Parse(dateParts[0]);
            int day = Int32.Parse(dateParts[1]);
            examDateAsDateTime = new DateTime(year, month, day);
        }
    }
}