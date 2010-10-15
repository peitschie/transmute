using Transmute.Internal;

namespace Transmute.Tests.Types
{
    public class OverrideDefaultCreators<TContext> : OneWayMap<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, TContext>
    {
        private readonly IMemberConsumer[] _creators;

        public OverrideDefaultCreators(IMemberConsumer[] creators)
        {
            _creators = creators;
        }

        public override void OverrideMapping(IMappingCollection<ClassWithSeveralPropertiesSrc, ClassWithSeveralPropertiesDest, TContext> mapping)
        {
            if(_creators == null)
                return;
            mapping.MemberConsumers.Clear();
            foreach (var mapCreator in _creators)
            {
                mapping.MemberConsumers.Add(mapCreator);    
            }
        }
    }
}