using System;
using System.Collections.Generic;

namespace Transmute.Internal
{
    public class RequiredMapEntry
    {
        public RequiredMapEntry()
        {
            Messages = new List<string>();
        }

        public Type Type1 { get; set; }
        public Type Type2 { get; set; }
        public IList<string> Messages { get; private set; }

        public override string ToString()
        {
            return string.Format("RequiredMap({0} => {1})", Type1, Type2);
        }
    }
}