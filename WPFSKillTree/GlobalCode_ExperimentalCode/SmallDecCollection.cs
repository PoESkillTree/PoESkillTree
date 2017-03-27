using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Media;

namespace CSharpGlobalCode.GlobalCode_ExperimentalCode
{
	//[Serializable]
	public class SmallDecCollection : CollectionBase, IFormattable, IList, ICollection, IList<SmallDec>, ICollection<SmallDec>, IEnumerable<SmallDec>, IEnumerable
	{

		public SmallDecCollection(IEnumerable<SmallDec> tics)
		{
			this.InnerList.Clear();
			this.InnerList.Add(tics);
		}

		public SmallDecCollection(IEnumerable<int> tics)
		{
			this.InnerList.Clear();
			this.InnerList.Add(tics);
		}

		public SmallDecCollection(IEnumerable<double> tics)
		{
			this.InnerList.Clear();
			this.InnerList.Add(tics);
		}

		public SmallDecCollection()
		{
			this.InnerList.Clear();
		}

		public SmallDec this[int index]
		{
			get
			{
				return (SmallDec)List[index];
			}

			set
			{
				this.List[index] = value;
			}
		}

		public static explicit operator DoubleCollection(SmallDecCollection self)
		{
			DoubleCollection NewCollection = new DoubleCollection();
			foreach(var Element in self)
			{
				NewCollection.Add((double)Element);
			}
			return NewCollection;
		}

		public static explicit operator SmallDecCollection(DoubleCollection self)
		{
			SmallDecCollection NewCollection = new SmallDecCollection();
			foreach (var Element in self)
			{
				NewCollection.Add((SmallDec)Element);
			}
			return NewCollection;
		}

		object IList.this[int index]
		{
			get
			{
				return (SmallDec)List[index];
			}

			set
			{
				List[index] = value;
			}
		}

		public bool IsReadOnly
		{
			get { return false; }
		}

		public int Add(object value)
		{
			return this.InnerList.Add((SmallDec)value);
		}

		public void Add(SmallDec value)
		{
			this.InnerList.Add(value);
		}

		public void AddRange(SmallDecCollection collection)
		{
			if (collection != null)
			{
				this.InnerList.AddRange(collection);
			}
		}

		//public void Clear();

		public bool Contains(object value)
		{
			return this.List.Contains((SmallDec)value);
		}

		public bool Contains(SmallDec value)
		{
			return this.List.Contains(value);
		}

		public void CopyTo(Array array, int index)
		{
			this.List.CopyTo(array, index);
		}

		public void CopyTo(SmallDec[] array, int arrayIndex)
		{
			foreach (int i in List)
			{
				array.SetValue(i, arrayIndex);
				arrayIndex = arrayIndex + 1;
			}
		}

		public int IndexOf(object value)
		{
			return base.List.IndexOf((SmallDec)value);
		}

		public int IndexOf(SmallDec value)
		{
			return base.List.IndexOf(value);
		}

		public void Insert(int index, object value)
		{
			if (index <= List.Count)
			{
				this.List.Insert(index, (SmallDec) value);
			}
		}

		public void Insert(int index, SmallDec value)
		{
			if (index <= List.Count)
			{
				this.List.Insert(index, value);
			}
		}

		public void Remove(object value)
		{
			this.InnerList.Remove((SmallDec)value);
		}

		public void Remove(SmallDec value)
		{
			this.InnerList.Remove(value);
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			throw new NotImplementedException();
		}

		IEnumerator<SmallDec> IEnumerable<SmallDec>.GetEnumerator()
		{
			return new SmallDecEnumerator(List);
		}

		bool ICollection<SmallDec>.Remove(SmallDec value)
		{
			throw new NotImplementedException();
		}
	}

	public class SmallDecEnumerator : IEnumerator<SmallDec>
	{
		private IList list;

		public SmallDecEnumerator(IList list)
		{
			this.list = list;
		}

		object IEnumerator.Current
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		internal static object Range(SmallDec smallDec1, SmallDec smallDec2)
		{
			throw new NotImplementedException();
		}

		internal static object Range(int v1, int v2)
		{
			throw new NotImplementedException();
		}

		SmallDec IEnumerator<SmallDec>.Current
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		bool IEnumerator.MoveNext()
		{
			throw new NotImplementedException();
		}

		void IEnumerator.Reset()
		{
			throw new NotImplementedException();
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: dispose managed state (managed objects).
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~Enumerator() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		void IDisposable.Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
