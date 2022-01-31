using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Hyper
{
    /// <summary>
    /// HyperSerializerLegacy\<typeparam name="T"></typeparam> is the original implementation pre-1.0.1 with limitations described in the generic type parameter's description.
    /// and collections (lists, arrays, dictionaries).
    /// </summary>
    /// <typeparam name="T">ValueType (e.g. int, Guid, string, decimal?, etc...) or heap based ref type (e.g. DTO object with properties to be serialized) to be serialized/deserialized.
    /// NOTE objects containing properties that are complex types (i.e. other objects with properties) are ignored during serialization.  Only value types and strings are supported.</typeparam>
    public static class HyperSerializerLegacy<T>
    {
        private static string _proxyTypeName = $"ProxyGen.SerializationProxy_{typeof(T).Name}";
        private static Type _proxyType;
        private static CSharpCompilation _compilation;
        private static Assembly _generatedAssembly;
        internal delegate Span<byte> Serializer(T obj);
        internal delegate T Deserializer(ReadOnlySpan<byte> bytes);
        internal static Serializer SerializeDynamic;
        internal static Deserializer DeserializeDynamic;

        static HyperSerializerLegacy()
            => Compile();
        /// <summary>
        /// Serialize <typeparam name="T"></typeparam> to binary non-async
        /// </summary>
        /// <param name="obj">object or value type to be serialized</param>
        /// <returns><seealso cref="Span{byte}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Serialize(T obj)
            => SerializeDynamic(obj);
        /// <summary>
        /// Deserialize binary to <typeparam name="T"></typeparam> non-async
        /// </summary>
        /// <param name="bytes"><seealso cref="ReadOnlySpan{byte}"/>, <seealso cref="Span{byte}"/> or byte[] to be deserialized</param>
        /// <returns><typeparam name="T"></typeparam></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize(ReadOnlySpan<byte> bytes)
            => DeserializeDynamic(bytes);
        /// <summary>
        /// Serialize <typeparam name="T"></typeparam> to binary async
        /// </summary>
        /// <param name="obj">object or value type to be serialized</param>
        /// <returns><seealso cref="Span{byte}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Memory<byte>> SerializeAsync(T obj)
            =>new ValueTask<Memory<byte>>(Serialize(obj).ToArray());
        /// <summary>
        /// Deserialize binary to <typeparam name="T"></typeparam> async
        /// </summary>
        /// <param name="bytes"><seealso cref="ReadOnlyMemory{byte}"/>, <seealso cref="Memory{byte}"/> or byte[] array to be deserialized</param>
        /// <returns><typeparam name="T"></typeparam></returns>
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
            var result = CodeGen<SnippetsSafe>.GenerateCode<T>();
            var generatedCode = result.Item1;
            _proxyTypeName = $"ProxyGen.SerializationProxy_{result.Item2}";
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
            var assemblyName = $"{_proxyTypeName}-{DateTime.Now.ToFileTimeUtc()}";
            var refPaths = CodeGen<SnippetsSafe>.GetReferences<T>();
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release)
            );
            #if DEBUG
            Debug.Write(generatedCode);
            #endif
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

#if NET451_OR_GREATER
                byte[] bytes = new bytes[ms.Length];
                ms.Read(ms, 0, ms.Length);
                _generatedAssembly = Assembly.Load(bytes);
#endif

                _generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                _proxyType = _generatedAssembly.GetType(_proxyTypeName);
            }
        }

    }
    /// <summary>
    /// HyperSerializer\<typeparam name="T"></typeparam> default implementation with support for value types, strings arrays and lists containing value types, and reference types (e.g. your DTO class).
    /// Note that reference types containing properties that are complex types (i.e. a child object/class with properties) and Dictionaries are not yet supported.  Properties of these types will be ignored during serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">ValueType (e.g. int, Guid, string, decimal?,etc,; arrays and lists of these types are supported as well) or heap based ref type (e.g. DTO class/object) containing properties to be serialized/deserialized.
    /// NOTE objects containing properties that are complex types (i.e. other objects with properties) and type Dictionary are ignored during serialization and deserialization.</typeparam>
    public static class HyperSerializer<T>
    {
        private static string _proxyTypeName = $"ProxyGen.SerializationProxy_{typeof(T).Name}";
        private static Type _proxyType;
        private static CSharpCompilation _compilation;
        private static Assembly _generatedAssembly;
        internal delegate Span<byte> Serializer(T obj);
        internal delegate T Deserializer(ReadOnlySpan<byte> bytes);
        internal static Serializer SerializeDynamic;
        internal static Deserializer DeserializeDynamic;

        static HyperSerializer()
            => Compile();

        /// <summary>
        /// Serialize <typeparam name="T"></typeparam> to binary non-async
        /// </summary>
        /// <param name="obj">object or value type to be serialized</param>
        /// <returns><seealso cref="Span{byte}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Serialize(T obj)
            => SerializeDynamic(obj);
        /// <summary>
        /// Deserialize binary to <typeparam name="T"></typeparam> non-async
        /// </summary>
        /// <param name="bytes"><seealso cref="ReadOnlySpan{byte}"/>, <seealso cref="Span{byte}"/> or byte[] to be deserialized</param>
        /// <returns><typeparam name="T"></typeparam></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize(ReadOnlySpan<byte> bytes)
            => DeserializeDynamic(bytes);
        /// <summary>
        /// Serialize <typeparam name="T"></typeparam> to binary async
        /// </summary>
        /// <param name="obj">object or value type to be serialized</param>
        /// <returns><seealso cref="Span{byte}"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<Memory<byte>> SerializeAsync(T obj)
            => new ValueTask<Memory<byte>>(Serialize(obj).ToArray());
        /// <summary>
        /// Deserialize binary to <typeparam name="T"></typeparam> async
        /// </summary>
        /// <param name="bytes"><seealso cref="ReadOnlyMemory{byte}"/>, <seealso cref="Memory{byte}"/> or byte[] array to be deserialized</param>
        /// <returns><typeparam name="T"></typeparam></returns>
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
            var result = CodeGenV3<SnippetsSafeV3>.GenerateCode<T>();
            var generatedCode = result.Item1;
            _proxyTypeName = $"ProxyGen.SerializationProxy_{result.Item2}";
            var syntaxTree = CSharpSyntaxTree.ParseText(generatedCode);
            var assemblyName = $"{_proxyTypeName}-{DateTime.Now.ToFileTimeUtc()}";
            var refPaths = CodeGenV3<SnippetsSafeV3>.GetReferences<T>();
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release)
            );
#if DEBUG
            Debug.Write(generatedCode);
#endif
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

#if NET451_OR_GREATER
                byte[] bytes = new bytes[ms.Length];
                ms.Read(ms, 0, ms.Length);
                _generatedAssembly = Assembly.Load(bytes);
#endif

                _generatedAssembly = AssemblyLoadContext.Default.LoadFromStream(ms);

                _proxyType = _generatedAssembly.GetType(_proxyTypeName);
            }
        }

    }
}