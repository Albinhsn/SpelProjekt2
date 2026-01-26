using System;
using System.Collections;
using System.Collections.Generic;

namespace srUtils
{
	public sealed class MinHeap<TCompare, TValue> : ICollection<KeyValuePair<TCompare, TValue>>, IReadOnlyCollection<KeyValuePair<TCompare, TValue>> where TCompare : IComparable
	{
		private const byte CSTP = 16;
		private KeyValuePair<TCompare, TValue>[] contents;
		private int maxUsedIndex;
		private int capacity;

		public MinHeap()
		{
			contents = new KeyValuePair<TCompare, TValue>[CSTP];
			maxUsedIndex = -1;
		}
		public MinHeap(KeyValuePair<TCompare, TValue>[] entries)
		{
			contents = new KeyValuePair<TCompare, TValue>[CSTP];
			maxUsedIndex = -1;
			AddRange(entries);
		}

		public void Add(TCompare key, TValue value)
		{
			Append(new KeyValuePair<TCompare, TValue>(key, value));
			PropagateUp(maxUsedIndex);
		}

		public void Add(KeyValuePair<TCompare, TValue> entry)
		{
			Append(entry);//Append
			PropagateUp(maxUsedIndex);
		}

		public void AddRange(KeyValuePair<TCompare, TValue>[] entries)
		{
			foreach (KeyValuePair<TCompare, TValue> obj in entries) Add(obj);
		}
	
		public KeyValuePair<TCompare, TValue> Peek()
		{
			return contents[0];
		}

		public bool TryPeek(out KeyValuePair<TCompare, TValue> result)
		{
			if (Count > 0)
			{
				result = contents[0];
				return true;
			}

			result = new KeyValuePair<TCompare, TValue>();
			return false;
		}
		public KeyValuePair<TCompare, TValue> Pop()
		{
			KeyValuePair<TCompare, TValue> output = contents[0];
			(contents[0], contents[maxUsedIndex]) = (contents[maxUsedIndex], contents[0]); //Swap with last
			Concatenate();
			PropagateDown(0);
			return output;
		}

		public bool TryPop(out KeyValuePair<TCompare, TValue> result)
		{
			if (maxUsedIndex != -1)
			{
				result = Pop();
				return true;
			}

			result = new KeyValuePair<TCompare, TValue>();
			return false;
		}

		private void PropagateUp(int index)
		{
			while (index > 0 && contents[index].Key.CompareTo(contents[(index - 1) / 2].Key) <= 0) //While current is smaller than parent. First check for underflow
			{
				(contents[(index - 1) / 2], contents[index]) = (contents[index], contents[(index - 1) / 2]);//Swap
				index = (index - 1) / 2;//Continue propagation at new pos
			}
		}
		private void PropagateDown(int index)
		{
			while (index * 2 + 1 <= maxUsedIndex) //While node is not leaf, only checks for left child node here
			{
				int swapWith = index;
				if (contents[swapWith].Key.CompareTo(contents[index * 2 + 1].Key) >= 0) swapWith = index * 2 + 1;//Left
				if (index * 2 + 2 <= maxUsedIndex && contents[swapWith].Key.CompareTo(contents[index * 2 + 2].Key) >= 0) swapWith = index * 2 + 2;//Right
			
				if (swapWith == index) return;//Complete if smaller than both children
				(contents[swapWith], contents[index]) = (contents[index], contents[swapWith]);//Swap
				index = swapWith;//Continue propagation at new pos
			}
		}

		private void Append(KeyValuePair<TCompare, TValue> obj)
		{
			if (maxUsedIndex + 1 == capacity) SetCapacity(capacity + CSTP);
			maxUsedIndex++;
			contents[maxUsedIndex] = obj;
		}
		private void Concatenate()//Lazy deletion
		{
			maxUsedIndex--;
			if (maxUsedIndex < capacity - CSTP) SetCapacity(capacity - CSTP);
		}

		private void SetCapacity(int nCap)
		{
			maxUsedIndex = maxUsedIndex < nCap ? maxUsedIndex : nCap - 1;
			KeyValuePair<TCompare, TValue>[] res = new KeyValuePair<TCompare, TValue>[nCap];
			for (int a = 0; a <= maxUsedIndex; a++)
			{
				res[a] = contents[a];
			}
			contents = res;
			capacity = nCap;
		}
	
	

		public IEnumerator<KeyValuePair<TCompare, TValue>> GetEnumerator()
		{
			for (int a = 0; a <= maxUsedIndex; a++)
			{
				yield return contents[a];
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	
		public int Capacity => capacity + 1;
		public int UnusedCapacity => capacity - maxUsedIndex;

		int IReadOnlyCollection<KeyValuePair<TCompare, TValue>>.Count => maxUsedIndex + 1;
	
		public bool IsSynchronized => true;//I have no idea how to use these
		public object SyncRoot => this;
	
	
		public void Clear()
		{
			maxUsedIndex = -1;
			contents = new KeyValuePair<TCompare, TValue>[CSTP];
		}

		public bool Contains(KeyValuePair<TCompare, TValue> item)
		{
			foreach (KeyValuePair<TCompare, TValue> obj in contents)
			{
				if (obj.Value.Equals(item.Value)) return true;
			}
			return false;
		}

		public bool ContainsKey(TCompare key)
		{
			foreach (KeyValuePair<TCompare, TValue> obj in contents)
			{
				if (key.Equals(obj.Key)) return true;
			}
			return false;
		}

		public void CopyTo(KeyValuePair<TCompare, TValue>[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public bool Remove(KeyValuePair<TCompare, TValue> item)
		{
			for (int a = 0; a < maxUsedIndex; a++)
			{
				if (item.Equals(contents[a]))
				{
					(contents[a], contents[maxUsedIndex]) = (contents[maxUsedIndex], contents[a]); //Swap with last
					Concatenate();
					PropagateDown(0);
					return true;
				}
			}
			return false;
		}

		public int Count => maxUsedIndex + 1;

		public bool IsReadOnly => throw new NotImplementedException();
	}
}
