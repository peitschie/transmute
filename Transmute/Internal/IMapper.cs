namespace Transmute.Internal
{
    public interface IMapper<TContext>
    {
        string Name { get; }
        MemberSetterAction<TContext> GenerateCopyValueCall();
    }
}