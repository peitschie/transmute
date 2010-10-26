using System;
namespace Transmute
{
    public class FuncBasedMap<TFrom, TTo, TContext> : IMap<TFrom, TTo, TContext>
    {
        public Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TTo> ConvertMethod;

        public bool IsInitialized { get { return ConvertMethod != null; } }

        public object MapObject(object from, object to, IResourceMapper<TContext> mapper, TContext context)
        {
            if (from == null)
            {
                return default(TTo);
            };
            return ConvertMethod((TFrom)from, (TTo)to, mapper, context);
        }

        public TTo Map(TFrom from, TTo to, IResourceMapper<TContext> mapper, TContext context)
        {
            if (from == null)
            {
                return default(TTo);
            };
            return ConvertMethod(from, to, mapper, context);
        }
    }
}

