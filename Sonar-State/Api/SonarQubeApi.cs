using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Sonar_State.Api
{
    public class SonarQubeApi
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public string Version { get; set; }
        public string Uuid { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime Date { get; set; }

        public List<Metrics> Msr { get; set; }

        public static string UrlDashboard
        {
            get
            {
                return string.Concat(System.Configuration.ConfigurationManager.AppSettings["UrlSonarQubeServer"].Trim('/'), System.Configuration.ConfigurationManager.AppSettings["UrlDashboard"]);
            }
        }
        public static string UrlApiResource
        {
            get
            {
                return string.Concat(System.Configuration.ConfigurationManager.AppSettings["UrlSonarQubeServer"].Trim('/'), System.Configuration.ConfigurationManager.AppSettings["UrlApiResource"]);
            }
        }
        public static string UrlApiResourceParameters
        {
            get
            {
                return System.Configuration.ConfigurationManager.AppSettings["UrlApiResourceParameters"].Trim('/');
            }
        }

        public IList<Metrics> MetricsErrors()
        {
            return Msr.Where(x => x.Alert == "ERROR").ToList();
        }
        
        public Metrics Metric(string key)
        {
            var metrics= Msr.Where(x => x.Key == key).FirstOrDefault();
            if (metrics == null)
            {
                metrics = new Metrics();
            }
            return metrics;
        }
        public Metrics MetricBlocker()
        {
            return Metric("blocker_violations");
        }
        public Metrics MetricCritical()
        {
            return Metric("critical_violations");
        }
        public Metrics MetricMajor()
        {
            return Metric("major_violations");
        }
        public Metrics MetricMinor()
        {
            return Metric("minor_violations");
        }
        public Metrics MetricInfo()
        {
            return Metric("info_violations");
        }
        public Metrics MetricCoverage()
        {
            return Metric("coverage");
        }

        public static SonarQubeApi GetStatus(string key)
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(UrlApiResource);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            var parameters = string.Format("{0}&amp;resource={1}", UrlApiResourceParameters, key);
            Console.WriteLine(string.Concat(UrlApiResource,parameters,"\n"));
            HttpResponseMessage response = client.GetAsync(parameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<IEnumerable<SonarQubeApi>>().Result;
                return dataObjects.FirstOrDefault();
            }
            else
            {
                throw new Exception(string.Format("{0}: {1}", (int)response.StatusCode, response.ReasonPhrase));
            }
        }

    }
    public class Metrics
    {
        public string Name { get; set; }
        public string Key { get; set; }
        public decimal Val { get; set; }
        public string Alert { get; set; }
        public string Alert_text { get; set; }
    }
}
