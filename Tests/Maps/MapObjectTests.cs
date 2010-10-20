using Moq;
using NUnit.Framework;
using Transmute.Exceptions;
using Transmute.Internal;
using Transmute.Maps;
using Transmute.Tests.Types;

namespace Transmute.Tests.Maps
{
    [TestFixture]
    public class MapObjectTests : TypeMapTestBase<MapObject<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, CloneableTestContext>, CloneableTestContext>
    {
        [Test]
        [Ignore("Need to find more suitable place for this")]
        public void SetOrder_IsPreserved()
        {
            Map.AcceptOverrides(new ClassWithSeveralPropertiesOverride<CloneableTestContext>().OverrideMapping);
            var destObject = InvokeMapper(new ClassWithSeveralPropertiesSrc(), new ClassWithSeveralPropertiesDest());
            Assert.IsNotNull(destObject);
            Assert.AreEqual(0, destObject.Property2);
            Assert.AreEqual(1, destObject.Property1);
            Assert.AreEqual(2, destObject.Property3);
        }

        [Test]
        [Ignore("Need to find more suitable place for this")]
        public void DefaultCreators_Null_UsesDefaultMapper()
        {
            Map.AcceptOverrides(new OverrideDefaultCreators<CloneableTestContext>(null).OverrideMapping);
            var destObject = InvokeMapper(new ClassWithSeveralPropertiesSrc(), new ClassWithSeveralPropertiesDest());
            Assert.IsNotNull(destObject);
            Assert.AreEqual(0, destObject.Property1);
            Assert.AreEqual(0, destObject.Property2);
            Assert.AreEqual(0, destObject.Property3);
        }

        [Test]
        public void DefaultCreators_EmptyList_OverridesResourceMapper()
        {
            Map.AcceptOverrides(new OverrideDefaultCreators<CloneableTestContext>(new IMemberConsumer[0]).OverrideMapping);
            Assert.Throws<UnmappedMembersException>(() => InvokeMapper(new ClassWithSeveralPropertiesSrc(), new ClassWithSeveralPropertiesDest()));
        }

        [Test]
        public void DefaultCreators_CustomCreator()
        {
            var creator = new Mock<IMemberConsumer>();
            Map.AcceptOverrides(new OverrideDefaultCreators<CloneableTestContext>(new[] { creator.Object }).OverrideMapping);
            Assert.Throws<UnmappedMembersException>(() => InvokeMapper(new ClassWithSeveralPropertiesSrc(), new ClassWithSeveralPropertiesDest()));
            creator.Verify(c => c.CreateMap(It.IsAny<IMappingCollection<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, CloneableTestContext>>()));
        }


        [Test]
        public override void CanMapFrom_AcceptedTypes()
        {
            Assert.IsTrue(Map.CanMap(typeof(ClassWithSeveralPropertiesSrc), typeof(ClassWithSeveralPropertiesDest)));
        }

        [Test]
        public override void CanMapFrom_RejectedTypes()
        {
            Assert.IsFalse(Map.CanMap(typeof(ClassWithSeveralPropertiesDest), typeof(ClassWithSeveralPropertiesSrc)));
            Assert.IsFalse(Map.CanMap(typeof(ClassWithSeveralPropertiesSrc), typeof(ClassWithSeveralPropertiesSrc)));
            Assert.IsFalse(Map.CanMap(typeof(ClassWithSeveralPropertiesDest), typeof(ClassWithSeveralPropertiesDest)));
            Assert.IsFalse(Map.CanMap(typeof(ClassWithSeveralPropertiesSrc), typeof(object)));
            Assert.IsFalse(Map.CanMap(typeof(object), typeof(ClassWithSeveralPropertiesSrc)));
        }

        [Test]
        [Ignore("Need to find more suitable place for this")]
        public override void Map_NullDestination()
        {
            ResourceMapper.Setup(c => c.ConstructOrThrow(typeof(ClassWithSeveralPropertiesDest))).Returns(new ClassWithSeveralPropertiesDest());
            InvokeMapper(new ClassWithSeveralPropertiesSrc(), null);
            ResourceMapper.Verify(c => c.ConstructOrThrow(typeof(ClassWithSeveralPropertiesDest)));
        }

        [Test]
        [Ignore("Need to find more suitable place for this")]
        public void Map_IgnoredAllProperties_NoExceptionThrown()
        {
            Map.AcceptOverrides(new IgnoreAllDefaultCreators<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, CloneableTestContext>().OverrideMapping);
            Map.Initialize();
            var mapper = Map.GetMapper(typeof (ClassWithSeveralPropertiesSrc), typeof (ClassWithSeveralPropertiesDest));
            Assert.IsNotNull(mapper);
            Assert.DoesNotThrow(() => mapper.Invoke(null, null, null, new object(), null, null));
        }

        [Test]
        [Ignore("Need to find more suitable place for this")]
        public void Map_ChangesContext_NewContextReceivedInChildMapper()
        {
            var originalContext = new CloneableTestContext();
            Map.AcceptOverrides(new AssertsClonedContext(originalContext).OverrideMapping);
            Map.Initialize();
            var mapper = Map.GetMapper(typeof(ClassWithSeveralPropertiesSrc), typeof(ClassWithSeveralPropertiesDest));
            Assert.IsNotNull(mapper);
            InvokeMapper(new ClassWithSeveralPropertiesSrc(), new ClassWithSeveralPropertiesDest(), originalContext);
        }

        [Test]
        public void CoverageBump_AccessorsForProperties()
        {
            Assert.AreEqual(typeof (ClassWithSeveralPropertiesSrc), Map.FromType);
            Assert.AreEqual(typeof(ClassWithSeveralPropertiesDest), Map.ToType);
        }

        private class AssertsClonedContext : OneWayMap<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, CloneableTestContext>
        {
            private readonly CloneableTestContext _originalContext;

            public AssertsClonedContext(CloneableTestContext originalContext)
            {
                _originalContext = originalContext;
            }

            public override void OverrideMapping(IMappingCollection<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, CloneableTestContext> mapping)
            {
                base.OverrideMapping(mapping);
                mapping.SetChildContext((from, to, mapper, context) => context);
                mapping.Set(to => to.Child, (from, to, mapper, context) =>
                    {
                        Assert.AreNotSame(_originalContext, context);
                        return null;
                    });
            }
        }

        private ClassWithSeveralPropertiesDest InvokeMapper(ClassWithSeveralPropertiesSrc from, ClassWithSeveralPropertiesDest to, CloneableTestContext context = default(CloneableTestContext))
        {
            return InvokeMapper<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest>(from, to, context);
        }
    }
}