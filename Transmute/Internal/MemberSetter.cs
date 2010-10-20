using System;
using System.Linq;
using System.Reflection;
using Transmute.Internal.FastMemberAccessor;

namespace Transmute.Internal
{
    public class MemberSetter<TContext> : IMapper<TContext>
    {
        private readonly MemberSource<TContext> _get;
        private readonly string _name;
        private readonly DestinationMemberSetter<TContext> _toAccessor;

        public MemberSetter(MemberInfo to, MemberSource<TContext> get) : this(new []{to}, get)
        {}

        public MemberSetter(MemberInfo[] to, MemberSource<TContext> get, MemberInfo[] fromPrefix=null, MemberInfo[] toPrefix=null)
        {
            if (to == null)
                throw new ArgumentNullException("to");
            if(to.Length == 0)
                throw new ArgumentException("At least one target property must be specified");
            if (get == null)
                throw new ArgumentNullException("get");
            if(toPrefix != null && to.Length > 0)
                to = toPrefix.Union(to).ToArray();
            _toAccessor = to.CreateConstructingAccessorChain<TContext>();
            _name = string.Join(".", to.Select(p => p.Name).ToArray());
            if (fromPrefix != null && fromPrefix.Length > 0)
            {
                var getterChain = fromPrefix.CreateAccessorChain();
                _get = (fromObj, toObj, mappr, context) => get(getterChain.Get(fromObj), toObj, mappr, context);
            }
            else
            {
                _get = get;
            }
        }

        public string Name { get { return _name; } }

        public MemberSetterAction<TContext> GenerateCopyValueCall()
        {
            return (tfrom, tto, from, to, mapper, context) => _toAccessor(to, _get(from, to, mapper, context), mapper, context);
        }
    }
}