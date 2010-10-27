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
            return new ListMapperEntry(_mapper.GetMapper(fromEntryType, toEntryType), constructor).Map;
        }

        private class ListMapperEntry
        {
            private readonly IMap<TContext> _typeMapper;
            private readonly Func<object> _constructor;

            public ListMapperEntry(IMap<TContext> typeMapper, Func<object> constructor)
            {
                _typeMapper = typeMapper;
                _constructor = constructor;
            }

            public object Map(object from, object to, TContext context)
            {
                if (to == null)
                {
                    to = _constructor();
                }

                var toList = (IList)to;
                toList.Clear();

                MapperUtils.CopyToList((IEnumerable)from, toList, _typeMapper, context);

                return to;   
            }
        }
    }
}