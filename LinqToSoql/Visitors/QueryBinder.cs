using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToSoql.Expressions;
using LinqToSoql.Sforce;

namespace LinqToSoql.Visitors
{
    internal class QueryBinder : ExpressionVisitor
    {
        private readonly ColumnProjector _columnProjector;
        private Dictionary<ParameterExpression, Expression> _map;
        private int _aliasCount;
        private readonly IQueryProvider _provider;

        public QueryBinder(IQueryProvider provider)
        {
            _provider = provider;
            _columnProjector = new ColumnProjector(CanBeColumn);
        }

        private bool CanBeColumn(Expression expression)
        {
            MemberExpression memberExpression = expression as MemberExpression;
            if (memberExpression != null)
            {
                return CanBeColumn(memberExpression.Expression);
            }

            return expression.NodeType == (ExpressionType) DbExpressionType.Column
                   || expression.NodeType == (ExpressionType) DbExpressionType.Projection;
        }

        public Expression Bind(Expression expression)
        {
            _map = new Dictionary<ParameterExpression, Expression>();
            return Visit(expression);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        private string GetNextAlias()
        {
            return "t" + (_aliasCount++);
        }

        private ProjectedColumns ProjectColumns(Expression expression, string newAlias, string existingAlias)
        {
            return _columnProjector.ProjectColumns(expression, newAlias, existingAlias);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) ||
                m.Method.DeclaringType == typeof(Enumerable))
            {
                switch (m.Method.Name)
                {
                    case "Where":
                        return BindWhere(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                    case "Select":
                        return BindSelect(m.Type, m.Arguments[0], (LambdaExpression)StripQuotes(m.Arguments[1]));
                }
                throw new NotSupportedException(string.Format("The method '{0}' is not supported!", m.Method.Name));
            }
            return base.VisitMethodCall(m);
        }

        private Expression BindWhere(Type resultType, Expression source, LambdaExpression predicate)
        {
            ProjectionExpression projection = (ProjectionExpression)Visit(source);
            _map[predicate.Parameters[0]] = projection.Projector;
            Expression where = Visit(predicate.Body);
            string alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(projection.Projector, alias, GetExistingAlias(projection.Source));
            return new ProjectionExpression(
                new SelectExpression(resultType, alias, pc.Columns, projection.Source, where),
                pc.Projector
                );
        }

        private Expression BindSelect(Type resultType, Expression source, LambdaExpression selector)
        {
            var v = Visit(source);
            ProjectionExpression projection = v as ProjectionExpression;
            if (projection == null)
            {
                var columnExpression = v as ColumnExpression;
                if (columnExpression == null)
                {
                    throw new NotSupportedException(String.Format("'{0}' source is not supported!", v.NodeType));
                }
                projection = GetTableProjection(columnExpression);
            }
            _map[selector.Parameters[0]] = projection.Projector;
            Expression expression = Visit(selector.Body);
            string alias = GetNextAlias();
            ProjectedColumns pc = ProjectColumns(expression, alias, GetExistingAlias(projection.Source));
            return new ProjectionExpression(
                new SelectExpression(resultType, alias, pc.Columns, projection.Source, null),
                pc.Projector
                );
        }

        private ProjectionExpression GetSubQueryProjection(ColumnExpression expression)
        {
            Type elementType = TypeSystem.GetElementType(expression.Type);
            var obj = Activator.CreateInstance(typeof(Query<>).MakeGenericType(expression.Type),
                             new object[] { _provider, expression });
            return GetTableProjection(obj);
        }

        private static string GetExistingAlias(Expression source)
        {
            switch ((DbExpressionType)source.NodeType)
            {
                case DbExpressionType.Select:
                    return ((SelectExpression)source).Alias;
                case DbExpressionType.Table:
                    return ((TableExpression)source).Alias;
                default:
                    throw new InvalidOperationException(string.Format("Invalid source node type '{0}'", source.NodeType));
            }
        }

        private bool IsTable(object value)
        {
            IQueryable q = value as IQueryable;
            return q != null && q.Provider == _provider && q.Expression.NodeType == ExpressionType.Constant;
        }

