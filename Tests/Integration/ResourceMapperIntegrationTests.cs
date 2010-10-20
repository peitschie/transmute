using System;
using NUnit.Framework;
using Transmute.Tests.Types;
using NGineer;
namespace Transmute.Tests.Integration
{
    [TestFixture]
    public class ResourceMapperIntegrationTests
    {
        private ResourceMapper<object> _mapper;
        private IBuilder _builder;

        [SetUp]
        public void SetUp()
        {
            _builder = new Builder();
            _mapper = new ResourceMapper<object>();
            _mapper.ExportMapsTo("complex");
        }

        [Test]
        public void ComplexMapping_ExercisesAllSetters()
        {
            _mapper.LoadStandardConverters();
            var domainClassObj = new DomainClassComplex();
            _mapper.RegisterOneWayMapping<ResourceClassSimple, DomainClassComplex>(mapping => {
                mapping.Set(to => to.IntConversionProperty, from => ((InheritsFromResourceClassSimple)from).StringProperty);
                mapping.Set(to => to.StringConversionProperty, (from, to, mapper, context) => ((InheritsFromResourceClassSimple)from).IntProperty);
                mapping.Set(to => to.RecursiveExampleProperty, () => domainClassObj, false);
                mapping.Set(to => to.ExamplePropertyList, from => new []{from});
                mapping.Set(to => to.ExamplePropertyArray, from => new []{from});
                mapping.Set(to => to.ExampleProperty, from => new InheritsFromDomainClassSimple(), false);
                mapping.Set(to => ((InheritsFromDomainClassSimple)to.ExampleProperty).StringProperty, from => ((InheritsFromResourceClassSimple)from).StringProperty);
            });
            _mapper.RegisterOneWayMapping<ResourceClassSimple, DomainClassSimple>( mapping => mapping.Ignore(to => to.RandomProperty) );
            _mapper.InitializeMap();

            var sourceObj = _builder.Build<InheritsFromResourceClassSimple>();

            var resultObj = _mapper.Map<ResourceClassSimple, DomainClassComplex>(sourceObj, null);

            Assert.AreEqual(sourceObj.StringProperty, resultObj.IntConversionProperty);
            Assert.AreEqual(sourceObj.IntProperty, resultObj.StringConversionProperty);
            Assert.AreSame(domainClassObj, resultObj.RecursiveExampleProperty);
            Assert.AreEqual(1, resultObj.ExamplePropertyList.Count);
            Assert.AreEqual(sourceObj.ExampleProperty, resultObj.ExamplePropertyList[0].ExampleProperty);
            Assert.AreEqual(1, resultObj.ExamplePropertyArray.Length);
            Assert.AreEqual(sourceObj.ExampleProperty, resultObj.ExamplePropertyArray[0].ExampleProperty);
            Assert.IsInstanceOf<InheritsFromDomainClassSimple>(resultObj.ExampleProperty);
            Assert.AreEqual(sourceObj.StringProperty, ((InheritsFromDomainClassSimple)resultObj.ExampleProperty).StringProperty);

        }
    }

    public class InheritsFromDomainClassSimple : DomainClassSimple
    {
        public string StringProperty { get; set; }
    }

    public class InheritsFromResourceClassSimple : ResourceClassSimple
    {
       public ResourceClassSimple AnotherLevel { get; set; }
       public string StringProperty { get; set; }
       public int IntProperty { get; set; }
    }
}

