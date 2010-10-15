namespace Transmute.Tests.Types
{
    public class ClassWithSeveralPropertiesOverride<TContext> : OneWayMap<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, TContext>
    {
        public static readonly string[] PropertySetOrder = new[] { "Property2", "Property1", "Property3" };

        public override void OverrideMapping(IMappingCollection<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, TContext> mapping)
        {
            int stringValue = 0;
            mapping.Set(to => to.Property2, () => stringValue++);
            mapping.Set(to => to.Property1, () => stringValue++);
            mapping.Set(to => to.Property3, () => stringValue++);
        }
    }

    public class ClassWithSeveralPropertiesNullableOverride<TContext> : OneWayMap<ClassWithSeveralPropertiesSrcNullable, ClassWithSeveralPropertiesDest, TContext>
    {
        public static readonly string[] PropertySetOrder = new[] { "Property2", "Property1", "Property3" };

        public override void OverrideMapping(IMappingCollection<ClassWithSeveralPropertiesSrcNullable, ClassWithSeveralPropertiesDest, TContext> mapping)
        {
            int stringValue = 0;
            mapping.Set(to => to.Property2, () => stringValue++);
            mapping.Set(to => to.Property1, () => stringValue++);
            mapping.Set(to => to.Property3, () => stringValue++);
        }
    }
}