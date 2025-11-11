using System;
using System.Collections;
using System.Collections.Generic;

namespace TaleUtil.Collections {
    // Input-Restricted Deque, designed specifically for Tale's needs.
    // O(1) read & front/back remove, amortized O(1) back insert.
    public class IRDeque<T> : IEnumerable<T> where T : class {
        T[] data;

        int start;
        int end;
        int count;

        int baseCapacity;

        public IRDeque(int baseCapacity) {
            this.baseCapacity = Math.CeilPowerOfTwo(baseCapacity);

            count = 0;

            Resize(this.baseCapacity);
        }

        public int Count { get { return count; } }
        public int Capacity { get { return data.Length; } }

        public T Fetch() =>
            data[start];

        public T FetchNext() =>
            data[(start + 1) & (Capacity - 1)];

        public T FetchLast() =>
            data[(end - 1 + Capacity) & (Capacity - 1)];

        public T FetchIfAny() =>
            count > 0 ? Fetch() : null;

        public void ForceClear() {
            if (count > 0) {
                if (start < end) {
                    Array.Clear(data, start, count);
                } else {
                    Array.Clear(data, start, Capacity - start);
                    Array.Clear(data, 0, end);
                }
            }

            start = 0;
            end = 0;
            count = 0;
        }

        // If the queue has actions, and the last action is this one, remove it.
        // This is used by multiple actions, such as QueueAction, which handle the
        // actions on their own.
        public void TakeLast(T action) {
            var index = (end - 1 + Capacity) & (Capacity - 1);

            if (data[index] == action) {
                end = index;
                data[end] = null;
                count--;
            }
        }

        public void TakeLast(T[] actions) {
            // C# arg order is left -> right, so the 'params' keyword always pushes to the queue in-order
            for (int i = actions.Length - 1; i >= 0; --i) {
                TakeLast(actions[i]);
            }
        }

        public T Enqueue(T act) {
            if (count == data.Length)
                Resize(data.Length * 2);

            data[end] = act;
            end = (end + 1) & (Capacity - 1);
            count++;

            return act;
        }

        public void Dequeue() {
            data[start] = null;
            start = (start + 1) & (Capacity - 1);
            count--;
        }

        public void Vacuum() {
            if (Capacity > baseCapacity && count <= Capacity / 2) {
                Resize(Capacity / 2);
            }
        }

        void Resize(int capacity) {
            T[] buff = new T[capacity];

            if (count > 0) {
                if (start < end) {
                    Array.Copy(data, start, buff, 0, count);
                } else {
                    int partLen = Capacity - start;
                    Array.Copy(data, start, buff, 0, partLen);
                    Array.Copy(data, 0, buff, partLen, end);
                }
            }

            data = buff;
            start = 0;
            end = count;
        }

        public IEnumerator<T> GetEnumerator() {
            for (int i = 0; i < Count; ++i) {
                yield return data[(start + i) & (Capacity - 1)];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}