using System;

namespace Transmute
{
    public class OneWayMap<TFrom, TTo, TContext> : IOneWayMap<TFrom, TTo, TContext>
    {
        public virtual void OverrideMapping(IMappingCollection<TFrom, TTo, TContext> mapping) {}

        public Type FromType
        {
            get { return typeof (TFrom); }
        }

        public Type ToType
        {
            get { return typeof (TTo); }
        }
    }
}