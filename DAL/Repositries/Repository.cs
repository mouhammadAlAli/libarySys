using Domain.Entities;
using Domain.Repositries;
using Domain.Repositries.Common;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Reflection;

namespace DAL.Repositories;

public class Repository<T> : IRepository<T> where T : BaseEntity, new()
{
    private readonly string _connectionString;

    public Repository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }

    private string TableName => typeof(T).Name + "s"; // Assumes table name matches class name

    public async Task CreateAsync(T entity)
    {
        using var connection = new SqlConnection(_connectionString);
        var properties = typeof(T).GetProperties()
            .Where(p => p.Name != "Id");
        var columns = string.Join(",", properties.Select(p => p.Name));
        var parameters = string.Join(",", properties.Select(p => $"@{p.Name}"));

        var insertCommand = $"INSERT INTO {TableName} ({columns}) VALUES ({parameters})";

        using var command = new SqlCommand(insertCommand, connection);

        foreach (var prop in properties)
        {
            command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
        }

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task CreateRangeAsync(IEnumerable<T> entities)
    {
        if (entities == null || !entities.Any())
            return;

        using var connection = new SqlConnection(_connectionString);
        var properties = typeof(T).GetProperties().Where(p => p.Name != "Id");
        var columns = string.Join(",", properties.Select(p => p.Name));

        var valuesList = new List<string>();
        var parameters = new List<SqlParameter>();
        int parameterIndex = 0;

        foreach (var entity in entities)
        {
            var valuePlaceholders = new List<string>();
            foreach (var prop in properties)
            {
                var parameterName = $"@param{parameterIndex}";
                valuePlaceholders.Add(parameterName);
                parameters.Add(new SqlParameter(parameterName, prop.GetValue(entity) ?? DBNull.Value));
                parameterIndex++;
            }
            valuesList.Add($"({string.Join(",", valuePlaceholders)})");
        }

        var insertCommand = $"INSERT INTO {TableName} ({columns}) VALUES {string.Join(",", valuesList)}";

        using var command = new SqlCommand(insertCommand, connection);
        command.Parameters.AddRange(parameters.ToArray());

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        using var connection = new SqlConnection(_connectionString);
        var properties = typeof(T).GetProperties();
        var idProperty = properties.FirstOrDefault(p => p.Name == "Id");

        if (idProperty == null)
            throw new Exception("Entity must have an 'Id' property for updates.");

        var setClause = string.Join(",", properties.Where(p => p.Name != "Id").Select(p => $"{p.Name}=@{p.Name}"));
        var updateCommand = $"UPDATE {TableName} SET {setClause} WHERE Id=@Id";

        using var command = new SqlCommand(updateCommand, connection);

        foreach (var prop in properties)
        {
            command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(entity) ?? DBNull.Value);
        }

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        using var connection = new SqlConnection(_connectionString);
        var idProperty = typeof(T).GetProperty("Id");

        if (idProperty == null)
            throw new Exception("Entity must have an 'Id' property for deletion.");

        var deleteCommand = $"DELETE FROM {TableName} WHERE Id=@Id";

        using var command = new SqlCommand(deleteCommand, connection);
        command.Parameters.AddWithValue("@Id", idProperty.GetValue(entity));

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<IEnumerable<T>> GetAllAsync(
        Expression<Func<T, bool>> filter = null,
        Dictionary<string, object> searchCriteria = null,
        params Expression<Func<T, object>>[] propertySelectors)
    {
        var result = new List<T>();

        using var connection = new SqlConnection(_connectionString);

        var selectColumns = propertySelectors.Any()
            ? string.Join(",", propertySelectors.Select(s => ((MemberExpression)s.Body).Member.Name))
            : "*";

        var selectCommand = $"SELECT {selectColumns} FROM {TableName}";

        var conditions = new List<string>();
        var parameters = new Dictionary<string, object>();

        if (filter != null)
        {
            var visitor = new SqlExpressionVisitor();
            visitor.Visit(filter);

            if (!string.IsNullOrEmpty(visitor.Sql))
            {
                conditions.Add(visitor.Sql);
                foreach (var param in visitor.Parameters)
                {
                    parameters[param.Key] = param.Value;
                }
            }
        }

        if (searchCriteria != null && searchCriteria.Any())
        {
            foreach (var kvp in searchCriteria)
            {
                var columnName = kvp.Key;
                var searchValue = kvp.Value;

                var parameterName = $"@search_{columnName}";
                conditions.Add($"{columnName} LIKE {parameterName}");
                parameters[parameterName] = $"%{searchValue}%";
            }
        }

        if (conditions.Any())
        {
            selectCommand += " WHERE " + string.Join(" AND ", conditions);
        }

        using var command = new SqlCommand(selectCommand, connection);
        foreach (var param in parameters)
        {
            command.Parameters.AddWithValue(param.Key, param.Value);
        }

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var entity = Activator.CreateInstance<T>();
            foreach (var property in typeof(T).GetProperties())
            {
                if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                {
                    property.SetValue(entity, reader[property.Name]);
                }
            }
            result.Add(entity);
        }

        return result;
    }

    public async Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter = null, params Expression<Func<T, object>>[] propertySelectors)
    {
        var result = (await GetAllAsync(filter, null, propertySelectors)).FirstOrDefault();
        return result;
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> filter)
    {
        var result = (await GetAllAsync(filter)).Any();
        return result;
    }

    public async Task<PageResult<T>> GetPageAsync(
        PageRequest pageRequest,
        Expression<Func<T, bool>> filter = null,
        Dictionary<string, object> searchCriteria = null,
        params Expression<Func<T, object>>[] propertySelectors)
    {
        var result = new List<T>();
        int totalCount;

        using (var connection = new SqlConnection(_connectionString))
        {
            var selectColumns = propertySelectors.Any()
                ? string.Join(",", propertySelectors.Select(s => ((MemberExpression)s.Body).Member.Name))
                : "*";

            var selectCommand = $"SELECT {selectColumns} FROM {TableName}";
            var countCommand = $"SELECT COUNT(*) FROM {TableName}";

            var conditions = new List<string>();
            var parameters = new Dictionary<string, object>();

            if (filter != null)
            {
                var visitor = new SqlExpressionVisitor();
                visitor.Visit(filter);

                if (!string.IsNullOrEmpty(visitor.Sql))
                {
                    conditions.Add(visitor.Sql);
                    foreach (var param in visitor.Parameters)
                    {
                        parameters[param.Key] = param.Value;
                    }
                }
            }

            if (searchCriteria != null && searchCriteria.Any())
            {
                foreach (var kvp in searchCriteria)
                {
                    var columnName = kvp.Key;
                    var searchValue = kvp.Value;

                    var parameterName = $"@search_{columnName}";
                    conditions.Add($"{columnName} LIKE {parameterName}");
                    parameters[parameterName] = $"%{searchValue}%";
                }
            }

            if (conditions.Any())
            {
                var whereClause = " WHERE " + string.Join(" AND ", conditions);
                selectCommand += whereClause;
                countCommand += whereClause;
            }

            selectCommand += $"\nORDER BY Id\nOFFSET @Offset ROWS\nFETCH NEXT @PageSize ROWS ONLY;";

            using (var command = new SqlCommand(selectCommand, connection))
            using (var countCmd = new SqlCommand(countCommand, connection))
            {
                command.Parameters.AddWithValue("@Offset", (pageRequest.Page - 1) * pageRequest.Rows);
                command.Parameters.AddWithValue("@PageSize", pageRequest.Rows);

                foreach (var param in parameters)
                {
                    command.Parameters.AddWithValue(param.Key, param.Value);
                    countCmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                await connection.OpenAsync();

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var entity = Activator.CreateInstance<T>();
                        foreach (var property in typeof(T).GetProperties())
                        {
                            if (!reader.IsDBNull(reader.GetOrdinal(property.Name)))
                            {
                                property.SetValue(entity, reader[property.Name]);
                            }
                        }
                        result.Add(entity);
                    }
                }

                totalCount = (int)await countCmd.ExecuteScalarAsync();
            }
        }

        return new PageResult<T>
        {
            Data = result,
            TotalRecords = totalCount
        };
    }
}

