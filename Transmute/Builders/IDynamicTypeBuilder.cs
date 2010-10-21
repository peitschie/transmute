using System;
using System.Reflection.Emit;
namespace Transmute
{
    public interface IDynamicTypeBuilder<TContext>
    {
        TypeBuilder Type { get; }
        object Instance { get; }
        MethodBuilder GetOrCreateConvertor(Type from, Type to);
    }
}

