using System.Collections.Generic;

namespace Transmute.Tests.Types
{
    public class DomainClassComplex
    {
        public DomainClassComplex RecursiveExampleProperty { get; set; }
        public DomainClassSimple ExampleProperty { get; set; }
        public IList<DomainClassSimple> ExamplePropertyList { get; set; }
        public DomainClassSimple[] ExamplePropertyArray { get; set; }
        public int StringConversionProperty { get; set; }
        public string IntConversionProperty { get; set; }
    }
}