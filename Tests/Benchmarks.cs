using System;
using System.Linq;
using NGineer;
using NGineer.Utils;
using NUnit.Framework;
using Transmute.Tests.Types;

namespace Transmute.Tests
{
    [TestFixture]
    [Explicit]
    public class Benchmarks
    {
        private const int Total = 10000;

        private IBuilder _builder;
        private ResourceClassComplex _resourceObj;

        [SetUp]
        public void SetUp()
        {
            _builder = new Builder(1)
                .SetCollectionSize<ResourceClassSimple>(20, 20)
                .SetMaximumDepth(5)
                .For<ResourceClassComplex>().Set(c => c.StringConversionProperty, "10");

            _resourceObj = _builder.Build<ResourceClassComplex>();
        }

        [Test]
        [Ignore]
        public void BenchmarkAutomapper()
        {
            /*
            Mapper.CreateMap<string, int>().ConstructUsing(Convert.ToInt32);
            Mapper.CreateMap<int, string>().ConstructUsing(Convert.ToString);
            Mapper.CreateMap<ResourceClassComplex, DomainClassComplex>();
            Mapper.CreateMap<ResourceClassSimple, DomainClassSimple>()
                .ForMember(c => c.RandomProperty, o => o.Ignore());
            Mapper.AssertConfigurationIsValid();
            start = DateTime.Now;
            for (int i = 0; i < total; i++)
            {
                var domainObj = new DomainClassComplex();
                Mapper.Map(resourceObj, domainObj, typeof (ResourceClassComplex), typeof (DomainClassComplex));
            }
            end = DateTime.Now;
            totalMs = (end - start).TotalMilliseconds;
            Console.Out.WriteLine("AutoMapper Conversion -  Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, total, 100*total/totalMs));
            */
        }

        [Test]
        public void BenchmarkTransmute()
        {
            // Resource mapper
            var mapper = new ResourceMapper<object>();
            mapper.LoadStandardConverters();
            mapper.RegisterOneWayMapping<ResourceClassComplex, DomainClassComplex>();
            mapper.RegisterOneWayMapping<ResourceClassSimple, DomainClassSimple>(mapping => mapping.Ignore(to => to.RandomProperty));
            mapper.InitializeMap();

            var start = DateTime.Now;
            for (int i = 0; i < Total; i++)
            {
                var domainObj = new DomainClassComplex();
                mapper.Map(_resourceObj, domainObj, null);
            }
            var end = DateTime.Now;
            var totalMs = (end - start).TotalMilliseconds;
            Assert.Pass("Mapper Conversion -  Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, Total, 100 * Total / totalMs));
        }

        [Test]
        public void BenchmarkNative()
        {
            var start = DateTime.Now;
            for (int i = 0; i < Total; i++)
            {
                var domainObj = new DomainClassComplex();
                Map(_resourceObj, domainObj);
            }
            var end = DateTime.Now;
            var totalMs = (end - start).TotalMilliseconds;
            Assert.Pass("Explicit Conversion -    Total elapsed time: {0}ms  Total conversions: {1}  Conversions: {2}/s".With(totalMs, Total, 100 * Total / totalMs));
        }

        private static void Map(ResourceClassComplex from, DomainClassComplex to)
        {
            to.ExampleProperty = from.ExampleProperty == null ? null : new DomainClassSimple() { ExampleProperty = from.ExampleProperty.ExampleProperty};
            to.IntConversionProperty = from.IntConversionProperty.ToString();
            //to.IntConversionProperty = from.IntConversionProperty;
            to.StringConversionProperty = int.Parse(from.StringConversionProperty);
            //to.StringConversionProperty = from.StringConversionProperty;
            to.ExamplePropertyList = from.ExamplePropertyList == null ? null : from.ExamplePropertyList.Select(p => p == null ? null : new DomainClassSimple() { ExampleProperty = p.ExampleProperty}).ToList();
            to.ExamplePropertyArray = from.ExamplePropertyArray == null ? null : from.ExamplePropertyArray.Select(p => p == null ? null : new DomainClassSimple() { ExampleProperty = p.ExampleProperty}).ToArray();
            if (from.RecursiveExampleProperty != null)
            {
                to.RecursiveExampleProperty = new DomainClassComplex();
                Map(from.RecursiveExampleProperty, to.RecursiveExampleProperty);
            }
        }
    }
}