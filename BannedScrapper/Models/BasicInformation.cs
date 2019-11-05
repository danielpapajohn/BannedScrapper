using System;
using System.Collections.Generic;

namespace BannedScrapper
{
    public class BasicInformation
    {
        public int individualId { get; set; }
        public string firstName { get; set; }
        public string middleName { get; set; }
        public string lastName { get; set; }
        public Sanctions sanctions { get; set; }
        public List<object> otherNames { get; set; }
        public string bcScope { get; set; }
        public string iaScope { get; set; }
        public int daysInIndustry { get; set; }

        public string getName()
        {
            string middle;
            if(String.IsNullOrEmpty(middleName))
            {
                middle = string.Empty;
            }
            else
            {
                middle = " " + middleName + " ";
            }
            return firstName + middle + lastName;
        }
    }
}