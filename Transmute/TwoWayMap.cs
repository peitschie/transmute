using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Transmute
{
    public abstract class TwoWayMap<TType1, TType2, TContext> : ITwoWayMap<TType1, TType2, TContext>
    {
        private readonly IOneWayMap<TType1, TType2, TContext> _type1ToType2;
        private readonly IOneWayMap<TType2, TType1, TContext> _type2ToType1;

        protected TwoWayMap()
        {
            _type1ToType2 = new OneWayMapT1ToT2(Override_TType1_To_TType2, Override_BothDirections);
            _type2ToType1 = new OneWayMapT2ToT1(Override_TType2_To_TType1, Override_BothDirections);
        }

        public Type Type1
        {
            get { return typeof(TType1); }
        }

        public Type Type2
        {
            get { return typeof(TType2); }
        }

        public IOneWayMap<TType1, TType2, TContext> Type1ToType2
        {
            get { return _type1ToType2; }
        }

        IOneWayMap<TContext> ITwoWayMap<TContext>.Type2ToType1
        {
            get { return Type2ToType1; }
        }


        IOneWayMap<TContext> ITwoWayMap<TContext>.Type1ToType2
        {
            get { return Type1ToType2; }
        }

        public IOneWayMap<TType2, TType1, TContext> Type2ToType1
        {
            get { return _type2ToType1; }
        }

        protected virtual void Override_TType1_To_TType2(IMappingCollection<TType1, TType2, TContext> mapping) {}
        protected virtual void Override_TType2_To_TType1(IMappingCollection<TType2, TType1, TContext> mapping) {}
        protected virtual void Override_BothDirections(IBidirectionMap<TType1, TType2> mapping) {}

        private class OneWayMapT1ToT2 : IOneWayMap<TType1, TType2, TContext>
        {
            private readonly Action<IMappingCollection<TType1, TType2, TContext>> _action;
            private readonly Action<IBidirectionMap<TType1, TType2>> _bothDirectionsMap;

            public OneWayMapT1ToT2(Action<IMappingCollection<TType1, TType2, TContext>> action, Action<IBidirectionMap<TType1, TType2>> bothDirectionsMap)
            {
                _action = action;
                _bothDirectionsMap = bothDirectionsMap;
            }

            public void OverrideMapping(IMappingCollection<TType1, TType2, TContext> mapping)
            {
                _bothDirectionsMap(new BidirectionMapWrapper<TType1, TType2, TContext>(mapping));
                _action(mapping);
            }

            public Type FromType
            {
                get { return typeof(TType1); }
            }

            public Type ToType
            {
                get { return typeof(TType2); }
            }
        }

        private class OneWayMapT2ToT1 : IOneWayMap<TType2, TType1, TContext>
        {
            private readonly Action<IMappingCollection<TType2, TType1, TContext>> _action;
            private readonly Action<IBidirectionMap<TType1, TType2>> _bothDirectionsMap;

            public OneWayMapT2ToT1(Action<IMappingCollection<TType2, TType1, TContext>> action, Action<IBidirectionMap<TType1, TType2>> bothDirectionsMap)
            {
                _action = action;
                _bothDirectionsMap = bothDirectionsMap;
            }

            public void OverrideMapping(IMappingCollection<TType2, TType1, TContext> mapping)
            {
                _bothDirectionsMap(new BidirectionMapWrapper<TType1, TType2, TContext>(mapping));
                _action(mapping);
            }

            public Type FromType
            {
                get { return typeof(TType2); }
            }

            public Type ToType
            {
                get { return typeof(TType1); }
            }
        }
    }

    internal class BidirectionMapWrapper<TType1, TType2, TContext> : IBidirectionMap<TType1, TType2>
    {
        private readonly IMappingCollection<TType1, TType2, TContext> _mapping = null;
        private readonly IMappingCollection<TType2, TType1, TContext> _reverseMapping = null;


        public BidirectionMapWrapper(IMappingCollection<TType1, TType2, TContext> mapping)
        {
            _mapping = mapping;
        }

        public BidirectionMapWrapper(IMappingCollection<TType2, TType1, TContext> mapping)
        {
            _reverseMapping = mapping;
        }

        public void Set(MemberInfo to, MemberInfo from)
        {
            if(_mapping != null)
            {
                _mapping.SetMember(to, from);
            }
            else
            {
                _reverseMapping.SetMember(from, to);
            }
        }

        public void Set(MemberInfo[] member, MemberInfo[] getter)
        {
            if (_mapping != null)
            {
                _mapping.SetMember(member, getter);
            }
            else
            {
                _reverseMapping.SetMember(getter, member);
            }
        }

        public void Set<TMemberType>(Expression<Func<TType2, TMemberType>> toExpression, Expression<Func<TType1, TMemberType>> fromExpression)
        {
            if (_mapping != null)
            {
                _mapping.Set(toExpression, fromExpression);
            }
            else
            {
                _reverseMapping.Set(fromExpression, toExpression);
            }
        }

        public void Set(Expression<Func<TType2, object>> toExpression, Expression<Func<TType1, object>> fromExpression, bool doConversion)
        {
            if (_mapping != null)
            {
                _mapping.Set(toExpression, fromExpression, doConversion);
            }
            else
            {
                _reverseMapping.Set(fromExpression, toExpression, doConversion);
            }
        }
    }
}