/*	Code Created by James Michael Armstrong (NexusName:BlazesRus)
    Latest Code Release at https://github.com/BlazesRus/NifLibEnvironment
*/

using System;

//Does not need BigMath library to compile

//CSharpGlobalCode.GlobalCode_ExperimentalCode.SmallDec
namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using static GlobalCode_VariableConversionFunctions.VariableConversionFunctions;

    //Aka SuperDec_Int16_4Decimal
    public partial struct SmallDec : IComparable<SmallDec>
    {
        public SmallDec Abs()
        {
            this.DecBoolStatus = 0;
            return this;
        }

        public static SmallDec Abs(SmallDec Value)
        {
            return Value.Abs();
        }

        public SmallDec Floor()
        {
            this.DecimalStatus = 0;
            return this;
        }

        public static SmallDec Floor(SmallDec value)
        {
            return value.Floor();
        }

        // Returns rounded value with all fractional digits after specified precision cut off.
        public static SmallDec Floor(SmallDec value, int precision)
        {

            if (precision == 0)
            {
                value.DecimalStatus = 0;
            }
            else if (precision == 1)
            {
                value.DecimalStatus /= 1000;
                value.DecimalStatus *= 1000;
            }
            else if (precision == 2)
            {
                value.DecimalStatus /= 100;
                value.DecimalStatus *= 100;
            }
            else if (precision == 3)
            {
                value.DecimalStatus /= 10;
                value.DecimalStatus *= 10;
            }
            return value;
        }

        public SmallDec Ceil()
        {
            if (this.DecimalStatus != 0)
            {
                this.DecimalStatus = 0;
                this.IntValue += 1;
            }
            return this;
        }

        public static SmallDec Ceiling(SmallDec value)
        {
            return value.Ceil();
        }


        public static SmallDec Round(SmallDec val)
        {
            return val.Round();
        }

        public static SmallDec Round(SmallDec value, int Precision)
        {
            if (Precision == 0)
            {
                return value.Round();
            }
            else
            {
                return SmallDec.Round(value, 1, MidpointRounding.AwayFromZero);
            }
        }

        public static SmallDec RoundHalfDownValue(SmallDec value, int precision)
        {
            if (precision == 0)
            {
                return value.Floor();
            }
            else
            {
                return SmallDec.Round(value, precision, 1);
            }
        }

        public SmallDec Round()
        {
            if (DecimalStatus >= 5000) { this.IntValue += 1; }
            this.DecimalStatus = 0;
            return this;
        }

        public SmallDec RoundToNonZero()
        {
            if (DecimalStatus >= 5000) { this.IntValue += 1; }
            if (this.IntValue == 0) { this.IntValue = 1; }
            this.DecimalStatus = 0;
            return this;
        }

        public static SmallDec Round(SmallDec value, int precision, int RoundingMethod)
        {//https://en.wikipedia.org/wiki/Rounding
            if (precision >= 4)
            {//X.XXXX
                return value;
            }
            else if (precision == 3)
            {//X.XXX0
                ushort NonRoundedPart = (ushort)(value.DecimalStatus / 10);
                NonRoundedPart *= 10;
                ushort RoundSection = (ushort)(value.DecimalStatus - NonRoundedPart);
                SmallDec NewValue = value;
                NewValue.DecimalStatus = NonRoundedPart;
                if (RoundingMethod == 0) { }
                else if (RoundingMethod == 1)
                {
                    if (RoundSection >= 5) { NewValue.DecimalStatus += 10; }
                    if (NewValue.DecimalStatus > 9999) { NewValue.DecimalStatus -= 10000; NewValue.IntValue += 1; }
                }
                else if (RoundingMethod == 2)
                {

                }
                else if (RoundingMethod == 3)
                {

                }
                else
                {

                }
                return NewValue;
            }
            else if (precision == 2)
            {//X.XX00
                ushort NonRoundedPart = (ushort)(value.DecimalStatus / 100);
                NonRoundedPart *= 100;
                ushort RoundSection = (ushort)(value.DecimalStatus - NonRoundedPart);
                SmallDec NewValue = value;
                NewValue.DecimalStatus = NonRoundedPart;
                if (RoundingMethod == 0) { }
                else if (RoundingMethod == 1)
                {
                    if (RoundSection >= 50) { NewValue.DecimalStatus += 100; }
                    if (NewValue.DecimalStatus > 9999) { NewValue.DecimalStatus -= 10000; NewValue.IntValue += 1; }
                }
                else if(RoundingMethod==2)
                {

                }
                else if (RoundingMethod == 3)
                {

                }
                else
                {

                }
                return NewValue;
            }
            else if (precision == 1)
            {//X.X000
                ushort NonRoundedPart = (ushort)(value.DecimalStatus / 1000);
                NonRoundedPart *= 1000;
                ushort RoundSection = (ushort)(value.DecimalStatus - NonRoundedPart);
                SmallDec NewValue = value;
                NewValue.DecimalStatus = NonRoundedPart;
                if (RoundingMethod == 0) { }
                else if (RoundingMethod == 1)
                {
                    if (RoundSection >= 500) { NewValue.DecimalStatus += 1000; }
                    if (NewValue.DecimalStatus > 9999) { NewValue.DecimalStatus -= 10000; NewValue.IntValue += 1; }
                }
                else if (RoundingMethod == 2)//Floor Round
                {

                }
                else if (RoundingMethod == 3)
                {

                }
                else//round to nearest
                {

                }
                return NewValue;
            }
            else
            {
                switch (RoundingMethod)
                {
                    case 0:

                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        break;
                    default://round to nearest
                        if (value.DecimalStatus >= 5000) { value.IntValue += 1; }
                        value.DecimalStatus = 0;
                        break;
                }
                return value;
            }
        }

        public static SmallDec Round(SmallDec value, int precision, MidpointRounding RoundingMethod)
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
    }
}