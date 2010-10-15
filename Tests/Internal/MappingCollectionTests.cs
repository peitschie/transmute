using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq;
using NGineer;
using NUnit.Framework;
using Transmute.Exceptions;
using Transmute.Internal;
using Transmute.Internal.Utils;
using Transmute.MemberResolver;
using Transmute.Tests.Types;

namespace Transmute.Tests.Internal
{
    [TestFixture]
    public class MappingCollectionTests
    {
        private Mock<IResourceMapper<CloneableTestContext>> _mapper;
        private MappingCollection<ClassWithSeveralPropertiesSrcNullable, ClassWithSeveralPropertiesDest, CloneableTestContext> _collection;
        private IBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new Builder();
            _mapper = new Mock<IResourceMapper<CloneableTestContext>>();
            _mapper.Setup(m => m.MemberConsumers).Returns(new PriorityList<IMemberConsumer> { new DefaultMemberConsumer() });
            _mapper.Setup(m => m.MemberResolvers).Returns(new PriorityList<IMemberResolver> { new IgnoreCaseNameMatcher() });
            _collection = new MappingCollection<ClassWithSeveralPropertiesSrcNullable, ClassWithSeveralPropertiesDest, CloneableTestContext>(_mapper.Object);
        }

        [Test]
        public void SetOrder_IsPreserved()
        {
            new ClassWithSeveralPropertiesNullableOverride<CloneableTestContext>().OverrideMapping(_collection);
            Assert.AreEqual(ClassWithSeveralPropertiesOverride<CloneableTestContext>.PropertySetOrder, _collection.Setters.Select(m => m.Name));
        }

        [Test]
        public void Set_MemberInfo_RemovesFromAvailableDestionation_AddsToSetters()
        {
            var member = GetDestInfo(c => c.Property1);
            _collection.SetMember(member, (from, to, mapper, context) => 10);
            VerifySettersAndAvailable(member);
        }

        private void VerifySettersAndAvailable(params MemberInfo[] member)
        {
            Assert.AreEqual(member.Length, _collection.Setters.Count());
            foreach (var memberInfo in member)
            {
                Assert.IsTrue(_collection.Setters.Any(c => c.Name == memberInfo.Name), string.Format("{0} was not found in setters", memberInfo.Name));
                Assert.IsFalse(_collection.Unmapped.Destination.Any(c => c.Name == memberInfo.Name));    
            }
        }

        [Test]
        public void ClassWithProtectedProperties_ProtectedAccessorsShouldNotBeAvailable()
        {
            // Arrange:
            // const string altogetherProtectedProperty = "ImAltogetherProtected"; // Not expected to be available either as a source or destination property.
            const string propertyWithProtectedGetter = "MyGetterIsProtected";
            var propertyWithProtectedSetter = MemberExpressions.GetMemberInfo<ClassWithProtectedProperties>(c => c.MySetterIsProtected).Name;
            var altogetherAccessibleProperty = MemberExpressions.GetMemberInfo<ClassWithProtectedProperties>(c => c.ImAltogetherAccessible).Name;

            var expectedSourcePropertyNames = new[] { propertyWithProtectedSetter, altogetherAccessibleProperty };
            var expectedDestinationProperyNames = new[] { propertyWithProtectedGetter, altogetherAccessibleProperty };

            // Act:
            var mappingCollection = new MappingCollection<ClassWithProtectedProperties, ClassWithProtectedProperties, object>(null);
            var availableSourcePropertyNames = mappingCollection.Unmapped.Source.Select(sourceMember => sourceMember.Name).ToArray();
            var availableDestinationPropertyNames = mappingCollection.Unmapped.Destination.Select(destMember => destMember.Name).ToArray();

            // Assert:
            Array.Sort(expectedSourcePropertyNames);
            Array.Sort(availableSourcePropertyNames);
            Assert.AreEqual(expectedSourcePropertyNames, availableSourcePropertyNames);

            Array.Sort(expectedDestinationProperyNames);
            Array.Sort(availableDestinationPropertyNames);
            Assert.AreEqual(expectedDestinationProperyNames, availableDestinationPropertyNames);
        }

