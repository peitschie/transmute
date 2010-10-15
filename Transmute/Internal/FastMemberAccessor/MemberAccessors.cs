using System;
using System.Reflection;
using Transmute.Exceptions;

namespace Transmute.Internal.FastMemberAccessor
{
    public static class MemberAccessors
    {
        public static IMemberAccessor Make(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return new FieldAccessor((FieldInfo)member);
                case MemberTypes.Property:
                    return MakeProperty((PropertyInfo)member);
                default:
                    throw new InvalidProgramException("Unreachable code executed");
            }
        }

        private static IMemberAccessor MakeProperty(PropertyInfo member)
        {
            if (member.GetIndexParameters().Length > 1)
                throw new MapperException("Multi-indexed properties not supported by Transmute");
            if(member.GetIndexParameters().Length == 1)
            {
                throw new MapperException("Indexed properties not supported by Transmute");
                //return (IMemberAccessor)typeof(IndexedPropertyAccessor<,,>)
                //    .MakeGenericType(member.DeclaringType, member.PropertyType, member.GetIndexParameters()[0].ParameterType)
                //    .GetConstructor(new[] { typeof(PropertyInfo) }).Invoke(new object[] { member });
            }
            return (IMemberAccessor)typeof (PropertyAccessor<,>).MakeGenericType(member.DeclaringType, member.PropertyType)
                    .GetConstructor(new[] {typeof (PropertyInfo)}).Invoke(new object[] {member});
        }
    }
}