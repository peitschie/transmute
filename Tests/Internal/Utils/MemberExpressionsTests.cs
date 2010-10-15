using System;
using System.Reflection;
using NUnit.Framework;
using Transmute.Internal.Utils;

namespace Transmute.Tests.Internal.Utils
{
    [TestFixture]
    public class MemberExpressionsTests
    {
        [Test]
        public void GetMemberInfo_ClassParent_SimpleProperty1()
        {
            var info = (PropertyInfo)MemberExpressions.GetMemberInfo<ClassParent>(c => c.Property1);
            Assert.IsNotNull(info);
            Assert.AreEqual(typeof(ClassParent), info.ReflectedType);
            Assert.AreEqual("Property1", info.Name);
        }

        [Test]
        public void GetMemberInfo_ClassParent_SimpleField1()
        {
            var info = (FieldInfo)MemberExpressions.GetMemberInfo<ClassParent>(c => c.Field1);
            Assert.IsNotNull(info);
            Assert.AreEqual(typeof(ClassParent), info.ReflectedType);
            Assert.AreEqual("Field1", info.Name);
        }

        [Test]
        public void GetMemberInfo_InheritedMember_ReturnsInheritingClassType()
        {
            var instance = new ClassChild1();
            const int number = 10;

            var info = (PropertyInfo)MemberExpressions.GetMemberInfo<ClassChild1>(c => c.Property1);
            Assert.AreEqual(typeof(ClassChild1), info.ReflectedType);
            info.SetValue(instance, number, null);
            Assert.AreEqual(number, instance.Property1);
        }

        [Test]
        public void GetMemberInfo_InheritedMember_NewProperty_HidesInherited()
        {
            var instance = new SpecificInterface();
            const int number = 10;

            var info = (PropertyInfo)MemberExpressions.GetMemberInfo<ISpecificInterface>(c => c.Property1);

            Assert.AreEqual(typeof(ISpecificInterface), info.ReflectedType);
            Assert.AreEqual(typeof(int), info.PropertyType);

            info.SetValue(instance, number, null);
            Assert.AreEqual(number, instance.Property1);
        }

        [Test]
        public void GetMemberInfo_InheritedMember_NewProperty_BaseStillAccessible()
        {
            var instance = new SpecificInterface();
            const int number = 10;

            var info = (PropertyInfo)MemberExpressions.GetMemberInfo<IGeneralInterface>(c => c.Property1);

            Assert.AreEqual(typeof(IGeneralInterface), info.ReflectedType);
            Assert.AreEqual(typeof(object), info.PropertyType);

            info.SetValue(instance, number, null);
            Assert.AreEqual(number, instance.Property1);
        }

        [Test]
        public void GetMemberInfo_LambdaExpression_Basic()
        {
            var info = (PropertyInfo)MemberExpressions.GetMemberInfo((ClassParent c) => c.Property1);

            Assert.AreEqual(typeof(ClassParent), info.ReflectedType);
            Assert.AreEqual(typeof(int), info.PropertyType);
        }

        [Test]
        public void GetMemberInfo_DeepMemberChain()
        {
            var memberInfo = MemberExpressions.GetMemberInfo<DeepClass>(c => c.String.Length);
            Assert.IsNotNull(memberInfo);
            Assert.AreEqual(MemberExpressions.GetMemberInfo<string>(c => c.Length), memberInfo);
        }

        [Test]
        public void GetMemberInfo_DeepMemberChain_WithCast()
        {
            var memberInfo = MemberExpressions.GetMemberInfo<DeepClass>(c => ((string)c.String).Length);
            Assert.IsNotNull(memberInfo);
            Assert.AreEqual(MemberExpressions.GetMemberInfo<string>(c => c.Length), memberInfo);
        }

        [Test]
        public void GetMemberInfo_NonPropertyOrFieldType_ThrowsException()
        {
            Assert.Throws<MemberExpressionException>(() => MemberExpressions.GetMemberInfo<DeepClass>(c => c.String.Length + 1));
        }

        [Test]
        public void GetMemberInfo_Generic_NullMember_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => MemberExpressions.GetMemberInfo<DeepClass>(null));
        }

        [Test]
        public void GetMemberInfo_NonGeneric_NullMember_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => MemberExpressions.GetMemberInfo(null));
        }

        [Test]
        public void GetExpressionChain_Generic_NullMember_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => MemberExpressions.GetExpressionChain<DeepClass>(null));
        }

        [Test]
        public void GetExpressionChain_NonGeneric_NullMember_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => MemberExpressions.GetExpressionChain(null));
        }

        [Test]
        public void GetExpressionChain_NonPropertyOrFieldType_ThrowsException()
        {
            Assert.Throws<MemberExpressionException>(() => MemberExpressions.GetExpressionChain<DeepClass>(c => c.String.Length + 1));
        }

        [Test]
        public void GetMemberChain_DeepMemberChain()
        {
            var memberInfo = MemberExpressions.GetExpressionChain<DeepClass>(c => c.String.Length);
            var expected = new[]
                {
                    MemberExpressions.GetMemberInfo<DeepClass>(c => c.String),
                    MemberExpressions.GetMemberInfo<string>(c => c.Length)
                };
            Assert.IsNotNull(memberInfo);
            Assert.AreEqual(expected, memberInfo);
        }

        [Test]
        public void GetMemberChain_DeepMemberChain_WithCast()
        {
            var memberInfo = MemberExpressions.GetExpressionChain<DeepClass>(c => ((string)c.String).Length);
            var expected = new[]
                {
                    MemberExpressions.GetMemberInfo<DeepClass>(c => c.String),
                    MemberExpressions.GetMemberInfo<string>(c => c.Length)
                };
            Assert.IsNotNull(memberInfo);
            Assert.AreEqual(expected, memberInfo);
        }

        // ReSharper disable ClassNeverInstantiated.Local
        private class DeepClass
        {
            public string String { get; set; }
        }
        // ReSharper restore ClassNeverInstantiated.Local

        public class ClassParent
        {
            public int Property1 { get; set; }
            public int Field1;
        }

        public class ClassChild1 : ClassParent { }

        public interface IGeneralInterface
        {
            object Property1 { get; set; }
        }

        public interface ISpecificInterface : IGeneralInterface
        {
            new int Property1 { get; set; }
        }

        public class SpecificInterface : ISpecificInterface
        {
            object IGeneralInterface.Property1
            {
                get { return Property1; }
                set { Property1 = (int)value; }
            }

            public int Property1 { get; set; }
        }
    }
}