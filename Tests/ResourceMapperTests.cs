using System;
using System.Collections.Generic;
using NGineer;
using NUnit.Framework;
using Transmute.Exceptions;
using Transmute.Tests.Types;
using System.IO;

namespace Transmute.Tests
{
    [TestFixture]
    public class ResourceMapperTests
    {
        private IBuilder _builder;
        private ResourceMapper<object> _mapper;

        [SetUp]
        public void SetUp()
        {
            _builder = new Builder(1)
                .SetMaximumDepth(5)
                .For<ResourceClassComplex>().Set(c => c.StringConversionProperty, (o, b, s) => b.Build<int>(s).ToString())
                ;
            _mapper = new ResourceMapper<object>();
            _mapper.RegisterOneWayMapping(new ClassSimpleOverrides<object>());
            _mapper.ExportMapsTo("maps");
        }

        [Test]
        public void RegisterOneWayMapping_SimpleObjects()
        {
            _mapper.InitializeMap();

            var resourceObj = _builder.Build<ResourceClassSimple>();
            var domainObj = new DomainClassSimple();

            _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj.ExampleProperty, domainObj.ExampleProperty);
            Assert.IsNull(domainObj.RandomProperty);
        }

        [Test]
        public void ExportMapsTo_RegisterOneWayMapping_SimpleObjects()
        {
            if(Directory.Exists("maps"))
            {
                Directory.Delete("maps", true);
            }

            _mapper.ExportMapsTo("maps");
            _mapper.InitializeMap();

            DirectoryAssert.IsNotEmpty("maps");
        }

        [Test]
        public void RegisterOneWayMapping_SimpleObjects_Recursive()
        {
            _mapper.RegisterOneWayMapping<ResourceClassSimpleRecurse, DomainClassSimpleRecurse>();
            _mapper.InitializeMap();

            var resourceObj = _builder.Build<ResourceClassSimpleRecurse>();
            var domainObj = new DomainClassSimpleRecurse();

            _mapper.Map(resourceObj, domainObj, null);
            var resource = resourceObj.Recursive;
            var domain = domainObj.Recursive;
            while (resource != null)
            {
                Assert.IsNotNull(domain);
                Assert.AreEqual(resource.IntProperty, domain.IntProperty);
                resource = resource.Recursive;
                domain = domain.Recursive;
            }
            Assert.IsNull(domain);
        }

        [Test]
        public void RequireOneWayMap_SimpleObjects_List_To_List()
        {
            _mapper.RequireOneWayMap<List<ResourceClassSimple>, List<DomainClassSimple>>("Test");
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1,1)
                .Build<List<ResourceClassSimple>>();
            var domainObj = new List<DomainClassSimple>();

