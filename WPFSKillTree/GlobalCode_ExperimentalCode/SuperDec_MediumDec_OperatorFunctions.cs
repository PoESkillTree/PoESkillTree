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
		public static bool operator <(MediumSuperDec self, MediumSuperDec Value)
		{
			if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return false; }
			else
			{
				// Positive Self <= -Value
				if (Value.DecBoolStatus == 1 && self.DecBoolStatus == 0) { return false; }
				// Negative Self <= Value
				else if (Value.DecBoolStatus == 0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					ulong SelfAsInt64 = self.IntValue;
					SelfAsInt64 *= 1000000000;
					SelfAsInt64 += self.DecimalStatus;
					ulong ValueAsInt64 = Value.IntValue;
					ValueAsInt64 *= 1000000000;
					ValueAsInt64 += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt64 < ValueAsInt64;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt64 < ValueAsInt64);
					}
				}
			}
		}

		// Self Less than Value
		public static bool operator <(MediumSuperDec self, dynamic Value)
		{
			if (Value is double || Value is float || Value is decimal)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					uint WholeHalf = (uint)Value;
					//Use x Int Operation instead if Value has no decimal places
					if (WholeHalf == Value)
					{
						if (self.DecimalStatus == 0)
						{
							if (self.DecBoolStatus == 0) { return self.IntValue < WholeHalf; }
							else { return !(self.IntValue < WholeHalf); }
						}
						else
						{
							ulong SelfAsInt64 = self.DecimalStatus;
							SelfAsInt64 += self.IntValue * 1000000000;
							if (self.DecBoolStatus == 0) { return SelfAsInt64 < (WholeHalf * 1000000000); }
							else { return !(SelfAsInt64 < (WholeHalf * 1000000000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						uint Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 250000000;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 500000000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV2(Value);
						}
						ulong SelfAsInt64 = self.IntValue;
						SelfAsInt64 *= 1000000000;
						SelfAsInt64 += self.DecimalStatus;
						ulong ValueAsInt64 = WholeHalf;
						ValueAsInt64 *= 1000000000;
						ValueAsInt64 += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt64 < ValueAsInt64; }
						else { return !(SelfAsInt64 < ValueAsInt64); }
					}
				}
			}
			else if (Value is String)
			{
				//return (String)Value == (String)self;
				return false;
			}
			else
			{
				if (Value < 0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					if (Value == self.IntValue) { return true; }
					else
					{
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return self.IntValue < Value;
						}
						else
						{//Larger number = farther down into negative
							return !(self.IntValue < Value);
						}
					}
				}
			}
		}

		public static bool operator <=(MediumSuperDec self, MediumSuperDec Value)
		{
			if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return true; }
			else
			{
				// Positive Self <= -Value
				if (Value.DecBoolStatus == 1 && self.DecBoolStatus == 0) { return false; }
				// Negative Self <= Value
				else if (Value.DecBoolStatus == 0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					ulong SelfAsInt64 = self.IntValue;
					SelfAsInt64 *= 1000000000;
					SelfAsInt64 += self.DecimalStatus;
					ulong ValueAsInt64 = Value.IntValue;
					ValueAsInt64 *= 1000000000;
					ValueAsInt64 += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt64 <= ValueAsInt64;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt64 <= ValueAsInt64);
					}
				}
			}
		}

		// Self Less than or equal to Value
		public static bool operator <=(MediumSuperDec self, dynamic Value)
		{
			if (Value is double || Value is float || Value is decimal)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					uint WholeHalf = (uint)Value;
					//Use x Int Operation instead if Value has no decimal places
					if (WholeHalf == Value)
					{
						if (self.DecimalStatus == 0)
						{
							if (self.DecBoolStatus == 0) { return self.IntValue <= WholeHalf; }
							else { return !(self.IntValue <= WholeHalf); }
						}
						else
						{
							ulong SelfAsInt64 = self.DecimalStatus;
							SelfAsInt64 += self.IntValue * 1000000000;
							if (self.DecBoolStatus == 0) { return SelfAsInt64 <= (WholeHalf * 1000000000); }
							else { return !(SelfAsInt64 <= (WholeHalf * 1000000000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						uint Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 250000000;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 500000000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV2(Value);
						}
						ulong SelfAsInt64 = self.IntValue;
						SelfAsInt64 *= 1000000000;
						SelfAsInt64 += self.DecimalStatus;
						ulong ValueAsInt64 = WholeHalf;
						ValueAsInt64 *= 1000000000;
						ValueAsInt64 += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt64 <= ValueAsInt64; }
						else { return !(SelfAsInt64 <= ValueAsInt64); }
					}
				}
			}
			else if (Value is String)
			{
				//return (String)Value == (String)self;
				return false;
			}
			else
			{
				if (Value < 0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					if (Value == self.IntValue) { return true; }
					else
					{
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return self.IntValue <= Value;
						}
						else
						{//Larger number = farther down into negative
							return !(self.IntValue <= Value);
						}
					}
				}
			}
		}

		public static bool operator >(MediumSuperDec self, MediumSuperDec Value)
		{
			if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return false; }
			else
			{
				// Positive Self >= -Value
				if (Value.DecBoolStatus == 1 && self.DecBoolStatus == 0) { return true; }
				// Negative Self >= Value
				else if (Value.DecBoolStatus == 0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					ulong SelfAsInt64 = self.IntValue;
					SelfAsInt64 *= 1000000000;
					SelfAsInt64 += self.DecimalStatus;
					ulong ValueAsInt64 = Value.IntValue;
					ValueAsInt64 *= 1000000000;
					ValueAsInt64 += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt64 > ValueAsInt64;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt64 > ValueAsInt64);
					}
				}
			}
		}

		// Self Greater than Value
		public static bool operator >(MediumSuperDec self, dynamic Value)
		{
			if (Value is double || Value is float || Value is decimal)
			{
				// Positive Self >= -Value
				if (Value < 0.0 && self.DecBoolStatus == 0) { return true; }
				// Negative Self >= Value
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					uint WholeHalf = (uint)Value;
					//Use x Int Operation instead if Value has no decimal places
					if (WholeHalf == Value)
					{
						if (self.DecimalStatus == 0)
						{
							if (self.DecBoolStatus == 0) { return self.IntValue > WholeHalf; }
							else { return !(self.IntValue > WholeHalf); }
						}
						else
						{
							ulong SelfAsInt64 = self.DecimalStatus;
							SelfAsInt64 += self.IntValue * 1000000000;
							if (self.DecBoolStatus == 0) { return SelfAsInt64 > (WholeHalf * 1000000000); }
							else { return !(SelfAsInt64 > (WholeHalf * 1000000000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						uint Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 250000000;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 500000000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV2(Value);
						}
						ulong SelfAsInt64 = self.IntValue;
						SelfAsInt64 *= 1000000000;
						SelfAsInt64 += self.DecimalStatus;
						ulong ValueAsInt64 = WholeHalf;
						ValueAsInt64 *= 1000000000;
						ValueAsInt64 += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt64 >= ValueAsInt64; }
						else { return !(SelfAsInt64 > ValueAsInt64); }
					}
				}
			}
			else if (Value is String)
			{
				//return (String)Value == (String)self;
				return false;
			}
			else
			{
				// Positive Self >= -Value
				if (Value < 0 && self.DecBoolStatus == 0) { return true; }
				// Negative Self >= Value
				else if (Value >= 0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					if (Value == self.IntValue) { return false; }
					else
					{
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return self.IntValue > Value;
						}
						else
						{//Larger number = farther down into negative
							return !(self.IntValue > Value);
						}
					}
				}
			}
		}

		public static bool operator >=(MediumSuperDec self, MediumSuperDec Value)
		{
			if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return true; }
			else
			{
				// Positive Self >= -Value
				if (Value.DecBoolStatus == 1 && self.DecBoolStatus == 0) { return true; }
				// Negative Self >= Value
				else if (Value.DecBoolStatus == 0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					ulong SelfAsInt64 = self.IntValue;
					SelfAsInt64 *= 1000000000;
					SelfAsInt64 += self.DecimalStatus;
					ulong ValueAsInt64 = Value.IntValue;
					ValueAsInt64 *= 1000000000;
					ValueAsInt64 += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt64 >= ValueAsInt64;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt64 >= ValueAsInt64);
					}
				}
			}
		}

		// Self Greater than or Equal to Value
		public static bool operator >=(MediumSuperDec self, dynamic Value)
		{
			if (Value is double||Value is float||Value is decimal)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return true; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					uint WholeHalf = (uint)Value;
					//Use x Int Operation instead if Value has no decimal places
					if (WholeHalf == Value)
					{
						if (self.DecimalStatus == 0)
						{
							if (self.DecBoolStatus == 0) { return self.IntValue >= WholeHalf; }
							else { return !(self.IntValue >= WholeHalf); }
						}
						else
						{
							ulong SelfAsInt64 = self.IntValue * 1000000000;
							SelfAsInt64 += self.DecimalStatus;
							if (self.DecBoolStatus == 0) { return SelfAsInt64 >= (WholeHalf * 1000000000); }
							else { return !(SelfAsInt64 >= (WholeHalf * 1000000000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						uint Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 250000000;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 500000000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV2(Value);
						}
						ulong SelfAsInt64 = self.IntValue * 1000000000;
						SelfAsInt64 += self.DecimalStatus;
						ulong ValueAsInt64 = WholeHalf;
						ValueAsInt64 *= 10000000000;
						ValueAsInt64 += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt64 >= ValueAsInt64; }
						else { return !(SelfAsInt64 >= ValueAsInt64); }
					}
				}
			}
			else if (Value is String)
			{
				//return (String)Value == (String)self;
				return false;
			}
			else
			{
				if (Value < 0 && self.DecBoolStatus == 0) { return true; }
				else if (Value >= 0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					if (Value == self.IntValue) { return true; }
					else
					{
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return self.IntValue >= Value;
						}
						else
						{//Larger number = farther down into negative
							return !(self.IntValue >= Value);
						}
					}
				}
			}
		}

		public static bool operator ==(MediumSuperDec self, MediumSuperDec Value)
		{
			if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return true; }
			else { return false; }
		}

		// Equality operator for comparing self to int type value
		public static bool operator ==(MediumSuperDec self, dynamic Value)
		{
			if (Value is double || Value is float || Value is decimal)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					uint WholeHalf = (uint)Value;
					//Use x Int Operation instead if Value has no decimal places
					if (WholeHalf == Value)
					{
						if (self.DecimalStatus == 0)
						{
							//Use normal simple (int value) * (int value) if not dealing with anValue decimals
							return self.IntValue != WholeHalf;
						}
						else
						{
							ulong SelfAsInt64 = self.DecimalStatus;
							SelfAsInt64 += self.IntValue * 1000000000;
							return SelfAsInt64 != (WholeHalf * 1000000000);
						}
					}
					else
					{
						Value -= WholeHalf;
						uint Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 250000000;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 500000000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV2(Value);
						}
						ulong SelfAsInt64 = self.IntValue * 1000000000;
						SelfAsInt64 += self.DecimalStatus;
						ulong ValueAsInt64 = WholeHalf;
						ValueAsInt64 *= 1000000000;
						ValueAsInt64 += Decimalhalf;
						return SelfAsInt64 == ValueAsInt64;
					}
				}
			}
			else if (Value is String)
			{
				return (String)Value == (String)self;
			}
			else
			{
				if (self.DecimalStatus != 0) { return false; }
				else if (Value < 0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					if (Value == self.IntValue) { return true; }
					else { return false; }
				}
			}
		}

		public static bool operator !=(MediumSuperDec self, MediumSuperDec Value)
		{
			if (self.DecBoolStatus != Value.DecBoolStatus || self.IntValue != Value.IntValue || self.DecimalStatus != Value.DecimalStatus) { return true; }
			else { return false; }
		}

		// Inequality operator for comparing self to multiple value types
		public static bool operator !=(MediumSuperDec self, dynamic Value)
		{
			if (Value is double || Value is float || Value is decimal)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return true; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					uint WholeHalf = (uint)Value;
					//Use x Int Operation instead if Value has no decimal places
					if (WholeHalf == Value)
					{
						if (self.DecimalStatus == 0)
						{
							//Use normal simple (int value) * (int value) if not dealing with anValue decimals
							return self.IntValue != WholeHalf;
						}
						else
						{
							ulong SelfAsInt64 = self.DecimalStatus;
							SelfAsInt64 += self.IntValue * 1000000000;
							return SelfAsInt64 != (WholeHalf * 1000000000);
						}
					}
					else
					{
						Value -= WholeHalf;
						uint Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 250000000;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 500000000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV2(Value);
						}
						ulong SelfAsInt64 = self.IntValue * 1000000000;
						SelfAsInt64 += self.DecimalStatus;
						ulong ValueAsInt64 = WholeHalf;
						ValueAsInt64 *= 10000000000;
						ValueAsInt64 += Decimalhalf;
						return SelfAsInt64 != ValueAsInt64;
					}
				}
			}
			else if (Value is String)
			{
				return (String)Value != (String)self;
			}
			else
			{
				if (self.DecimalStatus != 0) { return true; }
				else if (Value < 0 && self.DecBoolStatus == 0) { return true; }
				else if (Value >= 0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					if (Value == self.IntValue) { return false; }
					else { return true; }
				}
			}
		}

		public static MediumSuperDec operator %(MediumSuperDec self, double y)
		{
			if (y == 0.0)
			{
				self.IntValue = 0;
				self.DecimalStatus = 0;
				self.DecBoolStatus = 0;
			}
			else
			{
				if (y < 0.0) { self.SwapNegativeStatus(); y *= -1.0; }
				uint WholeHalf = (uint)y;
				//Use x Int Operation instead if y has no decimal places
				if (WholeHalf == y)
				{
					if (self.DecimalStatus == 0)
					{
						//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue %= (uint)y;
					}
					else
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 %= WholeHalf;
						self.IntValue = (uint)(SelfAsInt64 / 1000000000);
						SelfAsInt64 -= self.IntValue * 1000000000;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
				}
				else
				{
					y -= WholeHalf;
					uint Decimalhalf;
					if (y == 0.25)
					{
						Decimalhalf = 250000000;
					}
					else if (y == 0.5)
					{
						Decimalhalf = 50000000;
					}
					else
					{
						Decimalhalf = ExtractDecimalHalfV2(y);
					}
					ulong SelfAsInt128 = self.IntValue;
					SelfAsInt128 *= 1000000000;
					SelfAsInt128 += self.DecimalStatus;
					ulong YAsInt256 = WholeHalf;
					YAsInt256 *= 1000000000;
					YAsInt256 += Decimalhalf;
					SelfAsInt128 %= YAsInt256;
					SelfAsInt128 /= 1000000000;
					ulong TempStorage = SelfAsInt128 / 1000000000;
					self.IntValue = (uint)TempStorage;
					TempStorage = self.IntValue;
					TempStorage *= 1000000000;
					SelfAsInt128 -= TempStorage;
					self.DecimalStatus = (uint)SelfAsInt128;
				}
				//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
				if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
			}
			return self;
		}

		public static MediumSuperDec operator %(MediumSuperDec self, dynamic y)
		{
			if (y is MediumSuperDec)
			{
				if (y.IntValue == 0 && y.DecimalStatus == 0)
				{
					self.IntValue = 0;
					self.DecimalStatus = 0;
					self.DecBoolStatus = 0;
				}
				else
				{
					if (y.DecBoolStatus == 1) { self.SwapNegativeStatus(); }
					if (self.DecimalStatus == 0 && y.DecimalStatus == 0)
					{//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue %= y.IntValue;
					}
					else if (y.DecimalStatus == 0)
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 %= y.IntValue;
						self.IntValue = (uint)(SelfAsInt64 / 1000000000);
						SelfAsInt64 -= self.IntValue * 1000000000;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
					else
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						ulong YAsInt128 = y.IntValue;
						YAsInt128 *= 1000000000;
						YAsInt128 += y.DecimalStatus;
						SelfAsInt64 %= YAsInt128;
						SelfAsInt64 /= 1000000000;
						ulong TempStorage = SelfAsInt64 / 1000000000;
						self.IntValue = (uint)TempStorage;
						TempStorage = self.IntValue;
						TempStorage *= 1000000000;
						SelfAsInt64 -= TempStorage;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			else
			{
				if (y == 0)
				{
					self.IntValue = 0;
					self.DecimalStatus = 0;
					self.DecBoolStatus = 0;
				}
				else
				{
					if (y < 0) { self.SwapNegativeStatus(); y *= -1; }
					if (self.DecimalStatus == 0)
					{//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue %= (uint)y;
					}
					else
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 %= y;
						ulong TempStorage = SelfAsInt64 / 1000000000;
						self.IntValue = (uint)TempStorage;
						TempStorage *= 1000000000;
						SelfAsInt64 -= TempStorage;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			return self;
		}

#if (!BlazesGlobalCode_Disable128BitFeatures)
        public static MediumSuperDec operator +(MediumSuperDec self, MediumSuperDec y)
        {
            //Fix potential negative zero
            if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
            return self;
        }

		public static MediumSuperDec operator +(MediumSuperDec self, dynamic y)
		{
			if (y is double||y is float||y is decimal)
			{
                bool IsYNegative = (y < 0) ? true : false;
                y = Math.Abs(y);
                uint WholeHalfOfY = (uint)Math.Floor(y);
                y -= WholeHalfOfY;
                if (WholeHalfOfY == 0) { }
                else if (self.DecBoolStatus == 1 && IsYNegative)
                {// -X - Y (ex. -8 + -6)
                    self.IntValue = self.IntValue + WholeHalfOfY;
                }
                else if (self.DecBoolStatus == 0 && IsYNegative == false)
                {
                    //X + Y (ex. 8 + 6)
                    self.IntValue = self.IntValue + WholeHalfOfY;
                }
                else
                {
                    // -X + Y
                    if (self.DecBoolStatus == 1)
                    {   //ex. -8 + 9
                        if (y > self.IntValue)
                        {
                            self.IntValue = WholeHalfOfY - self.IntValue;
                            self.DecBoolStatus = 0;
                        }
                        else
                        {//ex. -8 +  4
                            self.IntValue = self.IntValue - WholeHalfOfY;
                        }
                    }// X-Y
                    else
                    {
                        if (self.IntValue > WholeHalfOfY)
                        {//ex. 9 + -6
                            self.IntValue = self.IntValue - WholeHalfOfY;
                        }
                        else
                        {//ex. 9 + -10
                            self.IntValue = WholeHalfOfY - self.IntValue;
                            self.DecBoolStatus = 1;
                        }
                    }
                }
                //Decimal Calculation Section
                if (self.DecBoolStatus != 0 || y != 0)
                {
                    uint SecondDec = (uint)(System.Math.Abs(y) - System.Math.Abs(WholeHalfOfY)) * 1000000000;
                    // ?.XXXXXX + ?.YYYYYY
                    if (self.DecBoolStatus == 0 && IsYNegative == false)
                    {
                        //Potential Overflow check
                        BigMath.Int128 DecimalStatusTemp = self.DecimalStatus + SecondDec;
                        if (DecimalStatusTemp > 999999999)
                        {
                            DecimalStatusTemp -= 1000000000;
                            self.IntValue += 1;
                        }
                        self.DecimalStatus = (uint)DecimalStatusTemp;
                    }
                    // -?.XXXXXX - ?.YYYYYY
                    else if (self.DecBoolStatus == 1 && IsYNegative == true)
                    {
                        //Potential Overflow check
                        BigMath.Int128 DecimalStatusTemp = self.DecimalStatus + SecondDec;
                        if (DecimalStatusTemp > 999999999)
                        {
                            DecimalStatusTemp -= 1000000000;
                            self.IntValue -= 1;
                        }
                        self.DecimalStatus = (uint)DecimalStatusTemp;
                    }
                    else
                    {
                        if (IsYNegative)
                        {
                            // ex. 0.6 + -0.5
                            if (self.DecimalStatus >= SecondDec)
                            {
                                self.DecimalStatus = self.DecimalStatus - SecondDec;
                            }// ex. 0.6 + -.7
                            else
                            {
                                self.DecimalStatus = SecondDec - self.DecimalStatus;
                                if (self.IntValue == 0)
                                {
                                    self.DecBoolStatus = 1;
                                }
                                else
                                {
                                    self.IntValue -= 1;
                                }
                            }
                        }
                        else
                        {
                            if (self.DecimalStatus >= SecondDec)
                            {
                                self.DecimalStatus = self.DecimalStatus - SecondDec;
                            }// ex. -1.6 + 0.7
                            else
                            {
                                self.DecimalStatus = SecondDec - self.DecimalStatus;
                                if (self.IntValue == 0)
                                {
                                    self.DecBoolStatus = 0;
                                }
                                else
                                {
                                    self.IntValue -= 1;
                                }
                            }
                        }
                    }
                }
            }
			else
			{
				if (self.DecBoolStatus == 1 && y < 0)
				{// -X - Y (ex. -8 + -6)
					self.IntValue = self.IntValue + (uint)Math.Abs(y);
				}
				else if (self.DecBoolStatus == 0 && y >= 0)
				{
					//X + Y (ex. 8 + 6)
					self.IntValue = self.IntValue + (uint)y;
				}
				else
				{
					// -X + Y
					if (self.DecBoolStatus == 1)
					{   //ex. -8 + 9
						if (y > self.IntValue)
						{
							self.IntValue = (uint)y - self.IntValue;
							self.DecBoolStatus = 0;
						}
						else
						{//ex. -8 +  4
							self.IntValue = self.IntValue - (uint)y;
						}
					}// X-Y
					else
					{
						uint TempY = Math.Abs(y);
						if (self.IntValue > TempY)
						{//ex. 9 + -6
							self.IntValue = self.IntValue - TempY;
						}
						else
						{//ex. 9 + -10
							self.IntValue = TempY - self.IntValue;
							self.DecBoolStatus = 1;
						}
					}
				}
			}
			//Fix potential negative zero
			if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
			return self;
		}

        public static MediumSuperDec operator -(MediumSuperDec self, MediumSuperDec y)
        {
            bool IsYNegative = (y.DecBoolStatus == 1) ? true : false;
            //ex. -9 - 9
            if (self.DecBoolStatus == 1 && IsYNegative == false)
            {// -X - Y
                self.IntValue = self.IntValue + y.IntValue;
            }//ex. 9 - (-1)
            else if (self.DecBoolStatus == 0 && IsYNegative == true)
            {
                //X - (-Y)
                self.IntValue = self.IntValue + y.IntValue;
            }
            else
            {
                // X - (Y)
                if (self.DecBoolStatus == 0)
                {
                    // ex. 8 - 9
                    if (y.IntValue > self.IntValue)
                    {
                        self.IntValue = y.IntValue - self.IntValue;
                        self.DecBoolStatus = 1;
                    } //ex. 8 - 7
                    else
                    {
                        self.IntValue = self.IntValue - y.IntValue;
                    }
                }// -X - (Y)
                else
                {
                    // ex. -8 - (-9)
                    if (self.IntValue > y.IntValue)
                    {
                        self.IntValue = y.IntValue - self.IntValue;
                        self.DecBoolStatus = 0;
                    }
                    else
                    {//ex. -8 - (-5)
                        self.IntValue = self.IntValue - y.IntValue;
                    }
                }
            }
            //Decimal Section
            if (self.DecimalStatus != 0 || y.DecimalStatus != 0)
            {
                // ex. -0.5 - 0.6
                if (self.DecBoolStatus == 1 && IsYNegative == false)
                {
                    //Potential Overflow check
                    uint DecimalStatusTemp = self.DecimalStatus + y.DecimalStatus;
                    if (DecimalStatusTemp > 999999999)
                    {
                        DecimalStatusTemp -= 1000000000;
                        self.IntValue += 1;
                    }
                    self.DecimalStatus = DecimalStatusTemp;
                }// ex. 0.5 - (-0.6)
                else if (self.DecBoolStatus == 0 && IsYNegative)
                {
                    //Potential Overflow check
                    uint DecimalStatusTemp = self.DecimalStatus + y.DecimalStatus;
                    if (DecimalStatusTemp > 999999999)
                    {
                        DecimalStatusTemp -= 1000000000;
                        self.IntValue -= 1;
                    }
                    self.DecimalStatus = DecimalStatusTemp;
                }
                else
                {
                    if (IsYNegative)
                    {// ex. -0.7 - (-0.6)
                        if (self.DecimalStatus >= y.DecimalStatus)
                        {
                            self.DecimalStatus = self.DecimalStatus - y.DecimalStatus;
                        }
                        else
                        {
                            self.DecimalStatus = y.DecimalStatus - self.DecimalStatus;
                            if (self.IntValue == 0)
                            {
                                self.DecBoolStatus = 0;
                            }
                            else
                            {
                                self.IntValue -= 1;
                            }
                        }
                    }
                    else
                    { //ex  0.6 - 0.5
                        if (self.DecimalStatus >= y.DecimalStatus)
                        {
                            self.DecimalStatus = self.DecimalStatus - y.DecimalStatus;
                        }
                        else
                        {
                            self.DecimalStatus = y.DecimalStatus - self.DecimalStatus;
                            if (self.IntValue == 0)
                            {
                                self.DecBoolStatus = 1;
                            }
                            else
                            {
                                self.IntValue -= 1;
                            }
                        }
                    }
                }
            }
            //Fix potential negative zero
            if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
            return self;
        }

		public static MediumSuperDec operator -(MediumSuperDec self, dynamic y)
		{
			if (y is double||y is float||y is decimal)
			{
                bool IsYNegative = (y < 0) ? true : false;
                y = Math.Abs(y);
                uint WholeHalfOfY = (uint)Math.Floor(y);
                y -= WholeHalfOfY;
                if (WholeHalfOfY == 0) { }
                //ex. -9 - 9
                else if (self.DecBoolStatus == 1 && IsYNegative == false)
                {// -X - Y
                    self.IntValue = self.IntValue + WholeHalfOfY;
                }//ex. 9 - (-1)
                else if (self.DecBoolStatus == 0 && IsYNegative)
                {
                    //X - (-Y)
                    self.IntValue = self.IntValue + WholeHalfOfY;
                }
                else
                {
                    // X - (Y)
                    if (self.DecBoolStatus == 0)
                    {
                        // ex. 8 - 9
                        if (WholeHalfOfY > self.IntValue)
                        {
                            self.IntValue = WholeHalfOfY - self.IntValue;
                            self.DecBoolStatus = 1;
                        } //ex. 8 - 7
                        else
                        {
                            self.IntValue = self.IntValue - WholeHalfOfY;
                        }
                    }// -X - (Y)
                    else
                    {
                        // ex. -8 - (-9)
                        if (self.IntValue > WholeHalfOfY)
                        {
                            self.IntValue = WholeHalfOfY - self.IntValue;
                            self.DecBoolStatus = 0;
                        }
                        else
                        {//ex. -8 - (-5)
                            self.IntValue = self.IntValue - WholeHalfOfY;
                        }
                    }
                }
                //Decimal Calculation Section
                uint SecondDec = (uint)(System.Math.Abs(y) - System.Math.Abs(WholeHalfOfY)) * 1000000000;
                if (self.DecimalStatus != 0 || SecondDec != 0)
                {
                    // ex. -0.5 - 0.6
                    if (self.DecBoolStatus == 1 && IsYNegative == false)
                    {
                        //Potential Overflow check
                        uint DecimalStatusTemp = self.DecimalStatus + SecondDec;
                        if (DecimalStatusTemp > 999999999)
                        {
                            DecimalStatusTemp -= 1000000000;
                            self.IntValue += 1;
                        }
                        self.DecimalStatus = DecimalStatusTemp;
                    }// ex. 0.5 - (-0.6)
                    else if (self.DecBoolStatus == 0 && IsYNegative)
                    {
                        //Potential Overflow check
                        uint DecimalStatusTemp = self.DecimalStatus + SecondDec;
                        if (DecimalStatusTemp > 999999999)
                        {
                            DecimalStatusTemp -= 1000000000;
                            self.IntValue -= 1;
                        }
                        self.DecimalStatus = DecimalStatusTemp;
                    }
                    else
                    {
                        if (IsYNegative)
                        {// ex. -0.7 - (-0.6)
                            if (self.DecimalStatus >= SecondDec)
                            {
                                self.DecimalStatus = self.DecimalStatus - SecondDec;
                            }
                            else
                            {
                                self.DecimalStatus = SecondDec - self.DecimalStatus;
                                if (self.IntValue == 0)
                                {
                                    self.DecBoolStatus = 0;
                                }
                                else
                                {
                                    self.IntValue -= 1;
                                }
                            }
                        }
                        else
                        { //ex  0.6 - 0.5
                            if (self.DecimalStatus >= SecondDec)
                            {
                                self.DecimalStatus = self.DecimalStatus - SecondDec;
                            }
                            else
                            {
                                self.DecimalStatus = SecondDec - self.DecimalStatus;
                                if (self.IntValue == 0)
                                {
                                    self.DecBoolStatus = 1;
                                }
                                else
                                {
                                    self.IntValue -= 1;
                                }
                            }
                        }
                    }
                }
            }
			else
			{
				//ex. -9 - 9
				if (self.DecBoolStatus == 1 && y >= 0)
				{// -X - Y
					self.IntValue = self.IntValue + (uint)y;
				}//ex. 9 - (-1)
				else if (self.DecBoolStatus == 0 && y < 0)
				{
					//X - (-Y)
					self.IntValue = self.IntValue + (uint)Math.Abs(y);
				}
				else
				{
					// X - (Y)
					if (self.DecBoolStatus == 0)
					{
						// ex. 8 - 9
						if (y > self.IntValue)
						{
							self.IntValue = (uint)y - self.IntValue;
							self.DecBoolStatus = 1;
						} //ex. 8 - 7
						else
						{
							self.IntValue = self.IntValue - (uint)y;
						}
					}// -X - (Y)
					else
					{
						uint TempY = (uint)Math.Abs(y);
						// ex. -8 - (-9)
						if (self.IntValue > TempY)
						{
							self.IntValue = TempY - self.IntValue;
							self.DecBoolStatus = 0;
						}
						else
						{//ex. -8 - (-5)
							self.IntValue = self.IntValue - TempY;
						}
					}
				}
			}
			//Fix potential negative zero
			if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
			return self;
		}

        public static MediumSuperDec operator *(MediumSuperDec self, MediumSuperDec y)
        {
            if (y.IntValue == 0 && y.DecimalStatus == 0)
            {
                self.IntValue = 0;
                self.DecimalStatus = 0;
                self.DecBoolStatus = 0;
            }
            else
            {
                if (y.DecBoolStatus == 1) { self.SwapNegativeStatus(); }
                if (self.DecimalStatus == 0 && y.DecimalStatus == 0)
                {//Use normal simple (int value) * (int value) if not dealing with any decimals
                    self.IntValue *= y.IntValue;
                }
                else if (y.DecimalStatus == 0)
                {
                    ulong SelfAsInt64 = self.DecimalStatus;
                    SelfAsInt64 += self.IntValue * 1000000000;
                    SelfAsInt64 *= y.IntValue;
                    self.IntValue = (uint)(SelfAsInt64 / 1000000000);
                    SelfAsInt64 -= self.IntValue * 1000000000;
                    self.DecimalStatus = (uint)SelfAsInt64;
                }
                else
                {
                    //((self.IntValue * 1000000000)+self.DecimalStatus)*(DecimalAsInt+(WholeHalf*1000000000))/1000000000 = ((self.IntValue*1000000000)+self.DecimalStatus))
                    BigMath.Int256 SelfAsInt128 = self.IntValue;
                    SelfAsInt128 *= 1000000000;
                    SelfAsInt128 += self.DecimalStatus;
                    BigMath.Int256 YAsInt256 = y.IntValue;
                    YAsInt256 *= 1000000000;
                    YAsInt256 += y.DecimalStatus;
                    SelfAsInt128 *= YAsInt256;
                    SelfAsInt128 /= 1000000000;
                    BigMath.Int256 TempStorage = SelfAsInt128 / 1000000000;
                    self.IntValue = (uint)TempStorage;
                    TempStorage = self.IntValue;
                    TempStorage *= 1000000000;
                    SelfAsInt128 -= TempStorage;
                    self.DecimalStatus = (uint)SelfAsInt128;
                }
                //Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
                if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
            }
            return self;
        }

        public static MediumSuperDec operator *(MediumSuperDec self, dynamic y)
		{
            if (y is double||y is float||y is decimal)
            {
                if (y == 0.0)
                {
                    self.IntValue = 0;
                    self.DecimalStatus = 0;
                    self.DecBoolStatus = 0;
                }
                else
                {
                    if (y < 0.0) { self.SwapNegativeStatus(); y *= -1.0; }
                    uint WholeHalf = (uint)y;
                    //Use x Int Operation instead if y has no decimal places
                    if (WholeHalf == y)
                    {
                        if (self.DecimalStatus == 0)
                        {
                            //Use normal simple (int value) * (int value) if not dealing with any decimals
                            self.IntValue *= (uint)y;
                        }
                        else
                        {
                            ulong SelfAsInt64 = self.DecimalStatus;
                            SelfAsInt64 += self.IntValue * 1000000000;
                            SelfAsInt64 *= WholeHalf;
                            self.IntValue = (uint)(SelfAsInt64 / 1000000000);
                            SelfAsInt64 -= self.IntValue * 1000000000;
                            self.DecimalStatus = (uint)SelfAsInt64;
                        }
                    }
                    else
                    {
                        y -= WholeHalf;
                        uint Decimalhalf;
                        if (y == 0.25)
                        {
                            Decimalhalf = 250000000;
                        }
                        else if (y == 0.5)
                        {
                            Decimalhalf = 50000000;
                        }
                        else
                        {
                            Decimalhalf = ExtractDecimalHalfV2(y);
                        }
                        BigMath.Int128 SelfAsInt128 = self.IntValue;
                        SelfAsInt128 *= 1000000000;
                        SelfAsInt128 += self.DecimalStatus;
                        ulong YAsInt64 = WholeHalf;
                        YAsInt64 *= 1000000000;
                        YAsInt64 += Decimalhalf;
                        SelfAsInt128 *= YAsInt64;
                        SelfAsInt128 /= 1000000000;
                        BigMath.Int128 TempStorage = SelfAsInt128 / 1000000000;
                        self.IntValue = (uint)TempStorage;
                        TempStorage = self.IntValue;
                        TempStorage *= 1000000000;
                        SelfAsInt128 -= TempStorage;
                        self.DecimalStatus = (uint)SelfAsInt128;
                    }
                    //Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
                    if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
                }
                return self;
            }
            else
			{
				if (y == 0)
				{
					self.IntValue = 0;
					self.DecimalStatus = 0;
					self.DecBoolStatus = 0;
				}
				else
				{
					if (y < 0) { self.SwapNegativeStatus(); y *= -1; }
					if (self.DecimalStatus == 0)
					{//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue *= (uint)y;
					}
					else
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 *= y;
						ulong TempStorage = SelfAsInt64 / 1000000000;
						self.IntValue = (uint)TempStorage;
						TempStorage *= 1000000000;
						SelfAsInt64 -= TempStorage;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			return self;
		}

		public static MediumSuperDec operator /(MediumSuperDec self, double y)
		{
			if (y == 0)
			{
				Console.WriteLine("Prevented dividing by zero");
			}
			else
			{
				if (y < 0.0) { self.SwapNegativeStatus(); y *= -1.0; }
				uint WholeHalf = (uint)y;
				//Use x Int Operation instead if y has no decimal places
				if (WholeHalf == y)
				{
					if (self.DecimalStatus == 0)
					{
						//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue /= WholeHalf;
					}
					else
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 /= WholeHalf;
						self.IntValue = (uint)(SelfAsInt64 / 1000000000);
						SelfAsInt64 -= self.IntValue * 1000000000;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
				}
				else
				{
					y -= WholeHalf;
					uint Decimalhalf;
					if (y == 0.25)
					{
						Decimalhalf = 250000000;
					}
					else if (y == 0.5)
					{
						Decimalhalf = 50000000;
					}
					else
					{
						Decimalhalf = ExtractDecimalHalfV2(y);
					}
					BigMath.Int128 SelfAsInt128 = self.IntValue;
					SelfAsInt128 *= 1000000000;
					SelfAsInt128 += self.DecimalStatus;
					ulong YAsInt64 = WholeHalf;
					YAsInt64 *= 1000000000;
					YAsInt64 += Decimalhalf;
					SelfAsInt128 /= YAsInt64;
					SelfAsInt128 /= 1000000000;
					BigMath.Int128 TempStorage = SelfAsInt128 / 1000000000;
					self.IntValue = (uint)TempStorage;
					TempStorage = self.IntValue;
					TempStorage *= 1000000000;
					SelfAsInt128 -= TempStorage;
					self.DecimalStatus = (uint)SelfAsInt128;
				}
				//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
				if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
			}
			return self;
		}

		public static MediumSuperDec operator /(MediumSuperDec self, dynamic y)
		{
			if (y is MediumSuperDec)
			{
				if (y.IntValue == 0 && y.DecimalStatus == 0)
				{
					Console.WriteLine("Prevented dividing by zero");
				}
				else
				{
					if (y.DecBoolStatus == 1) { self.SwapNegativeStatus(); }
					if (self.DecimalStatus == 0 && y.DecimalStatus == 0)
					{//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue /= y.IntValue;
					}
					else if (y.DecimalStatus == 0)
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 /= y.IntValue;
						self.IntValue = (uint)(SelfAsInt64 / 1000000000);
						SelfAsInt64 -= self.IntValue * 1000000000;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
					else
					{
						BigMath.Int128 SelfAsInt128 = self.IntValue;
						SelfAsInt128 *= 1000000000;
						SelfAsInt128 += self.DecimalStatus;
						ulong YAsInt256 = y.IntValue;
						YAsInt256 *= 1000000000;
						YAsInt256 += y.DecimalStatus;
						SelfAsInt128 /= YAsInt256;
						SelfAsInt128 /= 1000000000;
						BigMath.Int128 TempStorage = SelfAsInt128 / 1000000000;
						self.IntValue = (uint)TempStorage;
						TempStorage = self.IntValue;
						TempStorage *= 1000000000;
						SelfAsInt128 -= TempStorage;
						self.DecimalStatus = (uint)SelfAsInt128;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			else
			{
				if (y == 0)
				{
					Console.WriteLine("Prevented dividing by zero");
				}
				else
				{
					if (y < 0) { self.SwapNegativeStatus(); y *= -1; }
					if (self.DecimalStatus == 0)
					{//Use normal simple (int value) * (int value) if not dealing with any decimals
						self.IntValue /= (uint)y;
					}
					else
					{
						ulong SelfAsInt64 = self.DecimalStatus;
						SelfAsInt64 += self.IntValue * 1000000000;
						SelfAsInt64 /= y;
						ulong TempStorage = SelfAsInt64 / 1000000000;
						self.IntValue = (uint)TempStorage;
						TempStorage *= 1000000000;
						SelfAsInt64 -= TempStorage;
						self.DecimalStatus = (uint)SelfAsInt64;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0000000000000000001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			return self;
		}

		public static MediumSuperDec operator *(dynamic y, MediumSuperDec self)
		{
			MediumSuperDec YAsSuperDec = (MediumSuperDec)y;
			YAsSuperDec += self;
			return YAsSuperDec;
		}

		public static MediumSuperDec operator /(dynamic y, MediumSuperDec self)
		{
			MediumSuperDec YAsSuperDec = (MediumSuperDec)y;
			YAsSuperDec += self;
			return YAsSuperDec;
		}
#endif

		public static MediumSuperDec operator -(dynamic y, MediumSuperDec self)
		{
			MediumSuperDec YAsSuperDec = (MediumSuperDec)y;
			YAsSuperDec -= self;
			return YAsSuperDec;
		}

		public static MediumSuperDec operator +(dynamic y, MediumSuperDec self)
		{
			MediumSuperDec YAsSuperDec = (MediumSuperDec)y;
			YAsSuperDec += self;
			return YAsSuperDec;
		}

		public static MediumSuperDec operator -(MediumSuperDec Value)
		{
			if (Value.DecBoolStatus%2==1)//ODD
			{
				Value.DecBoolStatus -= 1;
			}
			else
			{
				Value.DecBoolStatus += 1;
			}
			return Value;
		}

	}
}
