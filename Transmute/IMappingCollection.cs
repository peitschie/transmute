using System;
using System.Linq.Expressions;
using System.Reflection;
using Transmute.Internal;

namespace Transmute
{
    public interface IMappingCollection<TFrom, TTo, TContext>
    {
        IAvailablePropertiesClass Unmapped { get; }

        IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo to, MemberInfo from);
        IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo to, MemberSource<TContext> getter);
        IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo[] member, MemberInfo[] getter, bool? remap=null);
        IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo[] member, MemberSource<TContext> source);
        
        IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Func<TGetterType> getter, bool remap=false);
        IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType>(Expression<Func<TTo, TPropertyType>> toExpression, Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TPropertyType> getter);
        IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TGetterType> getter, bool? remap=false);
        IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Expression<Func<TFrom, TGetterType>> fromExpression, bool? remap=null);

        IMappingCollection<TFrom, TTo, TContext> IgnoreMember(MemberInfo member);
        IMappingCollection<TFrom, TTo, TContext> Ignore<TMemberType>(Expression<Func<TTo, TMemberType>> expression);

        IMappingCollection<TFrom, TTo, TContext> SetChildContext(Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TContext> action);

        IPriorityList<IMemberConsumer> MemberConsumers { get; }
        IPriorityList<IMemberResolver> MemberResolvers { get; }

        IMappingCollection<TFrom, TTo, TContext> RequireOneWayMap(Type from, Type to);
        IMappingCollection<TFrom, TTo, TContext> RequireOneWayMap<TFromType, TToType>();

        IMappingCollection<TFrom, TTo, TContext> RequireTwoWayMap(Type type1, Type type2);
        IMappingCollection<TFrom, TTo, TContext> RequireTwoWayMap<TType1, TType2>();
        
        IMappingCollection<TFrom, TTo, TContext> DoAutomapping();
        void Overlay<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Expression<Func<TFrom, TGetterType>> fromExpression);
    }
}