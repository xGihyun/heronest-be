using System.Reflection;
using Dapper;

namespace Heronest.Features.Database;

[AttributeUsage(AttributeTargets.Class)]
public class SqlMapperAttribute : Attribute
{
    public CaseType CaseType { get; }

    public SqlMapperAttribute(CaseType caseType = CaseType.SnakeCase)
    {
        CaseType = caseType;
    }
}

public enum CaseType
{
    SnakeCase,
    CamelCase,
}

public static class SqlMapperConfig
{
    public static void ConfigureMappers(Assembly assembly)
    {
        var types = assembly
            .GetTypes()
            .Where(t => t.GetCustomAttribute<SqlMapperAttribute>() != null);

        foreach (var type in types)
        {
            var attribute = type.GetCustomAttribute<SqlMapperAttribute>();
            switch (attribute?.CaseType)
            {
                case CaseType.SnakeCase:
                    SqlMapper.SetTypeMap(type, new SnakeCaseColumnNameMapper(type));
                    break;
            }
        }
    }
}
