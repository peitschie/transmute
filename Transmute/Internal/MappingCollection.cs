using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Transmute.Exceptions;
using Transmute.Internal.Utils;
using Transmute.Maps;

namespace Transmute.Internal
{
    public class MappingCollection<TFrom, TTo, TContext> : IMappingCollection<TFrom, TTo, TContext>
    {
        private class AvailablePropertiesClass : IAvailablePropertiesClass
        {
            private readonly Func<MemberInfo[]> _from;
            private readonly Func<MemberInfo[]> _to;

            public AvailablePropertiesClass(Func<MemberInfo[]> from, Func<MemberInfo[]> to)
            {
                _from = from;
                _to = to;
            }

            public MemberInfo[] Source { get { return _from(); } }
            public MemberInfo[] Destination { get { return _to(); } }
        }

        private int _setOrder;
        private readonly IResourceMapper<TContext> _mapper;
        private readonly AvailablePropertiesClass _unmapped;
        private readonly IList<MemberEntry> _setters;
        private readonly List<MemberInfo> _fromList;
        private readonly MemberInfo[] _fromPrefix;
        private readonly MemberInfo[] _toPrefix;

        private bool _isAutomapped = false;

        public IAvailablePropertiesClass Unmapped { get { return _unmapped; } }
        public IEnumerable<MemberEntry> Setters { get { return _setters.Where(m => m.IsMapped).OrderBy(m => m.SetOrder); } }
        public bool UpdatesContext { get { return ContextUpdater != null; } }
        public Func<object, object, IResourceMapper<TContext>, TContext, TContext> ContextUpdater { get; private set; }

        public MappingCollection(IResourceMapper<TContext> mapper)
            : this(mapper, new MemberInfo[0], new MemberInfo[0], new List<MemberEntry>(), 0)
        {
        }

        protected MappingCollection(IResourceMapper<TContext> mapper,
                                    MemberInfo[] fromPrefix,
                                    MemberInfo[] toPrefix,
                                    IList<MemberEntry> setters,
                                    int setOrder)
        {
            _setOrder = setOrder;
            _fromPrefix = fromPrefix;
            _toPrefix = toPrefix;
            _setters = setters;
            foreach (var property in typeof(TTo).GetProperties()
                                        .Where(p => p.GetSetMethod() != null && !_setters.Any(s => s.IsForMember(_toPrefix, p))))
            {
                _setters.Add(new MemberEntry {
                        DestinationMember = _toPrefix.Union(new MemberInfo[]{property}).ToArray(),
                        DestinationType = property.PropertyType,
                        IsMapped = false,
                });
            }
            _fromList = typeof(TFrom).GetProperties().Where(p => p.GetGetMethod() != null).Cast<MemberInfo>().ToList();
            _unmapped = new AvailablePropertiesClass(_fromList.ToArray,
                () => _setters.Where(s => !s.IsMapped
                                          && s.DestinationMember.Length == _toPrefix.Length + 1
                                          && s.IsForPrefix(_toPrefix))
                              .Select(s => s.DestinationMember[_toPrefix.Length]).ToArray());
            _mapper = mapper;
        }

        private IPriorityList<IMemberConsumer> _mapCreators;
        public IPriorityList<IMemberConsumer> MemberConsumers
        {
            get { return _mapCreators ?? (_mapCreators = new PriorityList<IMemberConsumer>(_mapper.MemberConsumers)); }
        }

        private IPriorityList<IMemberResolver> _memberResolvers;
        public IPriorityList<IMemberResolver> MemberResolvers
        {
            get { return _memberResolvers ?? (_memberResolvers = new PriorityList<IMemberResolver>(_mapper.MemberResolvers)); }
        }

