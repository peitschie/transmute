using System;
using Transmute.Internal;

namespace Transmute.Maps
{
    public class MapNonNullableToNullable<TContext> : ITypeMap<TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;

        public MapNonNullableToNullable(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }

        public bool CanMap(Type from, Type to)
        {
            return MapperUtils.IsNullable(to) && _mapper.CanMap(from, to.GetGenericArguments()[0]);
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            var toNullableType = toType.GetGenericArguments()[0];
            _mapper.RequireOneWayMap(fromType, toNullableType, "MapNonNullableToNullable");
            var nonNullToNullMapper = _mapper.GetMapper(fromType, toNullableType);
            return (from, to, context) => nonNullToNullMapper.MapObject(from, to, context);
        }
    }
}