using Nintenlord.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nintenlord.Utility.Primitives;
using Nintenlord.Utility.Strings;

namespace Nintenlord.Event_Assembler.Core.Code.Language.Types.IntegerRepresentations
{
    sealed class IntRepresentation : IIntegerType<int>
    {
        public int FromInt(int value)
        {
            return value;
        }

        public int FromUInt(uint value)
        {
            return (int)value;
        }

        public int FromLong(long value)
        {
            return (int)value;
        }

        public int FromULong(ulong value)
        {
            return (int)value;
        }

        public CanCauseError<int> Parse(string text)
        {
            int result;
            if (text.TryGetValue(out result))
            {
                return result;
            }
            else
            {
                return CanCauseError<int>.Error("Improperly formatted integer.");
            }
        }

        public int Addition(int first, int second)
        {
            return first + second;
        }

        public int Subtraction(int first, int second)
        {
            return first - second;
        }

        public int Multiplication(int first, int second)
        {
            return first * second;
        }

        public int Division(int first, int second)
        {
            return first / second;
        }

        public int Modulus(int first, int second)
        {
            return first % second;
        }

        public int BitShift(int first, int second)
        {
            return first << second;
        }

        public int BitwiseOr(int first, int second)
        {
            return first | second;
        }

        public int BitwiseAnd(int first, int second)
        {
            return first & second;
        }

        public int Minus(int item)
        {
            return -item;
        }

        public int Complement(int item)
        {
            return ~item;
        }
        
        public byte[] GetBytes(int item)
        {
            return BitConverter.GetBytes(item);
        }
        
        public int FromBytes(byte[] bytes)
        {
            return BitConverter.ToInt32(bytes, 0);
        }

        public int FromBytes(byte[] bytes, int offset)
        {
            return BitConverter.ToInt32(bytes, offset);
        }


        public string ToString(int value, int numberBase)
        {
            switch (numberBase)
            {
                case 2:
                    return value.ToBinString("b");
                case 10:
                    return value.ToString();
                case 16:
                    return value.ToHexString("0x");
                default:
                    throw new ArgumentException("numberBase");
            }
        }


        public int GetIntValue(int item)
        {
            return item;
        }


        public int FromStream(IO.Input.IInputByteStream stream)
        {
            return stream.ReadInt32();
        }
    }
}
