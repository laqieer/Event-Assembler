using System;
using System.Collections.Generic;
using System.Text;
using Nintenlord.Event_Assembler.Core.Collections;
using Nintenlord.Utility;
using Nintenlord.Utility.Strings;

namespace Nintenlord.Event_Assembler.Core.Code.Preprocessors.StringReplacers
{
    sealed class NewReplacer : IStringReplacer
    {
        private IDictionary<string, IDictionary<int, IMacro>> values;
        private IDictionary<string, IMacro> builtInValues;
        private int maxIter;
        private int currentIter = 0;

        #region IStringReplacer Members

        public IDictionary<string, IDictionary<int, IMacro>> Values
        {
            set { values = value; }
        }
        public IDictionary<string, IMacro> BuiltInValues
        {
            set { builtInValues = value; }
        }
        public int MaxIter
        {
            set { maxIter = value; }
        }

        public bool Replace(string s, out string newString)
        {
            StringBuilder bldr = new StringBuilder(s);

            var error = this.Replace(bldr);

            bool result = error.CausedError;
            newString = bldr.ToString();
            currentIter--;
            return result;
        }

        public CanCauseError<string> Replace(string textToEdit)
        {
            StringBuilder bldr = new StringBuilder(textToEdit);
            var result = this.Replace(bldr);
            if (result.CausedError)
            {
                return CanCauseError<string>.Error(result.ToString());
            }
            else
            {
                return CanCauseError<string>.NoError(bldr.ToString());
            }
        }

        public CanCauseError Replace(StringBuilder textToEdit)
        {
            if (currentIter == maxIter)
            {
                return CanCauseError.Error("Maximun amount of replacement iterations exceeeded while applying macro.");
            }
            currentIter++;
            IDictionary<int, Tuple<int, IMacro, string[]>> replace = FindMacros(textToEdit);
            
            foreach (var item in replace)
            {
                string[] parameters = item.Value.Item3;
                string[] newParameters = new string[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    var innerResult = Replace(parameters[i]);
                    if (innerResult.CausedError)
                    {
                        return (CanCauseError)innerResult;
                    }
                    else
                    {
                        newParameters[i] = innerResult.Result;
                    }
                }
                string tempString = item.Value.Item2.Replace(newParameters);

                var secondResult = Replace(tempString);//, out tempString
                if (secondResult.CausedError)
                {
                    return (CanCauseError)secondResult;
                }

                string toReplace = textToEdit.Substring(item.Key, item.Value.Item1).ToString();
                textToEdit.Replace(toReplace, secondResult.Result, item.Key, toReplace.Length);
            }
            currentIter--;
            return CanCauseError.NoError;
        }

        private SortedDictionary<int, Tuple<int, IMacro, string[]>> FindMacros(StringBuilder s)
        {
            var replace =
                new SortedDictionary<int, Tuple<int, IMacro, string[]>>(
                    ReverseComparer<int>.Default
                    );

            StringBuilder macroName = new StringBuilder();

            for (int i = 0; i < s.Length; )
            {
                if (DefineCollectionOptimized.IsValidCharacter(s[i]))
                {
                    macroName.Append(s[i]);
                    i++;
                }
                else
                {
                    if (macroName.Length > 0)
                    {
                        GetMacroData(s, replace, ref i, macroName.ToString());
                        macroName.Clear();
                    }
                    else i++;
                }
            }
            int last = s.Length;
            GetMacroData(s, replace, ref last, macroName.ToString());
            return replace;
        }

        private void GetMacroData(StringBuilder s, SortedDictionary<int, Tuple<int, IMacro, string[]>> replace, ref int i, string name)
        {
            IMacro replacer;
            IDictionary<int, IMacro> replacers;
            bool isBuildIn = builtInValues.TryGetValue(name, out replacer);
            bool isValue = values.TryGetValue(name, out replacers);

            int paramLength;
            string[] parameters;
            if (isValue || isBuildIn)
            {
                if (i < s.Length && s[i] == '(')
                {
                    parameters = GetParameters(s, i, out paramLength);
                }
                else
                {
                    paramLength = 0;
                    parameters = new string[0];
                }
                if ((isBuildIn && replacer.IsCorrectAmountOfParameters(parameters.Length)) ||
                    (isValue && replacers.TryGetValue(parameters.Length, out replacer)))
                {
                    replace[i - name.Length] = new Tuple<int, IMacro, string[]>
                        (paramLength + name.Length, replacer, parameters);
                }

                i += paramLength;
            }
            else i++;
        }
        
        #endregion

        private static bool ContainsAt(StringBuilder s, int index, string toSearch)
        {
            bool contains = true;
            if (toSearch.Length > s.Length - index)
                contains = false;
            else
            {
                for (int i = 0; i < toSearch.Length; i++)
                {
                    if (s[index + i] != toSearch[i])
                    {
                        contains = false;
                        break;
                    }
                }
            }
            return contains;
        }

        private static int GetParameterLength(StringBuilder s, int index, out int parameters)
        {
            int depth = 1;
            parameters = 1;
            int i;
            for (i = index + 1; i < s.Length && depth != 0; i++)
            {
                switch (s[i])
                {
                    case '(':
                        depth++;
                        break;
                    case ')':
                        depth--;
                        break;
                    case ',':
                        if (depth == 1)//So that macros as macro parameters won't mess stuff up
                        {
                            parameters++;
                        }
                        break;
                    default:
                        break;
                }
            }
            return i - index;
        }

        private static string[] GetParameters(StringBuilder s, int index)
        {
            int dontCare;
            return GetParameters(s, index, out dontCare);
        }

        private static string[] GetParameters(StringBuilder s, int index, out int lengthInString)
        {
            List<string> parameters = new List<string>();
            int parentDepth = 1;
            int vectorDepth = 0;
            StringBuilder bldr = new StringBuilder();
            int i;
            for (i = index + 1; i < s.Length && parentDepth > 0; i++)
            {
                switch (s[i])
                {
                    case '(':
                        parentDepth++;
                        bldr.Append(s[i]);
                        break;
                    case ')':
                        parentDepth--;
                        bldr.Append(s[i]);
                        break;
                    case '[':
                        vectorDepth++;
                        bldr.Append(s[i]);
                        break;
                    case ']':
                        vectorDepth--;
                        bldr.Append(s[i]);
                        break;
                    case ',':
                        if (parentDepth == 1 && vectorDepth == 0)
                        {
                            parameters.Add(bldr.ToString());
                            bldr.Clear();
                        }
                        else
                        {
                            bldr.Append(s[i]);
                        }
                        break;
                    default:
                        bldr.Append(s[i]);
                        break;
                }
            }
            if (bldr.Length > 0)
            {
                parameters.Add(bldr.ToString(0, bldr.Length - 1));
            }
            lengthInString = i - index;
            for (i = 0; i < parameters.Count; i++)
            {
                parameters[i] = parameters[i].Trim();
            }
            return parameters.ToArray();
        }

        #region IStringReplacer Members


        #endregion
    }
}
