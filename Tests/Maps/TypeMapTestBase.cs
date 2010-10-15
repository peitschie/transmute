using System;
using System.Collections.Generic;
using Moq;
using Transmute.Internal.Utils;
using NUnit.Framework;
using Transmute.Internal;
using Transmute.Maps;
using Transmute.MemberResolver;
using Transmute.Tests.Types;

namespace Transmute.Tests.Maps
{
    public abstract class TypeMapTestBase<TTypeMap, TContext>
        where TTypeMap : ITypeMap<TContext>
    {
        protected readonly IList<Type> DuplicateTypes = new[] {typeof (int), typeof (string), typeof (ChildClass), typeof(DateTime)};
        protected TTypeMap Map;
        protected Mock<IResourceMapper<TContext>> ResourceMapper;

        [SetUp]
        public void FixtureSetUp()
        {
            ResourceMapper = new Mock<IResourceMapper<TContext>>();
            ResourceMapper.Setup(m => m.CanMap(It.Is<Type>(type => DuplicateTypes.Contains(type)), 
                It.Is<Type>(type => DuplicateTypes.Contains(type)))).Returns(true);
            ResourceMapper.Setup(m => m.Map(It.Is<Type>(type => DuplicateTypes.Contains(type)),
                It.Is<Type>(type => DuplicateTypes.Contains(type)), It.IsAny<object>(), It.IsAny<object>(), It.IsAny<TContext>()))
                .Returns<Type, Type, object, object, TContext>((tfrom, tto, from, to, context) => from);
            ResourceMapper.Setup(m => m.MemberResolvers).Returns(new PriorityList<IMemberResolver>() { new IgnoreCaseNameMatcher() });
            ResourceMapper.Setup(m => m.MemberConsumers).Returns(new PriorityList<IMemberConsumer>() { new DefaultMemberConsumer() });
            ResourceMapper.Setup(m => m.ConstructOrThrow(It.IsAny<Type>())).Returns<Type>(t => { throw new Exception("Test is attempting to construct {0}".With(t)); });
            var constructor = typeof(TTypeMap).GetConstructor(new[] {typeof (IResourceMapper<TContext>)});
            if (constructor != null)
            {
                Map = (TTypeMap)constructor.Invoke(new object[] { ResourceMapper.Object });
            }
            else
            {
                constructor = typeof (TTypeMap).GetConstructor(new Type[0]);
                if (constructor == null)
                    throw new Exception("No known constructor could be located");
                Map = (TTypeMap) constructor.Invoke(new object[0]);
            }
        }

        [Test]
        public abstract void CanMapFrom_AcceptedTypes();

        [Test]
        public abstract void CanMapFrom_RejectedTypes();

        [Test]
        public abstract void Map_NullDestination();

        protected TTo InvokeMapper<TFrom, TTo>(TFrom from, TTo to, TContext context = default(TContext))
        {
            return InvokeMapper<TFrom, TTo>((object) from, (object) to, context);
        }

        protected TTo InvokeMapper<TFrom, TTo>(object from, object to, TContext context = default(TContext))
        {
            var initializableMap = Map as IInitializableMap;
            if(initializableMap != null)
                initializableMap.Initialize();
            return (TTo)Map.GetMapper(typeof(TFrom), typeof(TTo)).Invoke(typeof(TFrom), typeof(TTo), from, to, ResourceMapper.Object, context);
        }
    }
}