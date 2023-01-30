using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace WebScrapping
{
    //url
    //identifier -> value
    //company_name
    //title
    //description
    //work_hours
    //requirements - skills
    //responsibilities
    //categories
    //position_level - employmentType
    class ScrappingData
    {
        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }
        public static List<string> getJobTitleURL(Dictionary<string, string> jobIdsAndTitles)
        {
            string urlStart = "https://careers.coupa.com/global/en/job/";
            List<string> urls = new List<string>();
            foreach (var job in jobIdsAndTitles)
            {
                urls.Add(urlStart + job.Key + "/" + job.Value);
            }

            return urls;
        }

        static void Main(string[] args)
        {
            HtmlAgilityPack.HtmlWeb website = new HtmlAgilityPack.HtmlWeb();

            HtmlAgilityPack.HtmlDocument document = website.Load("https://careers.coupa.com/global/en/search-results?s=1");

            var script = document
                .DocumentNode
                .Descendants()
                .Where(n => n.Name == "script")
                .First()
                .InnerText;

            var startString = "phApp.ddo = ";
            
            var endString = ",\"flashParams\"";

            var data = getBetween(script, startString, endString);

            var a = data + "}";


            JObject json = JObject.Parse(a);

            var token = json.SelectToken("eagerLoadRefineSearch");
            var dataToken = token.SelectToken("data");
            var jobs = dataToken.SelectToken("jobs").ToList();

            Dictionary<string, string> jobIdsAndTitles = new Dictionary<string, string>();

            foreach (var job in jobs)
            {
                jobIdsAndTitles.Add(job.SelectToken("jobId").ToString(), job.SelectToken("title").ToString());
            }

            List<string> jobsURLs = new List<string>();
            jobsURLs = getJobTitleURL(jobIdsAndTitles);

            HtmlAgilityPack.HtmlWeb innerWebsite = new HtmlAgilityPack.HtmlWeb();
            HtmlAgilityPack.HtmlDocument innerDocument = innerWebsite.Load(jobsURLs[0]);

            var innerScript = innerDocument
                .DocumentNode
                .Descendants()
                .Where(n => n.Name == "script" && n.Attributes.First().Value == "text/javascript")
                .First()
                .InnerText;

            var endSubString = ",\"caasLazyLoadWidgetVersions\"";

            var subData = getBetween(innerScript, startString, endSubString);

            var b = subData + "}";

            JObject subJson = JObject.Parse(b);

            var subToken = subJson.SelectToken("jobDetail");
            var subDataToken = subToken.SelectToken("data");
            var subDataJobToken = subDataToken.SelectToken("job");
            var subDataJobStructureToken = subDataJobToken.SelectToken("structureData");
            var subDataJobStructureIdentifierToken = subDataJobStructureToken.SelectToken("identifier");

            Console.WriteLine(subDataJobStructureIdentifierToken.SelectToken("value").ToString());


        }
    }
}