        [Test]
        public void Set_ChainedProperty_CorrectPropertySetterCreated()
        {
            _collection.Set(to => to.Child.String, () => "10");
            Assert.AreEqual(1, _collection.Setters.Count());
            Assert.IsFalse(_collection.Unmapped.Destination.Any(c => c.Name == GetDestInfo(o => o.Child).Name));
            var setterMethod = _collection.Setters.First().GenerateCopyValueCall();
            var result = new ClassWithSeveralPropertiesDest(){Child = new ChildClass()};
            setterMethod(null, null, null, result, null, null);
            Assert.AreEqual("10", result.Child.String);
        }

        [Test]
        public void Set_ImplicitConvert_Expression_Int_To_NullableInt_ThrowsException()
        {
            var exception = Assert.Throws<MemberMappingException>(() => _collection.Set(to => to.Property1, (from, to, mapper, context) => (int?) null));
            Assert.AreEqual(typeof(ClassWithSeveralPropertiesSrcNullable), exception.From);
            Assert.AreEqual(typeof(ClassWithSeveralPropertiesDest), exception.To);
            Assert.IsTrue(exception.Message.Contains(typeof(int?).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(int).ToString()));
        }

        [Test]
        public void Set_ImplicitConvert_Expression_Expression_Int_To_NullableInt_ThrowsException()
        {
            var exception = Assert.Throws<MemberMappingException>(() => _collection.Set(to => to.Property1, from => from.Property1, false));
            Assert.AreEqual(typeof(ClassWithSeveralPropertiesSrcNullable), exception.From);
            Assert.AreEqual(typeof(ClassWithSeveralPropertiesDest), exception.To);
            Assert.IsTrue(exception.Message.Contains(typeof(int?).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(int).ToString()));
        }

        [Test]
        public void Set_ExplicitConversion_Expression_Expression_Int_To_NullableInt_NoException()
        {
            _mapper.Setup(c => c.CanMap(typeof (int?), typeof (int))).Returns(true);
            var toMember = GetDestInfo(c => c.Property1);
            var fromMember = GetSrcInfo(c => c.Property1);
            _collection.Set(to => to.Property1, from => from.Property1, true);
            VerifySettersAndAvailable(toMember);
            Assert.IsNull(_collection.Unmapped.Source.FirstOrDefault(c => c.Name == fromMember.Name));
        }

        [Test]
        public void Set_ExplicitConversion_Expression_Expression_FromChildProperty_NoException()
        {
            _mapper.Setup(c => c.CanMap(typeof(string), typeof(int))).Returns(true);
            var toMember = GetDestInfo(c => c.Property1);
            var fromMember = GetSrcInfo(c => c.Child);
            _collection.Set(to => to.Property1, from => from.Child.String, true);
            VerifySettersAndAvailable(toMember);
            Assert.IsNotNull(_collection.Unmapped.Source.FirstOrDefault(c => c.Name == fromMember.Name));
        }

        [Test]
        public void Set_NoConversion_Expression_Expression_FromChildProperty_NoException()
        {
            var toMember = GetDestInfo(c => c.Property1);
            var fromMember = GetSrcInfo(c => c.Property2);

            _collection.Set(to => to.Property1, from => from.Property2);

            _mapper.Verify(c => c.CanMap(typeof(string), typeof(int)), Times.Never());
            VerifySettersAndAvailable(toMember);
            Assert.IsNull(_collection.Unmapped.Source.FirstOrDefault(c => c.Name == fromMember.Name));
        }

        [Test]
        public void Set_MemberInfo_To_MemberInfo_ConstructAndMap()
        {
            _mapper.Setup(c => c.Map(typeof(int?), typeof(ChildClass), It.IsAny<int?>(), It.IsAny<ChildClass>(), It.IsAny<CloneableTestContext>()))
                .Returns<Type, Type, int?, ChildClass, object>((tfrom, tto, from, to, context) => to);

            _collection.SetMember(GetDestInfo(c => c.Child),
                GetSrcInfo(c1 => c1.Property1));

            VerifySettersAndAvailable(GetDestInfo(c => c.Child));

            _collection.Setters.Last().GenerateCopyValueCall().Invoke(typeof(int?), typeof(ChildClass), 
                new ClassWithSeveralPropertiesSrcNullable() { Property1 = 11 }, 
                new ClassWithSeveralPropertiesDest(), _mapper.Object, null);

            _mapper.Verify(c => c.Map(typeof(int?), typeof(ChildClass), It.IsAny<int?>(), It.IsAny<ChildClass>(), It.IsAny<CloneableTestContext>()));
        }

