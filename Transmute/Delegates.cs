using System;

namespace Transmute
{
        public delegate object MapperAction<TContext>(object from, object to, TContext context);
        public delegate object DestinationMemberSetter<TContext>(object target, object value, TContext context);

        public delegate void MemberSetterAction<TContext>(object from, object to, TContext context);
}