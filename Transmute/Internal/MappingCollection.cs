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

		private int _setOrder = 0;
        private readonly IResourceMapper<TContext> _mapper;
        private readonly AvailablePropertiesClass _unmapped;
		private readonly List<MemberEntry> _setters;
        private readonly List<MemberInfo> _fromList;
        private readonly MemberInfo[] _fromPrefix;
        private readonly MemberInfo[] _toPrefix;

        private bool _isAutomapped = false;

        public IAvailablePropertiesClass Unmapped { get { return _unmapped; } }
        public IEnumerable<MemberEntry> Setters { get { return _setters.Where(m => m.IsMapped).OrderBy(m => m.SetOrder); } }
        public bool UpdatesContext { get { return ContextUpdater != null; } }
        public Func<object, object, IResourceMapper<TContext>, TContext, TContext> ContextUpdater { get; private set; }

        public MappingCollection(IResourceMapper<TContext> mapper)
        {
        	_setters = new List<MemberEntry>();
        	foreach (var property in typeof(TTo).GetProperties().Where(p => p.GetSetMethod() != null))
			{
				_setters.Add(new MemberEntry {
						DestinationMember = new MemberInfo[]{property},
						DestinationType = property.PropertyType,
						IsMapped = false,
				});
			}
            _fromPrefix = new MemberInfo[0];
            _toPrefix = new MemberInfo[0];
            _fromList = typeof(TFrom).GetProperties().Where(p => p.GetGetMethod() != null).Cast<MemberInfo>().ToList();
            _unmapped = new AvailablePropertiesClass(_fromList.ToArray, 
				() => _setters.Where(s => !s.IsMapped).Select(s => s.DestinationMember.Last()).ToArray());
            _mapper = mapper;
        }

        protected MappingCollection(IResourceMapper<TContext> mapper, List<MemberInfo> toList, MemberInfo[] fromPrefix, MemberInfo[] toPrefix, Dictionary<string, IMapper<TContext>> setters)
        {
        	throw new NotImplementedException();
//            _fromPrefix = fromPrefix;
//            _toPrefix = toPrefix;
//            _setters = setters;
//            _fromList = typeof(TFrom).GetProperties().Where(p => p.GetGetMethod() != null).Cast<MemberInfo>().ToList();
//            _toList = toList;
//            _unmapped = new AvailablePropertiesClass(_fromList, _toList);
//            _mapper = mapper;
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

        public void Overlay<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Expression<Func<TFrom, TGetterType>> fromExpression)
        {
            var toChain = toExpression.GetExpressionChain(true);
            if(toChain.Length > 0)
            {
                OverlayChild(toExpression, fromExpression);
            }
            else
            {
                OverlayRoot(toExpression, fromExpression);
            }
        }

        private void OverlayChild<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Expression<Func<TFrom, TGetterType>> fromExpression)
        {
        	throw new NotImplementedException();
//            var toChain = toExpression.GetExpressionChain();
//            _toList.Remove(_toList.FirstOrDefault(f => f.Name == toChain[0].Name));
//            DoAutomapping(); // If automapping is not done before overlaying the root, often the end results can get VERY unexpected
//            var toList = typeof(TPropertyType).GetProperties().Where(p => p.GetSetMethod() != null).Cast<MemberInfo>().ToList();
//            var subMapper = new MappingCollection<TGetterType, TPropertyType, TContext>(_mapper, toList, fromExpression.GetExpressionChain(), toChain, _setters);
//            subMapper.DoAutomapping();
        }

        private void OverlayRoot<TPropertyType, TGetterType>(Expression<Func<TTo, TPropertyType>> toExpression, Expression<Func<TFrom, TGetterType>> fromExpression)
        {
        	throw new NotImplementedException();
//            DoAutomapping(); // If automapping is not done before overlaying the root, often the end results can get VERY unexpected
//            var subMapper = new MappingCollection<TGetterType, TTo, TContext>(_mapper, _toList, fromExpression.GetExpressionChain(), new MemberInfo[0], _setters);
//            subMapper.DoAutomapping();
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo[] member, MemberSource<TContext> getter)
        {
        	AssertIsNotLocked();
        	try
            {
        		var setter = _setters.FirstOrDefault(s => s.IsForMember(member));
        		if (setter == null)
				{
        			setter = new MemberEntry { 
						DestinationMember = member, 
						DestinationType = member.Last().ReturnType() 
					};
        			_setters.Add(setter);
				}
        		setter.IsMapped = true;
        		setter.SourceObjectType = MemberEntryType.Function;
        		setter.SourceObject = getter;
        		setter.SourceType = typeof(object);
				setter.SetOrder = _setOrder++;
                return this;
            }
            catch (Exception e)
            {
                throw new MemberMappingException(typeof(TFrom), typeof(TTo), member, null, "Unable to create setter", e);
            }
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo to, MemberInfo from)
        {
            return SetMember(new[] { to }, new[] { from });
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo to, MemberSource<TContext> getter)
        {
            return SetMember(new[] { to }, getter);
        }

        public IMappingCollection<TFrom, TTo, TContext> SetMember(MemberInfo[] to, MemberInfo[] from, bool? remap=null)
        {
            return Set(to, to.Last().ReturnType(), from, from.Last().ReturnType(), RequiresRemappingByDefault(to.Last().ReturnType(), from.Last().ReturnType()));
        }

        private IMappingCollection<TFrom, TTo, TContext> Set(MemberInfo[] to, Type toPropertyType, MemberInfo[] from, Type fromPropertyType, bool remap)
        {
            return remap ? SetWithRemap(to, toPropertyType, from, fromPropertyType) : SetWithoutRemap(to, toPropertyType, from, fromPropertyType);    
        }

        private IMappingCollection<TFrom, TTo, TContext> SetWithoutRemap(MemberInfo[] to, Type toPropertyType, MemberInfo[] from, Type fromPropertyType)
        {
            VerifyReturnType(toPropertyType, fromPropertyType);
            var fromAccessor = MapperUtils.CreateAccessorChain(from);
            if (from.Length == 1)
            {
                // only remove the source property if it is not a child property, e.g., remove c.Child, but not c.Child.String
                _fromList.Remove(_fromList.FirstOrDefault(m => m.Name == from[0].Name));
            }
            return SetMember(to, (source, dest, mapper, context) => fromAccessor.Get(source));
        }

        private IMappingCollection<TFrom, TTo, TContext> SetWithRemap(MemberInfo[] to, Type toPropertyType, MemberInfo[] from, Type fromPropertyType)
        {
            if (from.Length == 1)
            {
                // only remove the source property if it is not a child property, e.g., remove c.Child, but not c.Child.String
                _fromList.Remove(_fromList.FirstOrDefault(m => m.Name == from[0].Name));
            }

            _mapper.RequireOneWayMap(fromPropertyType, toPropertyType, typeof(TFrom), typeof(TTo));
            var fromAccessor = MapperUtils.CreateAccessorChain(from);
            var toAccessor = MapperUtils.CreateAccessorChain(to);
            return SetMember(to, (source, dest, mapper, context) => mapper.Map(fromPropertyType, toPropertyType, fromAccessor.Get(source), toAccessor.Get(dest), context));
        }

        private IMappingCollection<TFrom, TTo, TContext> SetWithRemap<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TGetterType> getter)
        {
            var members = MemberExpressions.GetExpressionChain(toExpression);
            if (!_mapper.CanMap(typeof(TGetterType), typeof(TPropertyType)))
            {
                throw new MemberMappingException(typeof(TFrom), typeof(TTo), null, members.Last(), string.Format("Unable to map from {0} to {1}", typeof(TGetterType), typeof(TPropertyType)));
            }
            var toAccessor = MapperUtils.CreateAccessorChain(members);
            _mapper.RequireOneWayMap<TGetterType, TPropertyType, TFrom, TTo>();
            return SetMember(members, (from, to, mapper, context) => mapper.Map(typeof(TGetterType), typeof(TPropertyType), getter((TFrom)from, (TTo)to, mapper, context), toAccessor.Get(to), context));
        }

        private IMappingCollection<TFrom, TTo, TContext> SetWithoutRemap<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TGetterType> getter)
        {
            VerifyReturnType(typeof(TPropertyType), typeof(TGetterType));
            var members = MemberExpressions.GetExpressionChain(toExpression);
            return SetMember(members, (from, to, mapper, context) => getter((TFrom)from, (TTo)to, mapper, context));
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Func<TGetterType> getter,
            bool remap=false)
        {
            return remap ? SetWithRemap(toExpression, (from, to, mapper, context) => getter()) : SetWithoutRemap(toExpression, (from, to, mapper, context) => getter());
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TPropertyType> getter)
        {
            return Set(toExpression, getter, false);
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression,
            Func<TFrom, TTo, IResourceMapper<TContext>, TContext, TGetterType> getter, bool remap=false)
        {
            return remap ? SetWithRemap(toExpression, getter) : SetWithoutRemap(toExpression, getter);
        }

        public IMappingCollection<TFrom, TTo, TContext> Set<TPropertyType, TGetterType>(
            Expression<Func<TTo, TPropertyType>> toExpression, 
            Expression<Func<TFrom, TGetterType>> fromExpression,
            bool? remap=null)
        {
            if (remap == null)
                remap = RequiresRemappingByDefault(typeof(TPropertyType), typeof(TGetterType));
            var to = MemberExpressions.GetExpressionChain(toExpression); // This call should throw an error if toExpression is not a propery chain
            try
            {
                var from = MemberExpressions.GetExpressionChain(fromExpression);
                return Set(to, typeof(TPropertyType), from, typeof(TGetterType), remap.Value);
            }
            catch (MemberExpressionException)
            {
                var fromDelegate = fromExpression.Compile();
                return Set(toExpression, (frm, t, mapper, context) => fromDelegate(frm), remap.Value);
            }
        }

        public IMappingCollection<TFrom, TTo, TContext> Ignore<TMemberType>(Expression<Func<TTo, TMemberType>> expression)
        {
            return IgnoreMember(MemberExpressions.GetMemberInfo(expression));
        }

        public IMappingCollection<TFrom, TTo, TContext> IgnoreMember(MemberInfo member)
        {
            AssertIsNotLocked();
			var setter = _setters.First(s => s.IsForMember(member));
			setter.IsMapped = true;
			setter.SetOrder = _setOrder++;
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

        private static bool RequiresRemappingByDefault(Type toType, Type fromType)
        {
            return !(toType.IsAssignableFrom(fromType) && MapByVal<TContext>.IsValType(toType) && MapByVal<TContext>.IsValType(toType));
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
    }
}