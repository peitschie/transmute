using System;

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
            return to.IsGenericType && to.GetGenericTypeDefinition() == typeof (Nullable<>) && _mapper.CanMap(from, to.GetGenericArguments()[0]);
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            var toNullableType = toType.GetGenericArguments()[0];
            _mapper.RequireOneWayMap(fromType, toNullableType, "MapNonNullableToNullable");

            return (tfrom, tto, from, to, mapper, context) => mapper.Map(tfrom, toNullableType, from, to, context);
        }
    }
}