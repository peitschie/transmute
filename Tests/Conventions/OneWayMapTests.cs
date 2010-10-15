using NUnit.Framework;

namespace Transmute.Tests.Conventions
{
    [TestFixture]
    public class OneWayMapTests
    {
        [Test]
        public void OverrideMapping_DoesNothing()
        {
            new OneWayMap<object, object, object>().OverrideMapping(null);
        }
    }
}