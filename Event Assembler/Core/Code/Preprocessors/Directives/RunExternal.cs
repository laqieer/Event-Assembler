using System;
using Nintenlord.Utility;
using System.Collections.Generic;
using Nintenlord.Collections;
using System.Text;
using System.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Preprocessors.Directives
{
    internal class RunExternal : IDirective, INamed<string>, IParameterized
    {

        //Nintenlord.Event_Assembler.Core.IO.IOHelpers.FindFile
        public string Name
        {
            get
            {
                return "runext";
            }
        }

        public bool RequireIncluding
        {
            get
            {
                return true;
            }
        }

        public int MinAmountOfParameters
        {
            get
            {
                return 1;
            }
        }

        public int MaxAmountOfParameters
        {
            get
            {
                return -1;
            }
        }

        public CanCauseError Apply(string[] parameters, IDirectivePreprocessor host)
        {
            string file = IO.IOHelpers.FindFile(host.Input.CurrentFile, getFileName(parameters[0]));
            if (file.Length <= 0)
                return CanCauseError.Error("Tool " + parameters[0] + " not found.");

            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(host.Input.CurrentFile);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = file;
            string[] passedParams = parameters.GetRange(1);
            for(int i=0; i<passedParams.Length; i++)
            {
                if (passedParams[i].ContainsWhiteSpace())   
                    passedParams[i] = "\"" + passedParams[i] + "\"";
            }
            p.StartInfo.Arguments = passedParams.ToElementWiseString(" ", "", "");
            p.Start();
            /*MemoryStream outputBytes = new MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(outputBytes);*/
            p.WaitForExit();
            /*
            byte[] output = outputBytes.GetBuffer().GetRange(0, (int)outputBytes.Length);

            if(output.Length >= 7 && Encoding.ASCII.GetString(output.GetRange(0, 7)) == "ERROR: ")
            {
                return CanCauseError.Error(Encoding.ASCII.GetString(output.GetRange(7)));
            }*/
            return CanCauseError.NoError;
        }

        private string getFileName(string toolName)
        {
            switch(Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    return "\"./Tools/" + toolName + "\"";
                default:
                    return "\".\\Tools\\" + toolName + ".exe\"";
            }
        }
    }
}
