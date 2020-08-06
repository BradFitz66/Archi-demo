using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using UnityEngine;

namespace ZeroVector.Common.Reorderable {
    // If we implement the whole list, we don't even need to do it "the easy way":
    //
    // public abstract class ReorderableList<T> : List<T>, ISerializationCallbackReceiver {
    //     [SerializeField] private List<T> items = new List<T>();
    //
    //     public void OnBeforeSerialize() {
    // 	    items.Clear();
    // 	    items.AddRange(this);
    //     }
    //
    //     public void OnAfterDeserialize() {
    // 	    this.Clear();
    // 	    this.AddRange(items);
    //     }
    // }

    [SuppressMessage("ReSharper", "PublicConstructorInAbstractClass")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class ReorderableList<T> : Internal.BaseReorderableCollection,
        IList<T>, IList, IReadOnlyList<T> {
        //
        // ReSharper disable once FieldCanBeMadeReadOnly.Local, Unity.RedundantSerializeFieldAttribute, MemberInitializerValueIgnored
        // Internal list that we expose the functionality of, and that we can serialise -- but must be named "items"
        [SerializeField] private List<T> items = new List<T>();
        
        public ReorderableList() => items = new List<T>();

        public ReorderableList(int capacity) => items = new List<T>(capacity);

        public ReorderableList(IEnumerable<T> collection) => items = new List<T>(collection);


        public int Capacity => items.Capacity;
        public int Count => items.Count;
        bool IList.IsFixedSize => false;
        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;

        private object syncRoot;

        object ICollection.SyncRoot {
            get {
                if (this.syncRoot == null)
                    Interlocked.CompareExchange<object>(ref this.syncRoot, new object(), null);
                return this.syncRoot;
            }
        }

        public T this[int index] {
            get => items[index];
            set => items[index] = value;
        }

        object IList.this[int index] {
            get => this[index];
            set => this[index] = (T)value;
        }

        public void Add(T item) => items.Add(item);

        int IList.Add(object item) {
            items.Add((T)item);
            return Count - 1;
        }

        public void AddRange(IEnumerable<T> collection) => items.AddRange(collection);

        public ReadOnlyCollection<T> AsReadOnly() => items.AsReadOnly();

        public int BinarySearch(T item) => items.BinarySearch(item);
        public int BinarySearch(T item, IComparer<T> comparer) => items.BinarySearch(item, comparer);

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer) =>
            items.BinarySearch(index, count, item, comparer);

        public void Clear() => items.Clear();

        public bool Contains(T item) => items.Contains(item);

        bool IList.Contains(object item) => items.Contains((T)item);

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter) => items.ConvertAll(converter);

        public void CopyTo(T[] array) => items.CopyTo(array);

        void ICollection.CopyTo(Array array, int arrayIndex) {
            Array.Copy(ToArray(), 0, array, arrayIndex, Count);
        }

        public void CopyTo(int index, T[] array, int arrayIndex, int count) {
            Array.Copy(ToArray(), index, array, arrayIndex, count);
        }

        public void CopyTo(T[] array, int arrayIndex) {
            Array.Copy(ToArray(), 0, array, arrayIndex, Count);
        }


        public bool Exists(Predicate<T> match) => items.Exists(match);

        public T Find(Predicate<T> match) => items.Find(match);

        public List<T> FindAll(Predicate<T> match) => items.FindAll(match);

        public int FindIndex(Predicate<T> match) => items.FindIndex(match);
        public int FindIndex(int startIndex, Predicate<T> match) => items.FindLastIndex(startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match) =>
            items.FindIndex(startIndex, count, match);

        public T FindLast(Predicate<T> match) => items.FindLast(match);

        public int FindLastIndex(Predicate<T> match) => items.FindLastIndex(match);
        public int FindLastIndex(int startIndex, Predicate<T> match) => items.FindLastIndex(startIndex, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match) =>
            items.FindLastIndex(startIndex, count, match);

        public void ForEach(Action<T> action) => items.ForEach(action);

        public List<T>.Enumerator GetEnumerator() => items.GetEnumerator();
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => items.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => items.GetEnumerator();

        public List<T> GetRange(int index, int count) => items.GetRange(index, count);

        public int IndexOf(T item) => items.IndexOf(item);
        int IList.IndexOf(object item) => items.IndexOf((T)item);
        public int IndexOf(T item, int index) => items.IndexOf(item, index);
        public int IndexOf(T item, int index, int count) => items.IndexOf(item, index, count);

        public void Insert(int index, T item) => items.Insert(index, item);
        void IList.Insert(int index, object item) => items.Insert(index, (T)item);

        public void InsertRange(int index, IEnumerable<T> collection) => items.InsertRange(index, collection);

        public int LastIndexOf(T item) => items.LastIndexOf(item);
        public int LastIndexOf(T item, int index) => items.LastIndexOf(item, index);
        public int LastIndexOf(T item, int index, int count) => items.LastIndexOf(item, index, count);

        public bool Remove(T item) => items.Remove(item);

        void IList.Remove(object item) => items.Remove((T)item);

        public int RemoveAll(Predicate<T> match) => items.RemoveAll(match);

        public void RemoveAt(int index) => items.RemoveAt(index);

        public void RemoveRange(int index, int count) => items.RemoveRange(index, count);

        public void Reverse() => items.Reverse();
        public void Reverse(int index, int count) => items.Reverse(index, count);

        public void Sort() => items.Sort();
        public void Sort(IComparer<T> comparer) => items.Sort(comparer);
        public void Sort(Comparison<T> comparison) => items.Sort(comparison);
        public void Sort(int index, int count, IComparer<T> comparer) => items.Sort(index, count, comparer);

        public T[] ToArray() => items.ToArray();

        public void TrimExcess() => items.TrimExcess();

        public bool TrueForAll(Predicate<T> match) => items.TrueForAll(match);
    }
}