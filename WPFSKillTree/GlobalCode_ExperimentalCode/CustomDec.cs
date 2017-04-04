/*	Code Created by James Michael Armstrong (NexusName:BlazesRus)
    Latest Code Release at https://github.com/BlazesRus/NifLibEnvironment
*/
using System;

//Requires BigMath library to compile

//CSharpGlobalCode.GlobalCode_ExperimentalCode.SuperDec_ExtraDec32_19Decimal
namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using static GlobalCode_VariableConversionFunctions.VariableConversionFunctions;


    //Aka SuperDec_ExtraDec32_19Decimal
    public partial struct ModerateSuperDec : IComparable<ModerateSuperDec>
    {
        //0 = Positive;1=Negative;Other states at higher then 1
        public byte DecBoolStatus;
        
        //Stores decimal section info
        public ulong DecimalStatus;

        public uint IntValue;

        public static ModerateSuperDec Sum(IEnumerable<ModerateSuperDec> Value)
        {
            ModerateSuperDec TotalSum = ModerateSuperDec.Zero;
            foreach (var Element in Value)
            {
                TotalSum += Element;
            }
            return TotalSum;
        }

        public ModerateSuperDec Abs()
        {
            this.DecBoolStatus = 0;
            return this;
        }

        public ModerateSuperDec Floor()
        {
            this.DecimalStatus = 0;
            return this;
        }

        public ModerateSuperDec Ceil()
        {
            if (this.DecimalStatus != 0)
            {
                this.DecimalStatus = 0;
                this.IntValue += 1;
            }
            return this;
        }

        public ModerateSuperDec Round()
        {
            if (DecimalStatus >= 5000000000000000000) { this.IntValue += 1; }
            this.DecimalStatus = 0;
            return this;
        }

        public ModerateSuperDec(dynamic Value)
        {
            if(Value is double || Value is float || Value is decimal)
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
            else if(Value is sbyte||Value is ushort||Value is int||Value is long)
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

        public static explicit operator ModerateSuperDec(SmallDec Value)
        {
            ModerateSuperDec NewSelf;
            NewSelf.IntValue = Value.IntValue;
            NewSelf.DecimalStatus = (ulong)Value.DecimalStatus * 1000000000000000;
            NewSelf.DecBoolStatus = Value.DecBoolStatus;
            return NewSelf;
        }

        public ModerateSuperDec(SmallDec Value)
        {
            IntValue = Value.IntValue;
            DecimalStatus = (ulong)Value.DecimalStatus * 1000000000000000;
            DecBoolStatus = Value.DecBoolStatus;
        }

        //explicit Conversion from this to float
        public static explicit operator float(ModerateSuperDec self)
        {
            float Value = 0.0f;
            Value += self.IntValue;
            Value += (float)(self.DecimalStatus * 0.0000000000000000001);
            if (self.DecBoolStatus == 1) { Value *= -1; }
            return Value;
        }

        //explicit Conversion from this to double
        public static explicit operator double(ModerateSuperDec self)
        {
            double Value = 0.0;
            Value += self.IntValue;
            Value += (self.DecimalStatus * 0.0000000000000000001);
            if (self.DecBoolStatus == 1) { Value *= -1; }
            return Value;
        }

        //explicit Conversion from this to int
        public static explicit operator int(ModerateSuperDec self)
        {
            int Value = (int)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //explicit Conversion from this to int 64
        public static explicit operator long(ModerateSuperDec self)
        {
            long Value = self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //explicit Conversion from this to uint
        public static explicit operator uint(ModerateSuperDec self)
        {
            return self.IntValue;
        }

        //explicit Conversion from this to unsigned int 64
        public static explicit operator ulong(ModerateSuperDec self)
        {
            return self.IntValue;
        }

        //explicit Conversion from this to byte
        public static explicit operator byte(ModerateSuperDec self)
        {
            byte Value = (byte)self.IntValue;
            return Value;
        }

        //explicit Conversion from this to sbyte
        public static explicit operator sbyte(ModerateSuperDec self)
        {
            sbyte Value = (sbyte)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //explicit Conversion from this to ushort
        public static explicit operator ushort(ModerateSuperDec self)
        {
            ushort Value = (ushort)self.IntValue;
            return Value;
        }

        //explicit Conversion from this to short
        public static explicit operator short(ModerateSuperDec self)
        {
            short Value = (short)self.IntValue;
            if (self.DecimalStatus == 1) { Value *= -1; }
            return Value;
        }

        //explicit Conversion from this to string
        static public explicit operator string(ModerateSuperDec self)
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
            ulong DecimalHalf = self.DecimalStatus;
            for (sbyte Index = 18; Index >= 0; Index--)
            {
                CurrentDigit = (byte)(DecimalHalf / Math.Pow(10, Index));
                DecimalHalf -= (ulong)(CurrentDigit * Math.Pow(10, Index));
                Value += DigitAsChar(CurrentDigit);
            }
            return Value;
        }

        //explicit Conversion from this to float
        public static explicit operator ModerateSuperDec(double Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(int Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(uint Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(long Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(ulong Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(short Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(ushort Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(byte Value)
        {
            return new ModerateSuperDec(Value);
        }

        public static explicit operator ModerateSuperDec(sbyte Value)
        {
            return new ModerateSuperDec(Value);
        }


        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) { return false; }

            try
            {
                return this == (ModerateSuperDec)obj;
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
            if (DecBoolStatus%2==0) { DecBoolStatus += 1; }
            else { DecBoolStatus -= 1; }
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        private static ModerateSuperDec MaximumValue()
        {
            ModerateSuperDec NewSelf;
            NewSelf.IntValue = 4294967295;
            NewSelf.DecimalStatus = 9999999999999999999;
            NewSelf.DecBoolStatus = 0;
            return NewSelf;
        }

        //Returns value of highest non-infinite/Special Decimal State Value that can store
        public static readonly ModerateSuperDec Maximum = MaximumValue();

        //Returns value of lowest non-infinite/Special Decimal State Value that can store
        private static ModerateSuperDec MinimumValue()
        {
            ModerateSuperDec NewSelf;
            NewSelf.IntValue = 4294967295;
            NewSelf.DecimalStatus = 9999999999999999999;
            NewSelf.DecBoolStatus = 1;
            return NewSelf;
        }

        //Returns value of lowest non-infinite/Special Decimal State Value that can store
        public static readonly ModerateSuperDec Minimum = MinimumValue();

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

        public static ModerateSuperDec FloatParse(string s, IFormatProvider provider)
        {
            ModerateSuperDec NewSelf = (ModerateSuperDec) float.Parse(s, provider);
            return NewSelf;
        }

        public static ModerateSuperDec DoubleParse(string s, IFormatProvider provider)
        {
            ModerateSuperDec NewSelf = (ModerateSuperDec) double.Parse(s, provider);
            return NewSelf;
        }

        public string ToString(string s, IFormatProvider provider)
        {
            //float SelfAsFloat = this;
            //string StringValue = SelfAsFloat.ToString(s, provider);
            //return StringValue;
            return (string)this;
        }

        public static dynamic ConditionalReturn(bool Condition, dynamic X, dynamic Y)
        {
            if (Condition)	{	return X;	}
            else	{	return Y;	}
        }

        public dynamic DynamicConversion() {return this;}

        public bool IsNull()
        {
            if (DecBoolStatus == 202) { return true; }
            else { return false; }
        }

        public byte GetBoolStatus()
        {
            return DecBoolStatus;
        }

        public float AsFloat() { return (float)this; }
        public double AsDouble() { return (double)this; }
        public int AsInt() { return (int)this; }
        public string AsString() { return (string)this; }

        public string ToString(CultureInfo invariantCulture)
        {
            return (string)this;
        }


        private static ModerateSuperDec ZeroValue()
        {
            ModerateSuperDec NewSelf;
            NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0; NewSelf.DecBoolStatus = 0;
            return NewSelf;
        }

        public static readonly ModerateSuperDec Zero = ZeroValue();

        private static ModerateSuperDec NaNValue()
        {
            ModerateSuperDec NewSelf;
            NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0;
#if (BlazesGlobalCode_ModerateSuperDec_EnableSpecialDecStates)
            NewSelf.DecBoolStatus = 202;
#else
            NewSelf.DecBoolStatus = 0;
#endif
            return NewSelf;
        }

        public static readonly ModerateSuperDec NaN = NaNValue();

        int IComparable<ModerateSuperDec>.CompareTo(ModerateSuperDec other)
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
            ModerateSuperDec ConvertedTarget = (ModerateSuperDec)CompareTarget;
            if (CompareTarget == this) { return true; }
            else
            {
                ModerateSuperDec LeftRange = CompareTarget - RangeWithin;
                ModerateSuperDec RightRange = CompareTarget + RangeWithin;
                if (this == LeftRange || this == RightRange) { return true; }
                else if (CompareTarget > LeftRange && CompareTarget < RightRange) { return true; }
                else { return false; }
            }
        }
    }
}