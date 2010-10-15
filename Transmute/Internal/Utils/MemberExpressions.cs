using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Transmute.Internal.Utils
{
    /// <summary>
    /// // http://handcraftsman.wordpress.com/2008/11/11/how-to-get-c-property-names-without-magic-strings/ 
    /// </summary>
    internal static class MemberExpressions
    {
        public static MemberInfo GetMemberInfo<TMember>(Expression<Func<TMember, object>> expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            return GetMemberInfo(expression.Body);
        }

        public static MemberInfo GetMemberInfo(Expression expression)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                // Need to ensure this returns the property setter for the requested type, not
                // the type that defines the member.  I was unable to find a clean way of
                // changing the property accessor to by on the requested type.
                return memberExpression.Expression.Type.GetMember(memberExpression.Member.Name).FirstOrDefault();
            }
            else if (expression is UnaryExpression)
            {
                var unaryExpression = (UnaryExpression)expression;
                if (unaryExpression.NodeType == ExpressionType.Convert)
                {
                    return GetMemberInfo(unaryExpression.Operand);
                }
            }
            else if (expression is LambdaExpression)
            {
                var lambdaExpression = (LambdaExpression)expression;
                return GetMemberInfo(lambdaExpression.Body);
            }
            throw new MemberExpressionException(expression);
        }

        public static MemberInfo[] GetExpressionChain<TType>(this Expression<Func<TType, object>> expression, bool allowZeroLengthChains=false)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            return GetExpressionChain(expression.Body, allowZeroLengthChains);
        }

        public static MemberInfo[] GetExpressionChain(this Expression expression, bool allowZeroLengthChains = false)
        {
            if (expression == null) throw new ArgumentNullException("expression");
            var memberChain = new List<MemberInfo>();
            GetExpressionChain(expression, memberChain);
            if (!allowZeroLengthChains && memberChain.Count == 0)
                throw new MemberExpressionException(expression);
            return memberChain.ToArray();
        }

        private static void GetExpressionChain(Expression expression, List<MemberInfo> memberChain)
        {
            if (expression is MemberExpression)
            {
                var memberExpression = (MemberExpression)expression;
                if (memberExpression.Expression != null && (
                    memberExpression.Expression is MemberExpression
                    || (memberExpression.Expression is UnaryExpression && memberExpression.Expression.NodeType == ExpressionType.Convert)
                    || memberExpression.Expression is LambdaExpression))
                {
                    GetExpressionChain(memberExpression.Expression, memberChain);
                }
                memberChain.Add(GetMemberInfo(memberExpression));
            }
            else if (expression is UnaryExpression && expression.NodeType == ExpressionType.Convert)
            {
                var unaryExpression = (UnaryExpression)expression;
                GetExpressionChain(unaryExpression.Operand, memberChain);
            }
            else if (expression is LambdaExpression)
            {
                var lambdaExpression = (LambdaExpression)expression;
                GetExpressionChain(lambdaExpression.Body, memberChain);
            }
            else if (expression is ParameterExpression)
            {
                return; // A parameter is the last link in the chain.  Essentially, there are no member info entries required above this
            }
            else
            {
                throw new MemberExpressionException(expression);
            }
        }
    }

    public class MemberExpressionException : Exception
    {
        public MemberExpressionException(Expression expression) : base(string.Format("Cannot interpret member from {0}", expression.NodeType))
        {}
    }
}
