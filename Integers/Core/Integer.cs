using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Integers
{
    public enum Sign
    {
        Minus = -1,
        Plus = 1
    }

    public class Integer
    {
        private readonly List<byte> digits;
        public bool IsZero => digits.Count == 0 || digits.All(x => x == 0) || this == Zero;
        public static Integer Zero => new(0);
        public static Integer One => new Integer(1);

        private int Size => digits.Count;

        private Sign Sign { get; set; }
        
        private Integer(Sign sign, List<byte> bytes)
        {
            Sign = Sign.Plus;
            Sign = sign;
            digits = bytes.ToList();
            RemoveNulls();
        }

        public Integer(string s)
        {
            digits = new();
            Sign = Sign.Plus;
            if (s.StartsWith("-"))
            {
                Sign = Sign.Minus;
                s = s.Substring(1);
            }
            else Sign = Sign.Plus;

            foreach (var c in s.Reverse())
            {
                digits.Add(Convert.ToByte(c.ToString()));
            }

            RemoveNulls();
        }

        public Integer(uint x)
        {
            digits = new();
            Sign = Sign.Plus;
            Sign = Sign.Plus;
            digits.AddRange(GetBytes(x));
        }

        private Integer(int x)
        {
            digits = new List<byte>();
            Sign = Sign.Plus;
            if (x < 0)
            {
                Sign = Sign.Minus;
            }

            digits.AddRange(GetBytes((uint) Math.Abs(x)));
        }

        private static List<byte> GetBytes(uint num)
        {
            var bytes = new List<byte>();
            do
            {
                bytes.Add((byte) (num % 10));
                num /= 10;
            } while (num > 0);

            return bytes;
        }

        private void RemoveNulls()
        {
            var index = digits.Count - 1;
            while (index >= 0 && digits[index] == 0)
            {
                digits.RemoveAt(index);
                index--;
            }
        }

        private static Integer Exp(byte val, int exp)
        {
            var bigInt = Zero;
            bigInt.SetByte(exp, val);
            bigInt.RemoveNulls();
            return bigInt;
        }

        

        private byte GetByte(int i) => i < Size ? digits[i] : (byte) 0;

        private void SetByte(int i, byte b)
        {
            while (digits.Count <= i)
            {
                digits.Add(0);
            }

            digits[i] = b;
        }

        public override string ToString()
        {
            RemoveNulls();

            if (IsZero) return "0";
            var builder = new StringBuilder(Sign == Sign.Plus ? "" : "-");

            for (var i = digits.Count - 1; i >= 0; i--)
            {
                builder.Append(Convert.ToString(digits[i]));
            }

            return builder.ToString();
        }


        private static Integer Add(Integer a, Integer b)
        {
            var digits = new List<byte>();
            var maxLength = Math.Max(a.Size, b.Size);
            var reduce = 0;
            for (var i = 0; i < maxLength; i++)
            {
                var sum = (byte) (a.GetByte(i) + b.GetByte(i) + reduce);
                reduce = sum / 10;
                digits.Add((byte) (sum % 10));
            }

            if (reduce > 0)
                digits.Add((byte) reduce);

            return new Integer(a.Sign, digits);
        }

        private static Integer Subtract(Integer a, Integer b)
        {
            var digits = new List<byte>();
            var max = Zero;
            var min = Zero;
            var compare = Comparison(a, b, ignoreSign: true);

            switch (compare)
            {
                case -1:
                    min = a;
                    max = b;
                    break;
                case 0:
                    return Zero;
                case 1:
                    min = b;
                    max = a;
                    break;
            }

            var maxLength = Math.Max(a.Size, b.Size);

            var reduce = 0;
            for (var i = 0; i < maxLength; i++)
            {
                var sub = max.GetByte(i) - min.GetByte(i) - reduce;
                if (sub < 0)
                {
                    sub += 10;
                    reduce = 1;
                }
                else
                {
                    reduce = 0;
                }

                digits.Add((byte) sub);
            }

            return new Integer(max.Sign, digits);
        }

        private static Integer Multiply(Integer a, Integer b)
        {
            var result = Zero;

            for (var i = 0; i < a.Size; i++)
            {
                for (int j = 0, reduce = 0; (j < b.Size) || (reduce > 0); j++)
                {
                    var sum = result.GetByte(i + j) + a.GetByte(i) * b.GetByte(j) + reduce;
                    result.SetByte(i + j, (byte) (sum % 10));
                    reduce = sum / 10;
                }
            }

            result.Sign = a.Sign == b.Sign ? Sign.Plus : Sign.Minus;
            return result;
        }

        private static Integer Div(Integer a, Integer b)
        {
            var result = Zero;
            var current = Zero;

            for (var i = a.Size - 1; i >= 0; i--)
            {
                current += Exp(a.GetByte(i), i);

                var x = 0;
                var left = 0;
                var right = 10;
                while (left <= right)
                {
                    var middle = (left + right) / 2;
                    var cur = b * Exp((byte) middle, i);
                    if (cur <= current)
                    {
                        x = middle;
                        left = middle + 1;
                    }
                    else
                    {
                        right = middle - 1;
                    }
                }

                result.SetByte(i, (byte) (x % 10));
                var reduce = b * Exp((byte) x, i);
                current = current - reduce;
            }

            result.RemoveNulls();
            result.Sign = a.Sign == b.Sign ? Sign.Plus : Sign.Minus;
            return result;
        }

        private static Integer Mod(Integer a, Integer b)
        {
            var result = Zero;

            for (var i = a.Size - 1; i >= 0; i--)
            {
                result += Exp(a.GetByte(i), i);

                var x = 0;
                var left = 0;
                var right = 10;

                while (left <= right)
                {
                    var middle = (left + right) >> 1;
                    var current = b * Exp((byte) middle, i);
                    if (current <= result)
                    {
                        x = middle;
                        left = middle + 1;
                    }
                    else
                    {
                        right = middle - 1;
                    }
                }

                result -= b * Exp((byte) x, i);
            }

            result.RemoveNulls();

            result.Sign = a.Sign == b.Sign ? Sign.Plus : Sign.Minus;
            return result;
        }

        public static Integer ModPow(Integer value, Integer power, Integer module)
        {
            var binaryValue = ConvertToBinary(power);

            var arr = new Integer[binaryValue.Count];
            arr[0] = value;
            for (var i = 1; i < binaryValue.Count; i++)
                arr[i] = arr[i - 1] * arr[i - 1] % module;

            var multiplication = One;
            var zero = Zero;
            for (var j = 0; j < binaryValue.Count; j++)
                if (binaryValue[j] > zero)
                    multiplication *= binaryValue[j] * arr[j];

            return multiplication % module;
        }

        public static Integer Pow(Integer value, Integer power)
        {
            var two = new Integer(2);
            if (power.IsZero)
                return One;
            if (power == One)
                return value;
            if (power % two == One)
                return value * Pow(value, power - One);
            var b = Pow(value, power / two);
            return b * b;
        }

        private static List<Integer> ConvertToBinary(Integer value)
        {
            var copy = new Integer(value.Sign, value.digits.ToList());
            var two = new Integer(2);
            var result = new List<Integer>();
            while (!copy.IsZero)
            {
                result.Add(copy % two);
                copy /= two;
            }

            return result;
        }

        public static Integer GetModInverse(Integer a, Integer n)
        {
            var gdc = GreatestCommonDivisor(a, n, out var x, out var _);
            if (gdc != One)
                return Zero;
            return (x % n + n) % n;
        }

        public static Integer GreatestCommonDivisor(Integer number, Integer modulo, out Integer x,
                                                    out Integer y)
        {
            if (number.IsZero)
            {
                x = Zero;
                y = One;
                return modulo;
            }

            var d = GreatestCommonDivisor(modulo % number, number, out var x1, out var y1);
            x = y1 - (modulo / number) * x1;
            y = x1;
            return d;
        }

        private static int Comparison(Integer a, Integer b, bool ignoreSign = false)
        {
            return CompareSign(a, b, ignoreSign);
        }

        private static int CompareSign(Integer a, Integer b, bool ignoreSign = false)
        {
            if (!ignoreSign)
            {
                if (a.Sign < b.Sign)
                {
                    return -1;
                }

                if (a.Sign > b.Sign)
                {
                    return 1;
                }
            }

            return CompareSize(a, b);
        }

        private static int CompareSize(Integer a, Integer b)
        {
            if (a.Size < b.Size)
            {
                return -1;
            }
            else if (a.Size > b.Size)
            {
                return 1;
            }

            return CompareDigits(a, b);
        }

        private static int CompareDigits(Integer a, Integer b)
        {
            var maxLength = Math.Max(a.Size, b.Size);
            for (var i = maxLength; i >= 0; i--)
            {
                if (a.GetByte(i) < b.GetByte(i))
                {
                    return -1;
                } 
                if (a.GetByte(i) > b.GetByte(i))
                {
                    return 1;
                }
            }

            return 0;
        }


        public static Integer operator -(Integer a)
        {
            a.Sign = a.Sign == Sign.Plus ? Sign.Minus : Sign.Plus;
            return a;
        }

        public static Integer operator +(Integer a, Integer b) => a.Sign == b.Sign
            ? Add(a, b)
            : Subtract(a, b);

        public static Integer operator -(Integer a, Integer b) => a + -b;

        public static Integer operator *(Integer a, Integer b) => Multiply(a, b);

        public static Integer operator /(Integer a, Integer b) => Div(a, b);

        public static Integer operator %(Integer a, Integer b) => Mod(a, b);

        public static bool operator <(Integer a, Integer b) => Comparison(a, b) < 0;

        public static bool operator >(Integer a, Integer b) => Comparison(a, b) > 0;

        public static bool operator <=(Integer a, Integer b) => Comparison(a, b) <= 0;

        public static bool operator >=(Integer a, Integer b) => Comparison(a, b) >= 0;

        public static bool operator ==(Integer a, Integer b) => Comparison(a, b) == 0;

        public static bool operator !=(Integer a, Integer b) => Comparison(a, b) != 0;
        public static Integer operator ++(Integer integer) => integer + One;
        public static Integer operator --(Integer integer) => integer - One;

        public override bool Equals(object obj) =>
            obj is Integer integer && this == integer;
    }
}