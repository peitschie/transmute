using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Transmute.Internal;
using Transmute.Internal.Utils;
using Transmute.Tests.Types;

namespace Transmute.Tests.Internal
{
    [TestFixture]
    [Ignore("This class is soon to be redundant")]
    public class MemberSetterTests
    {
        private MemberSetter<object> _setter;

        [Test]
        public void New_NullGetter()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new MemberSetter<object>((PropertyInfo)MemberExpressions.GetMemberInfo<ChildClass>(c => c.String), null));
            Assert.AreEqual("get", exception.ParamName);
        }

        [Test]
        public void New_ReadonlyProperty()
        {
            var property = (PropertyInfo)MemberExpressions.GetMemberInfo<string>(c => c.Length);
            var exception = Assert.Throws<ArgumentException>(() => new MemberSetter<object>(property, (from, to, mapper, context) => 10));
            Assert.AreEqual("Target member {0} must be writeable".With(property), exception.Message);
        }

        [Test]
        public void New_ChainedProperty()
        {
            var properties = MemberExpressions.GetExpressionChain<DeepClass>(c => c.Child.String).Select(p => (PropertyInfo)p).ToArray();
            var setter = new MemberSetter<object>(properties, (from, to, mapper, context) => "10");
            var setterMethod = setter.GenerateCopyValueCall();
            var result = new DeepClass();
            setterMethod(null, null, null, result, null, null);
            Assert.AreEqual("10", result.Child.String);
            Assert.AreEqual("Child.String", setter.Name);
        }

        [Test]
        public void New_ChainedProperty_SetterWithPrefix()
        {
            var properties = MemberExpressions.GetExpressionChain<ChildClass>(c => c.String).Select(p => (PropertyInfo)p).ToArray();
            var setter = new MemberSetter<object>(properties, (from, to, mapper, context) => "10", null, MemberExpressions.GetExpressionChain<DeepClass>(c => c.Child));
            var setterMethod = setter.GenerateCopyValueCall();
            var result = new DeepClass();
            setterMethod(null, null, null, result, null, null);
            Assert.AreEqual("10", result.Child.String);
            Assert.AreEqual("Child.String", setter.Name);
        }

        [Test]
        public void New_ChainedProperty_SetterWithPrefix_MultiLevel()
        {
            var resourceMapper = new ResourceMapper<object>();
            resourceMapper.InitializeMap();
            var properties = MemberExpressions.GetExpressionChain<ChildClass>(c => c.String).Select(p => (PropertyInfo)p).ToArray();
            var setter = new MemberSetter<object>(properties, (from, to, mapper, context) => "10", null, MemberExpressions.GetExpressionChain<DeeperClass>(c => c.DeepClass.Child));
            var setterMethod = setter.GenerateCopyValueCall();
            var result = new DeeperClass();
            setterMethod(null, null, null, result, resourceMapper, null);
            Assert.AreEqual("10", result.DeepClass.Child.String);
            Assert.AreEqual("DeepClass.Child.String", setter.Name);
        }

        [Test]
        public void New_ChainedProperty_GetterWithPrefix()
        {
            var properties = MemberExpressions.GetExpressionChain<ChildClass>(c => c.String).Select(p => (PropertyInfo)p).ToArray();
            var setter = new MemberSetter<object>(properties, (from, to, mapper, context) => ((ChildClass)from).String, 
                MemberExpressions.GetExpressionChain<DeepClass>(c => c.Child), MemberExpressions.GetExpressionChain<DeepClass>(c => c.Child));
            var setterMethod = setter.GenerateCopyValueCall();

            var input = new DeepClass {Child = new ChildClass {String = "10"}};
            var result = new DeepClass();
            
            setterMethod(null, null, input, result, null, null);
            
            Assert.AreEqual("10", result.Child.String);
            Assert.AreEqual("Child.String", setter.Name);
        }

        [Test]
        public void Construct_PropertyInfo_NullGetter()
        {
            var argument = Assert.Throws<ArgumentNullException>(() => new MemberSetter<bool>(Property, null));
            Assert.AreEqual("get", argument.ParamName);
        }

        [Test]
        public void Construct_PropertyInfo_ReadonlyProperty()
        {
            var property = GetMember<string>(s => s.Length);
            var argument = Assert.Throws<ArgumentException>(() => new MemberSetter<bool>(property, delegate { return new object(); }));
            Assert.AreEqual("Target member {0} must be writeable".With(property), argument.Message);
        }

        [Test]
        public void Construct_PropertyInfoList_NullProperty()
        {
            var argument = Assert.Throws<ArgumentNullException>(() => new MemberSetter<bool>((PropertyInfo[])null, delegate { return new object(); }));
            Assert.AreEqual("to", argument.ParamName);
        }

        [Test]
        public void Construct_PropertyInfoList_EmptyPropertyList()
        {
            var argument = Assert.Throws<ArgumentException>(() => new MemberSetter<bool>(new PropertyInfo[0], delegate { return new object(); }));
            Assert.AreEqual("At least one target property must be specified", argument.Message);
        }

        [Test]
        public void Construct_PropertyInfoList_NullGetter()
        {
            var argument = Assert.Throws<ArgumentNullException>(() => new MemberSetter<bool>(Properties, null));
            Assert.AreEqual("get", argument.ParamName);
        }

        [Test]
        public void Construct_PropertyInfoList_ChainWithReadonlySetter()
        {
            var resourceMapper = new ResourceMapper<object>();
            resourceMapper.InitializeMap();
            const string expectedString = "teststring";
            var result = new DeepClassReadonly();
            _setter = new MemberSetter<object>(GetChain<DeepClassReadonly>(c => c.Child.String), delegate { return expectedString; });
            _setter.GenerateCopyValueCall().Invoke(null, null, null, result, resourceMapper, null);
            Assert.AreEqual(expectedString, result.Child.String);
        }

        [Test]
        public void Construct_PropertyInfoList_ChainWithSetter()
        {
            const string expectedString = "teststring";
            var result = new DeepClass();
            _setter = new MemberSetter<object>(GetChain<DeepClass>(c => c.Child.String), delegate { return expectedString; });
            _setter.GenerateCopyValueCall().Invoke(null, null, null, result, null, null);
            Assert.AreEqual(expectedString, result.Child.String);
        }

        [Test]
        public void Construct_PropertyInfoList_SingleWithSetter()
        {
            var expected = new ChildClass();
            var result = new DeepClass();
            _setter = new MemberSetter<object>(GetChain<DeepClass>(c => c.Child), delegate { return expected; });
            _setter.GenerateCopyValueCall().Invoke(null, null, null, result, null, null);
            Assert.AreSame(expected, result.Child);
        }

        [Test]
        public void Name_IsExpected()
        {
            _setter = new MemberSetter<object>(Property, delegate { return new object(); });
            Assert.AreEqual("String", _setter.Name);
        }

        [Test]
        public void MemberInfo_MethodTypeNotSupported()
        {
            var exception = Assert.Throws<ArgumentException>(() => new MemberSetter<bool>(GetType().GetMember("Name_IsExpected"), delegate { return new object(); }));
            Assert.AreEqual("Only Field or Property members are supported", exception.Message);
        }

        [Test]
        public void InvalidMember_ThrowExceptions_ReadonlyField()
        {
            var member = GetMember<ReadonlyPropertiesAndFields>(c => c.ReadonlyField);
            var exception = Assert.Throws<ArgumentException>(() => new MemberSetter<bool>(member, delegate { return new object ();} ));
            Assert.AreEqual("Target member {0} must be writeable".With(member), exception.Message);
        }

        [Test]
        public void InvalidMember_ThrowExceptions_ReadonlyProperty()
        {
            var member = GetMember<ReadonlyPropertiesAndFields>(c => c.ReadonlyProperty);
            var exception = Assert.Throws<ArgumentException>(() => new MemberSetter<bool>(member, delegate { return new object(); }));
            Assert.AreEqual("Target member {0} must be writeable".With(member), exception.Message);
        }

        [Test]
        public void InvalidMember_ThrowExceptions_ConstantField()
        {
            var member = typeof(ReadonlyPropertiesAndFields).GetMember("ConstField");
            var exception = Assert.Throws<ArgumentException>(() => new MemberSetter<bool>(member, delegate { return new object(); }));
            Assert.AreEqual("Target member {0} must be writeable".With(member), exception.Message);
        }

        [Test]
        public void ValidMember_Field()
        {
            var member = GetMember<NormalPropertiesAndFields>(c => c.ValidField);
            Assert.DoesNotThrow(() => _setter = new MemberSetter<object>(member, delegate { return new object(); }));
        }

        private static MemberInfo Property { get { return MemberExpressions.GetMemberInfo<ChildClass>(c => c.String); } }

        private static MemberInfo[] Properties { get { return GetChain<DeepClass>(c => c.Child.String); } }

        private static MemberInfo GetMember<TType>(Expression<Func<TType, object>> expression)
        {
            return MemberExpressions.GetMemberInfo(expression);
        }

        private static MemberInfo[] GetChain<TType>(Expression<Func<TType, object>> expression)
        {
            return MemberExpressions.GetExpressionChain(expression).Select(p => p as PropertyInfo).ToArray();
        }
    }
}