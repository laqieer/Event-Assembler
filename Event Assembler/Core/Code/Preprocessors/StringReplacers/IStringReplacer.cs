using System.Collections.Generic;
using System.Text;
using Nintenlord.Utility;

namespace Nintenlord.Event_Assembler.Core.Code.Preprocessors.StringReplacers
{
    interface IStringReplacer
    {
        IDictionary<string, IDictionary<int, IMacro>> Values { set; }
        IDictionary<string, IMacro> BuiltInValues { set;  }
        int MaxIter { set; }

        bool Replace(string textToEdit, out string newString);
        CanCauseError<string> Replace(string textToEdit);
        CanCauseError Replace(StringBuilder textToEdit);
    }
}
