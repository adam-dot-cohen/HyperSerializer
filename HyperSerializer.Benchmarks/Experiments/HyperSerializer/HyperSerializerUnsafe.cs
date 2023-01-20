using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using HyperSerializer.CodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace HyperSerializer.Benchmarks.Experiments.HyperSerializer
{
    public static class HyperSerializerUnsafe<T>
    {
        private static string generatedCode;
        private static string _proxyTypeName;
        private static Type _proxyType;
        private static CSharpCompilation _compilation;
        private static Assembly _generatedAssembly;
        internal delegate Span<byte> Serializer(T obj);
        internal delegate T Deserializer(ReadOnlySpan<byte> bytes);
        internal static Serializer SerializeDynamic;
        internal static Deserializer DeserializeDynamic;

        static HyperSerializerUnsafe()
            => Compile();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Serialize(T obj)
            => SerializeDynamic(obj);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize(ReadOnlySpan<byte> bytes)
            => DeserializeDynamic(bytes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Memory<byte>> SerializeAsync(T obj)
            => new ValueTask<Memory<byte>>(Serialize(obj).ToArray());
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<T> DeserializeAsync(ReadOnlyMemory<byte> bytes)
            => new ValueTask<T>(Deserialize(bytes.Span));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BuildDelegates()
        {
#if NET5_0_OR_GREATER
            var infos = _proxyType.GetMethod("Serialize");
            SerializeDynamic = infos.CreateDelegate<Serializer>();

            var infod = _proxyType.GetMethod("Deserialize");
            DeserializeDynamic = infod.CreateDelegate<Deserializer>();
#else
            var infos = _proxyType.GetMethod("Serialize");
            SerializeDynamic = (Serializer)infos.CreateDelegate(typeof(Serializer));

            var infod = _proxyType.GetMethod("Deserialize");
            DeserializeDynamic = (Deserializer)infod.CreateDelegate(typeof(Deserializer));
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Compile()
        {
            var result = CodeGen<SnippetsUnsafe>.GenerateCode<T>();
            generatedCode = result.Item1;

            _proxyTypeName = $"ProxyGen.SerializationProxy_{result.Item2}";
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
            string assemblyName = $"{_proxyTypeName}-{DateTime.Now.ToFileTimeUtc()}";
            var refPaths = CodeGen<SnippetsUnsafe>.GetReferences<T>(true);
            MetadataReference[] references = Enumerable.Select<PortableExecutableReference, PortableExecutableReference>(refPaths, r => r).ToArray();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release));

            _compilation = compilation;

            Emit();

            BuildDelegates();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Emit()
        {
            if (_proxyType != null) return;
            using (var ms = new MemoryStream())
            {
                var result = _compilation.Emit(ms);
                if (!result.Success)
                {
                    var compilationErrors = result.Diagnostics.Where(diagnostic =>
                            diagnostic.IsWarningAsError ||
                            diagnostic.Severity == DiagnosticSeverity.Error)
                        .ToList();
                    if (compilationErrors.Any())
                    {
                        var firstError = compilationErrors.First();
                        var errorNumber = firstError.Id;
                        var errorDescription = firstError.GetMessage();
                        var firstErrorMessage = $"{errorNumber}: {errorDescription};";
                        var exception = new Exception($"Compilation failed, first error is: {firstErrorMessage}");
                        compilationErrors.ForEach(e => { if (!exception.Data.Contains(e.Id)) exception.Data.Add(e.Id, e.GetMessage()); });
                        throw exception;
                    }
                }
                ms.Seek(0, SeekOrigin.Begin);

                _generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                _proxyType = _generatedAssembly.GetType(_proxyTypeName);
            }
        }
    }
   
    
     internal static class CodeGen<TSnippets>
        where TSnippets : ISnippetsSafeV3, new()
    {
        private static TSnippets snippets = new TSnippets();
        private const BindingFlags _flags = BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public;
        internal static IEnumerable<PortableExecutableReference> GetReferences<T>(bool includeUnsafe = false)
        {
            var refPaths = new List<PortableExecutableReference> {
                MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System),
                MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Console),
                    MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Private_CoreLib),
                        MetadataReference.CreateFromFile(FrameworkAssemblyPaths.System_Runtime),
                            MetadataReference.CreateFromFile(typeof(Unsafe).Assembly.Location),
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
                    if(!(prop.CanRead && prop.CanWrite && TypeSupport.IsSupportedType(prop.PropertyType)))
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
                var props = typeof(T).GetProperties(_flags).ToList();
#if NET5_0_OR_GREATER
                foreach (var prop in CollectionsMarshal.AsSpan(props))
                {
#else
                foreach (var prop in props)
                {
#endif
                    if(!(prop.CanRead && prop.CanWrite && TypeSupport.IsSupportedType(prop.PropertyType)))
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
                    if(!(prop.CanRead && prop.CanWrite && TypeSupport.IsSupportedType(prop.PropertyType)))
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
            if ((type == typeof(IEnumerable<>) && type.GetElementType()!.IsValueType))
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
            if ((type.IsArray && type.GetElementType()!.IsValueType) )
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
                sb.AppendFormat(snippets.PropertyTemplateDeserializeVarLenStr, fieldName, propertyName, typeof(System.Char));
                sb.AppendLine();
                var offsetStr = $"+({fieldName}?.Length ?? 0)*Unsafe.SizeOf<{typeof(System.Char)}>()";
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
}