        [Test]
        public void Set_MemberInfo_To_MemberInfo_MapOnly()
        {
            _mapper.Setup(c => c.Map(typeof(int?), typeof(ChildClass), It.IsAny<int?>(), It.IsAny<ChildClass>(), It.IsAny<CloneableTestContext>()))
                .Returns<Type, Type, int?, ChildClass, object>((tfrom, tto, from, to, context) => to);

            _collection.SetMember(GetDestInfo(c => c.Child),
                GetSrcInfo(c1 => c1.Property1));

            VerifySettersAndAvailable(GetDestInfo(c => c.Child));

            _collection.Setters.Last().GenerateCopyValueCall().Invoke(typeof(int?), typeof(ChildClass),
                new ClassWithSeveralPropertiesSrcNullable() { Property1 = 11 },
                new ClassWithSeveralPropertiesDest(), _mapper.Object, null);

            _mapper.Verify(c => c.Map(typeof(int?), typeof(ChildClass), It.IsAny<int?>(), It.IsAny<ChildClass>(), It.IsAny<CloneableTestContext>()));
        }

        [Test]
        public void Set_MemberInfo_To_MemberInfo()
        {
            _mapper.Setup(c => c.CanMap(typeof(int?), typeof(ChildClass))).Returns(true);
            _collection.SetMember(GetDestInfo(c => c.Child),GetSrcInfo(c1 => c1.Property1));
            VerifySettersAndAvailable(GetDestInfo(c => c.Child));
        }

        [Test]
        public void Set_MemberInfo_To_MemberSource_ThrowsIfMemberIsNull()
        {
            _mapper.Setup(c => c.CanMap(typeof(int?), typeof(ChildClass))).Returns(true);
            var exception = Assert.Throws<MemberMappingException>(() => _collection.SetMember((MemberInfo[])null, 
                (from, to, mapper, context) => GetSrcInfo(c1 => c1.Property1)));
            Assert.IsNull(exception.ToMember);
        }

        [Test]
        public void Set_Expression_To_Expression()
        {
            _collection.Set(to => to.Property2, from => from.Property2);
            VerifySettersAndAvailable(GetDestInfo(c => c.Property2));
        }

