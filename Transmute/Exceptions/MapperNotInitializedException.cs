namespace Transmute.Exceptions
{
    public class MapperNotInitializedException : MapperException
    {
        public MapperNotInitializedException() : base("The mapper cannot be used until it has been initialized")
        {
        }
    }
}