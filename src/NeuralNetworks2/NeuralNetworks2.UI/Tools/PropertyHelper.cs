using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace NeuralNetworks2.UI.Tools
{
    [DebuggerStepThrough]
    public static class PropertyHelper
    {
        public static string GetPropertyName<T>(Expression<Func<T, object>> e)
        {
            return GetPropertyName((LambdaExpression)e);
        }

        public static string GetPropertyName(Expression<Func<object>> e)
        {
            return GetPropertyInfo(e).Name;
        }

        public static string GetPropertyName(LambdaExpression e)
        {
            return GetPropertyInfo(e).Name;
        }

        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> e)
        {
            return GetPropertyInfo((LambdaExpression)e);
        }

        public static PropertyInfo GetPropertyInfo(LambdaExpression e)
        {
            var x = e.Body is UnaryExpression
                        ? (MemberExpression)((UnaryExpression)e.Body).Operand
                        : (MemberExpression)e.Body;
            return (PropertyInfo)x.Member;
        }
    }
}