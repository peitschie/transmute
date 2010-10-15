namespace Transmute.Internal
{
    public class DefaultMemberConsumer : IMemberConsumer
    {
        public void CreateMap<TFrom, TTo, TContext>(IMappingCollection<TFrom, TTo, TContext> mappers)
        {
            foreach (var resolver in mappers.MemberResolvers)
            {
                foreach (var pair in mappers.ResolveMap(resolver))
                {
                    mappers.SetMember(pair.Key, pair.Value);
                }
            }
        }
    }
}