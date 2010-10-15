using System;
using System.Reflection;

namespace Transmute.Internal.FastMemberAccessor
{
    public class PropertyAccessor<TObj, TProperty> : IMemberAccessor
    {
        private readonly Action<TObj, TProperty> _setter;
        private readonly Func<TObj, TProperty> _getter;
        private readonly PropertyInfo _field;

        public PropertyAccessor(PropertyInfo field)
        {
            _field = field;
            if (_field.GetGetMethod() != null)
            {
                _getter = (Func<TObj, TProperty>)Delegate.CreateDelegate(typeof(Func<TObj, TProperty>), null, _field.GetGetMethod());
            }
            if (_field.GetSetMethod() != null)
            {
                _setter = (Action<TObj, TProperty>)Delegate.CreateDelegate(typeof(Action<TObj, TProperty>), null, _field.GetSetMethod());
            }
        }

        public object Get(object target)
        {
            //Optimized delegate equivalent of return _field.GetValue(target, null);
            if (target == null) return null;
            return _getter((TObj)target);
        }

        public void Set(object target, object value)
        {
            //Optimized delegate equivalent of _field.SetValue(target, value, null);
            _setter((TObj)target, (TProperty)value);
        }

        public Type MemberType { get { return _field.PropertyType; } }

        public Type ReflectedType { get { return _field.ReflectedType; } }
    }
}