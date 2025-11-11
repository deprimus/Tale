using System;
using System.Collections;
using System.Collections.Generic;

namespace TaleUtil.Collections {
    // Unordered list in contiguous memory, designed for bulk-insert-once read/remove-many.
    // O(1) read & remove, amortized O(1) insert.
    public class FastUnorderedList<T> : IEnumerable<T> where T : class {
        T[] data;

        int count;
        int baseCapacity;

        public FastUnorderedList(int baseCapacity) {
            this.baseCapacity = Math.CeilPowerOfTwo(baseCapacity);

            count = 0;

            Resize(this.baseCapacity);
        }

        public int Count { get { return count; } }
        public int Capacity { get { return data.Length; } }

        public T this[int index] =>
            data[index];

        public void Remove(int index) {
            data[index] = data[--count];
            data[count] = null;
        }

        public void InsertMany(params T[] items) {
            Reserve(count + items.Length);

            for (int i = 0; i < items.Length; ++i) {
                data[count++] = items[i];
            }
        }

        public void ForceClear() {
            Array.Clear(data, 0, count);
            count = 0;
        }

        public void Vacuum() {
            if (Capacity > baseCapacity && count <= Capacity / 2) {
                Resize(Capacity / 2);
            }
        }

        void Reserve(int capacity) {
            int cap = Capacity;

            while (capacity > cap) {
                cap *= 2;
            }

            if (cap > Capacity) {
                Resize(cap);
            }
        }

        void Resize(int capacity) {
            T[] buff = new T[capacity];

            if (count > 0) {
                Array.Copy(data, 0, buff, 0, count);
            }

            data = buff;
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; ++i) {
                yield return data[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}