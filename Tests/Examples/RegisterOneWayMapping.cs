using System;
using NUnit.Framework;
namespace Transmute.Tests.Examples
{
    [TestFixture]
    public class RegisterOneWayMapping
    {
        public class SourceEntity
        {
            public int Id { get; set; }
            public int Number { get; set; }
            public int NumberToString;
        }

        public class DestEntity
        {
            public int Id { get; set; }
            public int DifferentlyNamedNumber { get; set; }
            public string DifferentlyNamedNumberToString { get; set; }
        }

        [Test]
        public void Example()
        {
            // Create our source object
            var sourceObj = new SourceEntity {
                  Number = 10,
                  NumberToString = -1000
               };

            var destObj = new DestEntity();

            // Create the object mapper
            var mapper = new ResourceMapper<object>();
            mapper.LoadStandardConverters(); // Load standard converters from System.Convert (e.g., int to string)
            mapper.RegisterOneWayMapping<SourceEntity, DestEntity>(mapping =>
            {
                mapping.Set(to => to.DifferentlyNamedNumber, from => from.Number);
                // Directly assigns the source value to the destination value
                mapping.Set(to => to.DifferentlyNamedNumberToString, from => from.NumberToString);
                // There are many other variations of Set on the IMappingCollection<T> interface.  Check these out on the API
                // Unspecified properties will be automapped after this point if not explicitly ignored using mapping.Ignore
            });
            mapper.InitializeMap();

            // Perform map
            destObj = mapper.Map(sourceObj, destObj, null);

            Assert.AreEqual(sourceObj.Id, destObj.Id);
            Assert.AreEqual(sourceObj.Number, destObj.DifferentlyNamedNumber);
            Assert.AreEqual(sourceObj.NumberToString.ToString(), destObj.DifferentlyNamedNumberToString);
        }
    }
}

