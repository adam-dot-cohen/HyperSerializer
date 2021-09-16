using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Hyperlnq.Serializer
{
    internal static class HyperSerializerGenerator
    {
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public;
        internal static IEnumerable<string> GetReferences<T>()
        {
            var refPaths = new List<string> {
                FrameworkAssemblyPaths.System,
                FrameworkAssemblyPaths.System_Private_CoreLib,
                FrameworkAssemblyPaths.System_Runtime,
                typeof(T).GetTypeInfo().Assembly.Location,
            };
            if (HyperSerializerTypeSupport.IsSupportedType<T>())
                refPaths.Add(typeof(T).Assembly.Location);
            else
            {
                foreach (var prop in typeof(T).GetProperties(_flags).Where(h => h.CanRead && h.CanWrite))
                {
                    Type t = default;
                    if ((t = Nullable.GetUnderlyingType(prop.PropertyType)) == null)
                        t = prop.PropertyType;
                    refPaths.Add(t.Assembly.Location);
                }
            }
            return refPaths.Distinct();
        }
        internal static (string, string) Serialize<T>()
        {
            var offset = 0;
            var offsetStr = string.Empty;
            var sb = new StringBuilder();
            if (HyperSerializerTypeSupport.IsSupportedType<T>())
                (offset, offsetStr) = GenerateSerializer<T>(sb);
            else
            {
                foreach (var prop in typeof(T).GetProperties(_flags).Where(h => h.CanRead && h.CanWrite))
                {
                    if (!HyperSerializerTypeSupport.IsSupportedType(prop.PropertyType)) continue;
                    var (len, str) = GenerateSerializer<T>(sb, "obj", prop);
                    offset += len;
                    offsetStr += str;
                }
            }
            return ($"{offset}{offsetStr}", sb.ToString());
        }
        public static (string, string) Deserialize<T>()
        {
            var offset = 0;
            var offsetStr = string.Empty;
            var sb = new StringBuilder();
            if (HyperSerializerTypeSupport.IsSupportedType<T>())
                (offset, offsetStr) = GenerateDeserializer<T>(sb);
            else
            {
                foreach (var prop in typeof(T).GetProperties(_flags).Where(h => h.CanRead && h.CanWrite))
                {

                    if (!HyperSerializerTypeSupport.IsSupportedType(prop.PropertyType)) continue;
                    var (len, str) = GenerateDeserializer<T>(sb, "obj", prop);
                    offset += len;
                    offsetStr += str;
                }
            }
            return ($"{offset}{offsetStr}", sb.ToString());
        }
        private static (int, string) GenerateSerializer<T>(StringBuilder sb, string parameterName = "obj", PropertyInfo propertyType = null)
        {
            var offset = 0;
            var fieldName = $"{parameterName}" + (propertyType != null ? $".{propertyType.Name}" : string.Empty);
            var propertyName = (propertyType != null ? $"{propertyType.Name}" : parameterName);
            var type = propertyType?.PropertyType ?? typeof(T);
            var typeName = $"{(Nullable.GetUnderlyingType(type) ?? type).FullName.Replace("+", ".")}";

            if (type == typeof(string))
            {
                //write length
                sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateSerialize, propertyName, nameof(Int32), 
                    string.Format(HyperSerializerCodeSnippets.StringLength, fieldName), typeof(int).SizeOf());
                offset = typeof(int).SizeOf();
                sb.AppendLine();

                //write value
                sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateSerializeVarLenStr, fieldName, propertyName);
                sb.AppendLine();
                var offsetStr = $"+{string.Format(HyperSerializerCodeSnippets.StringLength, fieldName)}";
                return (offset, offsetStr);
            }
            Type uType = null;
            if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
            {
                //write value
                sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateSerializeNullable, propertyName, 
                    parameterName, propertyName, uType.SizeOf());
                offset += uType.SizeOf() + 1;
                sb.AppendLine();

                return (offset, string.Empty);
            }
            //write non-nullable value
            sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateSerialize, propertyName, typeName, 
                fieldName, offset = type.SizeOf());
            sb.AppendLine();
            return (offset, string.Empty);

        }
        private static (int, string) GenerateDeserializer<T>(StringBuilder sb, string parameterName = "obj", PropertyInfo propertyType = null)
        {
            var offset = 0;

            var fieldName = $"{parameterName}" + (propertyType != null ? $".{propertyType.Name}" : string.Empty);
            var propertyName = (propertyType != null ? $"{propertyType.Name}" : parameterName);
            var type = propertyType?.PropertyType ?? typeof(T);
            var typeName = $"{(Nullable.GetUnderlyingType(type) ?? type).FullName.Replace("+", ".")}";

            if (type == typeof(string))
            {
                //write length
                sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateDeserializeLocal, propertyName, nameof(Int32), 
                    nameof(Int32), offset = typeof(int).SizeOf());
                sb.AppendLine();

                //write value
                sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateDeserializeVarLenStr, fieldName, propertyName);
                sb.AppendLine();
                var offsetStr = $"+{string.Format(HyperSerializerCodeSnippets.StringLength, fieldName)}";
                return (offset, offsetStr);
            }
            Type uType = null;
            if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
            {
                //write value
                var uTypeName = uType.FullName.Replace("+", ".");
                sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateDeserializeNullable, fieldName, 
                    typeName, typeName, uType.SizeOf());
                offset += uType.SizeOf() + 1;
                sb.AppendLine();
                return (offset, string.Empty);
            }
            //write non-nullable value
            sb.AppendFormat(HyperSerializerCodeSnippets.PropertyTemplateDeserialize, fieldName, typeName, 
                typeName, offset = type.SizeOf());
            sb.AppendLine();
            return (offset, string.Empty);

        }
    }

    internal static class HyperSerializerGeneratorV2
    {
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public;
        internal static IEnumerable<string> GetReferences<T>()
        {
            var refPaths = new List<string> {
                FrameworkAssemblyPaths.System,
                FrameworkAssemblyPaths.System_Private_CoreLib,
                FrameworkAssemblyPaths.System_Runtime,
                FrameworkAssemblyPaths.System_Runtime_CompilerServices_Unsafe,
                typeof(T).GetTypeInfo().Assembly.Location,
            };
            if (HyperSerializerTypeSupport.IsSupportedType<T>())
                refPaths.Add(typeof(T).Assembly.Location);
            else
            {
                foreach (var prop in typeof(T).GetProperties(_flags))
                {
                    Type t = default;
                    if ((t = Nullable.GetUnderlyingType(prop.PropertyType)) == null)
                        t = prop.PropertyType;
                    refPaths.Add(t.Assembly.Location);
                }
            }
            return refPaths.Distinct();
        }
        internal static (string, string) Serialize<T>()
        {
            var offset = 0;
            var offsetStr = string.Empty;
            var sb = new StringBuilder();
            if (HyperSerializerTypeSupport.IsSupportedType<T>())
                (offset, offsetStr) = GeneratePropertySerializer<T>(sb);
            else
            {
                foreach (var prop in typeof(T).GetProperties(_flags))
                {
                    if (!HyperSerializerTypeSupport.IsSupportedType(prop.PropertyType)) continue;
                    var (len, str) = GeneratePropertySerializer<T>(sb, "obj", prop);
                    offset += len;
                    offsetStr += str;
                }
            }
            return ($"{offset}{offsetStr}", sb.ToString());
        }
        internal static (string, string) Deserialize<T>()
        {
            var offset = 0;
            var offsetStr = string.Empty;
            var sb = new StringBuilder();
            if (HyperSerializerTypeSupport.IsSupportedType<T>())
                (offset, offsetStr) = GenerateDeserializer<T>(sb);
            else
            {
                foreach (var prop in typeof(T).GetProperties(_flags))
                {

                    if (!HyperSerializerTypeSupport.IsSupportedType(prop.PropertyType)) continue;
                    var (len, str) = GenerateDeserializer<T>(sb, "obj", prop);
                    offset += len;
                    offsetStr += str;
                }
            }
            return ($"{offset}{offsetStr}", sb.ToString());
        }
        private static (int, string) GeneratePropertySerializer<T>(StringBuilder sb, string parameterName = "obj", PropertyInfo propertyType = null)
        {
            var offset = 0;
            var fieldName = $"{parameterName}" + (propertyType != null ? $".{propertyType.Name}" : string.Empty);
            var propertyName = (propertyType != null ? $"{propertyType.Name}" : parameterName);
            var type = propertyType?.PropertyType ?? typeof(T);
            var typeName = $"{(Nullable.GetUnderlyingType(type) ?? type).FullName.Replace("+", ".")}";

            if (type == typeof(string))
            {
                //write length
                sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateSerialize, propertyName, nameof(Int32), 
                    string.Format(HyperSerializerCodeSnippetsV2.StringLength, fieldName), typeof(int).SizeOf());
                offset = typeof(int).SizeOf();
                sb.AppendLine();

                //write value
                sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateSerializeVarLenStr, fieldName, propertyName);
                sb.AppendLine();
                var offsetStr = $"+{string.Format(HyperSerializerCodeSnippetsV2.StringLength, fieldName)}";
                return (offset, offsetStr);
            }
            Type uType;
            if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
            {
                //write value
                sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateSerializeNullable, propertyName, 
                    parameterName, propertyName, uType.SizeOf(), typeName);
                offset += uType.SizeOf() + 1;
                sb.AppendLine();

                return (offset, string.Empty);
            }
            //write non-nullable value
            sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateSerialize, propertyName, typeName, 
                fieldName, offset = type.SizeOf());
            sb.AppendLine();
            return (offset, string.Empty);

        }
        private static (int, string) GenerateDeserializer<T>(StringBuilder sb, string parameterName = "obj", PropertyInfo propertyType = null)
        {
            var offset = 0;
            var fieldName = $"{parameterName}" + (propertyType != null ? $".{propertyType.Name}" : string.Empty);
            var propertyName = (propertyType != null ? $"{propertyType.Name}" : parameterName);
            var type = propertyType?.PropertyType ?? typeof(T);
            var typeName = $"{(Nullable.GetUnderlyingType(type) ?? type).FullName.Replace("+", ".")}";

            if (type == typeof(string))
            {
                //write length
                sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateDeserializeLocal, propertyName, nameof(Int32), nameof(Int32), offset = typeof(int).SizeOf());
                sb.AppendLine();

                //write value
                sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateDeserializeVarLenStr, fieldName, propertyName);
                sb.AppendLine();
                var offsetStr = $"+{string.Format(HyperSerializerCodeSnippetsV2.StringLength, fieldName)}";
                return (offset, offsetStr);
            }
            Type uType = null;
            if (type.IsGenericType && (uType = Nullable.GetUnderlyingType(type)) != null)
            {
                //write value
                var uTypeName = uType.FullName.Replace("+", ".");
                sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateDeserializeNullable, fieldName,  typeName, uType.SizeOf());
                offset += uType.SizeOf() + 1;
                sb.AppendLine();
                return (offset, string.Empty);
            }
            //write non-nullable value
            sb.AppendFormat(HyperSerializerCodeSnippetsV2.PropertyTemplateDeserialize, fieldName, typeName, offset = type.SizeOf());
            sb.AppendLine();
            return (offset, string.Empty);

        }

    }
}