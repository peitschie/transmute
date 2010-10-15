using NUnit.Framework;
using Transmute.Tests.Types;

namespace Transmute.Tests.Integration
{
    [TestFixture]
    public class EnumToIntBehaviourTests
    {
        private IResourceMapper<object> _resourceMapper;

        [SetUp]
        public void SetUp()
        {
            _resourceMapper = new ResourceMapper<object>();
        }

        [Test]
        public void Convert_EnumToIntBehavesCorrectly()
        {
            _resourceMapper.ConvertUsing((CustomEnum1 v) => (int)v);
            _resourceMapper.InitializeMap();
            Assert.AreEqual(1, _resourceMapper.Map<CustomEnum1, int>(CustomEnum1.Value2, null));
        }

        [Test]
        public void Convert_ObjectToIntBehavesCorrectly_NullInput()
        {
            _resourceMapper.ConvertUsing((object v) => 1);
            _resourceMapper.InitializeMap();
            Assert.AreEqual(0, _resourceMapper.Map<object, int>(null, null));
        }

        [Test]
        public void MappingFromEnumToIntClass_ConvertsCorrectly()
        {
            _resourceMapper.ConvertUsing((CustomEnum1 v) => (int)v);
            _resourceMapper.RegisterOneWayMapping<SourceWithEnum, DestWithInt>(
                mapping => mapping.Set(to => to.Enum, (from, to, mapper, context) => from.Enum, true)
            );
            _resourceMapper.InitializeMap();
            Assert.AreEqual(1, _resourceMapper.Map<SourceWithEnum, DestWithInt>(new SourceWithEnum{Enum = CustomEnum1.Value2}, null).Enum);
        }
    }

    public class SourceWithEnum
    {
        public CustomEnum1 Enum { get; set; }
    }

    public class DestWithInt
    {
        public int Enum { get; set; }
    }
}