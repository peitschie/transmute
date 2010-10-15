namespace Transmute.Exceptions
{
    public class MapperInitializedException : MapperException
    {
        public MapperInitializedException() : base("The mapper has been initialized.  No further changes can be made")
        {
        }
    }
}