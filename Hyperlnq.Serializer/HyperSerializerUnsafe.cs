using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace HyperSerializer
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
        public static Memory<byte> SerializeAsync(T obj)
            => Serialize(obj).ToArray();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeserializeAsync(ReadOnlyMemory<byte> bytes)
            => Deserialize(bytes.Span);

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
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
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
}