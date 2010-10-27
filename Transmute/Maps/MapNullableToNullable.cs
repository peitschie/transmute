using System;
using Transmute.Internal;

namespace Transmute.Maps
{
    public class MapNullableToNullable<TContext> : ITypeMap<TContext>
    {
        private readonly IResourceMapper<TContext> _mapper;

        public MapNullableToNullable(IResourceMapper<TContext> mapper)
        {
            _mapper = mapper;
        }

        public bool CanMap(Type from, Type to)
        {
            return
            (
                MapperUtils.IsNullable(to) && MapperUtils.IsNullable(from)
                && _mapper.CanMap(from.GetGenericArguments()[0], to.GetGenericArguments()[0])
            );
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            var toNullableType = toType.GetGenericArguments()[0];
            var fromNullableType = fromType.GetGenericArguments()[0];
            _mapper.RequireOneWayMap(fromNullableType, toNullableType, "MapNullableToNullable");
            var nullableTypeMapper = _mapper.GetMapper(fromNullableType, toNullableType);

            return 
            (
                (from, to, context) =>
                    from == null ? null : nullableTypeMapper.MapObject(from, to, context)
            );
        }
    }
}