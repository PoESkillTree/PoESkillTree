using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using static GlobalCode_VariableConversionFunctions.VariableConversionFunctions;

    //aka SuperDec_PercentValV2
    //Can represent +-8.XXXXXXXXXXXXXXXXXX
    public partial struct PercentValV2 : IComparable<PercentValV2>
    {
        public long ValueRep;

        public static PercentValV2 Sum(IEnumerable<PercentValV2> Value)
        {
            PercentValV2 TotalSum = PercentValV2.Zero;
            foreach (var Element in Value)
            {
                TotalSum += Element;
            }
            return TotalSum;
        }

        public PercentValV2 Abs()
        {
            if(ValueRep<0)
            {
                ValueRep *= -1;
            }
            return this;
        }

        public PercentValV2 Floor()
        {
            sbyte WholeHalf = (sbyte)(ValueRep/1000000000000000000);
            this.ValueRep = WholeHalf* 1000000000000000000;
            return this;
        }

        //public static PercentValV2 Parse(string value, CultureInfo invariantCulture)
        //{
        //    PercentValV2 NewValue = PercentValV2.StringToValue(value);
        //    return NewValue;
        //}

        //// Returns rounded value with all fractional digits after specified precision cut off.
        //public static PercentValV2 Floor(PercentValV2 value, int precision)
        //{
        //    throw new NotImplementedException();
        //}

        //public PercentValV2 Ceil()
        //{
        //    throw new NotImplementedException();
        //}


        //public static PercentValV2 Pow(PercentValV2 self, int Value)
        //{
        //    throw new NotImplementedException();
        //}

        //public static explicit operator PercentValV2(DependencyProperty v)
        //{
        //    throw new NotImplementedException();
        //}

        //public static PercentValV2 Pow(double self, double Value) { return PercentValV2.Pow((PercentValV2)self, (PercentValV2)Value); }
        //public static PercentValV2 Pow(PercentValV2 self, double Value) { return PercentValV2.Pow(self, (PercentValV2)Value); }
        //public static PercentValV2 Pow(PercentValV2 self, float Value) { return PercentValV2.Pow(self, (PercentValV2)Value); }

        ////Approximate version of Math.Pow(double self, double Value)
        //public static PercentValV2 Pow(PercentValV2 self, PercentValV2 Value)
        //{
        //    throw new NotImplementedException();
        //}

        ////PercentValV2 version of Math.Exp(double Value)
        //public static PercentValV2 Exp(PercentValV2 Value)
        //{
        //    throw new NotImplementedException();
        //}

        //public static PercentValV2 Max(dynamic LeftSide, dynamic RightSide)
        //{
        //    PercentValV2 LeftSideAsType = (PercentValV2)LeftSide;
        //    PercentValV2 RightSideAsType = (PercentValV2)RightSide;
        //    if (LeftSideAsType > RightSide) { return LeftSideAsType; }
        //    else { return RightSideAsType; }
        //}

        //public static PercentValV2 Min(dynamic LeftSide, dynamic RightSide)
        //{
        //    PercentValV2 LeftSideAsType = (PercentValV2)LeftSide;
        //    PercentValV2 RightSideAsType = (PercentValV2)RightSide;
        //    if (LeftSideAsType < RightSide) { return LeftSideAsType; }
        //    else { return RightSideAsType; }
        //}

        //public static PercentValV2 Round(PercentValV2 val)
        //{
        //    return val.Round();
        //}

        //public static PercentValV2 Round(PercentValV2 value, int Precision)
        //{
        //    if (Precision == 0)
        //    {
        //        return value.Round();
        //    }
        //    else
        //    {
        //        return PercentValV2.Round(value, 1, MidpointRounding.AwayFromZero);
        //    }
        //}

        //public static PercentValV2 RoundHalfDownValue(PercentValV2 value, int precision)
        //{
        //    if (precision == 0)
        //    {
        //        return value.Floor();
        //    }
        //    else
        //    {
        //        return PercentValV2.Round(value, precision, 1);
        //    }
        //}

        //public PercentValV2 Round()
        //{
        //    throw new NotImplementedException();
        //}

        //public PercentValV2 RoundToNonZero()
        //{
        //    throw new NotImplementedException();
        //}

        //public static PercentValV2 Round(PercentValV2 value, int precision, int RoundingMethod)
        //{//https://en.wikipedia.org/wiki/Rounding
        //    throw new NotImplementedException();
        //}

        //public static PercentValV2 Round(PercentValV2 value, int precision, MidpointRounding RoundingMethod)
        //{
        //    if (RoundingMethod == MidpointRounding.ToEven) { return Round(value, precision, 0); }
        //    else if (RoundingMethod == MidpointRounding.AwayFromZero)
        //    {
        //        return Round(value, precision, 1);
        //    }
        //    else
        //    {
        //        return Round(value, precision, 2);
        //    }
        //}

        public static PercentValV2 operator -(PercentValV2 Value)
        {
            Value.ValueRep *= -1;
            return Value;
        }

        //public static PercentValV2 StringToValue(string Value)
        //{
        //    throw new NotImplementedException();
        //}

        ////Method version to Initialize Type instead of with Explicit operators
        //public static PercentValV2 Initialize(dynamic Value)
        //{
        //    return new PercentValV2(Value);
        //}

        public PercentValV2(dynamic Value)
        {
            if (Value is string)
            {
                throw new NotImplementedException();
            }
            else if (Value is decimal||Value is double || Value is float)
            {
                ValueRep = (long)((Value)* 1000000000000000000);
            }
            else if (Value is SmallDec)
            {
                ValueRep = (long)Value.IntValue * 1000000000000000000;
            }
            else//if (Value is sbyte || Value is byte || Value is ushort || Value is int || Value is uint || Value is long|| Value is ulong)
            {
                ValueRep = (long)Value * 1000000000000000000;
            }
        }

        //From this type to Standard types

        public static explicit operator decimal(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator double(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator float(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator int(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator long(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator uint(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator ulong(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator byte(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator sbyte(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator ushort(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator short(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        static public explicit operator string(PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        //From Standard types to this type

#if (BlazesGlobalCode_EnablePercentValV2_ImplicitConversionFrom)
        public static implicit operator PercentValV2(decimal Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(double Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(float Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(int Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(uint Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(long Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(ulong Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(ushort Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(short Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(sbyte Value)   {   return new PercentValV2(Value); }

        public static implicit operator PercentValV2(byte Value)    {   return new PercentValV2(Value); }
#else
        public static explicit operator PercentValV2(decimal Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(double Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(float Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(int Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(uint Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(long Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(ulong Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(ushort Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(short Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(sbyte Value) { return new PercentValV2(Value); }

        public static explicit operator PercentValV2(byte Value) { return new PercentValV2(Value); }
#endif

        // Self Less than Value
        public static bool operator <(PercentValV2 self, PercentValV2 Value)
        {
            throw new NotImplementedException();
        }

        // Self Less than or equal to Value
        public static bool operator <=(PercentValV2 self, PercentValV2 Value)
        {
            throw new NotImplementedException();
        }

        // Self Greater than Value
        public static bool operator >(PercentValV2 self, PercentValV2 Value)
        {
            throw new NotImplementedException();
        }

        // Self Greater than or Equal to Value
        public static bool operator >=(PercentValV2 self, PercentValV2 Value)
        {
            throw new NotImplementedException();
        }

        // Equality operator for comparing self
        public static bool operator ==(PercentValV2 self, PercentValV2 Value)
        {
            throw new NotImplementedException();
        }

        // Inequality operator for comparing self
        public static bool operator !=(PercentValV2 self, PercentValV2 Value)
        {
            throw new NotImplementedException();
        }

        // Self Less than Value
        public static bool operator <(PercentValV2 self, SmallDec Value)
        {
            throw new NotImplementedException();
        }

        // Self Less than or equal to Value
        public static bool operator <=(PercentValV2 self, SmallDec Value)
        {
            throw new NotImplementedException();
        }

        // Self Greater than Value
        public static bool operator >(PercentValV2 self, SmallDec Value)
        {
            throw new NotImplementedException();
        }

        // Self Greater than or Equal to Value
        public static bool operator >=(PercentValV2 self, SmallDec Value)
        {
            throw new NotImplementedException();
        }

        // Equality operator for comparing self
        public static bool operator ==(PercentValV2 self, SmallDec Value)
        {
            throw new NotImplementedException();
        }

        // Inequality operator for comparing self
        public static bool operator !=(PercentValV2 self, SmallDec Value)
        {
            throw new NotImplementedException();
        }

        // Self Less than Value
        public static bool operator <(SmallDec Value, PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        // Self Less than or equal to Value
        public static bool operator <=(SmallDec Value, PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        // Self Greater than Value
        public static bool operator >(SmallDec Value, PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        // Self Greater than or Equal to Value
        public static bool operator >=(SmallDec Value, PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        // Equality operator for comparing self
        public static bool operator ==(SmallDec Value, PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        // Inequality operator for comparing self
        public static bool operator !=(SmallDec Value, PercentValV2 self)
        {
            throw new NotImplementedException();
        }

        // Self Less than Value
        public static bool operator <(PercentValV2 self, dynamic Value)
        {
            if (Value is decimal || Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is string)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator <(dynamic Value, PercentValV2 self)
        {
            return self > Value;
        }

        // Self Less than or equal to Value
        public static bool operator <=(PercentValV2 self, dynamic Value)
        {
            if (Value is decimal || Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is string)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator <=(dynamic Value, PercentValV2 self)
        {
            return self >= Value;
        }

        // Self Greater than Value
        public static bool operator >(PercentValV2 self, dynamic Value)
        {
            if (Value is decimal || Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is string)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator >(dynamic Value, PercentValV2 self)
        {
            return self < Value;
        }

        // Self Greater than or Equal to Value
        public static bool operator >=(PercentValV2 self, dynamic Value)
        {
            if (Value is decimal || Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is string)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator >=(dynamic Value, PercentValV2 self)
        {
            return self <= Value;
        }

        // Equality operator for comparing self to int type value
        public static bool operator ==(PercentValV2 self, dynamic Value)
        {
            if (Value is decimal || Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is string)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator ==(dynamic Value, PercentValV2 self)
        {
            return self == Value;
        }

        // Inequality operator for comparing self to multiple value types
        public static bool operator !=(PercentValV2 self, dynamic Value)
        {
            if (Value is decimal || Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is string)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator !=(dynamic Value, PercentValV2 self)
        {
            return self != Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            try
            {
                return this == (PercentValV2)obj;
            }
            catch
            {
                return false;
            }
        }


        // Override the Object.GetHashCode() method:
        public override int GetHashCode()
        {
            if (ValueRep <= Int32.MaxValue&&ValueRep>=Int32.MinValue)
            {
                return (int)ValueRep;
            }
            else
            {
                return (int)(ValueRep%Int32.MaxValue);
            }
        }

        public void SwapNegativeStatus()
        {
            ValueRep *= -1;
        }

        private static PercentValV2 MaximumValue()
        {
            PercentValV2 self;
            self.ValueRep = 8999999999999999999;
            return self;
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can stored
        public static PercentValV2 Maximum = MaximumValue();

        private static PercentValV2 MinimumValue()
        {
            PercentValV2 self;
            self.ValueRep = -8999999999999999999;
            return self;
        }

        //Returns value of lowest non-infinite/Special Decimal State Value that can stored
        public static PercentValV2 Minimum = MinimumValue();

        //Max/Min of long = Infinity
        public bool IsInfinity()
        {
            //Negative Infinity
            if (ValueRep == 9223372036854775807)
            { return true; }
            //Positive Infinity
            else if (ValueRep == -9223372036854775808)
            { return true; }
            else { return false; }
        }

        public string ToString(string s, IFormatProvider provider)
        {
            return (string)this;
        }

        public string ToString(string s)
        {
            return (string)this;
        }

        internal string ToString(CultureInfo invariantCulture)
        {
            return (string)this;
        }

        public override string ToString() { return (string)this; }

        public static PercentValV2 SumOfList(PercentValV2[] self)
        {
            PercentValV2 Total = PercentValV2.Zero;
            foreach (PercentValV2 Element in self)
            {
                Total += Element;
            }
            return Total;
        }

        public static PercentValV2 SumOfList(IEnumerable<PercentValV2> self)
        {
            PercentValV2 Total = PercentValV2.Zero;
            foreach (PercentValV2 Element in self)
            {
                Total += Element;
            }
            return Total;
        }

        public static PercentValV2 operator %(PercentValV2 self, PercentValV2 y)
        {
            throw new NotImplementedException();
        }

        public static PercentValV2 operator +(PercentValV2 self, PercentValV2 y)
        {
            throw new NotImplementedException();
        }

        public static PercentValV2 operator -(PercentValV2 self, PercentValV2 y)
        {
            throw new NotImplementedException();
        }

        public static PercentValV2 operator *(PercentValV2 self, PercentValV2 y)
        {
            throw new NotImplementedException();
        }

        public static PercentValV2 operator /(PercentValV2 self, PercentValV2 y)
        {
            throw new NotImplementedException();
        }

        public static PercentValV2 operator %(PercentValV2 self, dynamic y)
        {
            if (y is double || y is float || y is decimal)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
            //return self;
        }

        public static PercentValV2 operator +(PercentValV2 self, dynamic y)
        {
            if (y is double || y is float || y is decimal)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
            //return self;
        }

        public static PercentValV2 operator -(PercentValV2 self, dynamic y)
        {
            if (y is double || y is float || y is decimal)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
            //return self;
        }

        public static PercentValV2 operator *(PercentValV2 self, dynamic y)
        {
            if (y is double || y is float || y is decimal)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
            //return self;
        }

        public static PercentValV2 operator /(PercentValV2 self, dynamic y)
        {
            if (y is double || y is float || y is decimal)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
            //return self;
        }

        //Right side applications
        public static PercentValV2 operator -(dynamic y, PercentValV2 self)
        {
            PercentValV2 YAsSuperDec = (PercentValV2)y;
            YAsSuperDec -= self;
            return YAsSuperDec;
        }

        public static PercentValV2 operator +(dynamic y, PercentValV2 self)
        {
            PercentValV2 YAsSuperDec = (PercentValV2)y;
            YAsSuperDec += self;
            return YAsSuperDec;
        }

        public static PercentValV2 operator *(dynamic y, PercentValV2 self)
        {
            PercentValV2 YAsSuperDec = (PercentValV2)y;
            YAsSuperDec += self;
            return YAsSuperDec;
        }

        public static PercentValV2 operator /(dynamic y, PercentValV2 self)
        {
            PercentValV2 YAsSuperDec = (PercentValV2)y;
            YAsSuperDec += self;
            return YAsSuperDec;
        }

        public float AsFloat()
        {
            return (float)this;
        }

        public double AsDouble()
        {
            return (double)this;
        }

        public int AsInt()
        {
            return (int)this;
        }

        public string AsString()
        {
            return (string)this;
        }

        public bool IsNull()
        {
            if (ValueRep == 9000000000000000002) { return true; }
            else { return false; }
        }

        private static PercentValV2 NullValue()
        {
            PercentValV2 NewSelf;
            NewSelf.ValueRep = 900000000000000002;
            return NewSelf;
        }

        public static readonly PercentValV2 Null = NullValue();

        private static PercentValV2 ZeroValue()
        {
            PercentValV2 NewSelf;
            NewSelf.ValueRep = 0;
            return NewSelf;
        }

        public static readonly PercentValV2 Zero = ZeroValue();

        int IComparable<PercentValV2>.CompareTo(PercentValV2 other)
        {
            if (other == this)
            {
                return 0;
            }
            else if (this < other)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        public static dynamic ConditionalReturn(bool Condition, dynamic X, dynamic Y)
        {
            return CSharpGlobalCode.GlobalCode_ExperimentalCode.SuperDecGlobalCode.ConditionalReturn(Condition, X, Y);
        }

        public dynamic DynamicConversion()
        {
            return this;
        }
    }

}