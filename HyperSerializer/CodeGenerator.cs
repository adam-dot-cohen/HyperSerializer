using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HyperSerialize
{
    internal static class CodeGen<TSnippets>
        where TSnippets : ISnippets, new()
    {
        private static TSnippets snippets = new TSnippets();
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public;
        internal static IEnumerable<string> GetReferences<T>(bool includeUnsafe = false)
        {
            var refPaths = new List<string> {
                FrameworkAssemblyPaths.System,
                FrameworkAssemblyPaths.System_Console,
                FrameworkAssemblyPaths.System_Private_CoreLib,
                FrameworkAssemblyPaths.System_Runtime,
                typeof(T).GetTypeInfo().Assembly.Location,
            };
            if(includeUnsafe)
                refPaths.Add(
                    FrameworkAssemblyPaths.System_Runtime_CompilerServices_Unsafe);
            if (!TypeSupport.IsSupportedType<T>())
            {
                foreach (var prop in typeof(T).GetProperties(_flags).Where(h => h.CanRead && h.CanWrite && TypeSupport.IsSupportedType(h.PropertyType)))
                {
                    Type t = default;
                    if ((t = Nullable.GetUnderlyingType(prop.PropertyType)) == null)
                        t = prop.PropertyType;
                    refPaths.Add(t.Assembly.Location);
                }
            }
            return refPaths.Distinct();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static (string, string) GenerateCode<T>()
        {
            //if (Nullable.GetUnderlyingType(typeof(T)) != null)
            //    throw new Exception("Parameter 'T' must be non-nullable");
            var cType = Nullable.GetUnderlyingType(typeof(T));
            var cTypeName = cType != null ? $"{cType.Name}_Nullable" : typeof(T).Name;
            var pType = cType != null ? $"{cType.FullName}?" : typeof(T).FullName;
            var (length, serialize) = Serialize<T>();
            var (length3, deserialize) = Deserialize<T>();
            return (string.Format(snippets.ClassTemplate, cTypeName, pType.ToString().Replace("+", "."),
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
                foreach (var prop in typeof(T).GetProperties(_flags).Where(h => h.CanRead && h.CanWrite))
                {
                    if (!TypeSupport.IsSupportedType(prop.PropertyType)) continue;
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
                foreach (var prop in typeof(T).GetProperties(_flags).Where(h => h.CanRead && h.CanWrite))
                {

                    if (!TypeSupport.IsSupportedType(prop.PropertyType)) continue;
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
                sb.AppendFormat(snippets.PropertyTemplateSerialize, propertyName, nameof(Int32),
                    string.Format(snippets.StringLength, fieldName), typeof(int).SizeOf());
                offset = typeof(int).SizeOf();
                sb.AppendLine();

                //write value
                sb.AppendFormat(snippets.PropertyTemplateSerializeVarLenStr, fieldName, propertyName);
                sb.AppendLine();
                var offsetStr = $"+{string.Format(snippets.StringLengthSpan, fieldName)}";
                return (offset, offsetStr);
            }
            Type uType;
            if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
            {
                //write value

                var uTypeName = uType.FullName.Replace("+", ".");
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
                sb.AppendFormat(snippets.PropertyTemplateDeserializeVarLenStr, fieldName, propertyName);
                sb.AppendLine();
                var offsetStr = $"+{string.Format(snippets.StringLength, fieldName)}";
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
}