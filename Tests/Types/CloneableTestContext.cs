using System;

namespace Transmute.Tests.Types
{
    public class CloneableTestContext : ICloneable
    {
        public int Id = 10;

        public object Clone()
        {
            return new CloneableTestContext(){Id = 15};
        }
    }
}