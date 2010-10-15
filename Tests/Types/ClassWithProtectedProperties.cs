namespace Transmute.Tests.Types
{
    public class ClassWithProtectedProperties
    {
        public string MySetterIsProtected { get; protected set; }
        public string MyGetterIsProtected { protected get; set; }
        protected string ImAltogetherProtected { get; set; }
        public string ImAltogetherAccessible { get; set; }
    }
}