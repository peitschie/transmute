using System;
using NUnit.Framework;
using System.Collections.Generic;
namespace Transmute.Tests.Examples
{
    [TestFixture]
    public class ParentToChildContext
    {
        public class SimpleDictionaryContext : ICloneable
        {
            private Dictionary<string, object> _data = new Dictionary<string, object>();

            public TObject Get<TObject>(string variable)
            {
                return (TObject)_data[variable];
            }

            public SimpleDictionaryContext Set(string variable, object data)
            {
                _data[variable] = data;
                return this;
            }

            object ICloneable.Clone()
            {
                // this is only a shallow clone, so it is recommended not to store complex data
                // structures in this dictionary as child contexts may unintentionally modify it
                var newValue = new SimpleDictionaryContext();
                foreach(KeyValuePair<string, object> pair in _data)
                {
                    newValue._data[pair.Key] = pair.Value;
                }
                return newValue;
            }
        }

        public class ChildEntity
        {
            public int ParentId { get; set; }
            public int Variable { get; set; }
        }

        public class SourceEntity
        {
            public int Id { get; set; }
            public ChildEntity Child { get; set; }
        }

        public class DestEntity
        {
            public ChildEntity Child { get; set; }
        }

        [Test]
        public void Example()
        {
            // Create the object mapper
            var mapper = new ResourceMapper<SimpleDictionaryContext>();
            mapper.RegisterOneWayMapping<SourceEntity, DestEntity>(mapping =>
            {
                mapping.SetChildContext((from, to, map, context) => context.Set("ParentVariable", from.Id));
            });
            mapper.RegisterOneWayMapping<ChildEntity, ChildEntity>(mapping =>
            {
                mapping.Set(to => to.ParentId, (from, to, map, context) => context.Get<int>("ParentVariable"));
            });
            mapper.InitializeMap();

            // Create source object
            var sourceObj = new SourceEntity {
                Id = 10,
                Child = new ChildEntity { Variable = 103 }
            };

            var destObj = new DestEntity();

            var mapContext = new SimpleDictionaryContext();

            // Perform map
            mapper.Map(sourceObj, destObj, mapContext);

            Assert.Throws<KeyNotFoundException>(() => mapContext.Get<int>("ParentVariable"));
            Assert.AreEqual(sourceObj.Child.Variable, destObj.Child.Variable);
            Assert.AreEqual(sourceObj.Id, destObj.Child.ParentId);
        }
    }
}

