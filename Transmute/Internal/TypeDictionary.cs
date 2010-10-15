using System;
using System.Collections;
using System.Collections.Generic;

namespace Transmute.Internal
{
    public class TypeDictionary<TValue> : IEnumerable<KeyValuePair<int, TValue>>
    {
        private readonly Dictionary<int, TValue> _dictionary = new Dictionary<int, TValue>();
        private readonly IList<TypesMapEntry<TValue>> _mapEntries = new List<TypesMapEntry<TValue>>();

        public void Add(Type type1, Type type2, TValue value)
        {
            _dictionary.Add(ToKey(type1, type2), value);
            _mapEntries.Add(new TypesMapEntry<TValue>(type1, type2, ToKey(type1, type2), value));
        }

        public bool TryGetValue(Type type1, Type type2, out TValue value)
        {
            return _dictionary.TryGetValue(ToKey(type1, type2), out value);
        }

        public bool ContainsKey(Type type1, Type type2)
        {
            return _dictionary.ContainsKey(ToKey(type1, type2));
        }

        /// <summary>
        /// This ToKey method drives this whole dictionary type. The intent is to avoid creation of 
        /// numerous objects for comparison only, as would be the case if a composite key was used directly
        /// with the internal dictionary.  This lookup operation is performed many times per map, so needs to
        /// be as fast as possible.  The algorithm is order specific, such that type1,type2 does not
        /// return the same value as type2,type1. It is independant of the containing TypeDictionary definition however
        /// in that a call to TypeDictionary&lt;int&gt; and TypeDictionary&lt;string&gt; will both return the same value
        /// </summary>
        /// <param name="type1">First type</param>
        /// <param name="type2">Second type</param>
        /// <returns>integer hash that is relatively unique for each first & second type</returns>
        public static int ToKey(Type type1, Type type2)
        {
            return ((type1 != null ? type1.GetHashCode() : 0) * 397) ^ (type2 != null ? type2.GetHashCode() : 0);
        }

        /// <summary>
        /// Used for debugging purposes only.  As the ToKey algorithm essentially a one-way operation, it is impossible to tell what is within
        /// dictionary from the existing entries.  Therefore, this MapEntries collection is kept in sync, such that when viewed in a debugger
        /// it is possible to tell what is contained within the dictionary and how it maps to the types
        /// </summary>
        public IList<TypesMapEntry<TValue>> MapEntries { get { return _mapEntries; } }

        public IEnumerator<KeyValuePair<int, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class TypesMapEntry<TValue>
    {
        private readonly Type _type1;
        private readonly Type _type2;
        private readonly int _key;
        private readonly TValue _value;

        public TypesMapEntry(Type type1, Type type2, int key, TValue value)
        {
            _type1 = type1;
            _type2 = type2;
            _key = key;
            _value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}<{1},{2}>({3})={4}", GetType().Name, _type1, _type2, _value, _key);
        }
    }
}