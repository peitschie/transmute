namespace Transmute.Tests.Types
{
    public class ReadonlyPropertiesAndFields
    {
        public const int ConstField = 10;
        public readonly int ReadonlyField = 10;

        public int ReadonlyProperty { get; private set; }
    }
}