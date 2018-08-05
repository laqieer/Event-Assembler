using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nintenlord.Event_Assembler.Core.Code.Language;
using Nintenlord.Event_Assembler.Core.Code.Language.Old;
using Nintenlord.Utility;
using Nintenlord.Utility.Strings;

namespace Nintenlord.Event_Assembler.Core.IO.Input
{
    internal struct CommandLineArgs
    {
        //parameters
        public string operation;
        public string language;
        public DisassemblyMode? disassemblyMode;
        public int? offset;
        public Priority? priority;
        public int? size;

        //Flags
        public string rawsFolder;
        public string rawsExtension;
        public bool isDirectory;
        public bool addEndGuards;
        public string inputFile;
        public string outputFile;
        public string errorFile;
        public string docHeader;
        public string docFooter;

        int paramCount;

        public CanCauseError SetArgs(string[] args)
        {
            paramCount = 0;
            foreach (var arg in args)
            {
                if (!arg.StartsWith("-"))
                {
                    var parameter = arg;
                    var res = HandleParameter(parameter);
                    if (res.CausedError)
                    {
                        return res;
                    }
                }
                else
                {
                    var flag = arg.TrimStart('-');

                    int index = flag.IndexOf(':');
                    string flagName;
                    string option;
                    if (index >= 0)
                    {
                        flagName = flag.Substring(0, index);
                        option = flag.Substring(index + 1);
                    }
                    else
                    {
                        flagName = flag;
                        option = "";
                    } 

                    var res = HandleFlags(flagName, option);
                    if (res.CausedError)
                    {
                        return res;
                    }
                }
            }
            return CanCauseError.NoError;
        }

        private CanCauseError HandleParameter(string parameter)
        {
            switch (paramCount)
            {
                case 0: operation = parameter; break;
                case 1: language = parameter; break;
                case 2:
                    DisassemblyMode disassemblyMode;
                    if (!parameter.TryGetEnum(out disassemblyMode))
                        return CanCauseError.Error("{0} is not a valid disassembly mode.", parameter);
                    this.disassemblyMode = disassemblyMode;
                    break;
                case 3:
                    int offset;
                    if (!parameter.TryGetValue(out offset))
                        return CanCauseError.Error("{0} is not a valid offset.", parameter);
                    this.offset = offset;
                    break;
                case 4:
                    Priority priority;
                    if (!parameter.TryGetEnum(out priority))
                        return CanCauseError.Error("{0} is not a valid priority.", parameter);
                    this.priority = priority;
                    break;
                case 5:
                    int size;
                    if (!parameter.TryGetValue(out size) || size < 0)
                        return CanCauseError.Error("{0} is not a valid size.", parameter);
                    this.size = size;
                    break;
                default:
                    return CanCauseError.Error("Too many parameters.");
            }
            paramCount++;
            return CanCauseError.NoError;
        }

        private CanCauseError HandleFlags(string flagName, string option)
        {
            switch (flagName)
            {
                case "addEndGuards":
                    addEndGuards = true;
                    break;
                case "raws":
                    if (File.Exists(option))
                    {
                        rawsFolder = option;
                        isDirectory = false;
                    }
                    else if (Directory.Exists(option))
                    {
                        rawsFolder = option;
                        isDirectory = true;
                    }
                    else
                    {
                        return CanCauseError.Error("File or folder {0} doesn't exist.", option);
                    }
                    break;
                case "rawsExt":
                    if (!option.ContainsAnyOf(Path.GetInvalidFileNameChars()))
                    {
                        rawsExtension = option;
                    }
                    else
                    {
                        return CanCauseError.Error("Extension {0} is not valid.", option);
                    }
                    break;
                case "input":
                    inputFile = option;
                    break;
                case "output":
                    outputFile = option;
                    break;
                case "error":
                    if (IsValidFileName(option))
                    {
                        errorFile = option;
                    }
                    else
                    {
                        return CanCauseError.Error("Name {0} isn't valid for a file.", option);
                    }
                    break;
                case "docHeader":
                    if (IsValidFileName(option))
                    {
                        docHeader = option;
                    }
                    else
                    {
                        return CanCauseError.Error("Name {0} isn't valid for a file.", option);
                    }
                    break;
                case "docFooter":
                    if (IsValidFileName(option))
                    {
                        docFooter = option;
                    }
                    else
                    {
                        return CanCauseError.Error("Name {0} isn't valid for a file.", option);
                    }
                    break;
                default:
                    return CanCauseError.Error("Flag {0} doesn't exist.", flagName);
            }
            return CanCauseError.NoError;
        }

        private bool IsValidFileName(string name)
        {
            return true;
        }
    }
}
