using System.Linq.Expressions;

namespace PoESkillTree.Computation.Builders
{
    public static class ExpressionHelper
    {
        public static string ToString<T>(this Expression<T> @this, params object[] parameters)
        {
            // Make parameter names unique in Body.ToString to make sure the string.Replace below only replaces them.
            var exp = (Expression<T>) new ParameterUpdateVisitor().Visit(@this);
            var s = exp.Body.ToString();
            for (var i = 0; i < exp.Parameters.Count; i++)
            {
                var name = exp.Parameters[i].Name;
                s = s.Replace(name, parameters[i].ToString());
            }
            return s;
        }

        private class ParameterUpdateVisitor : ExpressionVisitor
        {
            protected override Expression VisitParameter(ParameterExpression node) => 
                Expression.Parameter(node.Type, $"~{node.Name}~");
        }
    }
}