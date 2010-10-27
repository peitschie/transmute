using System;
namespace Transmute
{
    public class FuncBasedMap<TFrom, TTo, TContext> : IMap<TFrom, TTo, TContext>
    {
        public Func<TFrom, TTo, TContext, TTo> ConvertMethod;

        public bool IsInitialized { get { return ConvertMethod != null; } }

        public object MapObject(object from, object to, TContext context)
        {
            if (from == null)
            {
                return default(TTo);
            };
            return ConvertMethod((TFrom)from, (TTo)to, context);
        }

        public TTo Map(TFrom from, TTo to, TContext context)
        {
            if (from == null)
            {
                return default(TTo);
            };
            return ConvertMethod(from, to, context);
        }
    }
}

