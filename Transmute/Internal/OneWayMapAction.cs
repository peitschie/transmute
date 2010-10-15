using System;

namespace Transmute.Internal
{
    public class OneWayMapAction<TFrom, TTo, TContext> : OneWayMap<TFrom, TTo, TContext>
    {
        private readonly Action<IMappingCollection<TFrom, TTo, TContext>> _overrides;

        public OneWayMapAction(Action<IMappingCollection<TFrom, TTo, TContext>> overrides)
        {
            if (overrides == null) throw new ArgumentNullException("overrides");
            _overrides = overrides;
        }

        public override void OverrideMapping(IMappingCollection<TFrom, TTo, TContext> mapping)
        {
            base.OverrideMapping(mapping);
            _overrides(mapping);
        }
    }
}