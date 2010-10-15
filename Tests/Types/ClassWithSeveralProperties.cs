namespace Transmute.Tests.Types
{
    public class ClassWithSeveralPropertiesSrc
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
        public int Property3 { get; set; }
        public ChildClass Child { get; set; }
    }

    public class ClassWithSeveralPropertiesSrcNullable
    {
        public int? Property1 { get; set; }
        public int Property2 { get; set; }
        public int Property3 { get; set; }
        public ChildClass Child { get; set; }
    }

    public class ClassWithSeveralPropertiesDest
    {
        public int Property1 { get; set; }
        public int Property2 { get; set; }
        public int Property3 { get; set; }
        public ChildClass Child { get; set; }
    }
}