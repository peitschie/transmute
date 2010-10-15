using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Transmute
{
    public interface IBidirectionMap<TFrom, TTo>
    {
        void Set(MemberInfo to, MemberInfo from);
        void Set(MemberInfo[] member, MemberInfo[] getter);
        void Set<TMemberType>(Expression<Func<TTo, TMemberType>> toExpression, Expression<Func<TFrom, TMemberType>> fromExpression);
        void Set(Expression<Func<TTo, object>> toExpression, Expression<Func<TFrom, object>> fromExpression, bool doConversion);
    }
}