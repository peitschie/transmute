using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Transmute
{
    public static class MapHelpers
    {
        /// <summary>
        /// Loads standard type converters from the System.Convert class.  This detects all methods that start with
        /// "To", and have a single input and a single output
        /// </summary>
        /// <param name="resourceMapper">
        /// A <see cref="IResourceMapper<TContext>"/>
        /// </param>
        public static void LoadStandardConverters<TContext>(this IResourceMapper<TContext> resourceMapper)
        {
            resourceMapper.LoadConverters(typeof(Convert), method => method.Name.StartsWith("To") 
                && !((method.ReturnType.IsValueType || method.ReturnType == typeof(string)) && method.ReturnType == method.GetParameters()[0].ParameterType));
        }

        public static void LoadConverters<TContext>(this IResourceMapper<TContext> resourceMapper, Type type)
        {
            resourceMapper.LoadConverters(type, method => true);
        }

        public static void LoadConverters<TContext>(this IResourceMapper<TContext> resourceMapper, Type type, params Func<MethodInfo, bool>[] verify)
        {
            foreach (var methodInfo in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.ExactBinding).Where(m => m.IsPublic 
                && m.ReturnType != null 
                && m.IsStatic
                && m.GetParameters().Length == 1
                && verify.Aggregate(true, (seed, check) => seed && check(m))))
            {
                var fromType = methodInfo.GetParameters()[0].ParameterType;
                var toType = methodInfo.ReturnType;
                var info = methodInfo;
                resourceMapper.ConvertUsing(fromType, toType, from => info.Invoke(null, new []{from}));
            }
        }

        public static IMappingCollection<TFrom, TTo, TContext> IgnoreUnmapped<TFrom, TTo, TContext>(this IMappingCollection<TFrom, TTo, TContext> mappingCollection)
        {
            foreach (var memberInfo in mappingCollection.Unmapped.Destination)
            {
                mappingCollection.IgnoreMember(memberInfo);
            }
            return mappingCollection;
        }

        public static IMappingCollection<TFrom, TTo, TContext> AutomapOnly<TFrom, TTo, TContext>(this IMappingCollection<TFrom, TTo, TContext> mappingCollection)
        {
            mappingCollection.DoAutomapping();
            foreach (var memberInfo in mappingCollection.Unmapped.Destination)
            {
                mappingCollection.IgnoreMember(memberInfo);
            }
            return mappingCollection;
        }
    }
}