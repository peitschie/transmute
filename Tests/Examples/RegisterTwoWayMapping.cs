using System;
using NUnit.Framework;
namespace Transmute.Tests.Examples
{
    [TestFixture]
    public class RegisterTwoWayMapping
    {
        public class SourceEntity
        {
           public int Id { get; set; }
           public int Number { get; set; }
           public int NumberToString { get; set; }
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
            mapper.RegisterTwoWayMapping<SourceEntity, DestEntity>(
               sourceToDest =>
               {
                  sourceToDest.Set(to => to.DifferentlyNamedNumber, from => from.Number);
                  sourceToDest.Set(to => to.DifferentlyNamedNumberToString, from => from.NumberToString);
                 // Unspecified properties will be automapped after this point if not explicitly ignored using mapping.Ignore
               },
               destToSource =>
               {
                  destToSource.Set(to => to.Number, from => from.DifferentlyNamedNumber);
                  destToSource.Set(to => to.NumberToString, from => from.DifferentlyNamedNumberToString);
                 // Unspecified properties will be automapped after this point if not explicitly ignored using mapping.Ignore
               });
            mapper.InitializeMap();
            
            // Perform map source => dest
            mapper.Map(sourceObj, destObj, null);
            Assert.AreEqual(sourceObj.Id, destObj.Id);
            Assert.AreEqual(sourceObj.Number, destObj.DifferentlyNamedNumber);
            Assert.AreEqual(sourceObj.NumberToString.ToString(), destObj.DifferentlyNamedNumberToString);

            // Perform map dest => source
            var newSourceObj = mapper.Map(destObj, new SourceEntity(), null);
            Assert.AreEqual(destObj.Id, newSourceObj.Id);
            Assert.AreEqual(destObj.DifferentlyNamedNumber, newSourceObj.Number);
            Assert.AreEqual(destObj.DifferentlyNamedNumberToString, newSourceObj.NumberToString.ToString());
        }
    }
}

