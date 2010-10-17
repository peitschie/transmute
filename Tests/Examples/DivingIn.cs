using System;
using NUnit.Framework;
namespace Transmute.Tests.Examples
{
    [TestFixture]
    public class DivingIn
    {
        public class SourceEntity
        {
           public int Number { get; set; }
           public int NumberToString { get; set; }
        }

        public class DestEntity
        {
           public int Number { get; set; }
           public string NumberToString { get; set; }
        }

        [Test]
        public void Example()
        {
            // Create our source object
            var sourceObj = new SourceEntity { Number = 10 };
            sourceObj.NumberToString = -1000;
            
            var destObj = new DestEntity();
            
            // Create the object mapper
            var mapper = new ResourceMapper<object>();
            mapper.LoadStandardConverters(); // Load standard converters from System.Convert (e.g., int to string)
            mapper.RegisterOneWayMapping<SourceEntity, DestEntity>();
            mapper.InitializeMap();
            
            // Perform map
            mapper.Map(sourceObj, destObj, null);

            Assert.AreEqual(sourceObj.Number, destObj.Number);
            Assert.AreEqual(sourceObj.NumberToString.ToString(), destObj.NumberToString);
        }
    }
}

