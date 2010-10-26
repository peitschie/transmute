using System;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Transmute.Exceptions;
using Transmute.Internal;
using Transmute.Internal.FastMemberAccessor;
using Transmute.Internal.Utils;
using Transmute.MemberResolver;
using Transmute.Tests.Types;

namespace Transmute.Tests.Internal.Utils
{
    [TestFixture]
    public class MapperUtilsTests
    {
        [Test]
        public void GetEnumerableElementType_NonGenericEnumerable()
        {
            Assert.IsNull(typeof(ArrayList).GetEnumerableElementType());
        }

        [Test]
        public void GetEnumerableElementType_GenericEnumerable_Array()
        {
            Assert.AreEqual(typeof(int), typeof(int[]).GetEnumerableElementType());
        }

        [Test]
        public void GetEnumerableElementType_GenericEnumerable_List()
        {
            Assert.AreEqual(typeof(int), typeof(List<int>).GetEnumerableElementType());
        }

        [Test]
        public void GetEnumerableElementType_GenericEnumerable_IList()
        {
            Assert.AreEqual(typeof(bool), typeof(IList<bool>).GetEnumerableElementType());
        }

        [Test]
        public void GetEnumerableElementType_GenericEnumerable_IEnumerable()
        {
            Assert.AreEqual(typeof(string), typeof(IEnumerable<string>).GetEnumerableElementType());
        }

        [Test]
        public void GetEnumerableElementType_GenericEnumerable_CustomInterface()
        {
            Assert.AreEqual(typeof(char), typeof(ICustomEnumerable).GetEnumerableElementType());
        }

        [Test]
        public void GetEnumerableElementType_GenericEnumerable_CustomClass()
        {
            Assert.AreEqual(typeof(char), typeof(ICustomEnumerable).GetEnumerableElementType());
        }

        [Test]
        public void IsGenericEnumerable_GenericEnumerable_CustomClass()
        {
            Assert.IsTrue(typeof(ICustomEnumerable).IsGenericEnumerable());
        }

        [Test]
        public void IsGenericEnumerable_GenericEnumerable_List()
        {
            Assert.IsTrue(typeof(List<int>).IsGenericEnumerable());
        }

        [Test]
        public void IsGenericEnumerable_GenericEnumerable_Array()
        {
            Assert.IsTrue(typeof(int[]).IsGenericEnumerable());
        }

        [Test]
        public void IsGenericEnumerable_GenericEnumerable_NonEnumerable()
        {
            Assert.IsFalse(typeof(int).IsGenericEnumerable());
        }

        [Test]
        public void GetAccessor_Field()
        {
            var accessor = MemberExpressions.GetMemberInfo<NormalPropertiesAndFields>(c => c.ValidField).GetAccessor();
            Assert.IsInstanceOf<FieldAccessor>(accessor);
        }

        [Test]
        public void GetAccessor_Property()
        {
            var accessor = MemberExpressions.GetMemberInfo<NormalPropertiesAndFields>(c => c.ValidProperty).GetAccessor();
            Assert.IsInstanceOf<PropertyAccessor<NormalPropertiesAndFields, int>>(accessor);
        }

        [Test]
        public void GetAccessor_IndexedProperty_ThrowsException()
        {
            var propertyInfo = typeof(ResourceCustomList).GetProperty("Item");
            Assert.IsNotNull(propertyInfo);
            Assert.Throws<MapperException>(() => propertyInfo.GetAccessor());
        }

        [Test]
        public void GetAccessor_Method_ThrowsException()
        {
            var exception = Assert.Throws<InvalidProgramException>(() => GetType().GetMember("GetAccessor_Property").First().GetAccessor());
            Assert.AreEqual("Unreachable code executed", exception.Message);
        }

        [Test]
        public void CopyToList_CallsMapperWithContext()
        {
            var context = new object();
            var mapper = new Mock<IMap<object>>();
            mapper.Setup(
                c => c.MapObject(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<IResourceMapper<object>>(), It.IsAny<object>()))
                .Returns<object, object, IResourceMapper<object>, object>((from, to, mappr, ctx) => from.ToString());
            var destinationList = new List<string>();
            MapperUtils.CopyToList(new []{1,2,3}, destinationList, mapper.Object, null, context);

            mapper.Verify(c => c.MapObject(1, It.IsAny<object>(), null, context));
            mapper.Verify(c => c.MapObject(2, It.IsAny<object>(), null, context));
            mapper.Verify(c => c.MapObject(3, It.IsAny<object>(), null, context));
            Assert.AreEqual(new []{"1","2","3"}, destinationList.ToArray());
        }

