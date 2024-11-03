using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Dapper;

namespace Heronest.Internal.Database;

public class SnakeCaseColumnNameMapper : SqlMapper.ITypeMap
{
    private readonly Dictionary<string, string> _columnMappings = new();
    private readonly Type _type;

    public SnakeCaseColumnNameMapper(Type type)
    {
        _type = type;
        foreach (var prop in type.GetProperties())
        {
            var columnAttr = prop.GetCustomAttribute<ColumnAttribute>();
            if (columnAttr != null)
            {
                _columnMappings[columnAttr.Name!] = prop.Name;
            }
            else
            {
                // Convert property name to snake_case if no Column attribute
                var snakeCaseName = string.Concat(
                        prop.Name.Select(
                            (x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString()
                        )
                    )
                    .ToLower();
                _columnMappings[snakeCaseName] = prop.Name;
            }
        }
    }

    public ConstructorInfo? FindConstructor(string[] names, Type[] types) =>
        _type.GetConstructors().FirstOrDefault();

    public ConstructorInfo? FindExplicitConstructor() => null;

    public SqlMapper.IMemberMap? GetConstructorParameter(
        ConstructorInfo constructor,
        string columnName
    )
    {
        return null;
    }

    public SqlMapper.IMemberMap? GetMember(string columnName)
    {
        if (_columnMappings.TryGetValue(columnName, out var propertyName))
        {
            return new SimpleMemberMap(columnName, _type.GetProperty(propertyName)!);
        }
        return null;
    }
}

public class SimpleMemberMap : SqlMapper.IMemberMap
{
    private readonly string _columnName;
    private readonly PropertyInfo _property;

    public SimpleMemberMap(string columnName, PropertyInfo property)
    {
        _columnName = columnName;
        _property = property;
    }

    public string ColumnName => _columnName;
    public Type MemberType => _property.PropertyType;
    public PropertyInfo Property => _property;
    public FieldInfo Field => null!;
    public ParameterInfo Parameter => null!;
}
