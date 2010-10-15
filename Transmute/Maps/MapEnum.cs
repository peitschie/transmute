using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Transmute.Internal;

namespace Transmute.Maps
{
    public class MapEnum<TContext> : ITypeMap<TContext>
    {
        private readonly TypeDictionary<bool> _canMap = new TypeDictionary<bool>();

        public bool CanMap(Type from, Type to)
        {
            bool result;
            if (!_canMap.TryGetValue(from, to, out result))
            {
                result = from.IsEnum && to.IsEnum &&
                         GetNameList(from).OrderBy(c => c).SequenceEqual(GetNameList(to).OrderBy(c2 => c2));
                _canMap.Add(from, to, result);
            }
            return result;
        }

        public MapperAction<TContext> GetMapper(Type fromType, Type toType)
        {
            return (tFrom, tTo, from, to, mapper, context) => Enum.Parse(tTo, from.ToString(), true);
        }

        private static IEnumerable<string> GetNameList(Type enumType)
        {
            var fiArray = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            return fiArray.Select(fi => fi.Name).ToList();
        }
    }
}