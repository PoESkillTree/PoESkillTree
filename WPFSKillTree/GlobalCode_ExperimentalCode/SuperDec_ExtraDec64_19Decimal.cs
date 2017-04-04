/*	Code Created by James Michael Armstrong (NexusName:BlazesRus)
    Latest Code Release at https://github.com/BlazesRus/NifLibEnvironment
*/
using System;

//Requires BigMath library to compile

//CSharpGlobalCode.GlobalCode_ExperimentalCode.SuperDec_ExtraDec64_19Decimal
namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using static GlobalCode_VariableConversionFunctions.VariableConversionFunctions;

    //Aka SuperDec_ExtraDec64_19Decimal
    public partial struct LargeSuperDec : IComparable<LargeSuperDec>
    {
        //0 = Positive;1=Negative;Other states at higher then 1
        public byte DecBoolStatus;

        //Stores decimal section info
        public ulong DecimalStatus;

        public ulong IntValue;

        public static LargeSuperDec Sum(IEnumerable<LargeSuperDec> Value)
        {
            LargeSuperDec TotalSum = LargeSuperDec.Zero;
            foreach (var Element in Value)
            {
                TotalSum += Element;
            }
            return TotalSum;
        }

        public LargeSuperDec Abs()
        {
            this.DecBoolStatus = 0;
            return this;
        }

        public LargeSuperDec Floor()
        {
            this.DecimalStatus = 0;
            return this;
        }

        public LargeSuperDec Ceil()
        {
            if (this.DecimalStatus!=0)
            {
                this.DecimalStatus = 0;
                this.IntValue += 1;
            }
            return this;
        }

        public LargeSuperDec Round()
        {
            if(DecimalStatus>=5000000000000000000){	this.IntValue += 1;}
            this.DecimalStatus = 0;
            return this;
        }

        public LargeSuperDec(dynamic Value)
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
                DecimalStatus = ExtractDecimalHalf(Value);
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
                if (Value > 18446744073709551615)
                {
                    Value = 18446744073709551615;
                }
                this.DecBoolStatus = 0;
                this.IntValue = (uint)Value;
                this.DecimalStatus = 0;
            }
            else
            {
                //Cap value if too big on initialize
                if (Value > 18446744073709551615)
                {
                    Value = 18446744073709551615;
                }
                this.DecBoolStatus = 0;
                this.IntValue = (uint)Value;
                this.DecimalStatus = 0;
            }
        }

        public static explicit operator LargeSuperDec(SmallDec self)
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = self.IntValue;
            NewSelf.DecimalStatus = (ulong)self.DecimalStatus * 1000000000000000;
            NewSelf.DecBoolStatus = self.DecBoolStatus;
            return NewSelf;
        }

        public static explicit operator LargeSuperDec(ModerateSuperDec self)
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = self.IntValue;
            NewSelf.DecimalStatus = self.DecimalStatus;
            NewSelf.DecBoolStatus = self.DecBoolStatus;
            return NewSelf;
        }

        public static explicit operator LargeSuperDec(MediumSuperDec self)
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = self.IntValue;
            NewSelf.DecimalStatus = (ulong)self.DecimalStatus*10000000000;
            NewSelf.DecBoolStatus = self.DecBoolStatus;
            return NewSelf;
        }

        public static explicit operator float(LargeSuperDec self)
        {
            float Value = 0.0f;
            Value += self.IntValue;
            Value += (float)(self.DecimalStatus * 0.0000000000000000001);
            if (self.DecBoolStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit Conversion from this to double
        public static explicit operator double(LargeSuperDec self)
        {
            double Value = 0.0;
            Value += self.IntValue;
            Value += (self.DecimalStatus * 0.0000000000000000001);
            if (self.DecBoolStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit Conversion from this to int
        public static explicit operator int(LargeSuperDec self)
        {
            int Value = (int)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit Conversion from this to int 64
        public static explicit operator long(LargeSuperDec self)
        {
            long Value = (long) self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //Explicit Conversion from this to uint
        public static explicit operator uint(LargeSuperDec self)
        {
            return (uint) self.IntValue;
        }

        //Explicit Conversion from this to unsigned int 64
        public static explicit operator ulong(LargeSuperDec self)
        {
            return self.IntValue;
        }

        static public explicit operator string(LargeSuperDec self)
        {
            System.String Value = "";
            ulong IntegerHalf = self.IntValue;
            byte CurrentDigit;
            if (self.DecBoolStatus == 1) { Value += "-"; }
            for (sbyte Index = NumberOfPlaces(IntegerHalf); Index >= 0; Index--)
            {
                CurrentDigit = (byte)(IntegerHalf / Math.Pow(10, Index));
                IntegerHalf -= (uint)(CurrentDigit * Math.Pow(10, Index));
                Value += DigitAsChar(CurrentDigit);
            }
            Value += ".";
            ulong DecimalHalf = self.DecimalStatus;
            for (sbyte Index = 18; Index >= 0; Index--)
            {
                CurrentDigit = (byte)(DecimalHalf / Math.Pow(10, Index));
                DecimalHalf -= (ulong)(CurrentDigit * Math.Pow(10, Index));
                Value += DigitAsChar(CurrentDigit);
            }
            return Value;
        }
        
        public static explicit operator byte(LargeSuperDec self)
        {
            byte Value = (byte)self.IntValue;
            return Value;
        }

        public static explicit operator sbyte(LargeSuperDec self)
        {
            sbyte Value = (sbyte)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        public static explicit operator ushort(LargeSuperDec self)
        {
            ushort Value = (ushort)self.IntValue;
            return Value;
        }

        public static explicit operator short(LargeSuperDec self)
        {
            short Value = (short)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //From Standard types to this type 
#if (BlazesGlobalCode_StandardExplicitConversionFrom)
        public static explicit operator LargeSuperDec(decimal Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(double Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(LargeSuperDec Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(int Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(uint Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(long Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(ulong Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(ushort Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(short Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(sbyte Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(byte Value)	{	return new LargeSuperDec(Value);	}

        public static explicit operator LargeSuperDec(string Value) { return new LargeSuperDec(Value); }

        public static explicit operator LargeSuperDec(DependencyProperty Value)
        {
            LargeSuperDec NewValue = Value.ToString();
            return NewValue;
        }
#else
        public static implicit operator LargeSuperDec(decimal Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(double Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(float Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(int Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(uint Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(long Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(ulong Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(ushort Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(short Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(sbyte Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(byte Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(string Value) { return new LargeSuperDec(Value); }

        public static implicit operator LargeSuperDec(DependencyProperty Value)
        {
            LargeSuperDec NewValue = Value.ToString();
            return NewValue;
        }
#endif

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            try
            {
                return this == (LargeSuperDec)obj;
            }
            catch
            {
                return false;
            }
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
        public ulong GetIntValue()
        {
            return IntValue;
        }

        public int GetIntValueAsInt32()
        {
            return (int)this;
        }

        public long GetIntValueAsInt64()
        {
            return (long)this;
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        private static LargeSuperDec MaximumValue()
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = 18446744073709551615;
            NewSelf.DecimalStatus = 9999999999999999999;
            NewSelf.DecBoolStatus = 0;
            return NewSelf;
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        public static readonly LargeSuperDec Maximum = MaximumValue();

        //Returns value of lowest non-infinite/Special Decimal State Value that can store
        private static  LargeSuperDec MinimumValue()
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = 18446744073709551615;
            NewSelf.DecimalStatus = 9999999999999999999;
            NewSelf.DecBoolStatus = 1;
            return NewSelf;
        }

        //Returns value of lowest non-infinite/Special Decimal State Value that can store
        public static readonly LargeSuperDec Minimum = MinimumValue();


        public float AsFloat() { return (float)this; }
        public double AsDouble() { return (double)this; }
        public int AsInt() { return (int)this; }
        public string AsString() { return (string)this; }

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

        public bool IsNull()
        {
            if (DecBoolStatus == 202) { return true; }
            else { return false; }
        }


        public byte GetBoolStatus()
        {
            return DecBoolStatus;
        }

        public void SwapNegativeStatus()
        {
            if (DecBoolStatus%2==0) { DecBoolStatus += 1; }
            else { DecBoolStatus -= 1; }
        }

        private static LargeSuperDec ZeroValue()
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0; NewSelf.DecBoolStatus = 0;
            return NewSelf;
        }

        public static readonly LargeSuperDec Zero = ZeroValue();

        private static LargeSuperDec NaNValue()
        {
            LargeSuperDec NewSelf;
            NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0;
#if (BlazesGlobalCode_LargeSuperDec_EnableSpecialDecStates)
            NewSelf.DecBoolStatus = 202;
#else
            NewSelf.DecBoolStatus = 0;
#endif
            return NewSelf;
        }

        public static readonly LargeSuperDec NaN = NaNValue();

        int IComparable<LargeSuperDec>.CompareTo(LargeSuperDec other)
        {
            if (other == this)
            {
                return 0;
            }
#if (BlazesGlobalCode_Disable128BitFeatures)
#else
            else if (this < other)
            {
                return -1;
            }
#endif
            else
            {
                return 1;
            }
        }

        public bool AlmostEquals(dynamic CompareTarget, dynamic RangeWithin)
        {
            LargeSuperDec ConvertedTarget = (LargeSuperDec)CompareTarget;
            if (CompareTarget == this) { return true; }
            else
            {
                LargeSuperDec LeftRange = CompareTarget - RangeWithin;
                LargeSuperDec RightRange = CompareTarget + RangeWithin;
                if (this == LeftRange || this == RightRange) { return true; }
                else if (CompareTarget > LeftRange && CompareTarget < RightRange) { return true; }
                else { return false; }
            }
        }
    }
}
