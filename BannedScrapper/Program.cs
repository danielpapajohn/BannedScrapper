using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text.RegularExpressions;
using BannedScrapper.Models.JsonRoot;

namespace BannedScrapper
{
    class Program
    {
        public static string[] columns = 
            {
                "Name",
                "CRD#",
                "totalnumberofdisclosures",
                "yearsintheindustry",
                "numberoffirmsworkedfor",
                "numberofexamspassed",
                "numberoflicenses",
                "isthereactiondetails?",
                "yearoffirstexam",
                "yearoffirstcomplaint",
                "damagesawardedtovictims",
                "coststovictims",
                "#ofconsumerCOMPLAINT DISCLOSURES",
                "#ofCriminalDISCLOSURES",
                "#regulatory DISCLOSURES",
                "#ofotherdisclosures",
                "banned"
            };

        public static int extractIntFromUrl(string inputUrl)
        {
            string[] strings = inputUrl.Split('/');
            return (int)UInt64.Parse(strings[strings.Length - 1]);
        }

        public static string convertWebUrlToAPIUrl(string inputUrl)
        {
            string[] strings = inputUrl.Split('/');
            return "https://api.brokercheck.finra.org/individual/" + extractIntFromUrl(inputUrl).ToString();
        }

        public static string convertWebUrlToAPIUrl(int id)
        {
            return "https://api.brokercheck.finra.org/individual/" + id;
        }

        public static string trimJsonResponse(string responseJson)
        {
            responseJson = responseJson.Replace("/**/(", String.Empty);
            responseJson = responseJson.Replace(");", String.Empty);
            return responseJson;
        }

        public static string getTSVLine(string[] strs)
        {
            string result = String.Empty;
            for(int i = 0; i < strs.Length; i++)
            {
                result += strs[i];
                if (i != strs.Length - 1)
                {
                    result += "\t";
                }
            }
            return result;
        }

