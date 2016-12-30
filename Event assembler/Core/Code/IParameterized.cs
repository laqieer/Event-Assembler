using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nintenlord.Event_Assembler.Core.Code
{
  public interface IParameterized
  {
        /// <summary>
        /// Minimum amount of parameters accepted or -1 if no minimun exists.
        /// </summary>
        int MinAmountOfParameters { get; }
        /// <summary>
        /// Maximum amount of parameters accepted or -1 if no maximun exists.
        /// </summary>
        int MaxAmountOfParameters { get; }
  }
}
