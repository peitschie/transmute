using System;
using System.Reflection;

namespace Transmute.Internal.Utils
{
    public static class MemberInfoExtensions
    {
        public static Type ReturnType(this MemberInfo self)
        {
            if (self.MemberType == MemberTypes.Field)
                return ((FieldInfo) self).FieldType;
            if (self.MemberType == MemberTypes.Property)
                return ((PropertyInfo)self).PropertyType;
            throw new ArgumentException(string.Format("Unsupported member type {0}", self.MemberType));
        }
    }
}
