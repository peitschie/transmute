namespace Transmute.Tests.Types
{
    public class DeepClass
    {
        public ChildClass Child { get; set; }

        public DeepClass()
        {
            Child = new ChildClass();
        }
    }
}