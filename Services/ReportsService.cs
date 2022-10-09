using Helbiz_Witness_Reports_Test.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Helbiz_Witness_Reports_Test.Services
{
    public class ReportsService : IReportsService
    {
        static readonly HttpClient client = new HttpClient();
        static readonly string fbiUrl = "https://api.fbi.gov/wanted/v1/list";
        static readonly string ipAddressUrl = "https://geo.ipify.org/api/v2/country";
        static readonly string apiKey = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("IpAddress")["ApiKey"];

        public async Task<bool> GetReportsByTitleAsync(string title, string phoneNumber)
        {
            Report report = new Report();

            if (!IsPhoneNumber(phoneNumber)) return false;

            string fbiUrlWithParams = BuildUrl("title", title, fbiUrl);
            string locationUrl = BuildUrl("apiKey", apiKey, ipAddressUrl);


            var fbiReport = await client.GetAsync(fbiUrlWithParams);
            var location = await client.GetAsync(locationUrl);

            var totalReports = GetDataFromJson(fbiReport, "total");
            var country = GetDataFromJson(location, "location", "country");

            if (totalReports == "0" || totalReports == null) return false;

            if (fbiReport.IsSuccessStatusCode)
            {
                report.PhoneNumber = phoneNumber;
                report.ReportContent = FormatJson(fbiReport.Content.ReadAsStringAsync().Result);
                report.Country = country;

                CreateFile(report);
            }

            return true;
        }

        private void CreateFile(Report report)
        {
            // File name with creation time so each file name will be unique
            string fileName = string.Format("Report-{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", DateTime.Now);
            string filePath = @"C:\Reports\" + fileName;
            CreateIfMissing(@"C:\Reports");
            FileInfo fi = new FileInfo(filePath);

            // All data is written to the file
            using (StreamWriter sw = fi.CreateText())
            {
                sw.WriteLine(report.ReportContent);
                sw.WriteLine("Customer phone number: " + report.PhoneNumber);
                sw.WriteLine("Country: " + report.Country);
            }
        }

        private void CreateIfMissing(string path)
        {
            if (!Directory.Exists(path))
            {
                // Try to create the directory.
                Directory.CreateDirectory(path);
            }
        }

        private string? GetDataFromJson(HttpResponseMessage response, string param)
        {
            // Get the specified data from the json file ( without nesting )
            var data = (JObject)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            return data[param]?.Value<string>();
        }

        private string? GetDataFromJson(HttpResponseMessage response, string param1, string param2)
        {
            // Get the specified data from the json file ( without one nesting )
            var data = (JObject)JsonConvert.DeserializeObject(response.Content.ReadAsStringAsync().Result);
            return data[param1]?[param2]?.Value<string>();
        }

        private string BuildUrl(string paramName, string param, string url)
        {
            // Build a URL with parameters
            var builder = new UriBuilder(url);
            builder.Port = -1;
            var query = HttpUtility.ParseQueryString(builder.Query);
            query[paramName] = param;
            builder.Query = query.ToString();
            return builder.ToString();
        }

        private static bool IsPhoneNumber(string number)
        {
            // Check if the phone number is valid with Regex
            return Regex.Match(number, @"\s*(?:\+?(\d{1,3}))?([-. (]*(\d{3})[-. )]*)?((\d{3})[-. ]*(\d{2,4})(?:[-.x ]*(\d+))?)\s*").Success;
        }

        private string FormatJson(string json)
        {
            // Format the Json file so that it can be readable when written to the file
            var deserializedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(deserializedJson, Formatting.Indented);
        }
    }
}
