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
	[System.ComponentModel.TypeConverter(typeof(SuperDec_SmallDec_TypeConverter))]
	public partial struct SmallDec : IComparable<SmallDec>
	{
#if (!BlazesGlobalCode_ReducedSmallDecSize)
		//0 = Positive;1=Negative;Other states at higher then 1;254 = Positive Infinity;255 = Negative Infinity
		public byte DecBoolStatus;
#endif

		//Stores decimal section info (4 Decimal places stored)
		public ushort DecimalStatus;

		public ushort IntValue;

		public static SmallDec Sum(IEnumerable<SmallDec> Value)
		{
			SmallDec TotalSum = SmallDec.Zero;
			foreach (var Element in Value)
			{
				TotalSum += Element;
			}
			return TotalSum;
		}

		public static SmallDec Parse(string value, CultureInfo invariantCulture)
		{
			SmallDec NewValue = SmallDec.StringToValue(value);
			return NewValue;
		}

		public static bool TryParse(string value, object smallDec, CultureInfo invariantCulture, out SmallDec f)
		{
			f = Parse(value, invariantCulture);
			return true;
		}

		public static SmallDec Pow(SmallDec self, int Value)
		{
			SmallDec NewSelf = self;
			if (Value == 0)
			{
				NewSelf.IntValue = 1;
				NewSelf.DecBoolStatus = 0;
			}
			else if(Value<0)
			{
				for (int NumberOfTimes = Value; NumberOfTimes > 0; --NumberOfTimes)
				{
					NewSelf /= self;
				}
			}
			else
			{
				for (int NumberOfTimes = Value; NumberOfTimes > 0; --NumberOfTimes)
				{
					NewSelf *= self;
				}
			}
			return NewSelf;
		}

		//public static SmallDec Pow(double self, double Value) { return SmallDec.Pow((SmallDec)self, (SmallDec)Value); }
		public static SmallDec Pow(SmallDec self, double Value) { return SmallDec.Pow(self, (SmallDec)Value); }
#if (BlazesGlobalCode_StandardExplicitConversionFrom)//Gets confused since it tries auto converting to SmallDec inside parameter first
		public static SmallDec Pow(SmallDec self, SmallDec Value) { return SmallDec.Pow(self, Value); }
#endif
		//Approximate version of Math.Pow(double self, double Value)
		public static SmallDec Pow(SmallDec self, SmallDec Value)
		{
			SmallDec NewSelf = self;
			//SmallDec version of Math.Pow()
			if (Value.DecimalStatus == 0)
			{
				if (Value.IntValue == 0)
				{
					NewSelf.IntValue = 1;
					NewSelf.DecBoolStatus = 0;
				}
				else if (Value.DecBoolStatus == 0)
				{
					for (ushort NumberOfTimes = Value.IntValue; NumberOfTimes > 0; --NumberOfTimes)
					{
						NewSelf *= self;
					}
				}
				else
				{
					for (ushort NumberOfTimes = Value.IntValue; NumberOfTimes > 0; --NumberOfTimes)
					{
						NewSelf /= self;
					}
				}
			}
			else
			{//To-Do need to finish remaining power part of code
			 //decimal SelfAsDecimal = (decimal) self;
				if (Value.IntValue == 0)
				{
					NewSelf.IntValue = 1;
					NewSelf.DecBoolStatus = 0;
				}
				else if (Value.DecBoolStatus == 0)
				{
					for (ushort NumberOfTimes = Value.IntValue; NumberOfTimes > 0; --NumberOfTimes)
					{
						NewSelf *= self;
					}
				}
				else
				{
					for (ushort NumberOfTimes = Value.IntValue; NumberOfTimes > 0; --NumberOfTimes)
					{
						NewSelf /= self;
					}
				}
				//Now need to deal with the remaining "power"
				Value.IntValue = 0;
				//Number is less then NewSelf Currently is (Sloping Curve in closeness to next power)
				double TempDouble = Math.Pow((double)self, (double)Value);
				if (Value.DecBoolStatus == 0)
				{
					NewSelf *= TempDouble;
				}
				else
				{
					NewSelf /= TempDouble;
				}
			}
			return NewSelf;
		}

		//SmallDec version of Math.Exp(double Value)
		public static SmallDec Exp(SmallDec Value)
		{
			double SelfAsDecimal = (double)Value;
			SelfAsDecimal = Math.Exp(SelfAsDecimal);
			return (SmallDec)SelfAsDecimal;
		}

		public static SmallDec Max(dynamic LeftSide, dynamic RightSide)
		{
			SmallDec LeftSideAsType = (SmallDec)LeftSide;
			SmallDec RightSideAsType = (SmallDec)RightSide;
			if (LeftSideAsType > RightSide) { return LeftSideAsType; }
			else { return RightSideAsType; }
		}

		public static SmallDec Min(dynamic LeftSide, dynamic RightSide)
		{
			SmallDec LeftSideAsType = (SmallDec)LeftSide;
			SmallDec RightSideAsType = (SmallDec)RightSide;
			if (LeftSideAsType < RightSide) { return LeftSideAsType; }
			else { return RightSideAsType; }
		}

		public static SmallDec StringToValue(string Value)
		{
			SmallDec NewSelf;
			NewSelf.IntValue = 0;
			NewSelf.DecimalStatus = 0;
			NewSelf.DecBoolStatus = 0;
			sbyte PlaceNumber;
			byte StringLength = (byte)Value.Length;
			string WholeNumberBuffer = "";
			string DecimalBuffer = "";
			bool ReadingDecimal = false;
			int TempInt;
			int TempInt02;
			foreach (char StringChar in Value)
			{
				if (IsDigit(StringChar))
				{
					if (ReadingDecimal)
					{
						DecimalBuffer += StringChar;
					}
					else
					{
						WholeNumberBuffer += StringChar;
					}
				}
				else if (StringChar == '-')
				{
					NewSelf.DecBoolStatus = 1;
				}
				else if (StringChar == '.')
				{
					ReadingDecimal = true;
				}
			}
			PlaceNumber = (sbyte)(WholeNumberBuffer.Length - 1);
			foreach (char StringChar in WholeNumberBuffer)
			{
				TempInt = CharAsInt(StringChar);
				TempInt02 = (ushort)(TempInt * Math.Pow(10, PlaceNumber));
				if (StringChar != '0')
				{
					NewSelf.IntValue += (ushort)TempInt02;
				}
				PlaceNumber--;
			}
			PlaceNumber = 3;
			foreach (char StringChar in DecimalBuffer)
			{
				//Limit stored decimal numbers to the amount it can store
				if (PlaceNumber > -1)
				{
					TempInt = CharAsInt(StringChar);
					TempInt02 = (ushort)(TempInt * Math.Pow(10, PlaceNumber));
					if (StringChar != '0')
					{
						NewSelf.DecimalStatus += (ushort)TempInt02;
					}
					PlaceNumber--;
				}
			}
			return NewSelf;
		}

		//Method version to Initialize Type instead of with Explicit operators
		public static SmallDec Initialize(dynamic Value)
		{
			return new SmallDec(Value);
		}

		//Initialize constructor
		public SmallDec(dynamic Value)
		{
#if (BlazesGlobalCode_ReducedSmallDecSize)
			if (Value is DependencyProperty)
			{
				SmallDec NewValue = (SmallDec)Value;
				this.IntValue = NewValue.IntValue;
				this.DecimalStatus = NewValue.DecimalStatus;
			}
			else if (Value is string)
			{
				IntValue = 0;
				DecimalStatus = 0;
				bool IsNegative = false;
				sbyte PlaceNumber;
				byte StringLength = (byte)Value.Length;
				string WholeNumberBuffer = "";
				string DecimalBuffer = "";
				bool ReadingDecimal = false;
				int TempInt;
				int TempInt02;
				foreach (char StringChar in Value)
				{
					if (IsDigit(StringChar))
					{
						if (ReadingDecimal)
						{
							DecimalBuffer += StringChar;
						}
						else
						{
							WholeNumberBuffer += StringChar;
						}
					}
					else if (StringChar == '-')
					{
						IsNegative = true;
					}
					else if (StringChar == '.')
					{
						ReadingDecimal = true;
					}
				}
				PlaceNumber = (sbyte)(WholeNumberBuffer.Length - 1);
				foreach (char StringChar in WholeNumberBuffer)
				{
					TempInt = CharAsInt(StringChar);
					TempInt02 = (ushort)(TempInt * Math.Pow(10, PlaceNumber));
					if (StringChar != '0')
					{
						IntValue += (ushort)TempInt02;
					}
					PlaceNumber--;
				}
				PlaceNumber = 3;
				foreach (char StringChar in DecimalBuffer)
				{
					//Limit stored decimal numbers to the amount it can store
					if (PlaceNumber > -1)
					{
						TempInt = CharAsInt(StringChar);
						TempInt02 = (ushort)(TempInt * Math.Pow(10, PlaceNumber));
						if (StringChar != '0')
						{
							DecimalStatus += (ushort)TempInt02;
						}
						PlaceNumber--;
					}
				}
			}
#else
			if (Value is DependencyProperty)
			{
				SmallDec NewValue = (SmallDec)Value;
				this.DecBoolStatus = NewValue.DecBoolStatus;
				this.IntValue = NewValue.IntValue;
				this.DecimalStatus = NewValue.DecimalStatus;
			}
			else if (Value is string)
			{
				IntValue = 0;
				DecimalStatus = 0;
				DecBoolStatus = 0;
				sbyte PlaceNumber;
				byte StringLength = (byte)Value.Length;
				string WholeNumberBuffer = "";
				string DecimalBuffer = "";
				bool ReadingDecimal = false;
				int TempInt;
				int TempInt02;
				foreach (char StringChar in Value)
				{
					if (IsDigit(StringChar))
					{
						if (ReadingDecimal)
						{
							DecimalBuffer += StringChar;
						}
						else
						{
							WholeNumberBuffer += StringChar;
						}
					}
					else if (StringChar == '-')
					{
						DecBoolStatus = 1;
					}
					else if (StringChar == '.')
					{
						ReadingDecimal = true;
					}
				}
				PlaceNumber = (sbyte)(WholeNumberBuffer.Length - 1);
				foreach (char StringChar in WholeNumberBuffer)
				{
					TempInt = CharAsInt(StringChar);
					TempInt02 = (ushort)(TempInt * Math.Pow(10, PlaceNumber));
					if (StringChar != '0')
					{
						IntValue += (ushort)TempInt02;
					}
					PlaceNumber--;
				}
				PlaceNumber = 3;
				foreach (char StringChar in DecimalBuffer)
				{
					//Limit stored decimal numbers to the amount it can store
					if (PlaceNumber > -1)
					{
						TempInt = CharAsInt(StringChar);
						TempInt02 = (ushort)(TempInt * Math.Pow(10, PlaceNumber));
						if (StringChar != '0')
						{
							DecimalStatus += (ushort)TempInt02;
						}
						PlaceNumber--;
					}
				}
			}
#	if (!BlazesGlobalCode_Disable128BitFeatures)
			else if (Value is MediumSuperDec)
			{
				IntValue = (ushort)Value.IntValue;
				uint TempDec = Value.DecimalStatus / 100000;
				DecimalStatus = (ushort)TempDec;
				DecBoolStatus = Value.DecBoolStatus;
			}
			else if (Value is ModerateSuperDec)
			{
				IntValue = (ushort)Value.IntValue;
				ulong TempDec = Value.DecimalStatus / 100000000000000;
				DecimalStatus = (ushort)TempDec;
				DecBoolStatus = Value.DecBoolStatus;
			}
			else if (Value is LargeSuperDec)
			{
				IntValue = (ushort)Value.IntValue;
				ulong TempDec = Value.DecimalStatus / 100000000000000;
				DecimalStatus = (ushort)TempDec;
				DecBoolStatus = Value.DecBoolStatus;
			}
#	endif
			else if (Value is double || Value is float || Value is decimal)
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
				ulong WholeValue = (ulong)Math.Floor(Value);
				//Cap value if too big on initialize (preventing overflow on conversion)
				if (Value > 65535)
				{
					Value = 65535;
				}
				Value -= WholeValue;
				IntValue = (ushort)WholeValue;
				DecimalStatus = (ushort)(Value * 10000);
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
				if (Value > 65535)
				{
					Value = 65535;
				}
				this.DecBoolStatus = 0;
				this.IntValue = (ushort)Value;
				this.DecimalStatus = 0;
			}
			else if(Value is SmallDec)
			{
				this.DecBoolStatus = Value.DecBoolStatus;
				this.IntValue = Value.IntValue;
				this.DecimalStatus = Value.DecimalStatus;
			}
			else
			{
				//Cap value if too big on initialize
				if (Value > 65535)
				{
					Value = 65535;
				}
				this.DecBoolStatus = 0;
				this.IntValue = (ushort)Value;
				this.DecimalStatus = 0;
			}
#endif
		}

		//From this type to Standard types

		public static explicit operator decimal(SmallDec self)
		{
			decimal Value = (decimal)self.IntValue;
			Value += (decimal)(self.DecimalStatus * 0.0001);
			if (self.DecBoolStatus == 1) { Value *= -1; }
			return Value;
		}

		public static explicit operator double(SmallDec self)
		{
			double Value = 0.0;
			Value += self.IntValue;
			Value += (self.DecimalStatus * 0.0001);
			if (self.DecBoolStatus == 1) { Value *= -1; }
			return Value;
		}

		public static explicit operator float(SmallDec self)
		{
			float Value = 0.0f;
			Value += self.IntValue;
			Value += (float)(self.DecimalStatus * 0.0001);
			if (self.DecBoolStatus == 1) { Value *= -1; }
			return Value;
		}

		public static explicit operator int(SmallDec self)
		{
			int Value = (int)self.IntValue;
			if (self.DecimalStatus == 1) { Value *= -1; }
			return Value;
		}

		public static explicit operator long(SmallDec self)
		{
			long Value = self.IntValue;
			if (self.DecimalStatus == 1) { Value *= -1; }
			return Value;
		}

		public static explicit operator uint(SmallDec self)
		{
			return self.IntValue;
		}

		public static explicit operator ulong(SmallDec self)
		{
			return self.IntValue;
		}

		public static explicit operator byte(SmallDec self)
		{
			byte Value = (byte)self.IntValue;
			return Value;
		}

		public static explicit operator sbyte(SmallDec self)
		{
			sbyte Value = (sbyte)self.IntValue;
			if (self.DecimalStatus == 1) { Value *= -1; }
			return Value;
		}

		public static explicit operator ushort(SmallDec self)
		{
			ushort Value = (ushort)self.IntValue;
			return Value;
		}

		public static explicit operator short(SmallDec self)
		{
			short Value = (short)self.IntValue;
			if (self.DecimalStatus == 1) { Value *= -1; }
			return Value;
		}

		static public explicit operator string(SmallDec self)
		{
			return self.ToOptimalString();
		}

		//From Standard types to this type 