            _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj[0].ExampleProperty, domainObj[0].ExampleProperty);
            Assert.IsNull(domainObj[0].RandomProperty);
        }

        [Test]
        public void RequireOneWayMap_SimpleObjects_List_To_IList()
        {
            _mapper.RequireOneWayMap<List<ResourceClassSimple>, IList<DomainClassSimple>>("Test");
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1, 1)
                .Build<List<ResourceClassSimple>>();
            IList<DomainClassSimple> domainObj = null;

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj[0].ExampleProperty, domainObj[0].ExampleProperty);
            Assert.IsNull(domainObj[0].RandomProperty);
        }

        [Test]
        public void RequireOneWayMap_SimpleObjects_IList_To_IList()
        {
            _mapper.RequireOneWayMap<IList<ResourceClassSimple>, IList<DomainClassSimple>>("Test");
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1, 1)
                .Build<IList<ResourceClassSimple>>();
            IList<DomainClassSimple> domainObj = null;

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj[0].ExampleProperty, domainObj[0].ExampleProperty);
            Assert.IsNull(domainObj[0].RandomProperty);
        }

        [Test]
        public void RequireOneWayMap_SimpleObjects_List_To_Array()
        {
            _mapper.RequireOneWayMap<List<ResourceClassSimple>, DomainClassSimple[]>("Test");
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1, 1)
                .Build<List<ResourceClassSimple>>();
            DomainClassSimple[] domainObj = null;

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj[0].ExampleProperty, domainObj[0].ExampleProperty);
            Assert.IsNull(domainObj[0].RandomProperty);
        }

        [Test]
        public void RequireOneWayMap_SimpleObjects_Array_To_List()
        {
            _mapper.RequireOneWayMap<ResourceClassSimple[], List<DomainClassSimple>>("Test");
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1, 1)
                .Build<ResourceClassSimple[]>();
            List<DomainClassSimple> domainObj = null;

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            int index = 0;
            foreach (var simple in resourceObj)
            {
                AssertAreEqual(simple, domainObj[index++]);
            }
        }

        [Test]
        public void RegisterOneWayMapping_SimpleObjects_CustomArray_To_CustomArray()
        {
            _mapper.RegisterOneWayMapping<ResourceClassSimpleList, DomainClassSimpleList>();
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1, 1)
                .Build<ResourceClassSimpleList>();
            var domainObj = new DomainClassSimpleList();

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj.ExampleProperty, domainObj.ExampleProperty);
            Assert.IsNotNull(domainObj.ListProperty);
            Assert.AreEqual(resourceObj.ListProperty.Length, domainObj.ListProperty.Length);
            int index = 0;
            foreach (var simple in resourceObj.ListProperty)
            {
                AssertAreEqual(simple, domainObj.ListProperty[index++]);
            }
        }

        [Test]
        public void RegisterOneWayMapping_ComplexObjects()
        {
            _mapper.ConvertUsing<string, int>(Convert.ToInt32);
            _mapper.ConvertUsing<int, string>(Convert.ToString);
            _mapper.RegisterOneWayMapping<ResourceClassComplex, DomainClassComplex>();
            _mapper.InitializeMap();

            var resourceObj = _builder.Build<ResourceClassComplex>();
            var domainObj = new DomainClassComplex();

            _mapper.Map(resourceObj, domainObj, null);
            AssertAreEqual(resourceObj, domainObj);
        }

        [Test]
        public void RegisterOneWayMapping_DuplicateThrowsException()
        {
            _mapper.RegisterOneWayMapping<ResourceClassComplex, DomainClassComplex>();
            Assert.Throws<DuplicateMapperException>(() => _mapper.RegisterOneWayMapping<ResourceClassComplex, DomainClassComplex>());
            Assert.Throws<DuplicateMapperException>(() => _mapper.ConvertUsing<ResourceClassComplex, DomainClassComplex>(o => null));
        }

        private static void AssertAreEqual(ResourceClassComplex resource, DomainClassComplex domain)
        {
            AssertAreEqual(resource.ExampleProperty, domain.ExampleProperty);

            if (resource.ExamplePropertyArray != null)
            {
                Assert.AreEqual(resource.ExamplePropertyArray.Count, domain.ExamplePropertyArray.Length);
                int index = 0;
                foreach (var simple in resource.ExamplePropertyArray)
                {
                    AssertAreEqual(simple, domain.ExamplePropertyArray[index++]);
                }
            }
            else
            {
                Assert.IsNull(domain.ExamplePropertyArray);
            }

            if (resource.ExamplePropertyList != null)
            {
                Assert.AreEqual(resource.ExamplePropertyList.Length, domain.ExamplePropertyList.Count);
                var index = 0;
                foreach (var simple in resource.ExamplePropertyList)
                {
                    AssertAreEqual(simple, domain.ExamplePropertyList[index++]);
                }
            }
            else
            {
                Assert.IsNull(domain.ExamplePropertyList);
            }

            if(resource.RecursiveExampleProperty != null)
            {
                Assert.IsNotNull(domain.RecursiveExampleProperty);
                AssertAreEqual(resource.RecursiveExampleProperty, domain.RecursiveExampleProperty);
            }
            else
            {
                Assert.IsNull(domain.RecursiveExampleProperty);
            }
        }

        private static void AssertAreEqual(ResourceClassSimple resource, DomainClassSimple domain)
        {
            if (resource == null)
            {
                Assert.IsNull(domain);
            }
            else
            {
                Assert.IsNotNull(domain);
                Assert.AreEqual(resource.ExampleProperty, domain.ExampleProperty);
            }
        }

        [Test]
        public void RequireOneWayMap_CustomList_To_IList()
        {
            _mapper.RequireOneWayMap<ResourceCustomList, IList<DomainClassSimple>>("Test");
            _mapper.InitializeMap();

            var resourceObj = _builder
                .CreateNew()
                .SetDefaultCollectionSize(1, 1)
                .WithGenerator((b, s) => {
                        var list = new ResourceCustomList();
                        list.AddRange(b.Build<List<ResourceClassSimple>>(s));
                        return list;
                    })
                .Build<ResourceCustomList>();
            IList<DomainClassSimple> domainObj = null;

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj[0].ExampleProperty, domainObj[0].ExampleProperty);
            Assert.IsNull(domainObj[0].RandomProperty);
        }

        [Test]
        public void AutomaticallyMap_BetweenValueOnlyTypes_Int32()
        {
            _mapper.RequireOneWayMap<int, int>("Test");
            _mapper.InitializeMap();
            var input = 10;
            var output = _mapper.Map<int, int>(input, null);
            Assert.AreEqual(input, output);
            Assert.AreNotSame(input, output);
            input = 11;
            Assert.AreNotEqual(input, output);
            Assert.AreEqual(10, output);
        }

        [Test]
        public void AutomaticallyMap_BetweenValueOnlyTypes_DateTime()
        {
            _mapper.RequireOneWayMap<DateTime, DateTime>("Test");
            _mapper.InitializeMap();
            const int timestamp = 1000;
            var input = new DateTime(timestamp);
            var output = _mapper.Map<DateTime, DateTime>(input, null);
            Assert.AreEqual(input, output);
            Assert.AreNotSame(input, output);
            input = new DateTime(timestamp+1);
            Assert.AreNotEqual(input, output);
            Assert.AreEqual(new DateTime(timestamp), output);
        }

        [Test]
        public void ConstructOrThrow_ExceptionIfNoDefaultConstructorFound()
        {
            _mapper.InitializeMap();
            var exception = Assert.Throws<MapperException>(() => _mapper.ConstructOrThrow(typeof (NoDefaultConstructor)));
            Assert.IsTrue(exception.Message.Contains(typeof(NoDefaultConstructor).ToString()));
        }

        [Test]
        public void ConstructOrThrow_NoExceptionIfConstructorRegistered_GenericCall()
        {
            _mapper.RegisterConstructor(() => new NoDefaultConstructor(10));
            _mapper.InitializeMap();
            object result = null;
            Assert.DoesNotThrow(() => result = _mapper.ConstructOrThrow(typeof(NoDefaultConstructor)));
            Assert.IsNotNull(result);
        }

        [Test]
        public void ConstructOrThrow_NoExceptionIfConstructorRegistered_NonGenericCall()
        {
            _mapper.RegisterConstructor(typeof(NoDefaultConstructor), () => new NoDefaultConstructor(10));
            _mapper.InitializeMap();
            object result = null;
            Assert.DoesNotThrow(() => result = _mapper.ConstructOrThrow(typeof(NoDefaultConstructor)));
            Assert.IsNotNull(result);
        }

        [Test]
        public void AssertCanMap_TypesNotPresent_ThrowsException_DescriptionProvided()
        {
            _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), "FirstLongRandomString");
            var exception = Assert.Throws<MapperException>(() => _mapper.InitializeMap());
            Assert.IsTrue(exception.Message.Contains(typeof(NoDefaultConstructor).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(int).ToString()));
            Assert.IsTrue(exception.Message.Contains("FirstLongRandomString"));
        }

        [Test]
        public void AssertCanMap_TypesNotPresent_ThrowsException_MultipleDescriptionsProvided()
        {
            _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), "FirstLongRandomString");
            _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), "YetAnotherString");
            var exception = Assert.Throws<MapperException>(() => _mapper.InitializeMap());
            Assert.IsTrue(exception.Message.Contains(typeof(NoDefaultConstructor).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(int).ToString()));
            Assert.IsTrue(exception.Message.Contains("FirstLongRandomString"));
            Assert.IsTrue(exception.Message.Contains("YetAnotherString"));
        }

        [Test]
        public void AssertCanMap_TypesNotPresent_ThrowsException_ParentTypesProvided()
        {
            _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), typeof(string), typeof(DateTime));
            var exception = Assert.Throws<MapperException>(() => _mapper.InitializeMap());
            Assert.IsTrue(exception.Message.Contains(typeof(NoDefaultConstructor).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(int).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(string).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(DateTime).ToString()));
        }

        [Test]
        public void AssertCanMap_TypesNotPresent_ThrowsException_MultipleParentTypesProvided()
        {
            _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), typeof(string), typeof(DateTime));
            _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), typeof(long), typeof(char));
            var exception = Assert.Throws<MapperException>(() => _mapper.InitializeMap());
            Assert.IsTrue(exception.Message.Contains(typeof(NoDefaultConstructor).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(int).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(string).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(DateTime).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(long).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(char).ToString()));
        }

        [Test]
        public void AssertCanMap_TypesPresent_NoException()
        {
            _mapper.RegisterOneWayMapping<NoDefaultConstructor, int>();
            Assert.DoesNotThrow(() => _mapper.RequireOneWayMap(typeof(NoDefaultConstructor), typeof(int), "Test"));
        }

        [Test]
        public void Map_NullFrom_PerformsConversionIf_SourceIsValueType()
        {
            bool called = false;
            _mapper.ConvertUsing<CustomEnum1, string>(o => { called = true; return o.ToString(); });
            _mapper.InitializeMap();
            Assert.IsNotNull(_mapper.Map(typeof(CustomEnum1), typeof(string), default(CustomEnum1), null, null));
            Assert.IsTrue(called, "Convert method should not have been called");
        }

        [Test]
        public void Map_NullFrom_DoesReturnsNullImmediately_IfNotValueType()
        {
            bool called = false;
            _mapper.ConvertUsing<CustomList, string>(o => { called = true; return o.ToString(); });
            _mapper.InitializeMap();
            Assert.IsNull(_mapper.Map(typeof(CustomList), typeof(string), null, null, null));
            Assert.IsFalse(called, "Convert method should not have been called");
        }

        [Test]
        public void Map_UndefinedMap_ThrowsException()
        {
            _mapper.InitializeMap();
            var exception = Assert.Throws<MapperException>(() => _mapper.Map(typeof(CustomEnum1), typeof(string), CustomEnum1.Value1, null, null));
            Assert.IsTrue(exception.Message.Contains(typeof(CustomEnum1).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(string).ToString()));
        }

        [Test]
        public void Map_InitializeWithUndefinedMap_ThrowsException_ListsAllUndefinedMaps()
        {
            _mapper.RequireOneWayMap<CustomList, string>("Description");
            _mapper.RequireOneWayMap<CustomEnum1, MapperException, CustomEnum1, ICustomFormatter>();
            var exception = Assert.Throws<MapperException>(() => _mapper.InitializeMap());
            Assert.IsTrue(exception.Message.Contains(typeof(CustomEnum1).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(CustomList).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(string).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(MapperException).ToString()));
            Assert.IsTrue(exception.Message.Contains(typeof(ICustomFormatter).ToString()));
            Assert.IsTrue(exception.Message.Contains("Description"));
        }

        [Test]
        public void ConvertUsing_MethodIsCalledWhenMapping_NonGeneric()
        {
            var called = false;
            Func<object, object> callback = s => { called = true; return int.Parse(s.ToString()); };
            _mapper.ConvertUsing(typeof(string), typeof(int), callback);
            _mapper.InitializeMap();
            _mapper.Map("1", 0, null);
            Assert.IsTrue(called);
        }

        [Test]
        public void ConvertUsing_MethodIsCalledWhenMapping_Generic()
        {
            var called = false;
            Func<string, int> callback = s => { called = true; return int.Parse(s); };
            _mapper.ConvertUsing(callback);
            _mapper.InitializeMap();
            _mapper.Map("1", 0, null);
            Assert.IsTrue(called);
        }

        [Test]
        public void ConvertUsing_MethodIsCalledWhenMapping_NullValueProperlyCaughtForValueTypes()
        {
            var called = false;
            Func<int, string> callback = s => { called = true; return s.ToString(); };
            _mapper.ConvertUsing(callback);
            _mapper.InitializeMap();
            var result = _mapper.Map(typeof(int), typeof(string), null, null, null);
            Assert.IsFalse(called);
            Assert.AreEqual(null, result);
        }

        [Test]
        public void ConvertUsing_DuplicateConverterThrowsException_NonGeneric()
        {
            Func<object, object> callback = s => int.Parse(s.ToString());
            _mapper.ConvertUsing(typeof(string), typeof(int), callback);
            Assert.Throws<DuplicateMapperException>(() => _mapper.ConvertUsing(typeof(string), typeof(int), callback));
            Assert.Throws<DuplicateMapperException>(() => _mapper.RegisterOneWayMapping(typeof(string), typeof(int)));
        }

        [Test]
        public void ConvertUsing_DuplicateConverterThrowsException_Generic()
        {
            Func<string, int> callback = s => int.Parse(s);
            _mapper.ConvertUsing(callback);
            Assert.Throws<DuplicateMapperException>(() => _mapper.ConvertUsing(callback));
            Assert.Throws<DuplicateMapperException>(() => _mapper.RegisterOneWayMapping(typeof(string), typeof(int)));
        }

        [Test]
        public void MapperIsInitialized_CallingInitializeMultipleTimesThrowsNoExceptions()
        {
            _mapper.InitializeMap();
            Assert.DoesNotThrow(() => _mapper.InitializeMap());
        }

        [Test]
        public void UnableToMapExceptions_SuggestRequireMapIfAvailable()
        {
            _mapper.InitializeMap();
            var exception = Assert.Throws<MapperException>(() => _mapper.Map(new List<int>(), new List<int>(), null));
            Assert.IsTrue(exception.Message.Contains("RequireMap"));
        }

        [Test]
        public void RegisterOneWayMap_Overrides_BuiltinMapCreators()
        {
            bool registerCalled = false;
            _mapper.RegisterOneWayMapping<List<int>, List<int>>(mapping =>
                {
                    mapping.Set(to => to.Capacity, (from, to, mapper, context) => { registerCalled = true; return 10; });
                    mapping.IgnoreUnmapped();
                });
            _mapper.InitializeMap();
            _mapper.Map(new List<int>(), new List<int>(), null);
            Assert.IsTrue(registerCalled);
        }

        [Test]
        public void RegisterOneWayMap_Overlay_ConsumesMembers()
        {
            _mapper.ConvertUsing<string, int>(Convert.ToInt32);
            _mapper.ConvertUsing<int, string>(Convert.ToString);
            _mapper.RegisterOneWayMapping<ResourceClassNested, DomainClassSimple>(mapping =>
                {
                    mapping.Ignore(to => to.RandomProperty);
                    mapping.Overlay(to => to, from => from.Child);
                });
            _mapper.InitializeMap();

            var resourceObj = _builder.Build<ResourceClassNested>();
            var domainObj = new DomainClassSimple();

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj.Child.ExampleProperty, domainObj.ExampleProperty);
        }

        [Test]
        public void RegisterOneWayMap_OverlayMultiple_ConsumesMembers()
        {
            _mapper.ConvertUsing<string, int>(Convert.ToInt32);
            _mapper.ConvertUsing<int, string>(Convert.ToString);
            _mapper.RegisterOneWayMapping<MultiSrc, MultiDest>(mapping =>
            {
                mapping.Overlay(to => to, from => from.Src1);
                mapping.Overlay(to => to, from => from.Src2);
            });
            _mapper.InitializeMap();

            var resourceObj = _builder.Build<MultiSrc>();
            var domainObj = new MultiDest();

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj.Src1.Property1, domainObj.Property1);
            Assert.AreEqual(resourceObj.Src2.Property2, domainObj.Property2);
        }

        [Test]
        public void RegisterOneWayMap_Overlay_Nested_ConsumesMembers()
        {
            _mapper.ConvertUsing<string, int>(Convert.ToInt32);
            _mapper.ConvertUsing<int, string>(Convert.ToString);
            _mapper.RegisterOneWayMapping<MultiSrc, MultiNestedDest>(mapping =>
            {
                mapping.Overlay(to => to.Dest, from => from.Src1);
                mapping.Overlay(to => to.Dest, from => from.Src2);
            });
            _mapper.InitializeMap();

            var resourceObj = _builder.Build<MultiSrc>();
            var domainObj = new MultiNestedDest();

            domainObj = _mapper.Map(resourceObj, domainObj, null);
            Assert.AreEqual(resourceObj.Src1.Property1, domainObj.Dest.Property1);
        }

        #region Uninitialized mapper cannot map or construct
        [Test]
        public void MapperIsNotInitialized_Map_ThrowsException()
        {
            Assert.IsFalse(_mapper.IsInitialized);
            Assert.Throws<MapperNotInitializedException>(() => _mapper.Map<ResourceClassSimple, DomainClassSimple>(null, null));
        }

        [Test]
        public void MapperIsNoInitialized_Construct_ThrowsException()
        {
            Assert.IsFalse(_mapper.IsInitialized);
            Assert.Throws<MapperNotInitializedException>(() => _mapper.ConstructOrThrow(typeof(ResourceClassSimple)));
        }
        #endregion

        #region Initialized mapper is readonly tests
        [Test]
        public void MapperIsInitialized_Maps_CannotRegisterNew_ThrowsException()
        {
            _mapper.InitializeMap();
            Assert.Throws<MapperInitializedException>(() => _mapper.RegisterOneWayMapping<ResourceClassSimple, DomainClassSimple>());
            Assert.Throws<MapperInitializedException>(() => _mapper.RegisterTwoWayMapping<ResourceClassSimple, DomainClassSimple>());
        }

        [Test]
        public void MapperIsInitialized_Constructors_CannotRegisterNew_ThrowsException()
        {
            _mapper.InitializeMap();
            Assert.Throws<MapperInitializedException>(() => _mapper.RegisterConstructor<ResourceClassSimple>(() => null));
            Assert.Throws<MapperInitializedException>(() => _mapper.RegisterConstructor(typeof(ResourceClassSimple), () => null));
        }

        [Test]
        public void MapperIsInitialized_Converters_CannotRegisterNew_ThrowsException()
        {
            _mapper.InitializeMap();
            Assert.Throws<MapperInitializedException>(() => _mapper.ConvertUsing<ResourceClassSimple, int>(o => 0));
            Assert.Throws<MapperInitializedException>(() => _mapper.ConvertUsing(typeof(ResourceClassSimple), typeof(int), o => 0));
        }
        #endregion
    }

    public class ClassSimpleOverrides<TContext> : OneWayMap<ResourceClassSimple, DomainClassSimple, TContext>
    {
        public override void OverrideMapping(IMappingCollection<ResourceClassSimple, DomainClassSimple, TContext> mapping)
        {
            mapping.Ignore(to => to.RandomProperty);
        }
    }
}