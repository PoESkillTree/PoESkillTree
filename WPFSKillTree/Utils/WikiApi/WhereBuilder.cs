using System.Text;

namespace POESKillTree.Utils.WikiApi
{
    public class WhereBuilder
    {
        private StringBuilder _where;

        public WhereBuilder Add(string field, string value)
        {
            var condition = $"{field}=\"{value}\"";
            _where = _where?.Append(" AND ").Append(condition) ?? new StringBuilder(condition);
            return this;
        }

        public override string ToString()
        {
            return _where.ToString();
        }
    }
}