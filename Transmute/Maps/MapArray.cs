using System;
using System.Collections;
using System.Collections.Generic;
using Transmute.Internal;

namespace Transmute.Maps
{
    public class MapArray<TContext> : ITypeMap<TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;

        public MapArray(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }

        public bool CanMap(Type from, Type to)
        {
            return from.IsGenericEnumerable() && to.IsArray
                && _mapper.CanMap(from.GetEnumerableElementType(), to.GetEnumerableElementType());
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            var fromEntryType = fromType.GetEnumerableElementType();
            var toEntryType = toType.GetEnumerableElementType();
            _mapper.RequireOneWayMap(fromEntryType, toEntryType, "MapArray");
            return new ArrayMapperEntry(fromEntryType, toEntryType).Map;
        }

        private class ArrayMapperEntry
        {
            private readonly Type _fromEntryType;
            private readonly Type _toEntryType;

            public ArrayMapperEntry(Type fromEntryType, Type toEntryType)
            {
                _fromEntryType = fromEntryType;
                _toEntryType = toEntryType;
            }

            public object Map(Type fromType, Type toType, object from, object to, IResourceMapper<TContext> mapper, TContext context)
            {
                var toArray = (Array) to;
                MapperUtils.CopyToArray((IEnumerable)from, ref toArray, _fromEntryType, _toEntryType, mapper, context);
                return toArray;
            }
        }
    }
}