        private string GetTableName(object table)
        {
            IQueryable query = table as IQueryable;
            if (query != null)
            {
                return query.ElementType.Name;
            }

            throw new NotSupportedException(String.Format("'{0}' table is not supported", table.GetType().Name));
        }

        private string GetColumnName(MemberInfo member)
        {
            return member.Name;
        }

        private Type GetColumnType(MemberInfo member)
        {
            FieldInfo fi = member as FieldInfo;
            if (fi != null)
            {
                return fi.FieldType;
            }
            PropertyInfo pi = (PropertyInfo)member;
            return pi.PropertyType;
        }

        private IEnumerable<MemberInfo> GetMappedMembers(Type rowType)
        {
            var fields = rowType.GetProperties();
            return fields;
        }

        private ProjectionExpression GetTableProjection(object value)
        {
            IQueryable table = value as IQueryable;
            ColumnExpression columnExpression = value as ColumnExpression;
            Type elementType;
            if (table != null)
            {
                elementType = table.ElementType;
            }
            else if (columnExpression != null)
            {
                elementType = TypeSystem.GetElementType(columnExpression.Type);
            }
            else
            {
                throw new NotSupportedException(String.Format("'{0}' table is not supported!", value.GetType().Name));
            }
            string tableAlias = GetNextAlias();
            string selectAlias = GetNextAlias();
            List<MemberBinding> bindings = new List<MemberBinding>();
            List<ColumnDeclaration> columns = new List<ColumnDeclaration>();
            foreach (MemberInfo mi in GetMappedMembers(elementType))
            {
                string columnName = GetColumnName(mi);
                Type columnType = GetColumnType(mi);
                bindings.Add(Expression.Bind(mi, new ColumnExpression(columnType, selectAlias, columnName)));
                columns.Add(new ColumnDeclaration(columnName, new ColumnExpression(columnType, tableAlias, columnName)));
            }
            Expression projector = Expression.MemberInit(Expression.New(elementType), bindings);
            Type resultType = typeof(IEnumerable<>).MakeGenericType(elementType);
            return new ProjectionExpression(
                new SelectExpression(
                    resultType,
                    selectAlias,
                    columns,
                    table != null? (Expression) new TableExpression(resultType, tableAlias, GetTableName(table)) : columnExpression,
                    null
                    ),
                projector
                );
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
           if (IsTable(c.Value))
           {
               return GetTableProjection(c.Value);
           }
           return c;
        }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            Expression e;
            if (_map.TryGetValue(p, out e))
            {
                return e;
            }
            return p;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            Expression source = Visit(m.Expression);
            switch (source.NodeType)
            {
                case ExpressionType.MemberInit:
                    MemberInitExpression min = (MemberInitExpression)source;
                    for (int i = 0, n = min.Bindings.Count; i < n; i++)
                    {
                        MemberAssignment assign = min.Bindings[i] as MemberAssignment;
                        if (assign != null && MembersMatch(assign.Member, m.Member))
                        {
                            return assign.Expression;
                        }
                    }
                    break;
                case ExpressionType.New:
                    NewExpression nex = (NewExpression)source;
                    if (nex.Members != null)
                    {
                        for (int i = 0, n = nex.Members.Count; i < n; i++)
                        {
                            if (MembersMatch(nex.Members[i], m.Member))
                            {
                                return nex.Arguments[i];
                            }
                        }
                    }
                    break;
            }
            if (source == m.Expression)
            {
                return m;
            }
            return MakeMemberAccess(source, m.Member);
        }

        private bool MembersMatch(MemberInfo a, MemberInfo b)
        {
            if (a == b)
            {
                return true;
            }
            if (a is MethodInfo && b is PropertyInfo)
            {
                return a == ((PropertyInfo)b).GetGetMethod();
            }
            else if (a is PropertyInfo && b is MethodInfo)
            {
                return ((PropertyInfo)a).GetGetMethod() == b;
            }
            return false;
        }

        private Expression MakeMemberAccess(Expression source, MemberInfo mi)
        {
            FieldInfo fi = mi as FieldInfo;
            if (fi != null)
            {
                return Expression.Field(source, fi);
            }
            PropertyInfo pi = (PropertyInfo)mi;
            return Expression.Property(source, pi);
        }
    }
}
