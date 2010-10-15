using System;
using NUnit.Framework;
using Transmute.Internal;
using Transmute.Internal.Utils;
using Transmute.Tests.Types;

namespace Transmute.Tests.Internal
{
    [TestFixture]
    public class IgnoredSetterTests
    {
        private IgnoredSetter<bool> _setter;

        [SetUp]
        public void SetUp()
        {
            _setter = new IgnoredSetter<bool>(MemberExpressions.GetMemberInfo<ChildClass>(c => c.String));
        }

        [Test]
        public void GenerateCopyValueCall_ReturnsNull()
        {
            Assert.IsNull(_setter.GenerateCopyValueCall());
        }

        [Test]
        public void Name_IsExpected()
        {
            Assert.AreEqual("String", _setter.Name);
        }

        [Test]
        public void NullMember_ThrowsException()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => _setter = new IgnoredSetter<bool>(null));
            Assert.AreEqual("member", exception.ParamName);
        }
    }
}