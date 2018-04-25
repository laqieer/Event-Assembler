// Decompiled with JetBrains decompiler
// Type: Nintenlord.Event_Assembler.Core.Program
// Assembly: Core, Version=9.10.4713.28131, Culture=neutral, PublicKeyToken=null
// MVID: 65F61606-8B59-4B2D-B4B2-32AA8025E687
// Assembly location: E:\crazycolorz5\Dropbox\Unified FE Hacking\ToolBox\EA V9.12.1\Core.exe

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

namespace Nintenlord.Event_Assembler.Core
{
	public static class Program
	{
		public static readonly StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
		private static IDictionary<string, EACodeLanguage> languages;

		public static bool CodesLoaded {
			get {
				return Program.languages != null;
			}
		}

		private static void Main (string[] args)
		{
			if (args.Length == 0)
				return;
			
			TextWriterMessageLog writerMessageLog = new TextWriterMessageLog (Console.Error);

			List<string> flags = new List<string> (args.Length);
			List<string> stringList = new List<string> (args.Length);

			foreach (string str in args) {
				if (str.StartsWith ("-"))
					flags.Add (str.TrimStart ('-'));
				else
					stringList.Add (str);
			}

			string rawsFolder = "Language Raws";
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
			return true;
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
						if (messageLog.ErrorCount == 0) {
							using (Stream stream = (Stream)File.OpenWrite (outputFile))
								changeStream.WriteToFile (stream);
						}
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
			GBAPointerMaker gbaPointerMaker = new GBAPointerMaker ();
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
			List<string> stringList = new List<string> ();
			stringList.Add ("_" + game + "_");
			stringList.Add ("_EA_");
			using (IPreprocessor preprocessor = (IPreprocessor)new Preprocessor (messageLog)) {
				preprocessor.AddReserved (eaCodeLanguage.GetCodeNames ());
				preprocessor.AddDefined ((IEnumerable<string>)stringList.ToArray ());
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
			using (IPreprocessor preprocessor = new Preprocessor (log)) {
				preprocessor.AddReserved (language.GetCodeNames ());
				preprocessor.AddDefined (new string[] { "_" + language.Name + "_", "_EA_" });

				using (IInputStream inputStream = new PreprocessingInputStream (input, preprocessor)) {
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
