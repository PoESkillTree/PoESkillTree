/*	Code Created by James Michael Armstrong (NexusName:BlazesRus)
    Latest Code Release at https://github.com/BlazesRus/NifLibEnvironment
*/
using System;

//Requires BigMath library to compile

//CSharpGlobalCode.GlobalCode_ExperimentalCode.SuperDec_Int32_9Decimal
namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using static GlobalCode_VariableConversionFunctions.VariableConversionFunctions;

    //Aka SuperDec_Int32_9Decimal
    public partial struct MediumSuperDec : IComparable<MediumSuperDec>
    {
        //0 = Positive;1=Negative;Other states at higher then 1;254 = Positive Infinity;255 = Negative Infinity
        public byte DecBoolStatus;

        //Stores decimal section info (9 Decimal places stored)
        public uint DecimalStatus;

        public uint IntValue;

        public static MediumSuperDec Sum(IEnumerable<MediumSuperDec> Value)
        {
            MediumSuperDec TotalSum = MediumSuperDec.Zero;
            foreach (var Element in Value)
            {
                TotalSum += Element;
            }
            return TotalSum;
        }

        public MediumSuperDec Abs()
        {
            this.DecBoolStatus = 0;
            return this;
        }

        public MediumSuperDec Floor()
        {
            this.DecimalStatus = 0;
            return this;
        }

        public MediumSuperDec Ceil()
        {
            if (this.DecimalStatus != 0)
            {
                this.DecimalStatus = 0;
                this.IntValue += 1;
            }
            return this;
        }

        public MediumSuperDec Round()
        {
            if (DecimalStatus >= 500000000) { this.IntValue += 1; }
            this.DecimalStatus = 0;
            return this;
        }

        public MediumSuperDec(SmallDec Value)
        {
            IntValue = (uint)Value.IntValue;
            DecimalStatus = (uint)Value.DecimalStatus * 100000;
            DecBoolStatus = Value.DecBoolStatus;
        }

        public static explicit operator MediumSuperDec(SmallDec self)
        {
            return new MediumSuperDec(self);
        }

        public MediumSuperDec(dynamic Value)
        {
            if (Value is double || Value is float)
            {
                if (Value < 0)
                {
                    Value *= -1;
                    DecBoolStatus = 1;
                }
                else
                {
                    DecBoolStatus = 0;
                }
                IntValue = (uint)System.Math.Floor(Value);
                Value -= IntValue;
                DecimalStatus = ExtractDecimalHalfV2(Value);
            }
            else if (Value is sbyte || Value is ushort || Value is int || Value is long)
            {
                if (Value < 0)
                {
                    this.DecBoolStatus = 1;
                    Value *= -1;
                }
                else
                {
                    this.DecBoolStatus = 0;
                }
                //Cap value if too big on initialize
                if (Value > 4294967295)
                {
                    Value = 4294967295;
                }
                this.DecBoolStatus = 0;
                this.IntValue = (uint)Value;
                this.DecimalStatus = 0;
            }
            else
            {
                //Cap value if too big on initialize
                if (Value > 4294967295)
                {
                    Value = 4294967295;
                }
                this.DecBoolStatus = 0;
                this.IntValue = (uint)Value;
                this.DecimalStatus = 0;
            }
        }

        //Explicit Conversion from this to double
        public static explicit operator double(MediumSuperDec self)
        {
            double Value = 0.0;
            Value += self.IntValue;
            Value += (self.DecimalStatus * 0.000000001);
            if (self.DecBoolStatus == 1) { Value *= -1; }
            return Value;
        }

        public static explicit operator float(MediumSuperDec self)
        {
            float Value = 0.0f;
            Value += self.IntValue;
            Value += (float)(self.DecimalStatus * 0.000000001);
            if (self.DecBoolStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit/explicit Conversion from this to int
        public static explicit operator int(MediumSuperDec self)
        {
            int Value = (int)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit Conversion from this to int 64
        public static explicit operator long(MediumSuperDec self)
        {
            long Value = self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit Conversion from this to uint
        public static explicit operator uint(MediumSuperDec self)
        {
            return self.IntValue;
        }

        //Explicit Conversion from this to unsigned int 64
        public static explicit operator ulong(MediumSuperDec self)
        {
            return self.IntValue;
        }

        public static explicit operator byte(MediumSuperDec self)
        {
            byte Value = (byte)self.IntValue;
            return Value;
        }

        public static explicit operator sbyte(MediumSuperDec self)
        {
            sbyte Value = (sbyte)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        public static explicit operator ushort(MediumSuperDec self)
        {
            ushort Value = (ushort)self.IntValue;
            return Value;
        }

        public static explicit operator short(MediumSuperDec self)
        {
            short Value = (short)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //public static ulong ForceConvertFromInt256(BigMath.Int256 Value)
        //{
        //	ulong ConvertedValue = 0;
        //	//Larger than ulong (default to zero)
        //	if (Value > 18446744073709551615)
        //	{
        //		Console.WriteLine("Overflow Detected");
        //	}
        //	else
        //	{
        //		BigMath.Int128 Value02 = (BigMath.Int128)Value;
        //		ConvertedValue = (ulong)Value02;
        //	}
        //	return ConvertedValue;
        //}

        static public explicit operator string(MediumSuperDec self)
        {
            System.String Value = "";
            uint IntegerHalf = self.IntValue;
            byte CurrentDigit;
            if (self.DecBoolStatus == 1) { Value += "-"; }
            for (sbyte Index = NumberOfPlaces(IntegerHalf); Index >= 0; Index--)
            {
                CurrentDigit = (byte)(IntegerHalf / Math.Pow(10, Index));
                IntegerHalf -= (uint)(CurrentDigit * Math.Pow(10, Index));
                Value += DigitAsChar(CurrentDigit);
            }
            Value += ".";
            uint DecimalHalf = self.DecimalStatus;
            for (sbyte Index = 8; Index >= 0; Index--)
            {
                CurrentDigit = (byte)(DecimalHalf / Math.Pow(10, Index));
                DecimalHalf -= (uint)(CurrentDigit * Math.Pow(10, Index));
                Value += DigitAsChar(CurrentDigit);
            }
            return Value;
        }

        //From Standard types to this type 
#if (BlazesGlobalCode_StandardExplicitConversionFrom)
        public static explicit operator MediumSuperDec(decimal Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(double Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(MediumSuperDec Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(int Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(uint Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(long Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(ulong Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(ushort Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(short Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(sbyte Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(byte Value)	{	return new MediumSuperDec(Value);	}

        public static explicit operator MediumSuperDec(string Value) { return new MediumSuperDec(Value); }

        public static explicit operator MediumSuperDec(DependencyProperty Value)
        {
            MediumSuperDec NewValue = Value.ToString();
            return NewValue;
        }
#else
        public static implicit operator MediumSuperDec(decimal Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(double Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(float Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(int Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(uint Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(long Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(ulong Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(ushort Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(short Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(sbyte Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(byte Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(string Value) { return new MediumSuperDec(Value); }

        public static implicit operator MediumSuperDec(DependencyProperty Value)
        {
            MediumSuperDec NewValue = Value.ToString();
            return NewValue;
        }
#endif

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            try
            {
                return this == (MediumSuperDec)obj;
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
            if (DecBoolStatus == 1)
            {
                return (int)(IntValue / 2 - 2147483648);
            }
            else
            {
                return (int)(IntValue / 2);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public uint GetIntValue()
        {
            return IntValue;
        }

        public int GetIntValueAsInt32()
        {
            return (int)IntValue;
        }

        public void SwapNegativeStatus()
        {
            if (DecBoolStatus == 1) { DecBoolStatus = 0; }
            else { DecBoolStatus = 1; }
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        private static MediumSuperDec MaximumValue()
        {
            MediumSuperDec NewSelf;
            NewSelf.IntValue = 4294967295;
            NewSelf.DecimalStatus = 999999999;
            NewSelf.DecBoolStatus = 0;
            return NewSelf;
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        public static readonly MediumSuperDec Maximum = MaximumValue();

        //Returns value of lowest non-infinite/Special Decimal State Value that can store
        private static MediumSuperDec MinimumValue()
        {
            MediumSuperDec NewSelf; 
            NewSelf.IntValue = 4294967295;
            NewSelf.DecimalStatus = 999999999;
            NewSelf.DecBoolStatus = 1;
            return NewSelf;
        }

        //Returns value of lowest non-infinite/Special Decimal State Value that can store
        public static readonly MediumSuperDec Minimum = MinimumValue();


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
        public static MediumSuperDec FloatParse(string s, IFormatProvider provider)
        {
            MediumSuperDec NewSelf = (MediumSuperDec) float.Parse(s, provider);
            return NewSelf;
        }

        public static MediumSuperDec DoubleParse(string s, IFormatProvider provider)
        {
            MediumSuperDec NewSelf = (MediumSuperDec) double.Parse(s, provider);
            return NewSelf;
        }

        public string ToString(string s, IFormatProvider provider)
        {
            //float SelfAsFloat = this;
            //string StringValue = SelfAsFloat.ToString(s, provider);
            //return StringValue;
            return (string)this;
        }

        internal string ToString(CultureInfo invariantCulture)
        {
            return (string)this;
        }

        //public static SuperDec_Int32_9Decimal operator ?(bool Condition, dynamic X, dynamic Y)
        //{
        //	if(Condition)
        //	{
        //		return X;
        //	}
        //	else
        //	{
        //		return Y;
        //	}
        //}
        public static dynamic ConditionalReturn(bool Condition, dynamic X, dynamic Y)
        {
            if (Condition)
            {
                return X;
            }
            else
            {
                return Y;
            }
        }

        public float AsFloat() { return (float)this; }
        public double AsDouble() { return (double)this; }
        public int AsInt() { return (int)this; }
        public string AsString() { return (string)this; }
        //public SmallDec AsSmallDec() { return (SmallDec)this; }

        private static MediumSuperDec ZeroValue()
        {
            MediumSuperDec NewSelf;
            NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0; NewSelf.DecBoolStatus = 0;
            return NewSelf;
        }

        public static readonly MediumSuperDec Zero = ZeroValue();

        private static MediumSuperDec NaNValue()
        {
            MediumSuperDec NewSelf;
            NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0;
#if (BlazesGlobalCode_MediumSuperDec_EnableSpecialDecStates)
            NewSelf.DecBoolStatus = 202;
#else
            NewSelf.DecBoolStatus = 0;
#endif
            return NewSelf;
        }

        public static readonly MediumSuperDec NaN = NaNValue();

        int IComparable<MediumSuperDec>.CompareTo(MediumSuperDec other)
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

        public bool AlmostEquals(dynamic CompareTarget, dynamic RangeWithin)
        {
            MediumSuperDec ConvertedTarget = (MediumSuperDec)CompareTarget;
            if (CompareTarget == this) { return true; }
            else
            {
                MediumSuperDec LeftRange = CompareTarget - RangeWithin;
                MediumSuperDec RightRange = CompareTarget + RangeWithin;
                if (this == LeftRange || this == RightRange) { return true; }
                else if (CompareTarget > LeftRange && CompareTarget < RightRange) { return true; }
                else { return false; }
            }
        }
    }
}