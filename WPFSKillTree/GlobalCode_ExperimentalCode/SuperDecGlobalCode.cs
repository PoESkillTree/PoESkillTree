/*	Code Created by James Michael Armstrong (NexusName:BlazesRus)
    Latest Code Release at https://github.com/BlazesRus/NifLibEnvironment
*/

using System;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows;
    using static GlobalCode_VariableConversionFunctions.VariableConversionFunctions;

    public class SuperDecGlobalCode
    {
        // SuperDec generic reused code section
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


        //End of SuperDec generic reused code section
        //Trigonometrical method section
        public static double RadianToAngleAsDouble(dynamic Value)
        {
            double ConvertedValue = (double)Value;

            return 0.0;
        }
        public static double AngleToRadianAsDouble(dynamic Value)
        {
            double ConvertedValue = (double)Value;

            return 0.0;
        }
        public static double SinFromAngleAsDouble(dynamic Value)
        {
            double ConvertedValue = (double)Value%360.0;

            return 0.0;
        }
        public static double ConFromAngleAsDouble(dynamic Value)
        {
            double ConvertedValue = (double)Value%360.0;

            return 0.0;
        }
        public static double TransFromAngleAsDouble(dynamic Value)
        {
            double ConvertedValue = (double)Value%360.0;

            return 0.0;
        }
        //end of Trigonometrical method section	
    }
    public interface SuperDecBase
    {
        //public static SuperDecBase Sum(IEnumerable<SuperDecBase> Value);
    }
}