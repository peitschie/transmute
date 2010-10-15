namespace Transmute.Tests.Types
{
    public class DeepClassReadonly
    {
        public ChildClass Child { get; private set; }

        public DeepClassReadonly()
        {
            Child = new ChildClass();
        }
    }
}