        [Test]
        public void Set_Expression_To_Expression_SourceCastsAreProperlyPreserved()
        {
            _collection.Set(to => to.Child, from => (object)from.Child);
            VerifySettersAndAvailable(GetDestInfo(c => c.Child));

            var source = new ClassWithSeveralPropertiesSrcNullable() { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest), source, dest, _mapper.Object, null);
            _mapper.Verify(c => c.Map(typeof(object), typeof(ChildClass), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CloneableTestContext>()));
        }

        [Test]
        public void Set_Expression_To_Expression_DestinationCastsAreProperlyPreserved()
        {
            _collection.Set(to => (object)to.Child, from => from.Child);
            VerifySettersAndAvailable(GetDestInfo(c => c.Child));

            var source = new ClassWithSeveralPropertiesSrcNullable { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest), source, dest, _mapper.Object, null);
            _mapper.Verify(c => c.Map(typeof(ChildClass), typeof(object), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CloneableTestContext>()));
        }

        [Test]
        public void Set_Expression_To_Func()
        {
            _collection.Set(to => to.Property2, (from, to, mapper, context) => from.Property2+1);
            VerifySettersAndAvailable(GetDestInfo(c => c.Property2));

            var source = new ClassWithSeveralPropertiesSrcNullable() { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(
                typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest),
                source, dest, _mapper.Object, null);
            Assert.AreEqual(source.Property2 + 1, dest.Property2);
        }

        [Test]
        public void Set_Expression_To_Expression_Func()
        {
            _mapper.Setup(c => c.CanMap(typeof(int), typeof(int))).Returns(true);

            _collection.Set(to => to.Property2, from => from.Property2 + 1);
            VerifySettersAndAvailable(GetDestInfo(c => c.Property2));

            var source = new ClassWithSeveralPropertiesSrcNullable() { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(
                typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest),
                source, dest, _mapper.Object, null);
            Assert.AreEqual(source.Property2 + 1, dest.Property2);
        }

        [Test]
        public void Set_Expression_To_Expression_Func_WithTypeRemap()
        {
            _mapper.Setup(c => c.CanMap(typeof(string), typeof(int))).Returns(true);
            _mapper.Setup(c => c.Map(typeof(string), typeof(int), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CloneableTestContext>()))
                .Returns<Type, Type, object, object, CloneableTestContext>((tfrom, tto, from, to, con) => int.Parse(from.ToString()));


            _collection.Set(to => to.Property2, from => (from.Property2 + 1).ToString(), true);
            VerifySettersAndAvailable(GetDestInfo(c => c.Property2));

            var source = new ClassWithSeveralPropertiesSrcNullable { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(
                typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest),
                source, dest, _mapper.Object, null);
            Assert.AreEqual(source.Property2 + 1, dest.Property2);
        }

        [Test]
        public void Set_Expression_To_Func_WithConversion()
        {
            _mapper.Setup(c => c.CanMap(typeof(long), typeof(int))).Returns(true);
            _mapper.Setup(c => c.Map(typeof(long), typeof(int), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CloneableTestContext>()))
                .Returns<Type, Type, object, object, object>((tfrom, tto, from, to, context) => Convert.ToInt32(from));
            
            _collection.Set(to => to.Property2, (from, to, mapper, context) => (long)from.Property2+1, true);
            VerifySettersAndAvailable(GetDestInfo(c => c.Property2));

            var source = new ClassWithSeveralPropertiesSrcNullable() { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(
                typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest), 
                source, dest, _mapper.Object, null);
            _mapper.Verify(c => c.Map(typeof(long), typeof(int), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CloneableTestContext>()));
            Assert.AreEqual(source.Property2+1, dest.Property2);
        }

        [Test]
        public void Set_Expression_To_Func_NoConversion_IfTypesMatch()
        {
            _collection.Set(to => to.Child, (from, to, mapper, context) => new ChildClass{String = "Hello!"});
            VerifySettersAndAvailable(GetDestInfo(c => c.Child));

            var source = new ClassWithSeveralPropertiesSrcNullable() { Property2 = 10 };
            var dest = new ClassWithSeveralPropertiesDest();
            _collection.Setters.First().GenerateCopyValueCall().Invoke(typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest), source, dest, _mapper.Object, null);
            _mapper.Verify(c => c.CanMap(typeof(ChildClass), typeof(ChildClass)), Times.Never());
            _mapper.Verify(c => c.Map(typeof(ChildClass), typeof(ChildClass), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CloneableTestContext>()), Times.Never());
            Assert.AreEqual("Hello!", dest.Child.String);
        }

        [Test]
        public void Set_Expression_To_Func_WithConversion_NoMappingAvailable()
        {
            _mapper.Setup(c => c.CanMap(typeof(int), typeof(int))).Returns(false);
            var exception = Assert.Throws<MemberMappingException>(() => _collection.Set(to => to.Property2, (from, to, mapper, context) => from.Property2, true));
            Assert.AreEqual(GetDestInfo(c => c.Property2), exception.ToMember);
        }

        [Test]
        public void Ignore_MemberInfo_AddsToSender()
        {
            var member = GetDestInfo(c => c.Child);
            _collection.IgnoreMember(member);
            Assert.AreEqual(1, _collection.Setters.Count());
            Assert.AreEqual(member.Name, _collection.Setters.First().Name);
            Assert.IsNull(_collection.Unmapped.Destination.FirstOrDefault(c => c.Name == member.Name));
        }

        [Test]
        public void Ignore_Expression_AddsToSender()
        {
            var member = GetDestInfo(c => c.Child);
            _collection.Ignore(c => c.Child);
            Assert.AreEqual(1, _collection.Setters.Count());
            Assert.AreEqual(member.Name, _collection.Setters.First().Name);
            Assert.IsNull(_collection.Unmapped.Destination.FirstOrDefault(c => c.Name == member.Name));
        }

        [Test]
        public void VerifyMap_AllMembersAccounted_NoException()
        {
            _collection.Ignore(c => c.Child);
            _collection.Ignore(c => c.Property1);
            _collection.Ignore(c => c.Property2);
            _collection.Ignore(c => c.Property3);
            Assert.DoesNotThrow(() =>_collection.VerifyMap());
        }

        [Test]
        public void VerifyMap_NotAllMembersAccounted_ThrowsException()
        {
            _collection.Ignore(c => c.Child);
            _collection.Ignore(c => c.Property2);
            _collection.Ignore(c => c.Property3);
            var exception = Assert.Throws<UnmappedMembersException>(() => _collection.VerifyMap());
            Assert.AreEqual(1, exception.UnmappedMembers.Count);
            Assert.AreEqual(GetDestInfo(c => c.Property1), exception.UnmappedMembers.First());
        }

        [Test]
        public void MapCreators_UsesMapperByDefault()
        {
            var mapCreators = new PriorityList<IMemberConsumer>() { new DefaultMemberConsumer() };
            _mapper.Setup(c => c.MemberConsumers).Returns(mapCreators);
            Assert.AreEqual(mapCreators, _collection.MemberConsumers);
        }

        [Test]
        public void MapCreators_Overridden_ReturnsNewValue()
        {
            var mapCreators = new PriorityList<IMemberConsumer>();
            _mapper.Setup(c => c.MemberConsumers).Returns(mapCreators);
            _collection.MemberConsumers.Clear();
            Assert.AreEqual(new IMemberConsumer[0], _collection.MemberConsumers.ToArray());
        }

        [Test]
        public void MemberResolvers_UsesMapperByDefault()
        {
            var memberResolvers = new PriorityList<IMemberResolver>() { new IgnoreCaseNameMatcher() };
            _mapper.Setup(c => c.MemberResolvers).Returns(memberResolvers);
            Assert.AreEqual(memberResolvers, _collection.MemberResolvers);
        }

        [Test]
        public void MemberResolvers_Overridden_ReturnsNewValue()
        {
            _collection.MemberResolvers.Clear();
            Assert.AreEqual(new IMemberResolver[0], _collection.MemberResolvers.ToArray());
        }

        [Test]
        public void PerformAutomapping_OnlyRunsOnce()
        {
            _collection.DoAutomapping();
            _collection.DoAutomapping();
            _collection.DoAutomapping();
            _mapper.Verify(c => c.MemberConsumers, Times.Exactly(1));
        }

        [Test]
        public void OverrideChildContext_ClassType_InvokesClone()
        {
            var testContext = new CloneableTestContext();
            _collection.SetChildContext((from, to, mapper, context) =>
                {
                    Assert.AreNotSame(testContext, context);
                    return context;
                });
            _collection.ContextUpdater(null, null, null, testContext);
        }

        [Test]
        public void OverrideChildContext_ThrowsException_UnCloneableClass()
        {
            var collection = new MappingCollection<object, object, NonCloneableTestContext>(null);
            Assert.Throws<InvalidOperationException>(() => collection.SetChildContext((from, to, mapper, context) => context));
        }

        [Test]
        public void OverrideChildContext_ValueType_ReturnsNewValue()
        {
            var collection = new MappingCollection<object, object, bool>(null);
            collection.SetChildContext((from, to, mapper, context) =>
            {
                Assert.IsTrue(context);
                return false;
            });
            Assert.IsFalse(collection.ContextUpdater(null, null, null, true));
        }

        [Test]
        public void AssertCanMap_NonGeneric_CallsMapper()
        {
            _collection.RequireOneWayMap(typeof (int), typeof (string));
            _mapper.Verify(m => m.RequireOneWayMap(typeof (int), typeof (string), typeof(ClassWithSeveralPropertiesSrcNullable), typeof(ClassWithSeveralPropertiesDest)));
        }

        [Test]
        public void AssertCanMap_Generic_CallsMapper()
        {
            _collection.RequireOneWayMap<int, string>();
            _mapper.Verify(m => m.RequireOneWayMap<int, string, ClassWithSeveralPropertiesSrcNullable, ClassWithSeveralPropertiesDest>());
        }

        [Test]
        public void Set_MemberInfo_To_MemberInfo_MapOnly_NullSource()
        {
            _mapper.Setup(c => c.Map(typeof(string), typeof(int), null, It.IsAny<int>(), It.IsAny<CloneableTestContext>())).Returns(0);

            _collection.SetMember(GetAllDestInfo(c => c.Property1), GetAllSrcInfo(c1 => c1.Child.String));

            _collection.Setters.Last().GenerateCopyValueCall().Invoke(typeof(int?), typeof(ChildClass),
                new ClassWithSeveralPropertiesSrcNullable(),
                new ClassWithSeveralPropertiesDest(), _mapper.Object, null);

            _mapper.Verify(c => c.Map(typeof(string), typeof(int), null, It.IsAny<int>(), It.IsAny<CloneableTestContext>()));
        }

        [Test]
        public void Set_MemberInfo_To_MemberInfo_ConstructAndMap_NullSource()
        {
            _mapper.Setup(c => c.Map(typeof(string), typeof(int), null, It.IsAny<int>(), It.IsAny<CloneableTestContext>())).Returns(0);

            _collection.SetMember(GetAllDestInfo(c => c.Property1), GetAllSrcInfo(c1 => c1.Child.String));

            _collection.Setters.Last().GenerateCopyValueCall().Invoke(typeof(int?), typeof(ChildClass),
                new ClassWithSeveralPropertiesSrcNullable(),
                new ClassWithSeveralPropertiesDest(), _mapper.Object, null);

            _mapper.Verify(c => c.Map(typeof(string), typeof(int), null, It.IsAny<int>(), It.IsAny<CloneableTestContext>()));
        }

        [Test]
        public void Overlay_Root_To_Expression()
        {
            var collection = new MappingCollection<ResourceClassNested, DomainClassSimple, CloneableTestContext>(_mapper.Object);
            
            collection.Overlay(to => to, from => from.Child);

            Assert.AreEqual(1, collection.Setters.Count());

            var input = _builder.Build<ResourceClassNested>();
            var result = new DomainClassSimple();
            collection.Setters.Last().GenerateCopyValueCall().Invoke(null, null, input, result, _mapper.Object, null);

            Assert.AreEqual(input.Child.ExampleProperty, result.ExampleProperty);
        }

        [Test]
        public void Overlay_Root_To_Expression_MultipleSources()
        {
            var collection = new MappingCollection<MultiSrc, MultiDest, CloneableTestContext>(_mapper.Object);
            collection.Overlay(to => to, from => from.Src1);
            collection.Overlay(to => to, from => from.Src2);

            Assert.AreEqual(new[] { "Property1", "Property2" }, collection.Setters.Select(m => m.Name).ToArray());
        }

        [Test]
        public void Overlay_Root_To_Expression_LocksFurtherSettersBeingSpecified()
        {
            _mapper.Setup(m => m.MemberConsumers).Returns(new PriorityList<IMemberConsumer> { new DefaultMemberConsumer() });
            _mapper.Setup(m => m.MemberResolvers).Returns(new PriorityList<IMemberResolver> { new IgnoreCaseNameMatcher() });
            _collection.Overlay(to => to, from => from.Child);
            
            Assert.Throws<MapperException>(() => _collection.Ignore(to => to.Property1));
        }

        [Test]
        public void Overlay_Expression_To_Expression()
        {
            var collection = new MappingCollection<MultiSrc, MultiNestedDest, CloneableTestContext>(_mapper.Object);

            collection.Overlay(to => to.Dest, from => from.Src1);

            Assert.AreEqual(new[] { "Dest.Property1" }, collection.Setters.Select(m => m.Name).ToArray());
        }

        [Test]
        public void Overlay_Expression_To_Expression_Multiple()
        {
            var collection = new MappingCollection<MultiSrc, MultiNestedDest, CloneableTestContext>(_mapper.Object);

            collection.Overlay(to => to.Dest, from => from.Src1);
            collection.Overlay(to => to.Dest, from => from.Src2);

            Assert.AreEqual(new[] { "Dest.Property1", "Dest.Property2" }, collection.Setters.Select(m => m.Name).ToArray());
        }

        private static MemberInfo[] GetAllDestInfo(Expression<Func<ClassWithSeveralPropertiesDest, object>> expression)
        {
            return MemberExpressions.GetExpressionChain(expression);
        }

        private static MemberInfo[] GetAllSrcInfo(Expression<Func<ClassWithSeveralPropertiesSrcNullable, object>> expression)
        {
            return MemberExpressions.GetExpressionChain(expression);
        }

        private static MemberInfo GetDestInfo(Expression<Func<ClassWithSeveralPropertiesDest, object>> expression)
        {
            return MemberExpressions.GetExpressionChain(expression).First();
        }

        private static MemberInfo GetSrcInfo(Expression<Func<ClassWithSeveralPropertiesSrcNullable, object>> expression)
        {
            return MemberExpressions.GetExpressionChain(expression).First();
        }
    }
}