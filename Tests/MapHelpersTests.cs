using NUnit.Framework;
using Transmute.Tests.Types;

namespace Transmute.Tests
{
    [TestFixture]
    public class MapHelpersTests
    {
        private IResourceMapper<object> _mapper;

        [SetUp]
        public void SetUp()
        {
            _mapper = new ResourceMapper<object>();
        }

        [Test]
        public void LoadConverters_FromCustomClass()
        {
            _mapper.LoadConverters(typeof(ConvertersTest));
            _mapper.InitializeMap();
            Assert.AreEqual((int) CustomEnum1.Value2, (int) _mapper.Map<int, CustomEnum1>(1, 0));
            Assert.AreEqual(1, _mapper.Map<CustomEnum1, int>(CustomEnum1.Value2, 0));
        }

        [Test]
        public void LoadConverters_FromSystemConvert()
        {
            _mapper.LoadStandardConverters();
            _mapper.InitializeMap();
            Assert.AreEqual(10, _mapper.Map<string, int>("10", 0));
            Assert.AreEqual("10", _mapper.Map<int, string>(10, ""));
        }

        private static class ConvertersTest
        {
            public static int Convert(CustomEnum1 value)
            {
                return (int)value;
            }

            public static CustomEnum1 Convert(int value)
            {
                return (CustomEnum1)value;
            }
        }


    }
}