#if (BlazesGlobalCode_StandardExplicitConversionFrom)
		public static explicit operator SmallDec(decimal Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(double Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(SmallDec Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(int Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(uint Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(long Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(ulong Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(ushort Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(short Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(sbyte Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(byte Value)	{	return new SmallDec(Value);	}

		public static explicit operator SmallDec(string Value) { return new SmallDec(Value); }

		public static explicit operator SmallDec(DependencyProperty Value)
		{
			SmallDec NewValue = Value.ToString();
			return NewValue;
		}
#else
		public static implicit operator SmallDec(decimal Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(double Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(float Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(int Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(uint Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(long Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(ulong Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(ushort Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(short Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(sbyte Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(byte Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(string Value) { return new SmallDec(Value); }

		public static implicit operator SmallDec(DependencyProperty Value)
		{
			SmallDec NewValue = Value.ToString();
			return NewValue;
		}
#endif
		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType()) { return false; }

			try
			{
				return this == (SmallDec)obj;
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

		public ushort GetDecimalStatus()
		{
			return DecimalStatus;
		}

		// Override the Object.GetHashCode() method:
		public override int GetHashCode()
		{
			if (DecBoolStatus == 1)
			{
				return ((int)IntValue + (int)DecimalStatus) * -1;
			}
			else
			{
				return (int)IntValue + (int)DecimalStatus;
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		public ushort GetIntValue()
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
		public SmallDec Maximum()
		{
			SmallDec NewSelf;
			NewSelf.IntValue = 65535;
			NewSelf.DecimalStatus = 9999;
			NewSelf.DecBoolStatus = 0;
			return NewSelf;
		}

		//Returns value of minimum non-infinite/Special Decimal State Value that can store
		public SmallDec Minimum()
		{
			SmallDec NewSelf;
			NewSelf.IntValue = 65535;
			NewSelf.DecimalStatus = 9999;
			NewSelf.DecBoolStatus = 1;
			return NewSelf;
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

		//Display string with empty decimal places removed
		public string ToOptimalString()
		{
			System.String Value = "";
			ushort IntegerHalf = IntValue;
			byte CurrentDigit;
			if (DecBoolStatus == 1) { Value += "-"; }
			for (sbyte Index = NumberOfPlaces(IntegerHalf); Index >= 0; Index--)
			{
				CurrentDigit = (byte)(IntegerHalf / Math.Pow(10, Index));
				IntegerHalf -= (ushort)(CurrentDigit * Math.Pow(10, Index));
				Value += DigitAsChar(CurrentDigit);
			}
			ushort DecimalHalf = DecimalStatus;
			if(DecimalStatus!=0)
			{
				Value += ".";
				for (sbyte Index = 3; Index >= 0; Index--)
				{
					if (DecimalStatus != 0)
					{
						CurrentDigit = (byte)(DecimalHalf / Math.Pow(10, Index));
						DecimalHalf -= (ushort)(CurrentDigit * Math.Pow(10, Index));
						Value += DigitAsChar(CurrentDigit);
					}
				}
			}
			return Value;
		}

		//Display string with empty decimal places show
		public string ToFullString()
		{
			System.String Value = "";
			ushort IntegerHalf = IntValue;
			byte CurrentDigit;
			if (DecBoolStatus == 1) { Value += "-"; }
			for (sbyte Index = NumberOfPlaces(IntegerHalf); Index >= 0; Index--)
			{
				CurrentDigit = (byte)(IntegerHalf / Math.Pow(10, Index));
				IntegerHalf -= (ushort)(CurrentDigit * Math.Pow(10, Index));
				Value += DigitAsChar(CurrentDigit);
			}
			Value += ".";
			ushort DecimalHalf = DecimalStatus;
			for (sbyte Index = 3; Index >= 0; Index--)
			{
					CurrentDigit = (byte)(DecimalHalf / Math.Pow(10, Index));
					DecimalHalf -= (ushort)(CurrentDigit * Math.Pow(10, Index));
					Value += DigitAsChar(CurrentDigit);
			}
			return Value;
		}


		public string ToString(string s, IFormatProvider provider)
		{
			return ToOptimalString();
		}

		public string ToString(string s)
		{
			return ToOptimalString();
		}

		internal string ToString(CultureInfo invariantCulture)
		{
			return ToOptimalString();
		}

		internal string ToString(NumberFormatInfo numberFormat)
		{
			return ToOptimalString();
		}

		public override string ToString() { return ToOptimalString(); }

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

		public dynamic DynamicConversion()
		{
			return this;
		}

		public static SmallDec DynamicConversionFrom(dynamic Value)
		{
			SmallDec NewSelf=(SmallDec) Value;
			return NewSelf;
		}

		public static SmallDec SumOfList(SmallDec[] self)
		{
			SmallDec Total = SmallDec.Zero;
			foreach(SmallDec Element in self)
			{
				Total += Element;
			}
			return Total;
		}

		public static SmallDec SumOfList(IEnumerable<SmallDec> self)
		{
			SmallDec Total = SmallDec.Zero;
			foreach (SmallDec Element in self)
			{
				Total += Element;
			}
			return Total;
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
			if (DecBoolStatus==202) { return true; }
			else { return false; }
		}

		private static SmallDec ZeroValue()
		{
			SmallDec NewSelf;
			NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0; NewSelf.DecBoolStatus = 0;
			return NewSelf;
		}

		public static readonly SmallDec Zero = ZeroValue();

		private static SmallDec NaNValue()
		{
			SmallDec NewSelf;
			NewSelf.IntValue = 0; NewSelf.DecimalStatus = 0;
#if (BlazesGlobalCode_SmallDec_EnableSpecialDecStates)
			NewSelf.DecBoolStatus = 202;
#else
			NewSelf.DecBoolStatus = 0;
#endif
			return NewSelf;
		}

		public static readonly SmallDec NaN = NaNValue();

		int IComparable<SmallDec>.CompareTo(SmallDec other)
		{
			if(other==this)
			{
				return 0;
			}
			else if(this<other)
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
			SmallDec ConvertedTarget = (SmallDec)CompareTarget;
			if(CompareTarget==this)	{	return true;	}
			else
			{
				SmallDec LeftRange = CompareTarget - RangeWithin;
				SmallDec RightRange = CompareTarget + RangeWithin;
				if(this==LeftRange||this==RightRange) { return true; }
				else if(CompareTarget> LeftRange&&CompareTarget< RightRange)	{	return true;	}
				else {	return false;	}
			}
		}

	}
}