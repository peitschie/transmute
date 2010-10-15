using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Transmute.Internal.Utils;

namespace Transmute.Tests.Internal.Utils
{
    [TestFixture]
    public class MemberInfoExtensionsTests
    {
        [Test]
        public void ReturnType_Field()
        {
            Assert.AreEqual(typeof(int), MemberExpressions.GetMemberInfo<DemoClass>(c => c.Field).ReturnType());
        }

        [Test]
        public void ReturnType_Property()
        {
            Assert.AreEqual(typeof(string), MemberExpressions.GetMemberInfo<DemoClass>(c => c.Property).ReturnType());
        }

        [Test]
        public void ReturnType_NonFieldOrProperty_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentException>(() => typeof(DemoClass).GetMember("Method").First().ReturnType());
            Assert.IsTrue(exception.Message.Contains(MemberTypes.Method.ToString()));
        }

#pragma warning disable 649
        private class DemoClass
        {
            public int Field;
            public string Property { get; set; }
            public object Method() { return null; }
        }
#pragma warning restore 649
    }
}