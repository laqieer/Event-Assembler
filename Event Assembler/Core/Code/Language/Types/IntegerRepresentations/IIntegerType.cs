using Nintenlord.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nintenlord.Event_Assembler.Core.IO.Input;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Types.IntegerRepresentations
{
    public interface IIntegerType<T>
    {
        T FromInt(int value);
        T FromUInt(uint value);
        T FromLong(long value);
        T FromULong(ulong value);
        T FromBytes(byte[] bytes);
        T FromBytes(byte[] bytes, int offset);
        T FromStream(IInputByteStream stream);

        byte[] GetBytes(T item);

        /// <summary>
        /// Temporary.
        /// </summary>
        /// <param name="item"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        [Obsolete("This should not be used", false)]
        int GetIntValue(T item);

        CanCauseError<T> Parse(string text);
        string ToString(T value, int numberBase);

        T Addition(T first, T second);
        T Subtraction(T first, T second);
        T Minus(T item);

        T Multiplication(T first, T second);
        T Division(T first, T second);
        T Modulus(T first, T second);

        T Complement(T item);
        T BitShift(T first, T second);
        T BitwiseOr(T first, T second);
        T BitwiseAnd(T first, T second);
    }
}
