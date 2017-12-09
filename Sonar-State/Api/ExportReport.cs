using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using Ionic.Zip;

namespace Sonar_State.Api
{
    public class ExportReport
    {
        public int Total { get; set; }
        public string P { get; set; }
        public int PS { get; set; }
        public Paging Paging { get; set; }

        private List<Issues> issues;
        public List<Issues> Issues {
            get {
                if (issues == null)
                {
                    issues = new List<Api.Issues>();
                }
                return issues;
            }
            set {
                issues = value;
            }
        }

        private List<Components> components;
        public List<Components> Components
        {
            get
            {
                if (components == null)
                {
                    components = new List<Api.Components>();
                }
                return components;
            }
            set
            {
                components = value;
            }
        }

        public static string UrlApiIssuesSearch
        {
            get
            {
                return string.Concat(System.Configuration.ConfigurationManager.AppSettings["UrlSonarQubeServer"].Trim('/'), System.Configuration.ConfigurationManager.AppSettings["UrlApiIssuesSearch"]);
            }
        }

        public static void SaveStatus(string path, ExportReport api, SonarQubeApi project)
        {
            var category = new List<string>
            {
                "BLOCKER",
                "CRITICAL",
                "MAJOR",
                "MINOR",
                "INFO",
            };
            ExportIssuesSummary(category, path, api, project);
            ExportIssuesCategory(category, path,api);
        }
         
        private static string Header()
        {
            string html = string.Empty;
            html += "<header>";
            html += "   <style>\n";
            html += "       table {\n";
            html += "           border-collapse: collapse;\n";
            html += "           width: 100%;";
            html += "       }\n";
            html += "       table, th, td {\n";
            html += "           border: 1px solid #f2f2f2;\n";
            html += "           padding: 5px;\n";
            html += "           font-family:Arial, sans-serif;";
            html += "           font-size:12px;";
            html += "       }\n";
            html += "       th {\n";
            //html += "           height: 50px;\n";
            html += "           font-weight:bold;";
            html += "           color:#fff;";
            html += "           background-color:#01579B;";
            html += "           font-family:Arial, sans-serif;";
            html += "           font-size:12px;";
            html += "           text-align:center;";
            html += "       }\n";
            html += "   </style>\n";
            html += "</header>";

            return html;
        }

        private static void ExportIssuesSummary(List<string> category, string path, ExportReport api,SonarQubeApi project)
        {
            string html = string.Empty;
            html += Header();

            html += "<body>\n"; 
            html += string.Format("<center><h1>{0}</h1></center>\n", project.Name);
            html += string.Format("<h4>SonarQube Key: <a href=\"{0}&id={1}\">{2}</a></h4>\n", SonarQubeApi.UrlDashboard, project.Key, project.Key);
            html += string.Format("<h4>SVN Version: {0}</h4>\n", project.Version);
            html += string.Format("<h4>Date Execution: {0}</h4>\n", project.Date.ToString("yyyy-MM-dd HH:mm"));
            html += "	<table style=\"border-collapse:collapse;border-spacing:0;border-color:#999\">";
            html += "	   <tr>";
            html += "		  <td colspan=\"7\" style=\"text-align:center;\">" + string.Format("<b>{0}</b>", project.Name) + "</td>";
            html += "	   </tr>";
            html += "	   <tr>";
            html += "		  <th rowspan=\"2\">Revisi&oacute;n</th>";
            html += "		  <th colspan=\"5\">An&aacute;lisis de Reglas de C#</th>";
            html += "		  <th rowspan=\"2\">Cobertura</th>";
            html += "	   </tr>";
            html += "	   <tr>";
            html += "		  <th>Bloqueantes</th>";
            html += "		  <th>Cr&iacute;ticas</th>";
            html += "		  <th>Mayores</th>";
            html += "		  <th>Menores</th>";
            html += "		  <th>Informaci&oacute;n</th>";
            html += "	   </tr>";
            html += "	   <tr>";
            html += "		  <td>" + string.Format("<a href=\"Blocker.HTML\">{0}</a>", project.Version) + "</td>";
            html += "		  <td style=\"text-align:center;\">" + string.Format("<a href=\"Blocker.HTML\">{0:N0}</a>", project.MetricBlocker().Val) + "</td>";
            html += "		  <td style=\"text-align:center;\">" + string.Format("<a href=\"Critical.HTML\">{0:N0}</a>", project.MetricCritical().Val) + "</td>";
            html += "		  <td style=\"text-align:center;\">" + string.Format("<a href=\"Major.HTML\">{0:N0}</a>", project.MetricMajor().Val) + "</td>";
            html += "		  <td style=\"text-align:center;\">" + string.Format("<a href=\"Minor.HTML\">{0:N0}</a>", project.MetricMinor().Val) + "</td>";
            html += "		  <td style=\"text-align:center;\">" + string.Format("<a href=\"Info.HTML\">{0:N0}</a>", project.MetricInfo().Val) + "</td>";
            html += "		  <td style=\"text-align:center;\"><b>" + string.Format("{0:N0}", project.MetricCoverage().Val) + "</b></td>";
            html += "	   </tr>";
            html += "	</table>";

            if (project.MetricsErrors().Count() > 0)
            {
                html += "	<br/><table style=\"border-collapse:collapse;border-spacing:0;border-color:#999\">";
                html += "	   <tr>";
                html += "		  <td colspan=\"4\" style=\"text-align:center;\"><h3>HAY GAP</h3></td>";
                html += "	   </tr>";
                html += "	   <tr>";
                html += "		  <th>Tipo</th>";
                html += "		  <th>Regla</th>";
                html += "		  <th>Actual</th>";
                html += "		  <th>Causa</th>";
                html += "	   </tr>";
                foreach (var item in project.MetricsErrors())
                {
                    html += "	   <tr>";
                    html += "		  <td>" + string.Format("{0:N0}", item.Alert) + "</td>";
                    html += "		  <td>" + string.Format("{0:N0}", item.Name) + "</td>";
                    html += "		  <td>" + string.Format("{0:N0}", item.Val) + "</td>";
                    html += "		  <td>" + string.Format("{0:N0}", item.Alert_text) + "</td>";
                    html += "	   </tr>";
                }
                html += "	</table>";
            }

            html += "</body>\n";

            //crear archivo
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            Console.WriteLine(string.Format("Create file {0}", "Index"));
            System.IO.File.WriteAllText(System.IO.Path.Combine(path, string.Format("{0}.html", "INDEX")), html);
        }

