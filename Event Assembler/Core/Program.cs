using Nintenlord.Collections;
using Nintenlord.Event_Assembler.Core.Code;
using Nintenlord.Event_Assembler.Core.Code.Language;
using Nintenlord.Event_Assembler.Core.Code.Language.Expression;
using Nintenlord.Event_Assembler.Core.Code.Language.Lexer;
using Nintenlord.Event_Assembler.Core.Code.Preprocessors;
using Nintenlord.Event_Assembler.Core.Code.Templates;
using Nintenlord.Event_Assembler.Core.GBA;
using Nintenlord.Event_Assembler.Core.IO.Input;
using Nintenlord.Event_Assembler.Core.IO.Logs;
using Nintenlord.IO;
using Nintenlord.Parser;
using Nintenlord.Utility;
using Nintenlord.Utility.Strings;
using Nintenlord.Utility.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Nintenlord.Event_Assembler.Core
{
	public static class Program
	{
		public static readonly StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

		private static IDictionary<string, EACodeLanguage> languages;
		private static ProgramRunConfig runConfig;

		public class ProgramRunConfig
		{
			public enum RunExecType
			{
				GenDoc,
				GenNppHighlight,
				GenPNHighlight,
				Assemble,
				Disassemble,
			}

			public RunExecType execType;

			public string language;
			public string rawsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language Raws");
			public string rawsExtension = ".txt";
			public bool   isDirectory = true;
			public bool   addEndGuards = false;
			public string inputFile = null;
			public string outputFile = null;
			public string errorFile = null;
			public string docHeader = null;
			public string docFooter = null;
			public string symbolOutputFile = null;

			public DisassemblyMode disassemblyMode = DisassemblyMode.Block;
			public int             disassemblyOffset = -1;
			public Priority        disassemblyPriority;
			public int             disassemblySize = 0;

			public bool ppSimulation = false;

			public bool         ppDepIgnoreMissingFiles = false;
			public bool         ppDepEnable = false;
			public string       ppDepOutput = null;
			public bool         ppDepAddEmptyTargets = false;
			public List<string> ppDepTargets = new List<string> ();

			public bool TrySetRawsPath(string path) {
				if (File.Exists (path)) {
					this.rawsFolder = path;
					this.isDirectory = false;

					return true;
				}

				if (Directory.Exists (path)) {
					this.rawsFolder = path;
					this.isDirectory = true;

					return true;
				}

				return false;
			}
		}

		public static ProgramRunConfig RunConfig {
			get {
				return Program.runConfig;
			}

			private set {
				Program.runConfig = value;
			}
		}

		public static bool CodesLoaded {
			get {
				return Program.languages != null;
			}
		}

		private static void Main(string[] args) {
			TextWriterMessageLog writerMessageLog = new TextWriterMessageLog (Console.Error);
			StreamWriter logWriter = null;

			if ((Program.RunConfig = ReadProgramArguments (args, (ILog)writerMessageLog)) != null) {
				if (Program.RunConfig.errorFile != null) {
					logWriter = new StreamWriter (Program.RunConfig.errorFile);
					writerMessageLog.Writer = logWriter;
				}

				// doc generation does raw loading on its own, load in a standard manner for everything else

				if (Program.RunConfig.execType != ProgramRunConfig.RunExecType.GenDoc) {
					Program.LoadCodes (
						Program.RunConfig.rawsFolder,
						Program.RunConfig.rawsExtension,
						Program.RunConfig.isDirectory,
						false
					);
				}

				switch (Program.RunConfig.execType) {

				case ProgramRunConfig.RunExecType.GenDoc:
					Program.MakeDoc (
						Program.RunConfig.outputFile,
						Program.RunConfig.rawsFolder,
						Program.RunConfig.rawsExtension,
						Program.RunConfig.isDirectory,
						Program.RunConfig.docHeader,
						Program.RunConfig.docFooter
					);

					break;

				case ProgramRunConfig.RunExecType.GenPNHighlight:
					try {
						HighlightingHelper.GetProgrammersNotepadlanguageDoc (
							(IEnumerable<EACodeLanguage>)Program.languages.Values,
							Program.RunConfig.outputFile
						);
					} catch (Exception e) {
						writerMessageLog.AddError (e.Message);
					}

					break;

				case ProgramRunConfig.RunExecType.GenNppHighlight:
					throw new NotImplementedException ();

				case ProgramRunConfig.RunExecType.Disassemble:
					Program.Disassemble (
						Program.RunConfig.inputFile,
						Program.RunConfig.outputFile,
						Program.RunConfig.language,
						Program.RunConfig.addEndGuards,
						Program.RunConfig.disassemblyMode,
						Program.RunConfig.disassemblyOffset,
						Program.RunConfig.disassemblyPriority,
						Program.RunConfig.disassemblySize,
						(ILog)writerMessageLog
					);

					break;

				case ProgramRunConfig.RunExecType.Assemble:
					Program.Assemble (
						Program.RunConfig.inputFile,
						Program.RunConfig.outputFile,
						Program.RunConfig.language,
						(ILog)writerMessageLog,
						Program.RunConfig.symbolOutputFile
					);

					break;

				}
			}

			writerMessageLog.PrintAll ();
			writerMessageLog.Clear ();

			if (logWriter != null)
				logWriter.Dispose ();
		}

		private static void OldMain (string[] args)
		{
			if (args.Length == 0) {
				Program.PrintUsage ();
				return;
			}
			
			TextWriterMessageLog writerMessageLog = new TextWriterMessageLog (Console.Error);

			List<string> flags = new List<string> (args.Length);
			List<string> stringList = new List<string> (args.Length);

			// ProgramRunConfig config = ReadProgramArguments (args, (ILog)writerMessageLog);

			foreach (string str in args) {
				if (str.StartsWith ("-"))
					flags.Add (str.TrimStart ('-'));
				else
					stringList.Add (str);
			}

			string rawsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language Raws");
			string rawsExtension = ".txt";
			bool isDirectory = true;
			bool addEndGuards = false;
			string inputFile = null;
			string outputFile = null;
			string errorFile = null;
			string docHeader = null;
			string docFooter = null;
			string symbolOutputFile = null;

			Program.HandleFlags (flags, (ILog)writerMessageLog, ref rawsFolder, ref rawsExtension, ref isDirectory, ref addEndGuards, ref inputFile, ref outputFile, ref errorFile, ref docHeader, ref docFooter, ref symbolOutputFile);
			StreamWriter streamWriter = (StreamWriter)null;

			if (errorFile != null) {
				streamWriter = new StreamWriter (errorFile);
				writerMessageLog.Writer = (TextWriter)streamWriter;
			}
			if (Program.stringComparer.Compare (stringList [0], "doc") == 0) {
				Program.MakeDoc (outputFile, rawsFolder, rawsExtension, isDirectory, docHeader, docFooter);
			} else if (inputFile != null) {
				if (Program.stringComparer.Compare (stringList [0], "plusplus") == 0)
					throw new NotImplementedException ();
				if (Program.stringComparer.Compare (stringList [0], "prognotepad") == 0) {
					Program.LoadCodes (rawsFolder, rawsExtension, isDirectory, false);
					try {
						HighlightingHelper.GetProgrammersNotepadlanguageDoc ((IEnumerable<EACodeLanguage>)Program.languages.Values, outputFile);
					} catch (Exception ex) {
						writerMessageLog.AddError (ex.Message);
					}
				} else {
					Program.LoadCodes (rawsFolder, rawsExtension, isDirectory, false);
					if (Program.languages.ContainsKey (stringList [1])) {
						if (Program.stringComparer.Compare (stringList [0], "A") == 0)
							Program.Assemble (inputFile, outputFile, stringList [1], (ILog)writerMessageLog, symbolOutputFile);
						else if (Program.stringComparer.Compare (stringList [0], "D") == 0) {
							DisassemblyMode result1;
							if (stringList [2].TryGetEnum<DisassemblyMode> (out result1)) {
								int offset;
								if (stringList [3].TryGetValue (out offset)) {
									int size = 0;
									Priority result2 = Priority.none;
									if (result1 != DisassemblyMode.Structure && !stringList [4].TryGetEnum<Priority> (out result2))
										writerMessageLog.AddError (stringList [4] + " is not a valid priority");
									else if (result1 == DisassemblyMode.Block && (!stringList [5].TryGetValue (out size) || size < 0))
										writerMessageLog.AddError (stringList [5] + " is not a valid size");
									else
										Program.Disassemble (inputFile, outputFile, stringList [1], addEndGuards, result1, offset, result2, size, (ILog)writerMessageLog);
								} else
									writerMessageLog.AddError (stringList [3] + " is not a valid number");
							} else
								writerMessageLog.AddError (stringList [2] + "is not a valid disassembly mode");
						} else
							writerMessageLog.AddError (stringList [0] + "is not a valid action to do");
					} else
						writerMessageLog.AddError (stringList [1] + "is not a valid language");
				}
			}

			writerMessageLog.PrintAll ();
			writerMessageLog.Clear ();

			if (streamWriter == null)
				return;

			streamWriter.Dispose ();

		}

		private static void PrintUsage() {
			// TODO?
		}

		private static ProgramRunConfig ReadProgramArguments(string[] args, ILog log) {
			IEnumerator<string> it = args.AsEnumerable().GetEnumerator ();

			if (!it.MoveNext ()) {
				Program.PrintUsage ();
				return null;
			}

			ProgramRunConfig result = new ProgramRunConfig ();

			// First argument is always what kind of exec we want

			switch (it.Current) {

			case "doc":
				result.execType = ProgramRunConfig.RunExecType.GenDoc;
				break;

			case "plusplus":
				result.execType = ProgramRunConfig.RunExecType.GenNppHighlight;
				break;

			case "prognotepad":
				result.execType = ProgramRunConfig.RunExecType.GenPNHighlight;
				break;

			case "A":
			case "assemble":
				result.execType = ProgramRunConfig.RunExecType.Assemble;
				break;

			case "D":
			case "disassemble":
				result.execType = ProgramRunConfig.RunExecType.Disassemble;
				break;

			}

			// For Assembling & Disassembling, second argument is what game we're doing that for

			switch (result.execType) {

			case ProgramRunConfig.RunExecType.Assemble:
			case ProgramRunConfig.RunExecType.Disassemble:
				if (!it.MoveNext ()) {
					log.AddError ("You need to specify a game for which to (dis)assemble!");
					return null;
				}

				result.language = it.Current;
				break;

			}

			// From now on, the argument order doesn't matter

			while (it.MoveNext ()) {
				// -addEndGuards
				if (it.Current.Equals ("-addEndGuards")) {
					if (result.execType != ProgramRunConfig.RunExecType.Disassemble)
						log.AddWarning ("`-addEndGuards` flag passed in non-disassembly mode. Ignoring.");
					else
						result.addEndGuards = true;

					continue;
				}

				// -raws <file>
				if (it.Current.Equals ("-raws")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-raws` passed without specifying a path.");
						return null;
					}

					if (!result.TrySetRawsPath (it.Current)) {
						log.AddError("File or folder `{0}` doesn't exist.", it.Current);
						return null;
					}

					continue;
				}

				// -raws:<file>
				if (it.Current.StartsWith ("-raws:")) {
					string path = it.Current.Substring ("-raws:".Length);

					if (!result.TrySetRawsPath (path)) {
						log.AddError("File or folder `{0}` doesn't exist.", path);
						return null;
					}

					continue;
				}

				// -rawsExt <ext>
				if (it.Current.Equals ("-rawsExt")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-rawsExt` passed without specifying an extension.");
						return null;
					}

					if (it.Current.ContainsAnyOf (Path.GetInvalidFileNameChars ())) {
						log.AddError("`{0}` isn't valid as a file extension.", it.Current);
						return null;
					}

					result.rawsExtension = it.Current;
					continue;
				}

				// -rawsExt:<ext>
				if (it.Current.StartsWith ("-rawsExt:")) {
					string ext = it.Current.Substring ("-rawsExt:".Length);

					if (ext.ContainsAnyOf (Path.GetInvalidFileNameChars ())) {
						log.AddError("`{0}` isn't valid as a file extension.", ext);
						return null;
					}

					result.rawsExtension = ext;
					continue;
				}

				// -input <file>
				if (it.Current.Equals ("-input")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-input` passed without specifying a file.");
						return null;
					}

					if (!File.Exists (it.Current)) {
						log.AddError ("File `{0}` doesn't exist.", it.Current);
						return null;
					}

					result.inputFile = it.Current;
					continue;
				}

				// -input:<file>
				if (it.Current.StartsWith ("-input:")) {
					string file = it.Current.Substring ("-input:".Length);

					if (!File.Exists (file)) {
						log.AddError ("File `{0}` doesn't exist.", file);
						return null;
					}

					result.inputFile = file;
					continue;
				}

				// -output <file>
				if (it.Current.Equals ("-output")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-output` passed without specifying a file.");
						return null;
					}

					if (!IsValidFileName (it.Current)) {
						log.AddError ("`{0}` isn't a valid file name.", it.Current);
						return null;
					}

					result.outputFile = it.Current;
					continue;
				}

				// -output:<file>
				if (it.Current.StartsWith ("-output:")) {
					string file = it.Current.Substring ("-output:".Length);

					if (!IsValidFileName (file)) {
						log.AddError ("`{0}` isn't a valid file name", file);
						return null;
					}

					result.outputFile = file;
					continue;
				}

				// -symOutput <file>
				if (it.Current.Equals ("-symOutput")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-symOutput` passed without specifying a file.");
						return null;
					}

					if (!IsValidFileName (it.Current)) {
						log.AddError ("`{0}` isn't a valid file name.", it.Current);
						return null;
					}

					result.symbolOutputFile = it.Current;
					continue;
				}

				// -symOutput:<file>
				if (it.Current.StartsWith ("-symOutput:")) {
					string file = it.Current.Substring ("-symOutput:".Length);

					if (!IsValidFileName (file)) {
						log.AddError ("`{0}` isn't a valid file name", file);
						return null;
					}

					result.symbolOutputFile = file;
					continue;
				}

				// -error <file>
				if (it.Current.Equals ("-error")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-error` passed without specifying a file.");
						return null;
					}

					if (!IsValidFileName (it.Current)) {
						log.AddError ("`{0}` isn't a valid file name.", it.Current);
						return null;
					}

					result.errorFile = it.Current;
					continue;
				}

				// -error:<file>
				if (it.Current.StartsWith ("-error:")) {
					string file = it.Current.Substring ("-error:".Length);

					if (!IsValidFileName (file)) {
						log.AddError ("`{0}` isn't a valid file name.", file);
						return null;
					}

					result.errorFile = file;
					continue;
				}

				// -docHeader <file>
				if (it.Current.Equals ("-docHeader")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-docHeader` passed without specifying a file.");
						return null;
					}

					if (!File.Exists (it.Current)) {
						log.AddError ("File `{0}` doesn't exist.", it.Current);
						return null;
					}

					result.docHeader = it.Current;
					continue;
				}

				// -docHeader:<file>
				if (it.Current.StartsWith ("-docHeader:")) {
					string file = it.Current.Substring ("-docHeader:".Length);

					if (!File.Exists (file)) {
						log.AddError ("File `{0}` doesn't exist.", file);
						return null;
					}

					result.docHeader = file;
					continue;
				}

				// -docFooter <file>
				if (it.Current.Equals ("-docFooter")) {
					if (!it.MoveNext ()) {
						log.AddError ("`-docFooter` passed without specifying a file.");
						return null;
					}

					if (!File.Exists (it.Current)) {
						log.AddError ("File `{0}` doesn't exist.", it.Current);
						return null;
					}

					result.docFooter = it.Current;
					continue;
				}

				// -docFooter:<file>
				if (it.Current.StartsWith ("-docFooter:")) {
					string file = it.Current.Substring ("-docFooter:".Length);

					if (!File.Exists (file)) {
						log.AddError ("File `{0}` doesn't exist.", file);
						return null;
					}

					result.docFooter = file;
					continue;
				}

				// special disassembly-specific parameters
				if (result.execType == ProgramRunConfig.RunExecType.Disassemble) {
					DisassemblyMode dMode;

					if (it.Current.TryGetEnum<DisassemblyMode> (out dMode)) {
						result.disassemblyMode = dMode;
						continue;
					}

					int dOffset;

					if (result.disassemblyOffset < 0 && it.Current.TryGetValue (out dOffset)) {
						result.disassemblyOffset = dOffset;
						continue;
					}

					Priority dPriority;

					if (it.Current.TryGetEnum<Priority> (out dPriority)) {
						result.disassemblyPriority = dPriority;
						continue;
					}

					int dSize;

					if (result.disassemblyMode == DisassemblyMode.Block && it.Current.TryGetValue (out dSize)) {
						result.disassemblySize = dSize;
						continue;
					}
				}

				// special assembly-specific parameters
				if (result.execType == ProgramRunConfig.RunExecType.Assemble) {
					if (it.Current.Equals ("-M")) {
						result.ppSimulation = true;
						result.ppDepEnable = true;
						continue;
					}

					if (it.Current.Equals ("-MD")) {
						result.ppDepEnable = true;
						continue;
					}

					if (it.Current.Equals ("-MG")) {
						result.ppDepIgnoreMissingFiles = true;
						continue;
					}

					if (it.Current.Equals ("-MP")) {
						result.ppDepAddEmptyTargets = true;
						continue;
					}

					// -MF <file>
					if (it.Current.Equals ("-MF")) {
						if (!it.MoveNext ()) {
							log.AddError ("`-MF` passed without specifying a file.");
							return null;
						}

						if (!IsValidFileName (it.Current)) {
							log.AddError ("`{0}` isn't a valid file name.", it.Current);
							return null;
						}

						result.ppDepOutput = it.Current;
						continue;
					}

					// -MF:<file>
					if (it.Current.StartsWith ("-MF:")) {
						string file = it.Current.Substring ("-MF:".Length);

						if (!IsValidFileName (file)) {
							log.AddError ("`{0}` isn't a valid file name.", file);
							return null;
						}

						result.ppDepOutput = file;
						continue;
					}

					// -MT <name>
					if (it.Current.Equals ("-MT")) {
						if (!it.MoveNext ()) {
							log.AddError ("`-MT` passed without specifying a target.");
							return null;
						}


						if (it.Current.Length <= 0)
							result.ppDepTargets.Clear ();
						else
							result.ppDepTargets.Add (it.Current);
						
						continue;
					}

					// -MT:<name>
					if (it.Current.StartsWith ("-MT:")) {
						string name = it.Current.Substring ("-MT:".Length);

						if (name.Length <= 0)
							result.ppDepTargets.Clear ();
						else
							result.ppDepTargets.Add (name);

						continue;
					}
				}

				log.AddWarning ("Unhandled parameter `{0}`. Ignoring.", it.Current);
			}

			return result;
		}

		private static void HandleFlags (List<string> flags, ILog messageLog, ref string rawsFolder, ref string rawsExtension, ref bool isDirectory, ref bool addEndGuards, ref string inputFile, ref string outputFile, ref string errorFile, ref string docHeader, ref string docFooter, ref string symbolOutputFile)
		{
			foreach (string flag in flags) {
				int length = flag.IndexOf (':');

				string str1;
				string str2;

				if (length >= 0) {
					str1 = flag.Substring (0, length);
					str2 = flag.Substring (length + 1);
				} else {
					str1 = flag;
					str2 = "";
				}

				switch (str1) {

				case "addEndGuards":
					addEndGuards = true;
					continue;

				case "raws":
					if (File.Exists (str2)) {
						rawsFolder = str2;
						isDirectory = false;
						continue;
					}
					if (Directory.Exists (str2)) {
						rawsFolder = str2;
						isDirectory = true;
						continue;
					}
					messageLog.AddError ("File or folder " + str2 + " doesn't exist.");
					continue;

				case "rawsExt":
					if (!str2.ContainsAnyOf (Path.GetInvalidFileNameChars ())) {
						rawsExtension = str2;
						continue;
					}
					messageLog.AddError ("Extension " + str2 + " is not valid.");
					continue;

				case "input":
					if (File.Exists (str2)) {
						inputFile = str2;
						continue;
					}
					messageLog.AddError ("File " + str2 + " doesn't exist.");
					continue;

				case "output":
					if (IsValidFileName (str2)) {
						outputFile = str2;
						continue;
					}
					messageLog.AddError ("Name " + str2 + " isn't valid for a file.");
					continue;

				case "symOutput":
					if (IsValidFileName (str2)) {
						symbolOutputFile = str2;
						continue;
					}
					messageLog.AddError ("Name " + str2 + " isn't valid for a file.");
					continue;

				case "error":
					if (Program.IsValidFileName (str2)) {
						errorFile = str2;
						continue;
					}
					messageLog.AddError ("Name " + str2 + " isn't valid for a file.");
					continue;

				case "docHeader":
					if (Program.IsValidFileName (str2)) {
						docHeader = str2;
						continue;
					}
					messageLog.AddError ("Name " + str2 + " isn't valid for a file.");
					continue;

				case "docFooter":
					if (Program.IsValidFileName (str2)) {
						docFooter = str2;
						continue;
					}
					messageLog.AddError ("Name " + str2 + " isn't valid for a file.");
					continue;

				default:
					messageLog.AddError ("Flag " + str1 + " doesn't exist.");
					continue;

				}
			}
		}

		private static bool IsValidFileName (string name)
		{
			return !name.ContainsAnyOf (Path.GetInvalidPathChars ());
		}

		public static void Assemble (string inputFile, string outputFile, string languageName, ILog messageLog)
		{
			Assemble (inputFile, outputFile, languageName, messageLog, null);
		}

		public static void Assemble (string inputFile, string outputFile, string languageName, ILog messageLog, string symbolOutputFile)
		{
			TextReader input;
			bool flag;

			if (inputFile != null) {
				input = (TextReader)File.OpenText (inputFile);
				flag = true;
			} else {
				input = Console.In;
				flag = false;
			}

			EACodeLanguage language = Program.languages [languageName];

			if (outputFile != null) {
				if (File.Exists (outputFile) && File.GetAttributes (outputFile).HasFlag ((Enum)FileAttributes.ReadOnly)) {
					messageLog.AddError ("outputFile is read-only.");
				} else {
					ChangeStream changeStream = new ChangeStream ();

					using (BinaryWriter output = new BinaryWriter ((Stream)changeStream)) {
						Program.Assemble (language, input, output, messageLog, symbolOutputFile);

						if (messageLog.ErrorCount == 0)
							using (Stream stream = (Stream)File.OpenWrite (outputFile))
								changeStream.WriteToFile (stream);
					}
				}
			} else
				messageLog.AddError ("outputFile needs to be specified for assembly.");

			if (!flag)
				return;
			
			input.Close ();
		}

		public static void Disassemble (string inputFile, string outputFile, string languageName, bool addEndGuards, DisassemblyMode mode, int offset, Priority priority, int size, ILog messageLog)
		{
			if (!File.Exists (inputFile))
				messageLog.AddError ("File " + inputFile + " doesn't exist.");
			else if (File.Exists (outputFile) && File.GetAttributes (outputFile).HasFlag ((Enum)FileAttributes.ReadOnly)) {
				messageLog.AddError ("Output cannot be written to. It is read-only.");
			} else {
				EACodeLanguage eaCodeLanguage = Program.languages [languageName];
				byte[] code = File.ReadAllBytes (inputFile);

				if (offset > code.Length) {
					messageLog.AddError ("Offset is larger than size of file.");
				} else {
					if (size <= 0 || size + offset > code.Length)
						size = code.Length - offset;

					IEnumerable<string[]> strArrays;
					string[] lines;

					switch (mode) {

					case DisassemblyMode.Block:
						strArrays = eaCodeLanguage.Disassemble (code, offset, size, priority, addEndGuards, messageLog);
						lines = CoreInfo.DefaultLines (eaCodeLanguage.Name, Path.GetFileName (inputFile), offset, new int? (size));
						break;
					
					case DisassemblyMode.ToEnd:
						strArrays = eaCodeLanguage.DisassembleToEnd (code, offset, priority, addEndGuards, messageLog);
						lines = CoreInfo.DefaultLines (eaCodeLanguage.Name, Path.GetFileName (inputFile), offset, new int? ());
						break;
					
					case DisassemblyMode.Structure:
						strArrays = eaCodeLanguage.DisassembleChapter (code, offset, addEndGuards, messageLog);
						lines = CoreInfo.DefaultLines (eaCodeLanguage.Name, Path.GetFileName (inputFile), offset, new int? ());
						break;
					
					default:
						throw new ArgumentException ();
					
					}

					if (messageLog.ErrorCount != 0)
						return;

					using (StreamWriter streamWriter = new StreamWriter (outputFile)) {
						streamWriter.WriteLine ();
						streamWriter.WriteLine (Program.Frame (lines, "//", 1));
						streamWriter.WriteLine ();
						foreach (string[] strArray in strArrays)
							streamWriter.WriteLine (((IEnumerable<string>)strArray).ToElementWiseString<string> (" ", "", ""));
					}
				}
			}
		}

		public static void LoadCodes (string rawsFolder, string extension, bool isDirectory, bool collectDocCodes)
		{
			Program.languages = (IDictionary<string, EACodeLanguage>)new Dictionary<string, EACodeLanguage> ();
			LanguageProcessor languageProcessor = new LanguageProcessor (collectDocCodes, new TemplateComparer (), stringComparer);
			IPointerMaker pointerMaker = (IPointerMaker)new GBAPointerMaker ();

			if (isDirectory)
				languageProcessor.ProcessCode (rawsFolder, extension);
			else
				languageProcessor.ProcessCode (rawsFolder);

			foreach (KeyValuePair<string, ICodeTemplateStorer> language in (IEnumerable<KeyValuePair<string, ICodeTemplateStorer>>)languageProcessor.Languages) {
				Tuple<string, List<Priority>>[][] pointerList;

				switch (language.Key) {

				case "FE6":
					pointerList = FE6CodeLanguage.PointerList;
					break;
				
				case "FE7":
					pointerList = FE7CodeLanguage.PointerList;
					break;
				
				case "FE8":
					pointerList = FE8CodeLanguage.PointerList;
					break;
				
				default:
					throw new NotSupportedException ("Language " + language.Key + " not supported.");
				
				}

				ICodeTemplateStorer codeStorer = language.Value;
				EACodeLanguage eaCodeLanguage = new EACodeLanguage (language.Key, pointerMaker, pointerList, codeStorer, Program.stringComparer);
				Program.languages [language.Key] = eaCodeLanguage;
			}
		}

		public static void MakeDoc (string output, string rawsFolder, string extension, bool isDirectory, string header, string footer)
		{
			LanguageProcessor languageProcessor = new LanguageProcessor (true, (IComparer<ICodeTemplate>)new TemplateComparer (), Program.stringComparer);

			if (isDirectory)
				languageProcessor.ProcessCode (rawsFolder, extension);
			else
				languageProcessor.ProcessCode (rawsFolder);

			using (StreamWriter text = File.CreateText (output)) {
				if (header != null) {
					text.WriteLine (File.ReadAllText (header));
					text.WriteLine ();
				}

				languageProcessor.WriteDocs ((TextWriter)text);

				if (footer == null)
					return;

				text.WriteLine (File.ReadAllText (footer));
				text.WriteLine ();
			}
		}

		public static void Preprocess (string originalFile, string outputFile, string game, ILog messageLog)
		{
			EACodeLanguage eaCodeLanguage = Program.languages [game];

			using (IPreprocessor preprocessor = (IPreprocessor)new Preprocessor (messageLog)) {
				preprocessor.AddReserved (eaCodeLanguage.GetCodeNames ());
				preprocessor.AddDefined (new string[] { "_" + game + "_", "_EA_" });

				using (StreamReader streamReader = File.OpenText (originalFile)) {
					using (IInputStream inputStream = (IInputStream)new PreprocessingInputStream ((TextReader)streamReader, preprocessor)) {
						StringWriter stringWriter = new StringWriter ();

						while (true) {
							string str = inputStream.ReadLine ();

							if (str != null)
								stringWriter.WriteLine (str);
							else
								break;
						}

						messageLog.AddMessage ("Processed code:\n" + stringWriter.ToString () + "\nEnd processed code");
					}
				}
			}
		}

		private static void Assemble (EACodeLanguage language, TextReader input, BinaryWriter output, ILog log, string symbolOutputFile)
		{
			using (IDirectivePreprocessor preprocessor = new Preprocessor (log)) {
				preprocessor.AddReserved (language.GetCodeNames ());
				preprocessor.AddDefined (new string[] { "_" + language.Name + "_", "_EA_" });

				DependencyMakingIncludeListener depMaker = null;

				if (Program.RunConfig.ppDepEnable) {
					depMaker = new DependencyMakingIncludeListener ();
					preprocessor.IncludeListener = depMaker;
				}

				using (IInputStream inputStream = new PreprocessingInputStream (input, preprocessor)) {
					if (Program.RunConfig.ppSimulation) {
						// Only preprocess to null output
						while (inputStream.ReadLine () != null)
							;
					} else {
						EAExpressionAssembler assembler = new EAExpressionAssembler (language.CodeStorage, new TokenParser<int> (new Func<string, int> (StringExtensions.GetValue)));
						assembler.Assemble (inputStream, output, log);

						try {
							// Outputting global symbols to another file

							if (symbolOutputFile != null) {
								if (File.Exists (symbolOutputFile))
									File.Delete (symbolOutputFile);

								using (FileStream fileStream = File.OpenWrite (symbolOutputFile))
								using (StreamWriter symOut = new StreamWriter (fileStream))
									foreach (KeyValuePair<string, int> symbol in assembler.GetGlobalSymbols())
										symOut.WriteLine ("{0}={1}", symbol.Key, symbol.Value.ToHexString ("$"));
							}
						} catch (Exception e) {
							log.AddError (e.ToString ());
						}
					}
				}

				if (depMaker != null) {
					try {
						depMaker.GenerateMakeDependencies (log);
					} catch (Exception e) {
						log.AddError (e.ToString ());
					}
				}
			}
		}

		private static string Frame (string[] lines, string toFrameWith, int padding)
		{
			int num = 0;

			for (int index = 0; index < lines.Length; ++index)
				num = Math.Max (num, lines [index].Length);

			string str1 = toFrameWith.Repeat (padding * 2 + toFrameWith.Length * 2 + num);
			string str2 = toFrameWith + " ".Repeat (padding * 2 + num) + toFrameWith;
			string str3 = " ".Repeat (padding);

			StringBuilder stringBuilder = new StringBuilder ();

			stringBuilder.AppendLine (str1);
			stringBuilder.AppendLine (str2);

			foreach (string line in lines)
				stringBuilder.AppendLine (toFrameWith + str3 + line.PadRight (num, ' ') + str3 + toFrameWith);

			stringBuilder.AppendLine (str2);
			stringBuilder.AppendLine (str1);

			return stringBuilder.ToString ();
		}
	}
}
