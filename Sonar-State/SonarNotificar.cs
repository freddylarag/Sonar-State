using Sonar_State.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;

namespace Sonar_State
{
    public class Notificar
    {
        public void Enviar(SonarQubeApi project, string emails, List<string> pathAttachments)
        {
            string subject = string.Empty;
            bool errorProceso = false;
            string body = Mensaje(project, out subject, out errorProceso);
            Send(emails, subject, body, true, errorProceso,pathAttachments);
        }

        public string EstadoBODY
        {
            get
            {
                return " style=\"font-family:Arial, sans-serif;font-size:12px;color:#444;\" ";
            }
        }
        public string EstadoHead
        {
            get
            {
                return " style=\"font-weight:bold;color:#fff;background-color:#01579B;font-family:Arial, sans-serif;font-size:12px;text-align:center;border:1px solid #999;overflow:hidden;word-break:normal;padding:5px 5px;\" ";
            }
        }
        public string EstadoItem
        {
            get
            {
                return " style=\"background-color:#FFFFFF;font-family:Arial, sans-serif;font-size:12px;color:#444;text-align:center;border:1px solid #999;overflow:hidden;word-break:normal;padding:5px 5px;\" ";
            }
        }
        public string EstadoFooter
        {
            get
            {
                return " style=\"background-color:#BBDEFB;font-family:Arial, sans-serif;font-size:12px;color:#444;text-align:center;border:1px solid #999;overflow:hidden;word-break:normal;padding:5px 5px;\" ";
            }
        }
        public string EstadoOk
        {
            get
            {
                return " style=\"color:#4CAF50;font-family:Arial, sans-serif;font-size:12px;\" ";
            }
        }
        public string EstadoGap
        {
            get
            {
                return " style=\"color:#FF3D00;font-family:Arial, sans-serif;font-size:12px;\" ";
            }
        }
        private string Footer()
        {
            string html = "";
            html += "	<br/><br/>";
            html += "	<span>Atte.</span><br/>";
            html += "	<br/><span style=\"font-size:11px;\">Nota: Correo enviado de forma autom&aacute;tica</span><br/>";
            html += "	<br/><br/>";
            return html;
        }

        private string Mensaje(SonarQubeApi project, out string subject, out bool errorProceso)
        {
            string html = "";
            html += "<div " + EstadoBODY + ">";
            html += "	<p>Estimados(as):<br/><br/></p>";
            html += Body(project, out subject, out errorProceso);
            html += Footer();
            html += "</div>";
            return html;
        }