        private static void ExportIssuesCategory(List<string> category, string path, ExportReport api)
        {
            foreach (var categoryItem in category)
            {
                string html = string.Empty;
                html += Header();

                html += "<body>\n";

                //menu
                html += "<table style=\"background-color: #f2f2f2\">\n";
                html += "   <tr>\n";
                html += "<td><center><b><a href=\"INDEX.HTML\">HOME</a></b></center></td>";
                foreach (var item in category)
                {
                    var count = api.Issues.Where(x => x.Severity == item).Count();
                    html += string.Format("<td><center><b><a href=\"{0}\">{1} [ {2} items ]</a></b></center></td>", item + ".html", item, count);
                }
                html += "   \n</tr>\n";
                html += "</table>\n";

                //

                //titulo
                html += "<br/>";
                html += string.Format("<center><h1>{0} Issues</h1></center>", categoryItem);
                var issues = api.Issues.Where(x => x.Severity == categoryItem).ToList();

                //tabla
                html += "<table>\n";
                html += "<tr><th>#</th><th>File</th><th>Line</th><th>Description</th><th>Technical Debt</th></tr>\n";
                int index = 1;
                foreach (var issuesItem in issues)
                {
                    var componente = api.Components.FirstOrDefault(x => x.Id == issuesItem.ComponentId).Path;
                    html += string.Format("<tr><td>{4}</td><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td></tr>\n", componente, issuesItem.Line, issuesItem.Message, issuesItem.Debt, index);
                    index++;
                }
                html += "</table>\n";
                html += "</body>\n";

                //crear archivo
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }
                Console.WriteLine(string.Format("Create file {0}", categoryItem));
                System.IO.File.WriteAllText(System.IO.Path.Combine(path, string.Format("{0}.html", categoryItem)), html);
            }
        }

        public static bool CreateZip(string source, string pathZip)
        {
            if (File.Exists(pathZip))
            {
                File.Delete(pathZip);
            }

            if (Directory.Exists(source))
            {
                using (ZipFile zip = new ZipFile())
                {
                    // add this map file into the "images" directory in the zip archive
                    zip.AddDirectory(source);
                    // add the report into a different directory in the archive
                    if (!Directory.Exists(Path.GetDirectoryName(pathZip)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(pathZip));
                    }
                    zip.Save(pathZip);
                    return true;
                }
            }
            return false;
        }

        public static ExportReport GetStatus(string key)
        {
            ExportReport result = new ExportReport();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(UrlApiIssuesSearch);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            // List data response.
            int indexPage = 1;
            int totalPage = 1;
            do
            {
                Console.WriteLine("\nGet Api Rest: Page {0}",indexPage);
                string parameters = string.Format("?componentRoots={0}&amp;pageIndex={1}", key, indexPage);
                Console.WriteLine(string.Concat(UrlApiIssuesSearch, parameters));
                HttpResponseMessage response = client.GetAsync(parameters).Result;  // Blocking call!
                if (response.IsSuccessStatusCode)
                {
                    // Parse the response body. Blocking!
                    var json = response.Content.ReadAsStringAsync().Result;
                    var dataObjects = response.Content.ReadAsAsync<ExportReport>().Result;
                    if(dataObjects.Issues.Count()>0 || dataObjects.Components.Count>0)
                    {
                        indexPage++;
                        totalPage++;

                        foreach (var item in dataObjects.Issues)
                        {
                            result.Issues.Add(item);
                        }
                        foreach (var item in dataObjects.Components)
                        {
                            result.Components.Add(item);
                        }
                    }
                    else
                    {
                        indexPage++;
                    }
                }
                else
                {
                    throw new Exception(string.Format("{0}: {1}", (int)response.StatusCode, response.ReasonPhrase));
                }
            } while (indexPage<=totalPage);
            Console.WriteLine();
            return result;
        }

    }

    public class Paging
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
    }

    public class Issues
    {
        public string Key { get; set; }
        public string Severity { get; set; }
        public string Component { get; set; }
        public long ComponentId { get; set; }
        public string Project { get; set; }
        public int Line { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string Debt { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime UpdateDate { get; set; }
    }

    public class Components
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public string Uuid { get; set; }
        public bool Enabled { get; set; }
        public string Qualifier { get; set; }
        public string Name { get; set; }
        public string LongName { get; set; }
        public string Path { get; set; }
        public long ProjectId { get; set; }
        public long SubProjectId { get; set; }
    }
}
