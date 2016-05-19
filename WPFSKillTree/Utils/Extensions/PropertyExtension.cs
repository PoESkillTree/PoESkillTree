using System;
using System.Linq.Expressions;

namespace POESKillTree.Utils.Extensions
{
    public static class PropertyExtension
    {
        public static string Name<T, TProp>(this T o, Expression<Func<T, TProp>> propertySelector)
        {
            var body = (MemberExpression)propertySelector.Body;
            return body.Member.Name;
        }
    }
}
