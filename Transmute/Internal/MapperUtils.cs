using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Transmute.Internal.FastMemberAccessor;
using Transmute.Internal.Utils;

namespace Transmute.Internal
{
    public static class MapperUtils
    {
        private static readonly Type EnumerableType = typeof(IEnumerable<>).GetGenericTypeDefinition();

        private static bool IsEnumerableType(Type type)
        {
            return type.IsGenericType && EnumerableType.Equals(type.GetGenericTypeDefinition());
        }

        public static Type GetEnumerableElementType(this Type type)
        {
            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else if (IsEnumerableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            else
            {
                var ifaces = type.GetInterfaces();
                var enumerableType = ifaces.FirstOrDefault(IsEnumerableType);
                return enumerableType != null ? enumerableType.GetGenericArguments()[0] : null;
            }
        }

        public static bool IsGenericEnumerable(this Type type)
        {
            return GetEnumerableElementType(type) != null;
        }

        public static IMemberAccessor GetAccessor(this MemberInfo member)
        {
            return MemberAccessors.Make(member);
        }

        public static IMemberAccessor CreateAccessorChain(this IEnumerable<MemberInfo> members)
        {
            if (members.Count() < 1)
                throw new ArgumentException("Collection must have at least one member", "members");

            var accessors = members.Select(prop => prop.GetAccessor()).ToList();
            Func<object, object> getter = o => o;
            Func<object, object, object> setter = (target, value) => value;
            foreach (var propertyAccessor in accessors)
            {
                var accessor = propertyAccessor;
                var localGetter = getter;
                // (((a).b).c).d : Given a, find b, then find c, then find d
                getter = o => accessor.Get(localGetter(o));
            }
            accessors.Reverse();
            foreach (var propertyAccessor in accessors)
            {
                var accessor = propertyAccessor;
                var localSetter = setter;
                // a = (((b = ((c = (d = 4)))) : Given a, get or create b and set this equal to c
                setter = (target, value) =>
                {
                    accessor.Set(target, localSetter(accessor.Get(target), value));
                    return target;
                };
            }
            return new FuncBasedAccessor(getter, (target, value) => setter(target, value), members.Last().ReturnType(), members.First().ReflectedType);
        }

        public static DestinationMemberSetter<TContext> CreateConstructingAccessorChain<TContext>(this IEnumerable<MemberInfo> members)
        {
            if(members.Count() < 1)
                throw new ArgumentException("Collection must have at least one member", "members");
            var accessors = members.Select(prop => prop.GetAccessor()).ToList();
            
            accessors.Reverse();
            DestinationMemberSetter<TContext> setter = null;
            foreach (var propertyAccessor in accessors)
            {
                var accessor = propertyAccessor;
                // a = (((b = ((c = (d = 4)))) : Given a, get or create b and set this equal to c...
                if (setter == null)
                {
                    setter = (target, value, mapper, context) =>
                    {
                        accessor.Set(target, value);
                        return target;
                    };
                }
                else
                {
                    var localSetter = setter;
                    setter = (target, value, mapper, context) =>
                        {
                            var destination = accessor.Get(target);
                            if(destination == null)
                            {
                                destination = mapper.ConstructOrThrow(accessor.MemberType);
                                accessor.Set(target, destination);

                            }
                            return localSetter(destination, value, mapper, context);
                        };
                }
            }
            return setter;
        }

        public static IMemberAccessor CreateAccessorChain<TClass, TReturn>(this Expression<Func<TClass, TReturn>> member)
        {
            return CreateAccessorChain(member.GetExpressionChain());
        }

        public static void CopyToList<TContext>(IEnumerable source, IList destination, Type fromEntryType, Type toEntryType, IResourceMapper<TContext> mapper, TContext context)
        {
            foreach (var fromEntry in source)
            {
                if (fromEntry != null)
                {
                    var toEntry = mapper.Map(fromEntryType, toEntryType, fromEntry, null, context);
                    destination.Add(toEntry);
                }
                else
                {
                    destination.Add(null);
                }
            }
        }

        public static void CopyToArray<TContext>(IEnumerable source, ref Array destination, Type fromEntryType, Type toEntryType, IResourceMapper<TContext> mapper, TContext context)
        {
            var toList = new List<object>();
            CopyToList(source, toList, fromEntryType, toEntryType, mapper, context);

            if (destination == null || destination.Length != toList.Count)
            {
                destination = Array.CreateInstance(toEntryType, toList.Count);
            }

            var index = 0;
            foreach (var toEntry in toList)
            {
                destination.SetValue(toEntry, index++);
            }
        }

        public static bool IsNullable(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static MemberInfo ResolveSource<TFrom, TTo, TContext>(this IMappingCollection<TFrom, TTo, TContext> resolvers, MemberInfo dest)
        {
            return resolvers.ResolveSource(dest, resolvers.Unmapped.Source.ToArray());
        }

        public static IList<KeyValuePair<MemberInfo, MemberInfo>> ResolveMap<TFrom, TTo, TContext>(this IMappingCollection<TFrom, TTo, TContext> resolvers, IMemberResolver resolver)
        {
            return resolvers.Unmapped.Destination.Select(d => new KeyValuePair<MemberInfo, MemberInfo>(d, resolvers.Unmapped.Source.FirstOrDefault(s => resolver.IsSourceFor(d, s, resolvers))))
                .Where(e => e.Value != null)
                .ToList();
        }

        public static MemberInfo ResolveSource<TFrom, TTo, TContext>(this IMappingCollection<TFrom, TTo, TContext> resolvers, MemberInfo dest, IEnumerable<MemberInfo> available)
        {
            foreach (var memberResolver in resolvers.MemberResolvers)
            {
                var resolver = memberResolver;
                var source = available.FirstOrDefault(m => resolver.IsSourceFor(dest, m, resolvers));
                if (source != null)
                    return source;
            }
            return null;
        }
    }
}