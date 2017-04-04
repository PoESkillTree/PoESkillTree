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

    public partial struct SuperDec_PercentVal : IComparable<SuperDec_PercentVal>
    {
        //0 = 0.XXXX ;1= -0.XXXX;2 = +1.XXXX; 3 = -1.XXXX
        //Other decimal states require alternative code turned on to use
        //if(DecBoolStatus%2==0)?IsPositive:IsNegative;
        public byte DecBoolStatus;

        //Stores decimal section info (19 Decimal places stored)
        public ulong DecimalStatus;

        //Enable use of multiple Special Decimal States
#		if (SuperDecPercentVal_EnableMultipleDecimalStatusFormulas)
        //0 : Default
        //1 : Storing Fractional
        //2 : Storing Value higher than +-1.XXXX (Needs alternative code to store larger amount of digits such as Int128 based formula) 
        public byte DecStatusType;
#		endif

        public static SuperDec_PercentVal Sum(IEnumerable<SuperDec_PercentVal> Value)
        {
            SuperDec_PercentVal TotalSum = SuperDec_PercentVal.Zero;
            foreach (var Element in Value)
            {
                TotalSum += Element;
            }
            return TotalSum;
        }

        public SuperDec_PercentVal Abs()
        {
            if (DecBoolStatus%2==1)//NegativeNumber
            {
                DecBoolStatus -= 1;
            }
            return this;
        }

        public SuperDec_PercentVal Floor()
        {
            this.DecimalStatus = 0;
            return this;
        }

        public static SuperDec_PercentVal Parse(string value, CultureInfo invariantCulture)
        {
            SuperDec_PercentVal NewValue = SuperDec_PercentVal.StringToValue(value);
            return NewValue;
        }

        // Returns rounded value with all fractional digits after specified precision cut off.
        public static SuperDec_PercentVal Floor(SuperDec_PercentVal value, int precision)
        {
            throw new NotImplementedException();
        }

        public SuperDec_PercentVal Ceil()
        {
            throw new NotImplementedException();
        }


        public static SuperDec_PercentVal Pow(SuperDec_PercentVal self, int Value)
        {
            throw new NotImplementedException();
        }

        public static explicit operator SuperDec_PercentVal(DependencyProperty v)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal Pow(double self, double Value) { return SuperDec_PercentVal.Pow((SuperDec_PercentVal)self, (SuperDec_PercentVal)Value); }
        public static SuperDec_PercentVal Pow(SuperDec_PercentVal self, double Value) { return SuperDec_PercentVal.Pow(self, (SuperDec_PercentVal)Value); }
        public static SuperDec_PercentVal Pow(SuperDec_PercentVal self, float Value) { return SuperDec_PercentVal.Pow(self, (SuperDec_PercentVal)Value); }

        //Approximate version of Math.Pow(double self, double Value)
        public static SuperDec_PercentVal Pow(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        //SuperDec_PercentVal version of Math.Exp(double Value)
        public static SuperDec_PercentVal Exp(SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal Max(dynamic LeftSide, dynamic RightSide)
        {
            SuperDec_PercentVal LeftSideAsType = (SuperDec_PercentVal)LeftSide;
            SuperDec_PercentVal RightSideAsType = (SuperDec_PercentVal)RightSide;
            if (LeftSideAsType > RightSide) { return LeftSideAsType; }
            else { return RightSideAsType; }
        }

        public static SuperDec_PercentVal Min(dynamic LeftSide, dynamic RightSide)
        {
            SuperDec_PercentVal LeftSideAsType = (SuperDec_PercentVal)LeftSide;
            SuperDec_PercentVal RightSideAsType = (SuperDec_PercentVal)RightSide;
            if (LeftSideAsType < RightSide) { return LeftSideAsType; }
            else { return RightSideAsType; }
        }

        public static SuperDec_PercentVal Round(SuperDec_PercentVal val)
        {
            return val.Round();
        }

        public static SuperDec_PercentVal Round(SuperDec_PercentVal value, int Precision)
        {
            if (Precision == 0)
            {
                return value.Round();
            }
            else
            {
                return SuperDec_PercentVal.Round(value, 1, MidpointRounding.AwayFromZero);
            }
        }

        public static SuperDec_PercentVal RoundHalfDownValue(SuperDec_PercentVal value, int precision)
        {
            if (precision == 0)
            {
                return value.Floor();
            }
            else
            {
                return SuperDec_PercentVal.Round(value, precision, 1);
            }
        }

        public SuperDec_PercentVal Round()
        {
            throw new NotImplementedException();
        }

        public SuperDec_PercentVal RoundToNonZero()
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal Round(SuperDec_PercentVal value, int precision, int RoundingMethod)
        {//https://en.wikipedia.org/wiki/Rounding
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal Round(SuperDec_PercentVal value, int precision, MidpointRounding RoundingMethod)
        {
            if (RoundingMethod == MidpointRounding.ToEven) { return Round(value, precision, 0); }
            else if (RoundingMethod == MidpointRounding.AwayFromZero)
            {
                return Round(value, precision, 1);
            }
            else
            {
                return Round(value, precision, 2);
            }
        }

        public static SuperDec_PercentVal operator -(SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal StringToValue(string Value)
        {
            throw new NotImplementedException();
        }

        //Method version to Initialize Type instead of with Explicit operators
        public static SuperDec_PercentVal Initialize(dynamic Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public SuperDec_PercentVal(dynamic Value)
        {
            if (Value is string)
            {
                throw new NotImplementedException();
            }
            else if (Value is decimal)
            {
                throw new NotImplementedException();
            }
            else if (Value is double || Value is float)
            {
                throw new NotImplementedException();
            }
            else if (Value is sbyte || Value is ushort || Value is int || Value is long)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static explicit operator decimal(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        //explicit Conversion from this to double
        public static explicit operator double(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator float(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        //Explicit/explicit Conversion from this to int
        public static explicit operator int(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        //Explicit Conversion from this to int 64
        public static explicit operator long(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        //Explicit Conversion from this to uint
        public static explicit operator uint(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        //Explicit Conversion from this to unsigned int 64
        public static explicit operator ulong(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator byte(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator sbyte(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator ushort(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator short(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        static public explicit operator string(SuperDec_PercentVal self)
        {
            throw new NotImplementedException();
        }

        public static explicit operator SuperDec_PercentVal(decimal Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(double Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(float Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(int Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(uint Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(long Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(ulong Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(ushort Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(short Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(sbyte Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        public static explicit operator SuperDec_PercentVal(byte Value)
        {
            return new SuperDec_PercentVal(Value);
        }

        // Self Less than Value
        public static bool operator <(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        // Self Less than or equal to Value
        public static bool operator <=(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        // Self Greater than Value
        public static bool operator >(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        // Self Greater than or Equal to Value
        public static bool operator >=(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        // Equality operator for comparing self to int type value
        public static bool operator ==(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        // Inequality operator for comparing self to multiple value types
        public static bool operator !=(SuperDec_PercentVal self, SuperDec_PercentVal Value)
        {
            throw new NotImplementedException();
        }

        // Self Less than Value
        public static bool operator <(SuperDec_PercentVal self, dynamic Value)
        {
            if (Value is double)
            {
                throw new NotImplementedException();
            }
            else if (Value is String)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator <(dynamic Value, SuperDec_PercentVal self)
        {
            return self > Value;
        }

        // Self Less than or equal to Value
        public static bool operator <=(SuperDec_PercentVal self, dynamic Value)
        {
            if (Value is double)
            {
                throw new NotImplementedException();
            }
            else if (Value is String)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator <=(dynamic Value, SuperDec_PercentVal self)
        {
            return self >= Value;
        }

        // Self Greater than Value
        public static bool operator >(SuperDec_PercentVal self, dynamic Value)
        {
            if (Value is double)
            {
                throw new NotImplementedException();
            }
            else if (Value is String)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator >(dynamic Value, SuperDec_PercentVal self)
        {
            return self < Value;
        }

        // Self Greater than or Equal to Value
        public static bool operator >=(SuperDec_PercentVal self, dynamic Value)
        {
            if (Value is double)
            {
                throw new NotImplementedException();
            }
            else if (Value is String)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator >=(dynamic Value, SuperDec_PercentVal self)
        {
            return self <= Value;
        }

        // Equality operator for comparing self to int type value
        public static bool operator ==(SuperDec_PercentVal self, dynamic Value)
        {
            if (Value is double)
            {
                throw new NotImplementedException();
            }
            else if (Value is String)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator ==(dynamic Value, SuperDec_PercentVal self)
        {
            return self == Value;
        }

        // Inequality operator for comparing self to multiple value types
        public static bool operator !=(SuperDec_PercentVal self, dynamic Value)
        {
            if (Value is double)
            {
                throw new NotImplementedException();
            }
            else if (Value is String)
            {
                throw new NotImplementedException();
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static bool operator !=(dynamic Value, SuperDec_PercentVal self)
        {
            return self != Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            try
            {
                return this == (SuperDec_PercentVal)obj;
            }
            catch
            {
                return false;
            }
        }

        public byte GetDecBoolStatus()
        {
            return DecBoolStatus;
        }

        public ulong GetDecimalStatus()
        {
            return DecimalStatus;
        }

        // Override the Object.GetHashCode() method:
        public override int GetHashCode()
        {
            if(DecimalStatus<=Int32.MaxValue)
            {
                return (int)DecimalStatus;
            }
            else
            {
                return Int32.MaxValue;
            }
        }

        public void SwapNegativeStatus()
        {
            if (DecBoolStatus % 2 == 1)
            {
                DecBoolStatus -= 1;
            }
            else
            {
                DecBoolStatus += 1;
            }
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        public static SuperDec_PercentVal Maximum()
        {
            throw new NotImplementedException();
        }

        //Returns value of minimum non-infinite/Special Decimal State Value that can store
        public static SuperDec_PercentVal Minimum()
        {
            throw new NotImplementedException();
        }

        public bool IsInfinity()
        {
            //Negative Infinity
            if (DecBoolStatus == 255)
            { return true; }
            //Positive Infinity
            else if (DecBoolStatus == 254)
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

        public static SuperDec_PercentVal SumOfList(SuperDec_PercentVal[] self)
        {
            SuperDec_PercentVal Total = SuperDec_PercentVal.Zero;
            foreach (SuperDec_PercentVal Element in self)
            {
                Total += Element;
            }
            return Total;
        }

        public static SuperDec_PercentVal SumOfList(IEnumerable<SuperDec_PercentVal> self)
        {
            SuperDec_PercentVal Total = SuperDec_PercentVal.Zero;
            foreach (SuperDec_PercentVal Element in self)
            {
                Total += Element;
            }
            return Total;
        }

        public static SuperDec_PercentVal operator %(SuperDec_PercentVal self, SuperDec_PercentVal y)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal operator +(SuperDec_PercentVal self, SuperDec_PercentVal y)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal operator -(SuperDec_PercentVal self, SuperDec_PercentVal y)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal operator *(SuperDec_PercentVal self, SuperDec_PercentVal y)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal operator /(SuperDec_PercentVal self, SuperDec_PercentVal y)
        {
            throw new NotImplementedException();
        }

        public static SuperDec_PercentVal operator %(SuperDec_PercentVal self, dynamic y)
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

        public static SuperDec_PercentVal operator +(SuperDec_PercentVal self, dynamic y)
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

        public static SuperDec_PercentVal operator -(SuperDec_PercentVal self, dynamic y)
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

        public static SuperDec_PercentVal operator *(SuperDec_PercentVal self, dynamic y)
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

        public static SuperDec_PercentVal operator /(SuperDec_PercentVal self, dynamic y)
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
        public static SuperDec_PercentVal operator -(dynamic y, SuperDec_PercentVal self)
        {
            SuperDec_PercentVal YAsSuperDec = (SuperDec_PercentVal)y;
            YAsSuperDec -= self;
            return YAsSuperDec;
        }

        public static SuperDec_PercentVal operator +(dynamic y, SuperDec_PercentVal self)
        {
            SuperDec_PercentVal YAsSuperDec = (SuperDec_PercentVal)y;
            YAsSuperDec += self;
            return YAsSuperDec;
        }

        public static SuperDec_PercentVal operator *(dynamic y, SuperDec_PercentVal self)
        {
            SuperDec_PercentVal YAsSuperDec = (SuperDec_PercentVal)y;
            YAsSuperDec += self;
            return YAsSuperDec;
        }

        public static SuperDec_PercentVal operator /(dynamic y, SuperDec_PercentVal self)
        {
            SuperDec_PercentVal YAsSuperDec = (SuperDec_PercentVal)y;
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
            if (DecBoolStatus == 202) { return true; }
            else { return false; }
        }

        private static SuperDec_PercentVal NullValue()
        {
            SuperDec_PercentVal NewSelf;
            NewSelf.DecimalStatus = 0;
            NewSelf.DecBoolStatus = 202;
            return NewSelf;
        }

        public static readonly SuperDec_PercentVal Null = NullValue();

        private static SuperDec_PercentVal ZeroValue()
        {
            SuperDec_PercentVal NewSelf;
            NewSelf.DecBoolStatus = 0; NewSelf.DecimalStatus = 0;
            return NewSelf;
        }

        public static readonly SuperDec_PercentVal Zero = ZeroValue();

        int IComparable<SuperDec_PercentVal>.CompareTo(SuperDec_PercentVal other)
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
