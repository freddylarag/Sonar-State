using Sonar_State.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Sonar_State
{
    class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("\n");
            Console.WriteLine("====================================================================");
            Console.WriteLine("                 SONAR-STATE v5.2     05/09/2017                    ");
            Console.WriteLine("  Descripcion: Consulta el estado de un proyecto SonarQube          ");
            Console.WriteLine("====================================================================");
            Console.WriteLine();

            InputParameters input = null;

            try
            {
                input = new InputParameters(args);
                if (input.Help)
                {
                    ShowHelp();
                }
                else
                {
                    //Comparar
                    var project = SonarQubeApi.GetStatus(input.ProjectKey);
                    Comparacion(project, input);

                    //Crear informe
                    var ws = ExportReport.GetStatus(project.Key);
                    string reportPath = Path.Combine(input.WorkSpace, "Report");
                    string zipPath=Path.Combine(input.WorkSpace,string.Format("SonarQube_{0}.zip", project.Version));
                    ExportReport.SaveStatus(reportPath, ws,project);
                    ExportReport.CreateZip(reportPath, zipPath);

                    //Notificar
                    var notificar = new Notificar();
                    notificar.Enviar(project, input.Emails, new List<string> { zipPath });

                    Console.WriteLine("\n");
                    Console.WriteLine("====================================================================");
                    Console.WriteLine("                         PROCESO FINALIZADO                         ");
                    Console.WriteLine("====================================================================");
                    Environment.ExitCode = 0;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex, input);
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("\nPresione una tecla...");
                Console.ReadKey();
            }
        }


        static int Comparacion(SonarQubeApi project, InputParameters input)
        {
            int result;
            if (project != null && project.Msr!=null)
            {
                Console.WriteLine("SonarQube Key:     {0}", project.Key);
                Console.WriteLine("SonarQube Name:    {0}", project.Name);
                Console.WriteLine("SonarQube Version: {0}", project.Version);
                Console.WriteLine("SonarQube Date:    {0}", project.Date.ToString("dd/MM/yyyy HH:mm:ss"));
                Console.WriteLine();
                Console.WriteLine("-------------------------------------------------------------------------------");
                Console.WriteLine("|     Revision     |Bloqueantes| Criticas  |  Mayores  |  Menores  |Cobertura |");
                Console.WriteLine("-------------------------------------------------------------------------------");
                StringBuilder summary = new StringBuilder();
                summary.Append(string.Format("|{0}", project.Version.PadRight(18)));
                summary.Append(string.Format("|{0}", project.MetricBlocker().Val.ToString("N0").PadLeft(11)));
                summary.Append(string.Format("|{0}", project.MetricCritical().Val.ToString("N0").PadLeft(11)));
                summary.Append(string.Format("|{0}", project.MetricMajor().Val.ToString("N0").PadLeft(11)));
                summary.Append(string.Format("|{0}", project.MetricMinor().Val.ToString("N0").PadLeft(11)));
                summary.Append(string.Format("|{0}|", project.MetricCoverage().Val.ToString("N2").PadLeft(10)));
                Console.WriteLine(summary.ToString());
                Console.WriteLine("-------------------------------------------------------------------------------");
                if (project.MetricsErrors().Count()>0)
                {
                    ConsoleColor colorOk = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("");
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.WriteLine("|                                HAY GAP                                      |");
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.WriteLine("| Tipo    | Regla                 |Actual| Causa                              |");
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    foreach (var item in project.MetricsErrors())
                    {
                        Console.WriteLine("|{0}|{1}|{2}|{3}|", item.Alert?.PadRight(9), item.Name.PadRight(23), item.Val.ToString("N0").PadLeft(6), item.Alert_text.PadRight(36));
                    }
                    Console.WriteLine("-------------------------------------------------------------------------------");

                    Console.ForegroundColor = colorOk;
                    result = 1;
                }
                else
                {
                    ConsoleColor colorOk = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("");
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.WriteLine("|                              NO HAY GAP                                     |");
                    Console.WriteLine("-------------------------------------------------------------------------------");
                    Console.ForegroundColor = colorOk;
                    result = 0;
                }

                Console.WriteLine("\nCuadro de Mando en SonarQube:");
                Console.WriteLine(string.Format("{0}&id={1}",SonarQubeApi.UrlDashboard,project.Key));
                return result;
            }
            else
            {
                throw new Exception("No se encontraron resultados para los parametros indicados.");
            }
        }

        static string Signo(int valor)
        {
            string result = "";
            if (valor > 0)
            {
                result = ("+" + valor.ToString("N0"));
            }
            else
            {
                result = valor.ToString("N0");
            }
            return result;
        }

        private static void ShowHelp()
        {
            Console.WriteLine();
            Console.WriteLine("Uso: SONAR-STATE [-p CurrentProjectKey] [-v CurrentProjectVersion]");
            Console.WriteLine("            [-w Workspace] [/?]");
            Console.WriteLine();
            Console.WriteLine("Requeridos:");
            Console.WriteLine("  -p CurrentProjectKey         Proyecto actual");
            Console.WriteLine("  -v CurrentProjectVersion     Version actual");
            Console.WriteLine();
            Console.WriteLine("Opcionales:");
            Console.WriteLine("  -e Emails                    Correos separados por comas");
            Console.WriteLine();
            Console.WriteLine("  /?   Help");
        }

        static void ShowError(Exception ex, InputParameters input)
        {
            ConsoleColor colorError = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n");
            Console.WriteLine("====================================================================");
            Console.WriteLine("====================================================================");
            Console.WriteLine("                   PROCESO FINALIZADO CON ERROR                     ");
            Console.WriteLine();
            Console.WriteLine("Error: {0}", ex.Message);
            Console.WriteLine();
            Console.WriteLine("====================================================================");
            Console.WriteLine("====================================================================");
            Environment.ExitCode = 1;
            Console.ForegroundColor = colorError;
        }
    }
}
