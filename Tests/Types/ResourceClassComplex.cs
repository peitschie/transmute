using System.Collections.Generic;

namespace Transmute.Tests.Types
{
    public class ResourceClassComplex
    {
        public ResourceClassComplex RecursiveExampleProperty { get; set; }
        public ResourceClassSimple ExampleProperty { get; set; }
        public ResourceClassSimple[] ExamplePropertyList { get; set; }
        public IList<ResourceClassSimple> ExamplePropertyArray { get; set; }
        public string StringConversionProperty { get; set; }
        public int IntConversionProperty { get; set; }
    }
}