        [Test]
        public void CopyToList_CallsMapperWithContext_NullEntries()
        {
            var context = new object();
            var mapper = new Mock<IMap<object>>();
            mapper.Setup(
                c => c.MapObject(It.IsAny<object>(), It.IsAny<object>(), It.IsAny<IResourceMapper<object>>(), It.IsAny<object>()))
                .Returns<object, object, IResourceMapper<object>, object>((from, to, mappr, ctx) => from == null ? null : from.ToString());
            var destinationList = new List<string>();
            MapperUtils.CopyToList(new int?[] { 1, null, 3 }, destinationList, mapper.Object, null, context);

            mapper.Verify(c => c.MapObject(1, It.IsAny<object>(), null, context));
            mapper.Verify(c => c.MapObject(null, It.IsAny<object>(), null, context), Times.Never());
            mapper.Verify(c => c.MapObject(3, It.IsAny<object>(), null, context));
            Assert.AreEqual(new []{"1",null,"3"}, destinationList.ToArray());
        }

        [Test]
        public void CreateAccessorChain_Expression()
        {
            Assert.IsNotNull(MapperUtils.CreateAccessorChain<ClassWithSeveralPropertiesDest, ChildClass>(c => c.Child));
        }

        [Test]
        public void CreateAccessorChain_NoElements_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => MapperUtils.CreateAccessorChain(new MemberInfo[0]));
        }

        [Test]
        public void CreateAccessorChain_SingleElement()
        {
            var chain = MapperUtils.CreateAccessorChain(MemberExpressions.GetExpressionChain<ClassWithSeveralPropertiesDest>(c => c.Child));
            Assert.IsNotNull(chain);
            var destination = new ClassWithSeveralPropertiesDest();
            var child = new ChildClass();
            chain.Set(destination, child);
            Assert.AreSame(child, destination.Child);
        }

        [Test]
        public void CreateAccessorChain_MultipleElements()
        {
            var chain = MapperUtils.CreateAccessorChain(MemberExpressions.GetExpressionChain<ClassWithSeveralPropertiesDest>(c => c.Child.String));
            Assert.IsNotNull(chain);
            var destination = new ClassWithSeveralPropertiesDest(){Child =  new ChildClass()};
            const string child = "teststring";
            chain.Set(destination, child);
            Assert.AreSame(child, destination.Child.String);
        }

        [Test]
        public void CreateAccessorChain_MultipleElements_WithConstruction()
        {
            var resourceMapper = new ResourceMapper<object>();
            resourceMapper.InitializeMap();
            var chain = MapperUtils.CreateConstructingAccessorChain<object>(MemberExpressions.GetExpressionChain<ClassWithSeveralPropertiesDest>(c => c.Child.String));
            Assert.IsNotNull(chain);
            var destination = new ClassWithSeveralPropertiesDest();
            const string child = "teststring";
            chain(destination, child, resourceMapper, null);
            Assert.AreSame(child, destination.Child.String);
        }

        [Test]
        public void CreateAccessorChain_MultipleElements_WithConstruction_DeeperClass()
        {
            var resourceMapper = new ResourceMapper<object>();
            resourceMapper.InitializeMap();
            var chain = MapperUtils.CreateConstructingAccessorChain<object>(MemberExpressions.GetExpressionChain<DeeperClass>(c => c.DeepClass.Child.String));
            Assert.IsNotNull(chain);
            var destination = new DeeperClass();
            const string child = "teststring";
            chain(destination, child, resourceMapper, null);
            Assert.AreSame(child, destination.DeepClass.Child.String);
        }

        [Test]
        public void ResolveSource_MappingCollection_ReturnsCorrectMember()
        {
            var members = new[]
                {
                    MemberExpressions.GetMemberInfo<ClassWithSeveralPropertiesDest>(c => c.Property1),
                    MemberExpressions.GetMemberInfo<ClassWithSeveralPropertiesDest>(c => c.Property2),
                    MemberExpressions.GetMemberInfo<ClassWithSeveralPropertiesDest>(c => c.Property3)
                };
            var mappingCollection = new Mock<IMappingCollection<object, object, object>>();
            mappingCollection.Setup(m => m.MemberResolvers).Returns(new PriorityList<IMemberResolver> { new IgnoreCaseNameMatcher() });
            mappingCollection.Setup(m => m.Unmapped.Source).Returns(members);
            Assert.AreSame(members[1], mappingCollection.Object.ResolveSource(members[1]));
        }

        [Test]
        public void ResolveSource_MappingCollection_ReturnsNullIfNoMemberFound()
        {
            var searchMember = MemberExpressions.GetMemberInfo<ClassWithSeveralPropertiesDest>(c => c.Property2);
            var members = new[]
                {
                    MemberExpressions.GetMemberInfo<ClassWithSeveralPropertiesDest>(c => c.Property1),
                    MemberExpressions.GetMemberInfo<ClassWithSeveralPropertiesDest>(c => c.Property3)
                };
            var mappingCollection = new Mock<IMappingCollection<object, object, object>>();
            mappingCollection.Setup(m => m.MemberResolvers).Returns(new PriorityList<IMemberResolver> { new IgnoreCaseNameMatcher() });
            mappingCollection.Setup(m => m.Unmapped.Source).Returns(members);
            Assert.IsNull(mappingCollection.Object.ResolveSource(searchMember));
        }

        private interface ICustomEnumerable : IEnumerable<char> {}
        private class CustomEnumerable : ICustomEnumerable {
            public IEnumerator<char> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}