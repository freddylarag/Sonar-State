using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sonar_State
{
    public class InputParameters
    {
        public string BCProjectKey { get; set; }
        public string BCVersion { get; set; }
        public string ProjectKey { get; set; }
        public string Version { get; set; }
        public bool Help { get; set; }
        public string WorkSpace { get; set; }
        public string FileProperty { get; set; }
        public string Emails { get; set; }

        public InputParameters(string[] input)
        {
            //Help
            string h = input.FirstOrDefault(x => x.Contains("/?"));
            if (string.IsNullOrWhiteSpace(h))
            {
                Help = false;
            }
            else
            {
                Help = true;
            }

            if (!Help)
            {
                WorkSpace = Parser(input, "-w");
                if (string.IsNullOrWhiteSpace(WorkSpace))
                {
                    WorkSpace = Path.GetFullPath(@".");
                }

                ProjectKey = Parser(input, "-p");
                Version = Parser(input, "-v");
                Emails = Parser(input, "-e");

                if (string.IsNullOrWhiteSpace(ProjectKey))
                {
                    throw new Exception("Parametros -p son incorrectos");
                }
                
            }

        }

        private string Parser(string[] parameters, string buscar)
        {
            for (int i = 0; i < parameters.Count(); i++)
            {
                if (parameters[i].ToLower().Trim() == buscar.Trim())
                {
                    if ((i + 1) < parameters.Count())
                    {
                        return parameters[i + 1];
                    }
                }
            }

            return "";
        }

    }
}