        static void Main(string[] args)
        {
            Random rnd = new Random();

            //initialize empty lists
            List<string> bannedIndividualUrls = new List<string>();
            List<string> unusableUrls = new List<string>();
            List<string> listUrls = new List<string>();

            //generate URLs for the alphabet listing of banned individuals
            for (int offset = 0; offset < 26; offset++)
            {
                listUrls.Add("https://www.finra.org/industry/individuals-barred-finra-" + (char)((char)'a' + offset));
            }

            //iterate through web pages with lists of banned individuals
            foreach (string url in listUrls)
            {
                Console.WriteLine("Visiting web page: " + url);
                var web = new HtmlWeb();
                var doc = web.Load(url);
                HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//tr/td/a");

                //parse each individual's link, to see if it is a usable html page or an unusable PDF
                foreach (HtmlNode node in htmlNodes)
                {
                    String individualStr = node.Attributes[0].Value;
                    if (individualStr.Contains("http://brokercheck.finra.org/Individual/Summary/"))
                    {
                        //html page, should be parsable
                        bannedIndividualUrls.Add(individualStr);
                    }
                    else
                    {
                        //unusable, probably a PDF
                        unusableUrls.Add(individualStr);
                    }
                }
            }

            //print results of list parsing
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Found banned individuals to be parsed: " + bannedIndividualUrls.Count);
            Console.WriteLine("Found banned individuals that can't be parsed: " + unusableUrls.Count);
            Console.WriteLine("Percent unparsable: " + (double)unusableUrls.Count / bannedIndividualUrls.Count * 100);
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Parsing banned individuals");

            //var web = new HtmlWeb();
            //var doc = web.Load("https://brokercheck.finra.org/individual/summary/307");
            //HtmlNodeCollection htmlNodes = doc.DocumentNode.SelectNodes("//div/div/div/div/div/div/div/div/div/div/div");


            //https://brokercheck.finra.org/individual/summary/307
            //https://api.brokercheck.finra.org/individual/307

            List<string> outputStrs = new List<string>();
            outputStrs.Add(getTSVLine(columns));

            Dictionary<int, bool> isBannedDictionary = new Dictionary<int, bool>();
            //isBannedDictionary.ContainsKey(123);
            //Console.WriteLine(isBannedDictionary[123]);

            int parsedIndividuals = 0;
            int minCrd = int.MaxValue;
            int maxCrd = int.MinValue;

            while (parsedIndividuals < bannedIndividualUrls.Count)
            {
                string url = bannedIndividualUrls[parsedIndividuals];
                string apiUrl = convertWebUrlToAPIUrl(url);
                //string apiUrl = convertWebUrlToAPIUrl(parsedIndividuals);

                string jsonStr;

                using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                {
                    client.BaseAddress = new Uri(apiUrl);
                    HttpResponseMessage response = client.GetAsync("").Result;
                    response.EnsureSuccessStatusCode();
                    jsonStr = response.Content.ReadAsStringAsync().Result;
                    jsonStr = trimJsonResponse(jsonStr);
                }

                RootObject root = JsonConvert.DeserializeObject<RootObject>(jsonStr);

                try
                {
                    if(root.hasData())
                    {
                        BannedIndividual banned = JsonConvert.DeserializeObject<BannedIndividual>(root.hits.hits[0]._source.content);
                        int crdNumber = extractIntFromUrl(url);
                        if(crdNumber < minCrd)
                        {
                            minCrd = crdNumber;
                        }
                        if(crdNumber > maxCrd)
                        {
                            maxCrd = crdNumber;
                        }
                        isBannedDictionary.Add(crdNumber, true);
                        Console.WriteLine(parsedIndividuals + ": " + banned.basicInformation.getName());

                        try
                        {
                            banned.init();
                            banned.isBanned = true;
                            outputStrs.Add(getTSVLine(banned.getColumnValues()));
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("ERROR! Could not format data for: " + parsedIndividuals);
                            Console.WriteLine(e.ToString());
                        }
                    }
                    else
                    {
                        Console.WriteLine(parsedIndividuals + ": DATA NOT AVAILABLE!!!!!!!");
                    }
                    parsedIndividuals++;
                }
                catch (Exception e)
                {
                    parsedIndividuals++;
                    Console.WriteLine("ERROR! Could not parse: " + apiUrl);
                    Console.WriteLine(e.ToString());
                }
            }

            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Getting a control group...");

            int bannedCount = outputStrs.Count;
            int controlGroup = 0;

            while (controlGroup < bannedCount)
            {
                //get a random individual
                int crd = rnd.Next(minCrd, maxCrd);
                string apiUrl = convertWebUrlToAPIUrl(crd);

                string jsonStr;

                try
                {
                    using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
                    {
                        client.BaseAddress = new Uri(apiUrl);
                        HttpResponseMessage response = client.GetAsync("").Result;
                        response.EnsureSuccessStatusCode();
                        jsonStr = response.Content.ReadAsStringAsync().Result;
                        jsonStr = trimJsonResponse(jsonStr);
                    }

                    RootObject root = JsonConvert.DeserializeObject<RootObject>(jsonStr);

                    //check if they are banned
                    try
                    {
                        if (root.hasData())
                        {
                            BannedIndividual banned = JsonConvert.DeserializeObject<BannedIndividual>(root.hits.hits[0]._source.content);


                            try
                            {
                                banned.init();
                                bool isEligible;
                                if (isBannedDictionary.ContainsKey(crd))
                                {
                                    //already added to the list
                                    isEligible = false;
                                    Console.WriteLine("SKIPPING! Already parsed this record: " + crd);
                                }
                                else
                                {
                                    //if not banned, try to parse and add them in to outputStrs
                                    outputStrs.Add(getTSVLine(banned.getColumnValues()));
                                    // add this record to the list of CRDs we've examined.
                                    isBannedDictionary.Add(crd, false);
                                    Console.WriteLine(controlGroup + ": " + banned.basicInformation.getName());
                                    //increment successes
                                    controlGroup++;
                                }

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("ERROR! Could not format data for: " + crd);
                                Console.WriteLine(e.ToString());
                            }
                        }
                        else
                        {
                            Console.WriteLine(apiUrl + ": DATA NOT AVAILABLE!!!!!!!");
                        }
                    }
                    catch (Exception e)
                    {
                        parsedIndividuals++;
                        Console.WriteLine("ERROR! Could not parse: " + apiUrl);
                        Console.WriteLine(e.ToString());
                    }
                }
                catch
                {
                    //something unexpected failed
                }
                
                //if not successfull, or duplicate, dont increment and try again
            }

            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.WriteLine("Writing to text file.");
            File.WriteAllLines("testFile.txt", outputStrs, System.Text.Encoding.Unicode);

            Console.WriteLine("Processing complete!");
        }
    }
}
