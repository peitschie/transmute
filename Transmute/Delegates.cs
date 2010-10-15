using System;

namespace Transmute
{
        public delegate object MapperAction<TContext>(Type fromType, Type toType, object from, object to, IResourceMapper<TContext> mapper, TContext context);
        public delegate object DestinationMemberSetter<TContext>(object target, object value, IResourceMapper<TContext> mapper, TContext context);

        public delegate void MemberSetterAction<TContext>(Type fromType, Type toType, object from, object to, IResourceMapper<TContext> mapper, TContext context);
        public delegate object MemberSource<TContext>(object from, object to, IResourceMapper<TContext> mapper, TContext context);
}