public class SqlExpressionVisitor : ExpressionVisitor
{
    public string Sql { get; private set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

    private int _parameterIndex = 0;

    protected override Expression VisitBinary(BinaryExpression node)
    {
        Sql += "(";
        Visit(node.Left);

        string operatorSql = node.NodeType switch
        {
            ExpressionType.Equal => "=",
            ExpressionType.NotEqual => "!=",
            ExpressionType.GreaterThan => ">",
            ExpressionType.GreaterThanOrEqual => ">=",
            ExpressionType.LessThan => "<",
            ExpressionType.LessThanOrEqual => "<=",
            ExpressionType.AndAlso => "AND",
            ExpressionType.OrElse => "OR",
            _ => throw new NotImplementedException($"Operator {node.NodeType} is not supported.")
        };

        Sql += $" {operatorSql} ";
        Visit(node.Right);
        Sql += ")";
        return node;
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
        {
            // Handles simple property access like "book.IsAvailable"
            Sql += node.Member.Name;
            return node;
        }

        if (node.Expression != null && node.Expression.NodeType == ExpressionType.Constant)
        {
            // Handles captured variables like "bookPageRequest.IsAvailable"
            var constantExpression = (ConstantExpression)node.Expression;
            var container = constantExpression.Value;
            var value = GetMemberValue(node.Member, container);

            var parameterName = $"@param{_parameterIndex++}";
            Parameters[parameterName] = value ?? DBNull.Value;
            Sql += parameterName;
            return node;
        }

        if (node.Expression != null && node.Expression.NodeType == ExpressionType.MemberAccess)
        {
            // Handles nested member access
            Visit(node.Expression);
            Sql += $".{node.Member.Name}";
            return node;
        }

        throw new NotImplementedException($"Unsupported MemberExpression: {node}");
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        // Generate a unique parameter name
        var parameterName = $"@param{_parameterIndex++}";
        Parameters[parameterName] = node.Value ?? DBNull.Value;
        Sql += parameterName;
        return node;
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        if (node.NodeType == ExpressionType.Convert)
        {
            // Handles conversions like (int)book.SomeProperty
            Visit(node.Operand);
            return node;
        }

        throw new NotImplementedException($"Unsupported UnaryExpression: {node}");
    }

    protected override Expression VisitLambda<T>(Expression<T> node)
    {
        Visit(node.Body);
        return node;
    }

    /// <summary>
    /// Resets the state of the visitor for reuse.
    /// </summary>
    public void Reset()
    {
        Sql = string.Empty;
        Parameters.Clear();
        _parameterIndex = 0;
    }

    /// <summary>
    /// Retrieves the value of a field or property from its container.
    /// </summary>
    private object GetMemberValue(MemberInfo member, object container)
    {
        return member switch
        {
            FieldInfo field => field.GetValue(container),
            PropertyInfo property => property.GetValue(container),
            _ => throw new InvalidOperationException($"Unsupported member type: {member.GetType().Name}")
        };
    }
}

