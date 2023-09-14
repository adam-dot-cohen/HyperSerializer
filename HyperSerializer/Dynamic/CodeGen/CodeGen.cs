using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Hyper;
using HyperSerializer.Dynamic.CodeGen.Snippets;
using HyperSerializer.Utilities;
using Microsoft.CodeAnalysis;

namespace HyperSerializer.Dynamic.CodeGen;

internal static class CodeGen<TSnippets>
    where TSnippets : ISnippetsSafeV3, new()
{
    private static TSnippets snippets = new();
    private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public;
    internal static IEnumerable<PortableExecutableReference> GetReferences<T>(bool includeUnsafe = false)
    {
        var refPaths = new List<PortableExecutableReference> {
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Console),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
            MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(T).GetTypeInfo().Assembly.Location),
        };
        if (includeUnsafe)
            refPaths.Add(
                MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime_CompilerServices_Unsafe));



        if (!TypeSupport.IsSupportedType<T>())
        {
            var props = typeof(T).GetProperties(_flags).ToList();
#if NET5_0_OR_GREATER
            foreach (var prop in CollectionsMarshal.AsSpan(props))
            {
#else
            foreach (var prop in props)
            {
#endif
                if (!(prop.CanRead && prop.CanWrite && TypeSupport.IsSupportedType(prop.PropertyType)))
                    continue;
                Type t = default;
                if ((t = Nullable.GetUnderlyingType(prop.PropertyType)) == null)
                    t = prop.PropertyType;
                refPaths.Add(MetadataReference.CreateFromFile(t.Assembly.Location));
            }
        }
        return refPaths;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static (string Code, string ClassName) GenerateCode<T>()
    {
        var cType = Nullable.GetUnderlyingType(typeof(T));
        string cTypeName = typeof(T).GetClassName<T>();
        var pType = cType != null ? $"{cType.Namespace}.{cTypeName}?" : typeof(T).FullName;
        var (length, serialize) = Serialize<T>();
        var (length3, deserialize) = Deserialize<T>();
        return (string.Format(snippets.ClassTemplate, cTypeName, pType.Replace("+", "."),
            length, serialize, deserialize, TypeSupport.IsSupportedType<T>() ? "default" : "new()"), cTypeName);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (string, string) Serialize<T>()
    {
        var offset = 0;
        var offsetStr = string.Empty;
        var sb = new StringBuilder();
        if (TypeSupport.IsSupportedType<T>())
            (offset, offsetStr) = GenerateSerializer<T>(sb);
        else
        {
            var props = typeof(T).GetProperties(_flags).ToList();
#if NET5_0_OR_GREATER
            foreach (var prop in CollectionsMarshal.AsSpan(props))
            {
#else
            foreach (var prop in props)
            {
#endif
                if (!(prop.CanRead && prop.CanWrite && TypeSupport.IsSupportedType(prop.PropertyType)))
                    continue;
                var (len, str) = GenerateSerializer<T>(sb, "obj", prop);
                offset += len;
                offsetStr += str;
            }
        }
        return ($"{offset}{offsetStr}", sb.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static (string, string) Deserialize<T>()
    {
        var offset = 0;
        var offsetStr = string.Empty;
        var sb = new StringBuilder();
        if (TypeSupport.IsSupportedType<T>())
            (offset, offsetStr) = GenerateDeserializer<T>(sb);
        else
        {

            var props = typeof(T).GetProperties(_flags).ToList();
#if NET5_0_OR_GREATER
            foreach (var prop in CollectionsMarshal.AsSpan(props))
            {
#else
            foreach (var prop in props)
            {
#endif
                if (!(prop.CanRead && prop.CanWrite && TypeSupport.IsSupportedType(prop.PropertyType)))
                    continue;
                var (len, str) = GenerateDeserializer<T>(sb, "obj", prop);
                offset += len;
                offsetStr += str;
            }
        }
        return ($"{offset}{offsetStr}", sb.ToString());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int, string) GenerateSerializer<T>(StringBuilder sb, string parameterName = "obj", PropertyInfo propertyType = null)
    {
        var offset = 0;
        var fieldName = $"{parameterName}" + (propertyType != null ? $".{propertyType.Name}" : string.Empty);
        var propertyName = propertyType != null ? $"{propertyType.Name}" : parameterName;
        var type = propertyType?.PropertyType ?? typeof(T);
        var typeName = $"{(Nullable.GetUnderlyingType(type) ?? type).FullName.Replace("+", ".")}";

        if (type == typeof(string))
        {
            //write length
            sb.AppendFormat(snippets.PropertyTemplateSerializeArrLen, propertyName, fieldName, typeof(char));
            offset = typeof(int).SizeOf();
            sb.AppendLine();

            //write value
            sb.AppendFormat(snippets.PropertyTemplateSerializeVarLenStr, fieldName, propertyName, typeof(char));
            sb.AppendLine();
            var offsetStr = $"+({fieldName}?.Length ?? 0)*Unsafe.SizeOf<{typeof(char)}>()";
            return (offset, offsetStr);
        }
        if (type == typeof(IEnumerable<>) && type.GetElementType()!.IsValueType)
        {
            //write length
            sb.AppendFormat(snippets.PropertyTemplateSerializeListLen, propertyName, fieldName, type.GetElementType().FullName);
            offset = typeof(int).SizeOf();
            sb.AppendLine();

            //write value
            sb.AppendFormat(snippets.PropertyTemplateSerializeVarLenArr, fieldName, propertyName, type.GetElementType().FullName);
            sb.AppendLine();
            var offsetStr = $"+({fieldName}?.Count() ?? 0)*Unsafe.SizeOf<{type.GetElementType().FullName}>()";
            return (offset, offsetStr);
        }
        if (type.IsArray && type.GetElementType()!.IsValueType)
        {
            //write length
            sb.AppendFormat(snippets.PropertyTemplateSerializeArrLen, propertyName, fieldName, type.GetElementType().FullName);
            offset = typeof(int).SizeOf();
            sb.AppendLine();

            //write value
            sb.AppendFormat(snippets.PropertyTemplateSerializeVarLenArr, fieldName, propertyName, type.GetElementType().FullName);
            sb.AppendLine();
            var offsetStr = $"+({fieldName}?.Length ?? 0)*Unsafe.SizeOf<{type.GetElementType().FullName}>()";
            return (offset, offsetStr);
        }

        Type uType;
        if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
        {
            //write value
            var uTypeName = uType.FullName?.Replace("+", ".");
            sb.AppendFormat(snippets.PropertyTemplateSerializeNullable, propertyName, fieldName, uType.SizeOf(), uType);
            offset += uType.SizeOf() + 1;
            sb.AppendLine();

            return (offset, string.Empty);
        }
        //write non-nullable value
        sb.AppendFormat(snippets.PropertyTemplateSerialize, propertyName, typeName,
            fieldName, offset = type.SizeOf());
        sb.AppendLine();
        return (offset, string.Empty);

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static (int, string) GenerateDeserializer<T>(StringBuilder sb, string parameterName = "obj", PropertyInfo propertyType = null)
    {
        var offset = 0;
        var fieldName = $"{parameterName}" + (propertyType != null ? $".{propertyType.Name}" : string.Empty);
        var propertyName = propertyType != null ? $"{propertyType.Name}" : parameterName;
        var type = propertyType?.PropertyType ?? typeof(T);
        var typeName = $"{(Nullable.GetUnderlyingType(type) ?? type).FullName.Replace("+", ".")}";

        if (type == typeof(string))
        {
            //write length
            sb.AppendFormat(snippets.PropertyTemplateDeserializeLocal, propertyName, nameof(Int32), offset = typeof(int).SizeOf());
            sb.AppendLine();

            //write value
            sb.AppendFormat(snippets.PropertyTemplateDeserializeVarLenStr, fieldName, propertyName, typeof(char));
            sb.AppendLine();
            var offsetStr = $"+({fieldName}?.Length ?? 0)*Unsafe.SizeOf<{typeof(char)}>()";
            return (offset, offsetStr);
        }

        if (type == typeof(IEnumerable<>) && type.GenericTypeArguments.FirstOrDefault()!.IsValueType)
        {
            //write length
            sb.AppendFormat(snippets.PropertyTemplateDeserializeLocal, propertyName, nameof(Int32), offset = typeof(int).SizeOf());
            sb.AppendLine();

            //write value
            sb.AppendFormat(snippets.PropertyTemplateDeserializeVarLenList, fieldName, propertyName, type.GetGenericArguments()[0].FullName);
            sb.AppendLine();
            var offsetStr = $"+({fieldName}?.Count() ?? 0)*Unsafe.SizeOf<{type.GetElementType().FullName}>()";
            return (offset, offsetStr);
        }

        if (type == typeof(IEnumerable) || type.IsArray)
        {
            //write length
            sb.AppendFormat(snippets.PropertyTemplateDeserializeLocal, propertyName, nameof(Int32), offset = typeof(int).SizeOf());
            sb.AppendLine();

            //write value
            sb.AppendFormat(snippets.PropertyTemplateDeserializeVarLenArr, fieldName, propertyName, type.GetElementType().FullName);
            sb.AppendLine();
            var offsetStr = $"+({fieldName}?.Length ?? 0)*Unsafe.SizeOf<{type.GetElementType().FullName}>()";
            return (offset, offsetStr);
        }
        Type uType = null;
        if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
        {
            //write value
            var uTypeName = uType.FullName.Replace("+", ".");
            sb.AppendFormat(snippets.PropertyTemplateDeserializeNullable, fieldName, uTypeName, uType.SizeOf());
            offset += uType.SizeOf() + 1;
            sb.AppendLine();
            return (offset, string.Empty);
        }
        //write non-nullable value
        sb.AppendFormat(snippets.PropertyTemplateDeserialize, fieldName, typeName, offset = type.SizeOf());
        sb.AppendLine();
        return (offset, string.Empty);
    }
}