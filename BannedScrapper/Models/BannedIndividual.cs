using System;
using System.Collections.Generic;
using System.Text;

namespace BannedScrapper
{
    class BannedIndividual
    {
        public BasicInformation basicInformation { get; set; }
        public List<object> currentEmployments { get; set; }
        public List<PreviousEmployment> previousEmployments { get; set; }
        public string disclosureFlag { get; set; }
        public string iaDisclosureFlag { get; set; }
        public List<Disclosure> disclosures { get; set; }
        public ExamsCount examsCount { get; set; }
        public List<StateExamCategory> stateExamCategory { get; set; }
        public List<object> principalExamCategory { get; set; }
        public List<ProductExamCategory> productExamCategory { get; set; }
        public RegistrationCount registrationCount { get; set; }
        public List<object> registeredStates { get; set; }
        public List<object> registeredSROs { get; set; }
        public BrokerDetails brokerDetails { get; set; }

        public bool isBanned { get; set; }
        public double damagesAwarded { get; set; }

        private double damagesToDouble(string damages)
        {
            damages = damages.Replace("$", "");
            damages = damages.Replace(",", "");
            return (double) Double.Parse(damages);
        }

        public void init()
        {
            try
            {
                foreach (StateExamCategory ex in stateExamCategory)
                {
                    ex.init();
                }
            }
            catch {}
            try
            {
                foreach (ProductExamCategory ex in productExamCategory)
                {
                    ex.init();
                }
            }
            catch {}
            try
            {
                foreach (Disclosure disc in disclosures)
                {
                    disc.init();
                }
            }
            catch {}
            try
            {
                double damagesTotal = 0;
                foreach(Disclosure d in disclosures)
                {
                    if (d.disclosureDetail != null && d.disclosureDetail.damagesGranted != null)
                    {
                        damagesTotal += damagesToDouble(d.disclosureDetail.damagesGranted);
                    }
                }
                damagesAwarded = damagesTotal;
            }
            catch {}

        }

        private string getDateOfFirstExam()
        {
            DateTime oldestExam = DateTime.MaxValue;

            foreach (StateExamCategory ex in stateExamCategory)
            {
                if (oldestExam.Equals(DateTime.MaxValue))
                {
                    oldestExam = ex.examDateAsDateTime;
                }
                if (ex.examDateAsDateTime.CompareTo(oldestExam) < 0)
                {
                    oldestExam = ex.examDateAsDateTime;
                }
            }
            foreach (ProductExamCategory ex in productExamCategory)
            {
                if (oldestExam.Equals(DateTime.MaxValue))
                {
                    oldestExam = ex.examDateAsDateTime;
                }
                if (ex.examDateAsDateTime.CompareTo(oldestExam) < 0)
                {
                    oldestExam = ex.examDateAsDateTime;
                }
            }

            if (oldestExam != DateTime.MaxValue)
            {
                return oldestExam.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        private string getDateOfFirstComplaint()
        {
            DateTime oldestExam = DateTime.MaxValue;

            foreach (Disclosure disc in disclosures)
            {
                if(oldestExam.Equals(DateTime.MaxValue))
                {
                    oldestExam = disc.eventDateAsDateTime;
                }
                if (disc.eventDateAsDateTime.CompareTo(oldestExam) < 0)
                {
                    oldestExam = disc.eventDateAsDateTime;
                }
            }

            if (oldestExam != DateTime.MaxValue)
            {
                return oldestExam.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        public int numberOfLicences()
        {
            return (registrationCount.approvedFinraRegistrationCount + registrationCount.approvedSRORegistrationCount + registrationCount.approvedStateRegistrationCount);
        }

        public string[] getColumnValues()
        {
            string[] result = new string[17];
            //Name
            result[0] = basicInformation.getName();
            //CRD
            result[1] = basicInformation.individualId.ToString();
            //totalnumberofdisclosures
            result[2] = disclosures.Count.ToString();
            //yearsintheindustry
            result[3] = ((int)(basicInformation.daysInIndustry / 365)).ToString();

            //numberoffirmsworkedfor
            result[4] = previousEmployments.Count.ToString();
            //numberofexamspassed
            result[5] = (productExamCategory.Count + stateExamCategory.Count).ToString();
            //numberoflicenses
            result[6] = numberOfLicences().ToString();
            //isthereactiondetails?
            result[7] = String.Empty;

            //yearoffirstexam
            result[8] = getDateOfFirstExam();
            //yearoffirstcomplaint
            result[9] = getDateOfFirstComplaint();
            //damagesawardedtovictims
            result[10] = damagesAwarded.ToString();
            //coststovictims
            result[11] = String.Empty;

            //#ofconsumerCOMPLAINT DISCLOSURES
            result[12] = String.Empty;
            //#ofCriminalDISCLOSURES
            result[13] = String.Empty;
            //#regulatory DISCLOSURES
            result[14] = String.Empty;
            //#ofotherdisclosures
            result[15] = String.Empty;

            //banned
            if (isBanned == null)
            {
                result[16] = "toast";
            }
            else
            {
                result[16] = isBanned.ToString();
            }

            return result;
        }
    }
}

