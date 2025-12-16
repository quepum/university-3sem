// <copyright file="Reflector.cs" author="Alina Letyagina">
// under MIT License.
// </copyright>

namespace MyReflector;

using System.Reflection;
using System.Text;

/// <summary>
/// Provides reflection-based utilities for analyzing and generating class structures.
/// </summary>
public class Reflector
{
    /// <summary>
    /// Generates a .cs file with a compilable stub of the given type.
    /// </summary>
    /// <param name="type">The type to reflect and generate code for.</param>
    public void PrintStructure(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var code = BuildClass(type);
        var fileName = $"{type.Name}.cs";
        File.WriteAllText(fileName, code);
    }

    /// <summary>
    /// Compares two types and returns members only in A, only in B, and differing common members.
    /// </summary>
    /// <param name="a">The first type to compare.</param>
    /// <param name="b">The second type to compare.</param>
    /// <returns>
    /// A tuple containing:
    /// <list type="bullet">
    ///   <item><description><c>OnlyInA</c>: members present in <paramref name="a"/> but not in <paramref name="b"/>.</description></item>
    ///   <item><description><c>OnlyInB</c>: members present in <paramref name="b"/> but not in <paramref name="a"/>.</description></item>
    ///   <item><description><c>DifferentInBoth</c>: members with the same signature key (e.g., same field name or same method name and parameter types) but different full declarations (e.g., different return type, access modifier, etc.).</description></item>
    /// </list>
    /// </returns>
    public (List<string> OnlyInA, List<string> OnlyInB, List<string> DifferentInBoth) DiffClasses(Type a, Type b)
    {
        if (a == null || b == null)
        {
            throw new ArgumentNullException();
        }

        var membersA = GetMemberSignatures(a);
        var membersB = GetMemberSignatures(b);

        var onlyInA = new List<string>();

        var onlyInB = (
            from key in membersB.Keys
            where !membersA.ContainsKey(key)
            select membersB[key]).ToList();

        var different = new List<string>();

        foreach (var key in membersA.Keys)
        {
            if (!membersB.TryGetValue(key, out var value))
            {
                onlyInA.Add(membersA[key]);
            }
            else if (membersA[key] != value)
            {
                different.Add($"A: {membersA[key]} | B: {value}");
            }
        }

        return (onlyInA, onlyInB, different);
    }

    /// <summary>
    /// Returns list of all declared members as readable strings.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    /// <returns>A list of member signatures as strings.</returns>
    public List<string> GetMemberList(Type type)
    {
        return GetMemberSignatures(type).Values.ToList();
    }

    private static Dictionary<string, string> GetMemberSignatures(Type type)
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static |
                                   BindingFlags.DeclaredOnly;

        var members = new Dictionary<string, string>();

        foreach (var field in type.GetFields(flags))
        {
            var access = GetAccessModifier(field);
            var isStatic = field.IsStatic ? " static" : string.Empty;
            var signature = $"{access}{isStatic} {field.FieldType.Name} {field.Name}";
            members[field.Name] = signature;
        }

        foreach (var method in type.GetMethods(flags))
        {
            if (method.IsSpecialName || method.IsConstructor)
            {
                continue;
            }

            var access = GetAccessModifier(method);
            var isStatic = method.IsStatic ? " static" : string.Empty;
            var parameters = string.Join(", ", method.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}"));
            var signature = $"{access}{isStatic} {method.ReturnType.Name} {method.Name}({parameters})";

            var paramTypes = string.Join(",", method.GetParameters()
                .Select(p => p.ParameterType.FullName ?? p.ParameterType.Name));
            var key = $"{method.Name}({paramTypes})";

            members[key] = signature;
        }

        foreach (var nested in type.GetNestedTypes(flags))
        {
            members[$"nested:{nested.Name}"] = $"nested class {nested.Name}";
        }

        return members;
    }

    private static string GetAccessModifier(MemberInfo member)
    {
        return member switch
        {
            FieldInfo f => f.IsPublic ? "public" :
                f.IsPrivate ? "private" :
                f.IsFamily ? "protected" : "internal",
            MethodInfo m => m.IsPublic ? "public" :
                m.IsPrivate ? "private" :
                m.IsFamily ? "protected" : "internal",
            _ => "internal",
        };
    }

    private static string BuildClass(Type type)
    {
        var sb = new StringBuilder();

        var access = type.IsPublic ? "public" : "internal";
        var kind = type.IsInterface ? "interface" :
            type.IsValueType ? "struct" : "class";

        sb.AppendLine($"{access} {kind} {type.Name}");
        sb.AppendLine("{");

        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic |
                                   BindingFlags.Instance | BindingFlags.Static |
                                   BindingFlags.DeclaredOnly;

        foreach (var field in type.GetFields(flags))
        {
            var accessMod = GetAccessModifier(field);
            var isStatic = field.IsStatic ? " static" : string.Empty;
            sb.AppendLine($"    {accessMod}{isStatic} {field.FieldType.Name} {field.Name};");
        }

        foreach (var method in type.GetMethods(flags))
        {
            if (method.IsSpecialName || method.IsConstructor)
            {
                continue;
            }

            var accessMod = GetAccessModifier(method);
            var isStatic = method.IsStatic ? " static" : string.Empty;
            var parameters = string.Join(", ", method.GetParameters()
                .Select(p => $"{p.ParameterType.Name} {p.Name}"));
            var body = method.ReturnType == typeof(void)
                ? " { }"
                : $" {{ return default; }}";

            sb.AppendLine($"    {accessMod}{isStatic} {method.ReturnType.Name} {method.Name}({parameters}){body}");
        }

        sb.AppendLine("}");
        return sb.ToString();
    }
}