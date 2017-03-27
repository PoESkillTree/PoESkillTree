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
		// Self Less than Value
		public static bool operator <(SmallDec self, SmallDec Value)
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
					ulong SelfAsInt = self.IntValue;
					SelfAsInt *= 10000;
					SelfAsInt += self.DecimalStatus;
					ulong ValueAsInt = Value.IntValue;
					ValueAsInt *= 10000;
					ValueAsInt += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt < ValueAsInt;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt < ValueAsInt);
					}
				}
			}
		}

		// Self Less than or equal to Value
		public static bool operator <=(SmallDec self, SmallDec Value)
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
					uint SelfAsInt = self.IntValue;
					SelfAsInt *= 10000;
					SelfAsInt += self.DecimalStatus;
					uint ValueAsInt = Value.IntValue;
					ValueAsInt *= 10000;
					ValueAsInt += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt <= ValueAsInt;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt <= ValueAsInt);
					}
				}
			}
		}

		// Self Greater than Value
		public static bool operator >(SmallDec self, SmallDec Value)
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
					uint SelfAsInt = self.IntValue;
					SelfAsInt *= 10000;
					SelfAsInt += self.DecimalStatus;
					uint ValueAsInt = Value.IntValue;
					ValueAsInt *= 10000;
					ValueAsInt += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt > ValueAsInt;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt > ValueAsInt);
					}
				}
			}
		}

		// Self Greater than or Equal to Value
		public static bool operator >=(SmallDec self, SmallDec Value)
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
					ulong SelfAsInt = self.IntValue;
					SelfAsInt *= 10000;
					SelfAsInt += self.DecimalStatus;
					ulong ValueAsInt = Value.IntValue;
					ValueAsInt *= 10000;
					ValueAsInt += Value.DecimalStatus;
					//Both are either positive or negative numbers
					if (self.DecBoolStatus == 0)
					{
						return SelfAsInt >= ValueAsInt;
					}
					else
					{//Larger number = farther down into negative
						return !(SelfAsInt >= ValueAsInt);
					}
				}
			}
		}

		// Equality operator for comparing self to int type value
		public static bool operator ==(SmallDec self, SmallDec Value)
		{
			if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return true; }
			else { return false; }
		}

		// Inequality operator for comparing self to multiple value types
		public static bool operator !=(SmallDec self, SmallDec Value)
		{
			if (self.DecBoolStatus != Value.DecBoolStatus || self.IntValue != Value.IntValue || self.DecimalStatus != Value.DecimalStatus) { return true; }
			else { return false; }
		}

		// Self Less than Value
		public static bool operator <(SmallDec self, dynamic Value)
		{
			if (Value is SmallDec)
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
						ulong SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						ulong ValueAsInt = Value.IntValue;
						ValueAsInt *= 10000;
						ValueAsInt += Value.DecimalStatus;
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return SelfAsInt < ValueAsInt;
						}
						else
						{//Larger number = farther down into negative
							return !(SelfAsInt < ValueAsInt);
						}
					}
				}
			}
			else if (Value is double)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					ushort WholeHalf = (ushort)Value;
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
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							if (self.DecBoolStatus == 0) { return SelfAsInt < (WholeHalf * 1000000000); }
							else { return !(SelfAsInt < (WholeHalf * 1000000000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						ushort Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(Value);
						}
						uint SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = WholeHalf;
						ValueAsInt *= 10000;
						ValueAsInt += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt < ValueAsInt; }
						else { return !(SelfAsInt < ValueAsInt); }
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

		public static bool operator <(dynamic Value, SmallDec self)
		{
			return self > Value;
		}

		// Self Less than or equal to Value
		public static bool operator <=(SmallDec self, dynamic Value)
		{
			if (Value is SmallDec)
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
						uint SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = Value.IntValue;
						ValueAsInt *= 10000;
						ValueAsInt += Value.DecimalStatus;
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return SelfAsInt <= ValueAsInt;
						}
						else
						{//Larger number = farther down into negative
							return !(SelfAsInt <= ValueAsInt);
						}
					}
				}
			}
			else if (Value is double)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					ushort WholeHalf = (ushort)Value;
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
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							if (self.DecBoolStatus == 0) { return SelfAsInt <= (uint)(WholeHalf * 1000); }
							else { return !(SelfAsInt <= (uint)(WholeHalf * 10000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						ushort Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(Value);
						}
						uint SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = WholeHalf;
						ValueAsInt *= 10000;
						ValueAsInt += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt <= ValueAsInt; }
						else { return !(SelfAsInt <= ValueAsInt); }
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

		public static bool operator <=(dynamic Value, SmallDec self)
		{
			return self >= Value;
		}

		// Self Greater than Value
		public static bool operator >(SmallDec self, dynamic Value)
		{
			if (Value is SmallDec)
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
						uint SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = Value.IntValue;
						ValueAsInt *= 10000;
						ValueAsInt += Value.DecimalStatus;
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return SelfAsInt > ValueAsInt;
						}
						else
						{//Larger number = farther down into negative
							return !(SelfAsInt > ValueAsInt);
						}
					}
				}
			}
			else if (Value is double)
			{
				// Positive Self >= -Value
				if (Value < 0.0 && self.DecBoolStatus == 0) { return true; }
				// Negative Self >= Value
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					ushort WholeHalf = (ushort)Value;
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
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							if (self.DecBoolStatus == 0) { return SelfAsInt > (uint)(WholeHalf * 10000); }
							else { return !(SelfAsInt > (uint)(WholeHalf * 10000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						ushort Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(Value);
						}
						uint SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = WholeHalf;
						ValueAsInt *= 10000;
						ValueAsInt += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt >= ValueAsInt; }
						else { return !(SelfAsInt > ValueAsInt); }
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

		public static bool operator >(dynamic Value, SmallDec self)
		{
			return self < Value;
		}

		// Self Greater than or Equal to Value
		public static bool operator >=(SmallDec self, dynamic Value)
		{
			if (Value is SmallDec)
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
						ulong SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						ulong ValueAsInt = Value.IntValue;
						ValueAsInt *= 10000;
						ValueAsInt += Value.DecimalStatus;
						//Both are either positive or negative numbers
						if (self.DecBoolStatus == 0)
						{
							return SelfAsInt >= ValueAsInt;
						}
						else
						{//Larger number = farther down into negative
							return !(SelfAsInt >= ValueAsInt);
						}
					}
				}
			}
			else if (Value is double)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return true; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					ushort WholeHalf = (ushort)Value;
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
							uint SelfAsInt = (uint)(self.IntValue * 10000);
							SelfAsInt += self.DecimalStatus;
							if (self.DecBoolStatus == 0) { return SelfAsInt >= (uint)(WholeHalf * 10000); }
							else { return !(SelfAsInt >= (uint)(WholeHalf * 10000)); }
						}
					}
					else
					{
						Value -= WholeHalf;
						ushort Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(Value);
						}
						uint SelfAsInt = (uint)(self.IntValue * 10000);
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = WholeHalf;
						ValueAsInt *= 10000;
						ValueAsInt += Decimalhalf;
						if (self.DecBoolStatus == 0) { return SelfAsInt >= ValueAsInt; }
						else { return !(SelfAsInt >= ValueAsInt); }
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

		public static bool operator >=(dynamic Value, SmallDec self)
		{
			return self <= Value;
		}

		// Equality operator for comparing self to int type value
		public static bool operator ==(SmallDec self, dynamic Value)
		{
			if (Value is SmallDec)
			{
				if (self.DecBoolStatus == Value.DecBoolStatus && self.IntValue == Value.IntValue && self.DecimalStatus == Value.DecimalStatus) { return true; }
				else { return false; }
			}
			else if (Value is double)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return false; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return false; }
				else
				{
					Value = Math.Abs(Value);
					ushort WholeHalf = (ushort)Value;
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
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							return SelfAsInt != (uint)(WholeHalf * 1000);
						}
					}
					else
					{
						Value -= WholeHalf;
						ushort Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(Value);
						}
						uint SelfAsInt = (uint)(self.IntValue * 10000);
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = WholeHalf;
						ValueAsInt *= 10000;
						ValueAsInt += Decimalhalf;
						return SelfAsInt == ValueAsInt;
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

		public static bool operator ==(dynamic Value, SmallDec self)
		{
			return self == Value;
		}

		// Inequality operator for comparing self to multiple value types
		public static bool operator !=(SmallDec self, dynamic Value)
		{
			if (Value is SmallDec)
			{
				if (self.DecBoolStatus != Value.DecBoolStatus || self.IntValue != Value.IntValue || self.DecimalStatus != Value.DecimalStatus) { return true; }
				else { return false; }
			}
			else if (Value is double)
			{
				if (Value < 0.0 && self.DecBoolStatus == 0) { return true; }
				else if (Value >= 0.0 && self.DecBoolStatus == 1) { return true; }
				else
				{
					Value = Math.Abs(Value);
					ushort WholeHalf = (ushort)Value;
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
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							return SelfAsInt != (uint)(WholeHalf * 10000);
						}
					}
					else
					{
						Value -= WholeHalf;
						ushort Decimalhalf;
						if (Value == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (Value == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(Value);
						}
						uint SelfAsInt = (ushort)(self.IntValue * 10000);
						SelfAsInt += self.DecimalStatus;
						uint ValueAsInt = WholeHalf;
						ValueAsInt *= 10000;
						ValueAsInt += Decimalhalf;
						return SelfAsInt != ValueAsInt;
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

		public static bool operator !=(dynamic Value, SmallDec self)
		{
			return self != Value;
		}


		public static SmallDec operator %(SmallDec self, SmallDec y)
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
					uint SelfAsInt = self.DecimalStatus;
					SelfAsInt += (uint)(self.IntValue * 10000);
					SelfAsInt %= y.IntValue;
					self.IntValue = (ushort)(SelfAsInt / 1000);
					SelfAsInt -= (uint)(self.IntValue * 10000);
					self.DecimalStatus = (ushort)SelfAsInt;
				}
				else
				{
					ulong SelfAsInt = self.DecimalStatus;
					SelfAsInt += (ulong)(self.IntValue * 10000);
					uint YAsInt = y.IntValue;
					YAsInt *= 10000;
					YAsInt += y.DecimalStatus;
					SelfAsInt %= YAsInt;
					SelfAsInt /= 10000;
					ulong TempStorage = SelfAsInt / 10000;
					self.IntValue = (ushort)TempStorage;
					TempStorage = self.IntValue;
					TempStorage *= 10000;
					SelfAsInt -= TempStorage;
					self.DecimalStatus = (ushort)SelfAsInt;
				}
				//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
				if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
			}
			return self;
		}

		public static SmallDec operator +(SmallDec self, SmallDec y)
		{
			bool IsYNegative = (y.DecBoolStatus == 1) ? true : false;
			if (self.DecBoolStatus == 1 && IsYNegative)
			{// -X - Y (ex. -8 + -6)
				self.IntValue = (ushort)(self.IntValue + y.IntValue);
			}
			else if (self.DecBoolStatus == 0 && IsYNegative == false)
			{
				//X + Y (ex. 8 + 6)
				self.IntValue = (ushort)(self.IntValue + y.IntValue);
			}
			else
			{
				// -X + Y
				if (self.DecBoolStatus == 1)
				{   //ex. -8 + 9
					if (y.IntValue > self.IntValue)
					{
						self.IntValue = (ushort)(y.IntValue - self.IntValue);
						self.DecBoolStatus = 0;
					}
					else
					{//ex. -8 +  4
						self.IntValue = (ushort)(self.IntValue - y.IntValue);
					}
				}// X + -Y
				else
				{
					if (self.IntValue > y.IntValue)
					{//ex. 9 + -6
						self.IntValue = (ushort)(self.IntValue - y.IntValue);
					}
					else
					{//ex. 9 + -10
						self.IntValue = (ushort)(y.IntValue - self.IntValue);
						self.DecBoolStatus = 1;
					}
				}
			}
			//Decimal Section
			if (self.DecimalStatus != 0 || y.DecimalStatus != 0)
			{
				// ?.XXXXXX + ?.YYYYYY (ex. 0.9 + 0.2)
				if (self.DecBoolStatus == 0 && IsYNegative == false)
				{
					//Potential Overflow check
					ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + y.DecimalStatus);
					if (DecimalStatusTemp > 9999)
					{
						DecimalStatusTemp -= 10000;
						self.IntValue += 1;
					}
					self.DecimalStatus = DecimalStatusTemp;
				}
				// -?.XXXXXX - ?.YYYYYY (ex. -0.9 + -0.2)
				else if (self.DecBoolStatus == 1 && IsYNegative)
				{
					//Potential Overflow check
					ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + y.DecimalStatus);
					if (DecimalStatusTemp > 9999)
					{
						DecimalStatusTemp -= 10000;
						self.IntValue -= 1;
					}
					self.DecimalStatus = DecimalStatusTemp;
				}
				else
				{
					if (IsYNegative)
					{
						// ex. 0.6 + -0.5
						if (self.DecimalStatus >= y.DecimalStatus)
						{
							self.DecimalStatus = (ushort)(self.DecimalStatus - y.DecimalStatus);
						}// ex. 0.6 + -.7
						else
						{
							self.DecimalStatus = (ushort)(y.DecimalStatus - self.DecimalStatus);
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
					{ //ex -0.6 + 0.5
						if (self.DecimalStatus >= y.DecimalStatus)
						{
							self.DecimalStatus = (ushort)(self.DecimalStatus - y.DecimalStatus);
						}// ex. -1.6 + 0.7
						else
						{
							self.DecimalStatus = (ushort)(y.DecimalStatus - self.DecimalStatus);
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
			//Fix potential negative zero
			if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
			return self;
		}

		public static SmallDec operator -(SmallDec self, SmallDec y)
		{
			bool IsYNegative = (y.DecBoolStatus == 1) ? true : false;
			//ex. -9 - 9
			if (self.DecBoolStatus == 1 && IsYNegative == false)
			{// -X - Y
				self.IntValue = (ushort)(self.IntValue + y.IntValue);
			}//ex. 9 - (-1)
			else if (self.DecBoolStatus == 0 && IsYNegative == true)
			{
				//X - (-Y)
				self.IntValue = (ushort)(self.IntValue + y.IntValue);
			}
			else
			{
				// X - (Y)
				if (self.DecBoolStatus == 0)
				{
					// ex. 8 - 9
					if (y.IntValue > self.IntValue)
					{
						self.IntValue = (ushort)(y.IntValue - self.IntValue);
						self.DecBoolStatus = 1;
					} //ex. 8 - 7
					else
					{
						self.IntValue = (ushort)(self.IntValue - y.IntValue);
					}
				}// -X - (Y)
				else
				{
					// ex. -8 - (-9)
					if (self.IntValue > y.IntValue)
					{
						self.IntValue = (ushort)(y.IntValue - self.IntValue);
						self.DecBoolStatus = 0;
					}
					else
					{//ex. -8 - (-5)
						self.IntValue = (ushort)(self.IntValue - y.IntValue);
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
					ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + y.DecimalStatus);
					if (DecimalStatusTemp > 9999)
					{
						DecimalStatusTemp -= 10000;
						self.IntValue += 1;
					}
					self.DecimalStatus = DecimalStatusTemp;
				}// ex. 0.5 - (-0.6)
				else if (self.DecBoolStatus == 0 && IsYNegative)
				{
					//Potential Overflow check
					ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + y.DecimalStatus);
					if (DecimalStatusTemp > 9999)
					{
						DecimalStatusTemp -= 10000;
						self.IntValue += 1;
					}
					self.DecimalStatus = DecimalStatusTemp;
				}
				else
				{
					if (IsYNegative)
					{// ex. -0.7 - (-0.6)
						if (self.DecimalStatus >= y.DecimalStatus)
						{
							self.DecimalStatus = (ushort)(self.DecimalStatus - y.DecimalStatus);
						}
						else
						{
							self.DecimalStatus = (ushort)(y.DecimalStatus - self.DecimalStatus);
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
							self.DecimalStatus = (ushort)(self.DecimalStatus - y.DecimalStatus);
						}
						else
						{
							self.DecimalStatus = (ushort)(y.DecimalStatus - self.DecimalStatus);
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

		public static SmallDec operator *(SmallDec self, SmallDec y)
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
					uint SelfAsInt = self.DecimalStatus;
					SelfAsInt += (uint)(self.IntValue * 10000);
					SelfAsInt *= y.IntValue;
					self.IntValue = (ushort)(SelfAsInt / 1000);
					SelfAsInt -= (uint)(self.IntValue * 10000);
					self.DecimalStatus = (ushort)SelfAsInt;
				}
				else
				{
					ulong SelfAsInt = self.IntValue;
					SelfAsInt *= 10000;
					SelfAsInt += self.DecimalStatus;
					uint YAsInt = y.IntValue;
					YAsInt *= 10000;
					YAsInt += y.DecimalStatus;
					SelfAsInt *= YAsInt;
					SelfAsInt /= 10000;
					ulong TempStorage = SelfAsInt / 10000;
					self.IntValue = (ushort)TempStorage;
					TempStorage = self.IntValue;
					TempStorage *= 10000;
					SelfAsInt -= TempStorage;
					self.DecimalStatus = (ushort)SelfAsInt;
				}
				//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
				if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
			}
			return self;
		}

		public static SmallDec operator /(SmallDec self, SmallDec y)
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
					uint SelfAsInt = self.DecimalStatus;
					SelfAsInt += (uint)(self.IntValue * 10000);
					SelfAsInt /= y.IntValue;
					self.IntValue = (ushort)(SelfAsInt / 1000);
					SelfAsInt -= (uint)(self.IntValue * 10000);
					self.DecimalStatus = (ushort)SelfAsInt;
				}
				else
				{
					ulong SelfAsInt = self.IntValue;
					SelfAsInt *= 10000;
					SelfAsInt += self.DecimalStatus;
					uint YAsInt = y.IntValue;
					YAsInt *= 10000;
					YAsInt += y.DecimalStatus;
					SelfAsInt /= YAsInt;
					SelfAsInt /= 10000;
					ulong TempStorage = SelfAsInt / 10000;
					self.IntValue = (ushort)TempStorage;
					TempStorage = self.IntValue;
					TempStorage *= 10000;
					SelfAsInt -= TempStorage;
					self.DecimalStatus = (ushort)SelfAsInt;
				}
				//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
				if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
			}
			return self;
		}

		public static SmallDec operator %(SmallDec self, dynamic y)
		{
			if (y is double || y is SmallDec || y is decimal)
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
					ushort WholeHalf = (ushort)y;
					//Use x Int Operation instead if y has no decimal places
					if (WholeHalf == y)
					{
						if (self.DecimalStatus == 0)
						{
							//Use normal simple (int value) * (int value) if not dealing with any decimals
							self.IntValue %= WholeHalf;
						}
						else
						{
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							SelfAsInt %= WholeHalf;
							self.IntValue = (ushort)(SelfAsInt / 1000);
							SelfAsInt -= (uint)(self.IntValue * 10000);
							self.DecimalStatus = (ushort)SelfAsInt;
						}
					}
					else
					{
						y -= WholeHalf;
						ushort Decimalhalf;
						if (y == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (y == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(y);
						}
						ulong SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						ulong YAsInt = WholeHalf;
						YAsInt *= 10000;
						YAsInt += Decimalhalf;
						SelfAsInt %= YAsInt;
						SelfAsInt /= 10000;
						ulong TempStorage = SelfAsInt / 10000;
						self.IntValue = (ushort)TempStorage;
						TempStorage = self.IntValue;
						TempStorage *= 10000;
						SelfAsInt -= TempStorage;
						self.DecimalStatus = (ushort)SelfAsInt;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
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
						self.IntValue %= (ushort)y;
					}
					else
					{
						uint SelfAsInt = self.DecimalStatus;
						SelfAsInt += (uint)(self.IntValue * 10000);
						SelfAsInt %= y;
						uint TempStorage = SelfAsInt / 10000;
						self.IntValue = (ushort)TempStorage;
						TempStorage *= 10000;
						SelfAsInt -= TempStorage;
						self.DecimalStatus = (ushort)SelfAsInt;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			return self;
		}

		public static SmallDec operator +(SmallDec self, dynamic y)
		{
			if (y is double || y is SmallDec || y is decimal)
			{
				bool IsYNegative = (y < 0) ? true : false;
				y = Math.Abs(y);
				ushort WholeHalfOfY = (ushort)Math.Floor(y);
				y -= WholeHalfOfY;
				if (WholeHalfOfY == 0) { }
				else if (self.DecBoolStatus == 1 && IsYNegative)
				{// -X - Y (ex. -8 + -6)
					self.IntValue = (ushort)(self.IntValue + WholeHalfOfY);
				}
				else if (self.DecBoolStatus == 0 && IsYNegative == false)
				{
					//X + Y (ex. 8 + 6)
					self.IntValue = (ushort)(self.IntValue + WholeHalfOfY);
				}
				else
				{
					// -X + Y
					if (self.DecBoolStatus == 1)
					{   //ex. -8 + 9
						if (y > self.IntValue)
						{
							self.IntValue = (ushort)(WholeHalfOfY - self.IntValue);
							self.DecBoolStatus = 0;
						}
						else
						{//ex. -8 +  4
							self.IntValue = (ushort)(self.IntValue - WholeHalfOfY);
						}
					}// X-Y
					else
					{
						if (self.IntValue > WholeHalfOfY)
						{//ex. 9 + -6
							self.IntValue = (ushort)(self.IntValue - WholeHalfOfY);
						}
						else
						{//ex. 9 + -10
							self.IntValue = (ushort)(WholeHalfOfY - self.IntValue);
							self.DecBoolStatus = 1;
						}
					}
				}
				//Decimal Calculation Section
				if (self.DecBoolStatus != 0 || y != 0)
				{
					ushort SecondDec = (ushort)((System.Math.Abs(y) - System.Math.Abs(WholeHalfOfY)) * 10000);
					// ?.XXXXXX + ?.YYYYYY
					if (self.DecBoolStatus == 0 && IsYNegative == false)
					{
						//Potential Overflow check
						ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + SecondDec);
						if (DecimalStatusTemp > 9999)
						{
							DecimalStatusTemp -= 10000;
							self.IntValue += 1;
						}
						self.DecimalStatus = DecimalStatusTemp;
					}
					// -?.XXXXXX - ?.YYYYYY
					else if (self.DecBoolStatus == 1 && IsYNegative == true)
					{
						//Potential Overflow check
						ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + SecondDec);
						if (DecimalStatusTemp > 9999)
						{
							DecimalStatusTemp -= 10000;
							self.IntValue += 1;
						}
						self.DecimalStatus = DecimalStatusTemp;
					}
					else
					{
						if (IsYNegative)
						{
							// ex. 0.6 + -0.5
							if (self.DecimalStatus >= SecondDec)
							{
								self.DecimalStatus = (ushort)(self.DecimalStatus - SecondDec);
							}// ex. 0.6 + -.7
							else
							{
								self.DecimalStatus = (ushort)(SecondDec - self.DecimalStatus);
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
								self.DecimalStatus = (ushort)(self.DecimalStatus - SecondDec);
							}// ex. -1.6 + 0.7
							else
							{
								self.DecimalStatus = (ushort)(SecondDec - self.DecimalStatus);
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
					self.IntValue = (ushort)(self.IntValue + (ushort)Math.Abs(y));
				}
				else if (self.DecBoolStatus == 0 && y >= 0)
				{
					//X + Y (ex. 8 + 6)
					self.IntValue = (ushort)(self.IntValue + y);
				}
				else
				{
					// -X + Y
					if (self.DecBoolStatus == 1)
					{   //ex. -8 + 9
						if (y > self.IntValue)
						{
							self.IntValue = (ushort)(y - self.IntValue);
							self.DecBoolStatus = 0;
						}
						else
						{//ex. -8 +  4
							self.IntValue = (ushort)(self.IntValue - y);
						}
					}// X-Y
					else
					{
						ushort TempY = Math.Abs(y);
						if (self.IntValue > TempY)
						{//ex. 9 + -6
							self.IntValue = (ushort)(self.IntValue - TempY);
						}
						else
						{//ex. 9 + -10
							self.IntValue = (ushort)(TempY - self.IntValue);
							self.DecBoolStatus = 1;
						}
					}
				}
			}
			//Fix potential negative zero
			if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
			return self;
		}

		public static SmallDec operator -(SmallDec self, dynamic y)
		{
			if (y is double || y is SmallDec || y is decimal)
			{
				bool IsYNegative = (y < 0) ? true : false;
				y = Math.Abs(y);
				ushort WholeHalfOfY = (ushort)(Math.Floor(y));
				y -= WholeHalfOfY;
				if (WholeHalfOfY == 0) { }
				//ex. -9 - 9
				else if (self.DecBoolStatus == 1 && IsYNegative == false)
				{// -X - Y
					self.IntValue = (ushort)(self.IntValue + WholeHalfOfY);
				}//ex. 9 - (-1)
				else if (self.DecBoolStatus == 0 && IsYNegative)
				{
					//X - (-Y)
					self.IntValue = (ushort)(self.IntValue + WholeHalfOfY);
				}
				else
				{
					// X - (Y)
					if (self.DecBoolStatus == 0)
					{
						// ex. 8 - 9
						if (WholeHalfOfY > self.IntValue)
						{
							self.IntValue = (ushort)(WholeHalfOfY - self.IntValue);
							self.DecBoolStatus = 1;
						} //ex. 8 - 7
						else
						{
							self.IntValue = (ushort)(self.IntValue - WholeHalfOfY);
						}
					}// -X - (Y)
					else
					{
						// ex. -8 - (-9)
						if (self.IntValue > WholeHalfOfY)
						{
							self.IntValue = (ushort)(WholeHalfOfY - self.IntValue);
							self.DecBoolStatus = 0;
						}
						else
						{//ex. -8 - (-5)
							self.IntValue = (ushort)(self.IntValue - WholeHalfOfY);
						}
					}
				}
				//Decimal Calculation Section
				ushort SecondDec = (ushort)((System.Math.Abs(y) - System.Math.Abs(WholeHalfOfY)) * 10000);
				if (self.DecimalStatus != 0 || SecondDec != 0)
				{
					// ex. -0.5 - 0.6
					if (self.DecBoolStatus == 1 && IsYNegative == false)
					{
						//Potential Overflow check
						ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + SecondDec);
						if (DecimalStatusTemp > 9999)
						{
							DecimalStatusTemp -= 10000;
							self.IntValue += 1;
						}
						self.DecimalStatus = DecimalStatusTemp;
					}// ex. 0.5 - (-0.6)
					else if (self.DecBoolStatus == 0 && IsYNegative)
					{
						//Potential Overflow check
						ushort DecimalStatusTemp = (ushort)(self.DecimalStatus + SecondDec);
						if (DecimalStatusTemp > 9999)
						{
							DecimalStatusTemp -= 10000;
							self.IntValue += 1;
						}
						self.DecimalStatus = DecimalStatusTemp;
					}
					else
					{
						if (IsYNegative)
						{// ex. -0.7 - (-0.6)
							if (self.DecimalStatus >= SecondDec)
							{
								self.DecimalStatus = (ushort)(self.DecimalStatus - SecondDec);
							}
							else
							{
								self.DecimalStatus = (ushort)(SecondDec - self.DecimalStatus);
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
								self.DecimalStatus = (ushort)(self.DecimalStatus - SecondDec);
							}
							else
							{
								self.DecimalStatus = (ushort)(SecondDec - self.DecimalStatus);
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
					self.IntValue = (ushort)(self.IntValue + y);
				}//ex. 9 - (-1)
				else if (self.DecBoolStatus == 0 && y < 0)
				{
					//X - (-Y)
					self.IntValue = (ushort)(self.IntValue + Math.Abs(y));
				}
				else
				{
					// X - (Y)
					if (self.DecBoolStatus == 0)
					{
						// ex. 8 - 9
						if (y > self.IntValue)
						{
							self.IntValue = (ushort)(y - self.IntValue);
							self.DecBoolStatus = 1;
						} //ex. 8 - 7
						else
						{
							self.IntValue = (ushort)(self.IntValue - y);
						}
					}// -X - (Y)
					else
					{
						ushort TempY = Math.Abs(y);
						// ex. -8 - (-9)
						if (self.IntValue > TempY)
						{
							self.IntValue = (ushort)(TempY - self.IntValue);
							self.DecBoolStatus = 0;
						}
						else
						{//ex. -8 - (-5)
							self.IntValue = (ushort)(self.IntValue - TempY);
						}
					}
				}
			}
			//Fix potential negative zero
			if (self.IntValue == 0 && self.DecBoolStatus == 1 && self.DecimalStatus == 0) { self.DecBoolStatus = 0; }
			return self;
		}

		public static SmallDec operator *(SmallDec self, dynamic y)
		{
			if (y is double || y is SmallDec || y is decimal)
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
					ushort WholeHalf = (ushort)Math.Floor(y);
					//Use x Int Operation instead if y has no decimal places
					if (WholeHalf == y)
					{
						if (self.DecimalStatus == 0)
						{
							//Use normal simple (int value) * (int value) if not dealing with any decimals
							self.IntValue *= WholeHalf;
						}
						else
						{
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							SelfAsInt *= WholeHalf;
							self.IntValue = (ushort)(SelfAsInt / 1000);
							SelfAsInt -= (uint)(self.IntValue * 10000);
							self.DecimalStatus = (ushort)SelfAsInt;
						}
					}
					else
					{
						y -= WholeHalf;
						ushort Decimalhalf;
						if (y == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (y == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(y);
						}
						ulong SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint YAsInt = WholeHalf;
						YAsInt *= 10000;
						YAsInt += Decimalhalf;
						SelfAsInt *= YAsInt;
						SelfAsInt /= 10000;
						ulong TempStorage = SelfAsInt / 10000;
						self.IntValue = (ushort)TempStorage;
						TempStorage = self.IntValue;
						TempStorage *= 10000;
						SelfAsInt -= TempStorage;
						self.DecimalStatus = (ushort)SelfAsInt;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
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
						self.IntValue *= y;
					}
					else
					{
						uint SelfAsInt = self.DecimalStatus;
						SelfAsInt += (uint)(self.IntValue * 10000);
						SelfAsInt *= y;
						uint TempStorage = SelfAsInt / 10000;
						self.IntValue = (ushort)TempStorage;
						TempStorage *= 10000;
						SelfAsInt -= TempStorage;
						self.DecimalStatus = (ushort)SelfAsInt;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			return self;
		}

		public static SmallDec operator /(SmallDec self, dynamic y)
		{
			if (y is double || y is SmallDec || y is decimal)
			{
				if (y == 0)
				{
					Console.WriteLine("Prevented dividing by zero");
				}
				else
				{
					if (y < 0.0) { self.SwapNegativeStatus(); y *= -1.0; }
					ushort WholeHalf = (ushort)Math.Floor(y);
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
							uint SelfAsInt = self.DecimalStatus;
							SelfAsInt += (uint)(self.IntValue * 10000);
							SelfAsInt /= WholeHalf;
							self.IntValue = (ushort)(SelfAsInt / 1000);
							SelfAsInt -= (uint)(self.IntValue * 10000);
							self.DecimalStatus = (ushort)SelfAsInt;
						}
					}
					else
					{
						y -= WholeHalf;
						ushort Decimalhalf;
						if (y == 0.25)
						{
							Decimalhalf = 2500;
						}
						else if (y == 0.5)
						{
							Decimalhalf = 5000;
						}
						else
						{
							Decimalhalf = ExtractDecimalHalfV3(y);
						}
						ulong SelfAsInt = self.IntValue;
						SelfAsInt *= 10000;
						SelfAsInt += self.DecimalStatus;
						uint YAsInt = WholeHalf;
						YAsInt *= 10000;
						YAsInt += Decimalhalf;
						SelfAsInt /= YAsInt;
						SelfAsInt /= 10000;
						ulong TempStorage = SelfAsInt / 10000;
						self.IntValue = (ushort)TempStorage;
						TempStorage = self.IntValue;
						TempStorage *= 10000;
						SelfAsInt -= TempStorage;
						self.DecimalStatus = (ushort)SelfAsInt;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
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
						self.IntValue /= (ushort)y;
					}
					else
					{
						uint SelfAsInt = self.DecimalStatus;
						SelfAsInt += (uint)(self.IntValue * 10000);
						SelfAsInt /= y;
						uint TempStorage = SelfAsInt / 10000;
						self.IntValue = (ushort)TempStorage;
						TempStorage *= 10000;
						SelfAsInt -= TempStorage;
						self.DecimalStatus = (ushort)SelfAsInt;
					}
					//Prevent dividing/multiplying value into nothing by dividing too small (set to .0001 instead of having value set as zero)
					if (self.IntValue == 0 && self.DecimalStatus == 0) { self.DecimalStatus = 1; }
				}
			}
			return self;
		}

		//Right side applications
		public static SmallDec operator -(dynamic y, SmallDec self)
		{
			SmallDec YAsSuperDec = (SmallDec)y;
			YAsSuperDec -= self;
			return YAsSuperDec;
		}

		public static SmallDec operator +(dynamic y, SmallDec self)
		{
			SmallDec YAsSuperDec = (SmallDec)y;
			YAsSuperDec += self;
			return YAsSuperDec;
		}

		public static SmallDec operator *(dynamic y, SmallDec self)
		{
			SmallDec YAsSuperDec = (SmallDec)y;
			YAsSuperDec += self;
			return YAsSuperDec;
		}

		public static SmallDec operator /(dynamic y, SmallDec self)
		{
			SmallDec YAsSuperDec = (SmallDec)y;
			YAsSuperDec += self;
			return YAsSuperDec;
		}

		public static SmallDec operator -(SmallDec Value)
		{//Place DecBoolStatus>1 checks above in V2 of type
			if (Value.DecBoolStatus % 2==1)//ODD
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