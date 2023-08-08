using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NET5_0_OR_GREATER
using System.Runtime.Loader;
#endif
using System.Threading.Tasks;
using HyperSerializer.CodeGen;
using HyperSerializer.CodeGen.Snipets;
using HyperSerializer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Hyper
{
    /// <summary>
    /// HyperSerializer\<typeparam name="T"></typeparam> default implementation with support for value types, strings arrays and lists containing value types, and reference types (e.g. your DTO class).
    /// Note that reference types containing properties that are complex types (i.e. a child object/class with properties) and Dictionaries are not yet supported.  Properties of these types will be ignored during serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">ValueType (e.g. int, Guid, string, decimal?,etc,; arrays and lists of these types are supported as well) or heap based ref type (e.g. DTO class/object) containing properties to be serialized/deserialized.
    /// NOTE objects containing properties that are complex types (i.e. other objects with properties) and type Dictionary are ignored during serialization and deserialization.</typeparam>
    public static class HyperSerializer<T>
    {
        /// <summary>
        /// Dynamic serialization proxy name
        /// </summary>
        private static readonly string _proxyTypeName = $"ProxyGen.SerializationProxy_{typeof(T).GetClassName<T>()}";

        /// <summary>
        /// Dynamic serialization proxy type
        /// </summary>
        private static Type _proxyType;

        /// <summary>
        /// Serialization proxy delegate definition for <typeparam name="T"></typeparam>
        /// </summary>
        /// <param name="obj">object or value type to be serialized</param>
        /// <returns><seealso cref="Span{byte}"/></returns>
        internal delegate Span<byte> Serializer(T obj);

        /// <summary>
        /// Static serialization delegate cache
        /// </summary>
        internal static Serializer SerializeDynamic;
        internal delegate T Deserializer(ReadOnlySpan<byte> bytes);
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
            => new(Serialize(obj).ToArray());

        /// <summary>
        /// Deserialize binary to <typeparam name="T"></typeparam> async
        /// </summary>
        /// <param name="bytes"><seealso cref="ReadOnlyMemory{byte}"/>, <seealso cref="Memory{byte}"/> or byte[] array to be deserialized</param>
        /// <returns><typeparam name="T"></typeparam></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<T> DeserializeAsync(ReadOnlyMemory<byte> bytes)
            => new(Deserialize(bytes.Span));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void BuildDelegates()
        {
            var infos = _proxyType.GetMethod("Serialize");
            SerializeDynamic = infos.CreateDelegate<Serializer>();

            var infod = _proxyType.GetMethod("Deserialize");
            if (infod != null) DeserializeDynamic = infod.CreateDelegate<Deserializer>();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Compile()
        {
            var result = CodeGen<SnippetsSafeV3>.GenerateCode<T>();

            var syntaxTree = CSharpSyntaxTree.ParseText(result.Code);

            var compilation = CSharpCompilation.Create(
                assemblyName: $"ProxyGen.SerializationProxy_{result.ClassName}_{DateTime.Now.ToFileTimeUtc()}")
                    .AddSyntaxTrees(syntaxTree)
                    .WithReferences(CodeGen<SnippetsSafeV3>.GetReferences<T>(includeUnsafe: true))
                    .WithOptions(new CSharpCompilationOptions(
                        outputKind: OutputKind.DynamicallyLinkedLibrary, 
                        allowUnsafe: true, 
                        optimizationLevel: OptimizationLevel.Release));
#if DEBUG
            Debug.Write(syntaxTree.GetRoot().NormalizeWhitespace().ToFullString());
#endif
            
            Emit(compilation);
            BuildDelegates();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Emit(CSharpCompilation _compilation)
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
#if NET5_0_OR_GREATER

                AssemblyLoadContext context = new CollectibleLoadContext();
                var generatedAssembly = context.LoadFromStream(ms);
#else
                var generatedAssembly = Assembly.Load(ms.ToArray());
                
#endif
                _proxyType = generatedAssembly.GetType(_proxyTypeName);

                Span<byte> buffer = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(buffer);
                AppDomain.CurrentDomain.Load(buffer.ToArray());
            }
        }
    }
}