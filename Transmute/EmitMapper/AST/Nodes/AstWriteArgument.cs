using System;
using System.Reflection.Emit;
using EmitMapper.AST.Helpers;
using EmitMapper.AST.Interfaces;
using EmitMapper.AST;
namespace Transmute
{
    class AstWriteArgument : IAstNode
    {
        public int argumentIndex;
        public Type argumentType;
        public IAstRefOrValue value;

        public AstWriteArgument(int argumentIndex, Type argumentType, IAstRefOrValue value)
        {
            this.argumentIndex = argumentIndex;
            this.argumentType = argumentType;
            this.value = value;
        }

        public void Compile(CompilationContext context)
        {
            value.Compile(context);
            CompilationHelper.PrepareValueOnStack(context, argumentType, value.itemType);
            context.Emit(OpCodes.Starg_S, argumentIndex);
        }
    }
}

