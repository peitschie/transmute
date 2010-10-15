namespace Transmute.Internal
{
    public interface IMemberConsumer
    {
        void CreateMap<TFrom, TTo, TContext>(IMappingCollection<TFrom, TTo, TContext> mapper);
    }
}