using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Transmute.Internal.Utils
{
    public static class OptimizationUtils
    {
        public static ConstructorInfo DefaultConstructor(this Type type)
        {
            return type.GetConstructor(new Type[0]);
        }

        public static Func<object> CompileConstructor(this ConstructorInfo constructorInfo)
        {
            return Expression.Lambda<Func<object>>(Expression.New(constructorInfo)).Compile();
        }
    }
}