        public string Body(SonarQubeApi project, out string subject, out bool errorProceso, bool incluirGlosaEncabezado = true)
        {
            string html = "";
            try
            {
                string anchoCelda = "100";
                subject = string.Format("Revisión de Código - C# - {0}", project.Name);
                if (incluirGlosaEncabezado)
                {
                    html += "<p>";
                    if (project.MetricsErrors().Count()>0)
                    {
                        html += "	<b " + EstadoGap + ">La aplicaci&oacute;n esta en Rojo, por lo tanto esta nueva funcionalidad no est&aacute; apta para pasar a producci&oacute;n.</b>";
                    }
                    else
                    {
                        html += "	<b " + EstadoOk + ">La aplicaci&oacute;n esta en Verde, por lo tanto esta nueva funcionalidad est&aacute; apta para pasar a producci&oacute;n.</b>";
                    }
                    html += "	</p>";
                }

                html += "	<table style=\"border-collapse:collapse;border-spacing:0;border-color:#999\">";
                html += "	   <tr>";
                html += "		  <td " + EstadoItem + " colspan=\"6\">" + string.Format("<b>{0}</b>", project.Name) + "</td>";
                html += "	   </tr>";
                html += "	   <tr>";
                html += "		  <th " + EstadoHead + " rowspan=\"2\" width=\"" + anchoCelda + "\">Revisi&oacute;n</th>";
                html += "		  <th " + EstadoHead + " colspan=\"4\">An&aacute;lisis de Reglas de C#</th>";
                html += "		  <th " + EstadoHead + " rowspan=\"2\" width=\"" + anchoCelda + "\">Cobertura</th>";
                html += "	   </tr>";
                html += "	   <tr>";
                html += "		  <td width=\"" + anchoCelda + "\" " + EstadoHead + ">Bloqueantes</td>";
                html += "		  <td width=\"" + anchoCelda + "\" " + EstadoHead + ">Cr&iacute;ticas</td>";
                html += "		  <td width=\"" + anchoCelda + "\" " + EstadoHead + ">Mayores</td>";
                html += "		  <td width=\"" + anchoCelda + "\" " + EstadoHead + ">Menores</td>";
                html += "	   </tr>";
                html += "	   <tr>";
                html += "		  <td " + EstadoItem + ">" + string.Format("<a href=\"{0}&id={1}\">{2}</a>", SonarQubeApi.UrlDashboard, project.Key,project.Version) + "</td>";
                html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", project.MetricBlocker().Val) + "</td>";
                html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", project.MetricCritical().Val) + "</td>";
                html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", project.MetricMajor().Val) + "</td>";
                html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", project.MetricMinor().Val) + "</td>";
                html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", project.MetricCoverage().Val) + "</b></td>";
                html += "	   </tr>";
                html += "	</table>";

                //Base de Comparacion
                if (project.MetricsErrors().Count() >0)
                {
                    html += "	<br/><table style=\"border-collapse:collapse;border-spacing:0;border-color:#999\">";
                    html += "	   <tr>";
                    html += "		  <td " + EstadoItem + " colspan=\"4\">HAY GAP</td>";
                    html += "	   </tr>";
                    html += "	   <tr>";
                    html += "		  <th " + EstadoHead + ">Tipo</th>";
                    html += "		  <th " + EstadoHead + ">Regla</th>";
                    html += "		  <th " + EstadoHead + ">Actual</th>";
                    html += "		  <th " + EstadoHead + ">Causa</th>";
                    html += "	   </tr>";
                    foreach (var item in project.MetricsErrors())
                    {
                        html += "	   <tr>";
                        html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", item.Alert) + "</td>";
                        html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", item.Name) + "</td>";
                        html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", item.Val) + "</td>";
                        html += "		  <td " + EstadoItem + ">" + string.Format("{0:N0}", item.Alert_text) + "</td>";
                        html += "	   </tr>";
                    }
                    html += "	</table>";
                }
                errorProceso = false;
            }
            catch
            {
                subject = "Revisi&oacute;n de C&oacute;digo Autom&aacute;tico - Error";
                html = string.Format("<h3>Error al intentar obtener los resultados del proyecto Sonar {0} y Revisi&oacute;n {1}.</h3>", project.Key, project.Version);
                html += string.Format("<br/><a href=\"{0}&id={1}\">Ver revisi&oacute;n en el Dashboard</a>.", SonarQubeApi.UrlDashboard, project.Key);
                errorProceso = true;
            }

            return html;
        }

        private void Send(string to, string subject, string message, bool isHtml, bool errorProceso,List<string> pathAttachments)
        {
            Console.WriteLine("\nEnviando notificaciones por correo...");
            using (MailMessage mail = new MailMessage())
            {
                if (!errorProceso)
                {
                    if (string.IsNullOrWhiteSpace(to))
                    {
                        string[] destinatarios = ConfigurationManager.AppSettings["DestinatariosFijos"].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in destinatarios)
                        {
                            try
                            {
                                mail.To.Add(item);
                            }
                            catch
                            {
                            }
                        }
                    }
                    else
                    {
                        string[] destinatarios = to.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in destinatarios)
                        {
                            try
                            {
                                mail.To.Add(item);
                            }
                            catch
                            {
                            }
                        }

                        string[] cc = ConfigurationManager.AppSettings["DestinatariosFijos"].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var item in cc)
                        {
                            try
                            {
                                mail.CC.Add(item);
                            }
                            catch
                            {
                            }
                        }
                    }
                }
                else
                {
                    string[] cc = ConfigurationManager.AppSettings["DestinatariosError"].Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in cc)
                    {
                        mail.To.Add(item);
                    }
                }
                mail.IsBodyHtml = isHtml;
                mail.Subject = subject;
                mail.Body = message;

                foreach (var item in pathAttachments)
                {
                    if (System.IO.File.Exists(item))
                    {
                        mail.Attachments.Add(new Attachment(item));
                    }
                }
                using (SmtpClient client = new SmtpClient())
                {
                    client.Send(mail);
                }
            }
        }
    }
}