using NUnit.Framework;
using Transmute.Exceptions;

namespace Transmute.Tests.Exceptions
{
    [TestFixture]
    public class MemberMappingExceptionTests
    {
        [Test]
        public void Construct_TouchFromMember_CoverageBump()
        {
            Assert.IsNull(new MemberMappingException(null, null, null, null, null).FromMember);
        }
    }
}