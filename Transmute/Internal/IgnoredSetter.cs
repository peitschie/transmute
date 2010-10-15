using System;
using System.Reflection;

namespace Transmute.Internal
{
    public class IgnoredSetter<TContext> : IMapper<TContext>
    {
        private readonly MemberInfo _member;

        public IgnoredSetter(MemberInfo member)
        {
            if (member == null) throw new ArgumentNullException("member");
            _member = member;
        }

        public string Name
        {
            get { return _member.Name; }
        }

        public MemberSetterAction<TContext> GenerateCopyValueCall()
        {
            return null;
        }
    }
}