using System;
using NUnit.Framework;
using Transmute.Internal;
using Transmute.Internal.Utils;

namespace Transmute.Tests.Internal
{
    [TestFixture]
    public class TypeDictionaryTests
    {
        private Type _type1;
        private Type _type2;
        private object _value;
        private int _key;
        private TypeDictionary<object> _dictionary;

        [SetUp]
        public void SetUp()
        {
            _value = new object();
            _type1 = typeof (int);
            _type2 = typeof (string);
            _dictionary = new TypeDictionary<object>();
            _key = TypeDictionary<object>.ToKey(_type1, _type2);
        }

        [Test]
        public void Add_AddsToMapEntries()
        {
            _dictionary.Add(_type1, _type2, _value);
            Assert.AreEqual(1, _dictionary.MapEntries.Count);
            Assert.AreEqual("{0}<{1},{2}>({3})={4}".With(typeof(TypesMapEntry<object>).Name, _type1, _type2, _value, _key), _dictionary.MapEntries[0].ToString());
        }

        [Test]
        public void Add_MultipleAddsThrowsExceptions()
        {
            _dictionary.Add(_type1, _type2, _value);
            Assert.Throws<ArgumentException>(() => _dictionary.Add(_type1, _type2, _value));
        }

        [Test]
        public void Add_TryGetRetrievesFromDictionary()
        {
            _dictionary.Add(_type1, _type2, _value);
            Assert.AreSame(_value, Get(_type1, _type2));
        }

        [Test]
        public void Add_DictionaryContainsKey()
        {
            _dictionary.Add(_type1, _type2, _value);
            Assert.IsTrue(_dictionary.ContainsKey(_type1, _type2));
        }

        [Test]
        public void ToKey_DeterministicPlease()
        {
            var key = TypeDictionary<object>.ToKey(_type1, _type2);
            Assert.AreEqual(key, TypeDictionary<int>.ToKey(_type1, _type2));
            Assert.AreEqual(key, TypeDictionary<object>.ToKey(_type1, _type2));
            Assert.AreEqual(key, TypeDictionary<object>.ToKey(_type1, _type2));
        }

        [Test, Explicit("Useful for benchmarking changes or tweaks to TypeDictionary key generation. SLOOOW when using coverage of any sort and highly cpu dependant")]
        public void Benchmark_TryGetValue()
        {
            int count = 0;
            object result;
            _dictionary.Add(_type1, _type2, _value);
            var endTime = DateTime.Now.AddMilliseconds(1000);
            while (endTime > DateTime.Now)
            {
                _dictionary.TryGetValue(_type1, _type2, out result);
                count++;
            }
            //Assert.Pass(count.ToString());
            Assert.IsTrue(count > 800000, "To slow: {0}".With(count));
        }

        [Test, Explicit("Useful for benchmarking changes or tweaks to TypeDictionary key generation. SLOOOW when using coverage of any sort and highly cpu dependant")]
        public void Benchmark_ContainsKey()
        {
            int count = 0;
            _dictionary.Add(_type1, _type2, _value);
            var endTime = DateTime.Now.AddMilliseconds(1000);
            while (endTime > DateTime.Now)
            {
                _dictionary.ContainsKey(_type1, _type2);
                count++;
            }
            //Assert.Pass(count.ToString());
            Assert.IsTrue(count > 800000, "To slow: {0}".With(count));
        }

        [Test]
        private object Get(Type type1, Type type2)
        {
            object result;
            _dictionary.TryGetValue(type1, type2, out result);
            return result;
        }
    }
}