        public void Overlay<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression,
                                                        Expression<Func<TFrom, TGetterType>> fromExpression,
                                                        Action<IMappingCollection<TGetterType, TPropertyType, TContext>> map=null)
        {
            var toChain = toExpression.GetExpressionChain(true);
            if(toChain.Length > 0 && !IsMapped(toChain))
            {
                MapEntry(toChain);
            }
            var subMapper = new MappingCollection<TGetterType, TPropertyType, TContext>(_mapper,
                fromExpression.GetExpressionChain(), toChain, _setters, _setOrder);
            if(map != null)
                map(subMapper);
            subMapper.DoAutomapping();
            _setOrder = subMapper._setOrder;
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo[] member, MemberSource<TContext> getter)
        {
            if(member == null)
                throw new ArgumentNullException("member");
            AssertIsNotLocked();
            var setter = MapEntry(member);
            setter.DestinationType = member.Last().ReturnType();
            setter.Remap = false;
            setter.SourceObjectType = MemberEntryType.Function;
            setter.SourceObject = getter;
            setter.SourceType = null;
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo to, MemberSource<TContext> getter)
        {
            return SetMember(new[] { to }, getter);
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo to, MemberInfo from)
        {
            return SetMember(new []{to}, new []{from});
        }
        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo[] to, MemberInfo[] from, bool? remap=null)
        {
            return Set(to, to.Last().ReturnType(), from, from.Last().ReturnType(), remap);
        }

        private IMappingCollection<TFrom, TTo, TContext> Set(MemberInfo[] to, Type toPropertyType,
                                                             MemberInfo[] from, Type fromPropertyType, bool? remap=null)
        {
            if(to == null) throw new ArgumentNullException("to");
            if(toPropertyType == null) throw new ArgumentNullException("toPropertyType");
            if(from == null) throw new ArgumentNullException("from");
            if(fromPropertyType == null) throw new ArgumentNullException("fromPropertyType");
            AssertIsNotLocked();
            var setter = MapEntry(to);
            setter.DestinationType = toPropertyType;
            setter.SourceObjectType = MemberEntryType.Member;
            setter.SourceObject = from;
            setter.SourceType = fromPropertyType;
            setter.Remap = remap ?? RequiresRemappingByDefault(setter.DestinationType, setter.SourceType, false);
            if(!setter.Remap)
            {
                VerifyReturnType(toPropertyType, fromPropertyType);
            }
            if (from.Length == 1)
            {
                // only remove the source property if it is not a child property, e.g., remove c.Child, but not c.Child.String
                _fromList.Remove(_fromList.FirstOrDefault(m => m.Name == from[0].Name));
            }
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Func<TGetterType> getter,
            bool? remap=null)
        {
            AssertIsNotLocked();
            var setter = MapEntry(MemberExpressions.GetExpressionChain(toExpression));
            setter.DestinationType = typeof(TPropertyType);
            setter.SourceObject = (MemberSource<TContext>)((from, to, mapper, context) => getter());
            setter.SourceType = typeof(TGetterType);
            setter.SourceObjectType = MemberEntryType.Function;
            setter.Remap = remap ?? RequiresRemappingByDefault(setter.DestinationType, setter.SourceType, true);
            if(!setter.Remap)
            {
                VerifyReturnType(typeof(TPropertyType), typeof(TGetterType));
            }
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TPropertyType> getter)
        {
            return Set(toExpression, getter, false);
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression,
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TGetterType> getter, bool? remap=null)
        {
            AssertIsNotLocked();
            var setter = MapEntry(MemberExpressions.GetExpressionChain(toExpression));
            setter.DestinationType = typeof(TPropertyType);
            setter.SourceObject = (MemberSource<TContext>)((from, to, mapper, context) => getter((TFrom)from, (TTo)to, mapper, context));
            setter.SourceType = typeof(TGetterType);
            setter.SourceObjectType = MemberEntryType.Function;
            setter.Remap = remap ?? RequiresRemappingByDefault(typeof(TGetterType), typeof(TPropertyType), true);
            if(!setter.Remap)
            {
                VerifyReturnType(typeof(TPropertyType), typeof(TGetterType));
            }
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Expression<Func<TFrom, TGetterType>> fromExpression,
            bool? remap=null)
        {
            var to = MemberExpressions.GetExpressionChain(toExpression); // This call should throw an error if toExpression is not a propery chain
            try
            {
                var from = MemberExpressions.GetExpressionChain(fromExpression);
                return Set(to, typeof(TPropertyType), from, typeof(TGetterType), remap);
            }
            catch (MemberExpressionException)
            {
                var fromDelegate = fromExpression.Compile();
                return Set(toExpression, (frm, t, mapper, context) => fromDelegate(frm), remap);
            }
        }

        public IMappingCollection<TFrom, TTo, TContext> Ignore<TMemberType>(Expression<Func<TTo, TMemberType>> expression)
        {
            return IgnoreMember(MemberExpressions.GetMemberInfo(expression));
        }

        public IMappingCollection<TFrom, TTo, TContext> IgnoreMember(MemberInfo member)
        {
            if(member == null) throw new ArgumentNullException("member");
            AssertIsNotLocked();
            var setter = MapEntry(member);
            setter.SourceObject = null;
            return this;
        }

        private void AssertIsNotLocked()
        {
            if (_isAutomapped)
                throw new MapperException("Automapping has been completed.  No further overrides can be performed");
        }

        public IMappingCollection<TFrom, TTo, TContext> SetChildContext(Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TContext> updateContext)
        {
            if (typeof(ICloneable).IsAssignableFrom(typeof(TContext)))
            {
                ContextUpdater = (from, to, mapper, context) => updateContext((TFrom) from, (TTo) to, mapper, (TContext) ((ICloneable) context).Clone());
            }
            else if (typeof(TContext).IsValueType) // value types are immutable... so changing this value will not change the existing context
            {
                ContextUpdater = (from, to, mapper, context) => updateContext((TFrom)from, (TTo)to, mapper, context);
            }
            else
            {
                throw new InvalidOperationException("Changing of the child context is not allowed as context type {0} is not a value type and cannot be cloned using the ICloneable interface".With(typeof(TContext)));
            }
            return this;
        }

        private static bool RequiresRemappingByDefault(Type toType, Type fromType, bool isFunction)
        {
            return !(toType.IsAssignableFrom(fromType)
                     && (isFunction
                         || (MapByVal<TContext>.IsValType(toType) && MapByVal<TContext>.IsValType(toType))));
        }

        public IMappingCollection<TFrom, TTo, TContext> RequireOneWayMap(Type from, Type to)
        {
            _mapper.RequireOneWayMap(from, to, typeof(TFrom), typeof(TTo));
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> RequireOneWayMap<TFromType, TToType>()
        {
            _mapper.RequireOneWayMap<TFromType, TToType, TFrom, TTo>();
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> RequireTwoWayMap(Type type1, Type type2)
        {
            _mapper.RequireTwoWayMap(type1, type2, typeof(TFrom), typeof(TTo));
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> RequireTwoWayMap<TType1, TType2>()
        {
            _mapper.RequireTwoWayMap<TType1, TType2, TFrom, TTo>();
            return this;
        }

        public IMappingCollection<TFrom, TTo, TContext> DoAutomapping()
        {
            if (!_isAutomapped)
            {
                foreach (var mapCreator in MemberConsumers)
                {
                    mapCreator.CreateMap(this);
                }
                _isAutomapped = true;
            }
            return this;
        }

        public void VerifyMap()
        {
            if (_setters.Any(s => !s.IsMapped))
            {
                throw new UnmappedMembersException(typeof(TFrom), typeof(TTo), 
                                        _setters.Where(s => !s.IsMapped).Select(s => s.DestinationMember.Last()).ToList());
            }
        }

        private static void VerifyReturnType(Type to, Type from)
        {
            if (!to.IsAssignableFrom(from))
                throw new MemberMappingException(typeof(TFrom), typeof(TTo), from, to, string.Format("Unable to assign method return type {0} to property type {1}", from, to));
        }

        private bool IsMapped(params MemberInfo[] member)
        {
            return _setters.Any(s => s.IsForMember(_toPrefix, member) && s.IsMapped);
        }

        private MemberEntry MapEntry(params MemberInfo[] member)
        {
            var setter = _setters.FirstOrDefault(s => s.IsForMember(_toPrefix, member));
            if (setter == null)
            {
                setter = new MemberEntry();
                _setters.Add(setter);
            }
            setter.DestinationMember = _toPrefix.Union(member).ToArray();
            setter.SourceRoot = _fromPrefix;
            setter.IsMapped = true;
            setter.SetOrder = _setOrder++;
            return setter;
        }
    }
}