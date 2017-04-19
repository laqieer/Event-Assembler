using System;
using Nintenlord.Utility;
using System.Collections.Generic;
using Nintenlord.Collections;
using System.Text;
using System.IO;

namespace Nintenlord.Event_Assembler.Core.Code.Preprocessors.Directives
{
    internal class IncludeToolEvent : IDirective, INamed<string>, IParameterized
    {

        //Nintenlord.Event_Assembler.Core.IO.IOHelpers.FindFile
        public string Name
        {
            get
            {
                return "inctevent";
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
            //from http://stackoverflow.com/a/206347/1644720

            // Start the child process.
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            // Redirect the output stream of the child process.
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(host.Input.CurrentFile);
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = file;
            string[] passedParams = parameters.GetRange(1);
            /*
            string[] finalParams = new string[passedParams.Length];
            for(int i=0; i<passedParams.Length; i++)
            {
                CanCauseError<string> res = host.DefCol.ApplyDefines(passedParams[i]);
                if (!res.CausedError)
                {
                    finalParams[i] = res.Result;
                } else
                {
                    finalParams[i] = passedParams[i];
                }
                if (finalParams[i].ContainsWhiteSpace())
                    finalParams[i] = "\"" + finalParams[i] + "\"";
            }*/
            
            p.StartInfo.Arguments = passedParams.ToElementWiseString(" ", "", " --to-stdout");
            p.Start();
            // Do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // p.WaitForExit();
            // Read the output stream first and then wait.
            MemoryStream outputBytes = new MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(outputBytes);
            p.WaitForExit();

            byte[] output = outputBytes.GetBuffer().GetRange(0, (int)outputBytes.Length);

            if(output.Length >= 7 && Encoding.ASCII.GetString(output.GetRange(0, 7)) == "ERROR: ")
            {
                return CanCauseError.Error(Encoding.ASCII.GetString(output.GetRange(7)));
            }
            
            string eventCode = System.Text.Encoding.UTF8.GetString(output);
            string[] lines = eventCode.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            
            host.Input.AddNewLines(lines);
            return CanCauseError.NoError;
        }

        private static string getFileName(string toolName)
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
