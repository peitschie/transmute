using System;
using System.Collections;
using System.Collections.Generic;
using Transmute.Internal;
using Transmute.Internal.Utils;

namespace Transmute.Maps
{
    public class MapList<TContext> : ITypeMap<TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;

        public MapList(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }
        
        public bool CanMap(Type from, Type to)
        {
            return from.IsGenericEnumerable() 
                && to.IsGenericEnumerable() 
                && to.IsAssignableFrom(typeof(List<>).MakeGenericType(to.GetEnumerableElementType()))
                && _mapper.CanMap(from.GetEnumerableElementType(), to.GetEnumerableElementType());
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            var fromEntryType = fromType.GetEnumerableElementType();
            var toEntryType = toType.GetEnumerableElementType();
            var constructor = typeof(List<>).MakeGenericType(toEntryType).DefaultConstructor().CompileConstructor();
            _mapper.RequireOneWayMap(fromEntryType, toEntryType, "MapList");
            return new ListMapperEntry(fromEntryType, toEntryType, constructor).Map;
        }

        private class ListMapperEntry
        {
            private readonly Type _fromEntryType;
            private readonly Type _toEntryType;
            private readonly Func<object> _constructor;

            public ListMapperEntry(Type fromEntryType, Type toEntryType, Func<object> constructor)
            {
                _fromEntryType = fromEntryType;
                _toEntryType = toEntryType;
                _constructor = constructor;
            }

            public object Map(Type fromType, Type toType, object from, object to, IResourceMapper<TContext> mapper, TContext context)
            {
                if (to == null)
                {
                    to = _constructor();
                }

                var toList = (IList)to;
                toList.Clear();
                MapperUtils.CopyToList((IEnumerable)from, toList, _fromEntryType, _toEntryType, mapper, context);

                return to;   
            }
        }
    }
}