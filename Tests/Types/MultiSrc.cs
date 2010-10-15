namespace Transmute.Tests.Types
{
    public class MultiSrc
    {
        public MultiSrc1 Src1 { get; set; }
        public MultiSrc2 Src2 { get; set; }
    }

    public class MultiSrc1
    {
        public int Property1 { get; set; }
    }

    public class MultiSrc2
    {
        public int Property2 { get; set; }
    }
}