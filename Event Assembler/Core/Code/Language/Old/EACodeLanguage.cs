using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nintenlord.Event_Assembler.Core.Code.Language.Assembly;
using Nintenlord.Event_Assembler.Core.Code.Language.Parser;
using Nintenlord.Event_Assembler.Core.Code.Language.Types.IntegerRepresentations;
using Nintenlord.Event_Assembler.Core.Code.Templates;
using Nintenlord.Event_Assembler.Core.IO.Input;
using Nintenlord.Event_Assembler.Core.IO.Logs;
using Nintenlord.Utility.Strings;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Old
{
    /// <summary>
    /// Event assemblers code language
    /// </summary>
    public sealed class EACodeLanguage
    {
        readonly string name;
        readonly ICodeTemplateStorer codeStorage;
        readonly List<string> reservedWords;
        readonly EAExpressionAssembler<int> assembler;
        readonly EACodeLanguageDisassembler<int> disassembler;


        /// <summary>
        /// Name of the langauge.
        /// </summary>
        public string Name
        {
            get { return name; }
        }


        /// <summary>
        /// Priorities for disassembling main codes
        /// </summary>
        public static List<Priority> MainPriorities
        {
            get 
            {
                return new List<Priority>(new Priority[] { Priority.main, Priority.low });
            }
        }
        /// <summary>
        /// Priorities for disassembling unit codes
        /// </summary>
        public static List<Priority> UnitPriorities
        {
            get
            {
                return new List<Priority>(new Priority[] { Priority.unit, Priority.low });
            }
        }
        /// <summary>
        /// Priorities for disassembling ballista codes
        /// </summary>
        public static List<Priority> TrapPriorities
        {
            get
            {
                return new List<Priority>(new Priority[] { Priority.ballista, Priority.low });
            }
        }
        /// <summary>
        /// Priorities for disassembling normal codes
        /// </summary>
        public static List<Priority> NormalPriorities
        {
            get
            {
                return new List<Priority>(new Priority[] { Priority.none, Priority.low });
            }
        }
        
        private const string offsetChanger = "ORG";
        private const string currentOffset = "CURRENTOFFSET";
        private const string messagePrinter = "MESSAGE";
        private const string errorPrinter = "ERROR";
        private const string warningPrinter = "WARNING";
        private const string alignOffset = "ALIGN";

        /// <summary>
        /// Creates a new Event Assembler code language
        /// </summary>
        /// <param name="name">Name of the language</param>
        /// <param name="pointerMaker">Pointer maker for this language</param>
        /// <param name="pointerList">Pointer list of this langauge, String is the name of the 
        /// label to point to, List are the priorities that are pointed to.</param>
        /// <param name="pointerListParameters">Array of amount of pointers per POIN code for pointer list.</param>
        public EACodeLanguage(string name, IPointerMaker<int> pointerMaker,
            Tuple<string, List<Priority>>[][] pointerList,
            ICodeTemplateStorer codeStorer, StringComparer stringComparer)
        {
            this.name = name;
            this.codeStorage = codeStorer;

            //codeStorage.AddCode(new RawCodeTemplate(stringComparer), Priority.low);
            //codeStorage.AddCode(new CodeFillerTemplate(stringComparer), Priority.low);

            //foreach (ICodeTemplate template in codeStorer)
            //{
            //    CodeTemplate template2 = template as CodeTemplate;
            //    if (template2 != null)
            //    {
            //        template2.PointerMaker = pointerMaker;
            //    }
            //}

            reservedWords = new List<string> { 
                offsetChanger,  //Offset changing code
                alignOffset,    //Offset aligning code
                currentOffset,  //Referances current offset
                messagePrinter, //Print message to message box/something
                errorPrinter,   //Print message to message box/something
                warningPrinter  //Print message to message box/something
            };
            var pres = new IntRepresentation();
            this.assembler = new EAExpressionAssembler<int>(
                codeStorage, 
                new TokenParser<int>(StringExtensions.GetValue), 
                new IntRepresentation(), 
                pointerMaker);

            this.disassembler = new EACodeLanguageDisassembler<int>(
                codeStorage, pointerMaker, pointerList, pres);
        }

        public void Assemble(IPositionableInputStream input, BinaryWriter output, ILog log)
        {
            assembler.Assemble(input, output, log);
        }

        public IEnumerable<string[]> Disassemble(byte[] code, int offset, int length, Priority priority, bool addEndingLinest, ILog messageLog)
        {
            return disassembler.Disassemble(code, offset, length, priority, messageLog, addEndingLinest);
        }

        public IEnumerable<string[]> DisassembleChapter(byte[] code, int offset, bool addEndingLinest, ILog messageLog)
        {
            return disassembler.DisassembleChapter(code, offset, messageLog, addEndingLinest);
        }

        public IEnumerable<string[]> DisassembleToEnd(byte[] code, int offset, Priority priority, bool addEndingLinest, ILog messageLog)
        {
            return disassembler.DisassembleToEnd(code, offset, priority, messageLog, addEndingLinest);            
        }



        /// <summary>
        /// Checks if code should be undefinable. Do not raise errors based on this.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        public bool IsReserved(string word)
        {
            if (codeStorage.IsUsedName(word))
            {
                return true;
            }
            foreach (string item in reservedWords)
            {
                if (item.Equals(word))
                {
                    return true;
                }
            }

            return false;
        }

        public IEnumerable<string> GetCodeNames()
        {
            return codeStorage.GetNames().Concat(reservedWords);
        }

        private bool IsValidLableName(string label)
        {
            return !this.IsReserved(label) && 
                label.All(x => char.IsLetterOrDigit(x) | x == '_') &&
                label.Any(x => char.IsLetter(x));
        }


        public override string ToString()
        {
            return name;
        }
    }
}
