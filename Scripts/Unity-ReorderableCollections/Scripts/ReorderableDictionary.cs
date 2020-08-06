using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;
using UnityEngine;

namespace ZeroVector.Common.Reorderable {
    // This is the easy way, but alas, it doesn't work if we want to inherit from an empty base reorderable collection class.
    //
    // public abstract class ReorderableDictionary<TKey, TValue, TContainer> : Dictionary<TKey, TValue>,
    //     ISerializationCallbackReceiver
    //     where TContainer : ReorderableDictionary<TKey, TValue, TContainer>.KeyValuePair, new() {
    //     //
    //     [SerializeField] private List<TContainer> items;
    //
    //     public abstract class KeyValuePair {
    //         public TKey key;
    //         public TValue value;
    //     }
    //
    //     public void OnBeforeSerialize() {
    //         // Unpack "items" list into dict elements and serialisedKeys and values
    //         items.Clear();
    //
    //         foreach (var keyValuePair in this) {
    //             items.Add(new TContainer {key = keyValuePair.Key, value = keyValuePair.Value});
    //         }
    //     }
    //
    //     public void OnAfterDeserialize() {
    //         // Unpack serialised keys and values into "items" and this dict itself also
    //         Clear(); // clear "this", the dictionary itself
    //         foreach (var item in items) {
    //             try {
    //                 Add(item.key, item.value);
    //             }
    //             catch (ArgumentException e) {
    //                 Debug.LogError(
    //                     $"ArgumentException: An item with the same key already exists. Key: {item.key}. Skipping.");
    //             }
    //         }
    //     }
    // }
    //
    // Thus, we have to re-implement the entire dictionary functionality...

    [SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
    [Serializable]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public abstract class ReorderableDictionary<TKey, TValue, TContainer> : Internal.BaseReorderableCollection,
        IDictionary<TKey, TValue>,
        IDictionary, IReadOnlyDictionary<TKey, TValue>,
        ISerializable, IDeserializationCallback, ISerializationCallbackReceiver
        where TContainer : ReorderableDictionary<TKey, TValue, TContainer>.KeyValuePair, new() {
        //
        // Dictionary to wrap the functionality of
        private readonly Dictionary<TKey, TValue> internalDict;

        // This is the actual list that's gonna get drawn
        [SerializeField] private List<TContainer> items = new List<TContainer>();

        public abstract class KeyValuePair {
            // ReSharper disable InconsistentNaming
            public TKey Key;
            public TValue Value;
            // ReSharper restore InconsistentNaming
            
            // Allow implicit casting of real pairs to these pairs, just in case
            public static implicit operator KeyValuePair<TKey, TValue>(KeyValuePair kvp) {
                return new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value);
            }
        }

        public void OnBeforeSerialize() {
            // Unpack "items" list into dict elements and serialisedKeys and values
            items.Clear();

            foreach (var keyValuePair in internalDict) {
                items.Add(new TContainer {Key = keyValuePair.Key, Value = keyValuePair.Value});
            }
        }

        public void OnAfterDeserialize() {
            // Unpack serialised keys and values into "items" and this dict itself also
            Clear(); // clear "this", the dictionary itself
            foreach (var item in items) {
                var newKey = item.Key;
                var i = 0;
                while (ContainsKey(newKey) && i < 100) {
                    newKey = DeduplicateKey(newKey);
                    i++;
                }
                // If we didn't find a good value after 100 iterations, abort.
                if (ContainsKey(newKey)) 
                    throw new ArgumentException($"Attempted to add an existing key to the dictionary: " +
                                                $"{newKey}. Make sure deduplication has been implemented properly.");
                Add(newKey, item.Value);
            }
        }

        /// <summary>
        /// Method used to generate new keys for duplicate elements. Whenever an element with an existing key is added
        /// to the dictionary, this implementation-specific function is called in order to mutate it into a usable new key. 
        /// </summary>
        /// <param name="duplicateKey">The value of the extant key.</param>
        /// <returns>A new key that should NOT exist in the dictionary.</returns>
        public abstract TKey DeduplicateKey(TKey duplicateKey);


        ////////////////////////////////////////////////// Regular dictionary functionality implemented below this point

        protected ReorderableDictionary() {
            internalDict = new Dictionary<TKey, TValue>(0, null);
        }

        protected ReorderableDictionary(int capacity) {
            internalDict = new Dictionary<TKey, TValue>(capacity, null);
        }

        protected ReorderableDictionary(IEqualityComparer<TKey> comparer) {
            internalDict = new Dictionary<TKey, TValue>(0, comparer);
        }

        protected ReorderableDictionary(int capacity, IEqualityComparer<TKey> comparer) {
            internalDict = new Dictionary<TKey, TValue>(capacity, comparer);
        }

        protected ReorderableDictionary(IDictionary<TKey, TValue> dictionary) {
            internalDict = new Dictionary<TKey, TValue>(dictionary, null);
        }

        protected ReorderableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) {
            internalDict = new Dictionary<TKey, TValue>(dictionary, comparer);
        }

        public IEqualityComparer<TKey> Comparer => internalDict.Comparer;

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index) {
            throw new NotImplementedException();
        }

        public int Count => internalDict.Count;
        public bool IsSynchronized => false;

        private object syncRoot;

        public object SyncRoot {
            get {
                if (syncRoot == null)
                    Interlocked.CompareExchange<object>(ref syncRoot, new object(), null);
                return syncRoot;
            }
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
        bool IDictionary.IsReadOnly => false;

        object IDictionary.this[object key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => internalDict[(TKey)key];
            set => internalDict[(TKey)key] = (TValue)value;
        }

        public TValue this[TKey key] {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => internalDict[key];
            set => internalDict[key] = value;
        }


        public ICollection Keys => internalDict.Keys;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => internalDict.Keys;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => internalDict.Keys;
        public ICollection Values => internalDict.Values;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => internalDict.Values;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => internalDict.Values;

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair) =>
            internalDict.Add(keyValuePair.Key, keyValuePair.Value);

        public void Add(TKey key, TValue value) => internalDict.Add(key, value);
        public void Add(object key, object value) => internalDict.Add((TKey)key, (TValue)value);

        public void Clear() => internalDict.Clear();

        private static bool IsCompatibleKey(object key) {
            if (key == null) throw new ArgumentNullException(nameof(key));
            return key is TKey;
        }

        public bool Contains(object key) => IsCompatibleKey(key) && ContainsKey((TKey)key);
        IDictionaryEnumerator IDictionary.GetEnumerator() => internalDict.GetEnumerator();

        public void Remove(object key) => internalDict.Remove((TKey)key);

        public bool IsFixedSize => false;

        public bool Contains(KeyValuePair<TKey, TValue> item) => internalDict.Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) =>
            throw new NotImplementedException();

        public bool ContainsKey(TKey key) => internalDict.ContainsKey(key);
        public bool ContainsValue(TValue value) => internalDict.ContainsValue(value);
        
        
        IEnumerator IEnumerable.GetEnumerator() => internalDict.GetEnumerator();
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() =>
            internalDict.GetEnumerator();

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) =>
            internalDict.GetObjectData(info, context);

        public virtual void OnDeserialization(object sender) => internalDict.OnDeserialization(sender);
        public bool Remove(TKey key) => internalDict.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => internalDict.TryGetValue(key, out value);

    }
}