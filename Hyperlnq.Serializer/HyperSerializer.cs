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

namespace Hyperlnq.Serializer
{

    public static class HyperSerializer<T>
    {
        private static string _generatedCode;
        private static string _proxyTypeName = $"ProxyGen.SerializationProxy_{typeof(T).Name}";
        private static Type _proxyType;
        private static CSharpCompilation _compilation;
        private static Assembly _generatedAssembly;
        public delegate Span<byte> Serializer(T obj);
        public delegate T Deserializer(ReadOnlySpan<byte> bytes);
        public delegate Memory<byte> SeriailzerAsync(T obj);
        public delegate T DeserializerAsync(Memory<byte> bytes);
        public static Serializer SerializeDynamic;
        public static Deserializer DeserializeDynamic;
        public static SeriailzerAsync SerializeDynamicAsync;
        public static DeserializerAsync DeserializeDynamicAsync;

     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> Serialize(T obj)
            => SerializeDynamic(obj);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Deserialize(ReadOnlySpan<byte> bytes)
            => DeserializeDynamic(bytes);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Memory<byte> SerializeAsync(T obj)
            =>Serialize(obj).ToArray();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DeserializeAsync(Memory<byte> bytes)
            => Deserialize(bytes.Span);
        
        internal static void BuildDelegates()
        {
            SerializeDynamic = _proxyType.GetMethod("Serialize")?.CreateDelegate<Serializer>();
            DeserializeDynamic = _proxyType.GetMethod("Deserialize")?.CreateDelegate<Deserializer>();
            SerializeDynamicAsync = _proxyType.GetMethod("SerializeAsync")?.CreateDelegate<SeriailzerAsync>();
            DeserializeDynamicAsync = _proxyType.GetMethod("DeserializeAsync")?.CreateDelegate<DeserializerAsync>();
        }
		
        static HyperSerializer()
        {
            Initialize();
        }
        static void Initialize()
        {
            Generate();
            Compile();
            BuildDelegates();
        }
        static void Generate()
        {
            var (length, serialize) = HyperSerializerGenerator.Serialize<T>();
            var (length3, deserialize) = HyperSerializerGenerator.Deserialize<T>();
            _generatedCode = string.Format(HyperSerializerCodeSnippets.ClassTemplate, typeof(T).Name, typeof(T).ToString().Replace("+", "."),
                length, serialize, deserialize, HyperSerializerTypeSupport.IsSupportedType<T>() ? "default" : "new()");
        }
        static void Compile()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(_generatedCode);

            string assemblyName = $"{_proxyTypeName}-{DateTime.Now.ToFileTimeUtc()}";
            var refPaths = HyperSerializerGenerator.GetReferences<T>();
            MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
                    optimizationLevel: OptimizationLevel.Release)
                        );

            _compilation = compilation;
            Emit();
        }
        static void Emit()
        {
            if (_proxyType != null) return;
            using (var ms = new MemoryStream())
            {
                var result = _compilation.Emit(ms);
                if (!result.Success)
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
#if NET46
            // Different in full .Net framework
           _generatedAssembly = Assembly.Load(ms.ToArray());
#endif

                _proxyType = _generatedAssembly.GetType(_proxyTypeName);
            }
        }
    }

    
public static class HyperSerializerV2<T>
{
	private static string _generatedCode;
	private static string _proxyTypeName = $"ProxyGen.SerializationProxy_{typeof(T).Name}";
	private static Type _proxyType;
	private static CSharpCompilation _compilation;
	private static Assembly _generatedAssembly;
	public delegate Span<byte> Serializer(T obj);
	public delegate T Deserializer(ReadOnlySpan<byte> bytes);
	public delegate Memory<byte> SeriailzerAsync(T obj);
	public delegate T DeserializerAsync(Memory<byte> bytes);
	public static Serializer SerializeDynamic;
	public static Deserializer DeserializeDynamic;
	public static SeriailzerAsync SerializeDynamicAsync;
	public static DeserializerAsync DeserializeDynamicAsync;

	static HyperSerializerV2()
	{
		Initialize();
	}

	private static void Initialize()
	{
		Generate();
		Compile();
		BuildDelegates();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Span<byte> Serialize(T obj)
		=> SerializeDynamic(obj);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static T Deserialize(ReadOnlySpan<byte> bytes)
		=> DeserializeDynamic(bytes);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<Memory<byte>> SerializeAsync(T obj)
	{
		Memory<byte> bytes = default;
		try
		{
			bytes = SerializeDynamicAsync(obj);
		}
		catch (ArgumentException)
		{
			Initialize();
			bytes = SerializeDynamicAsync(obj);
		}
		return await Task.FromResult(bytes);

	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static async Task<T> DeserializeAsync(Memory<byte> bytes)
	{
		T val = default;
		try
		{
			val = DeserializeDynamicAsync(bytes);
		}
		catch (ArgumentException)
		{
			Initialize();
			val = DeserializeDynamicAsync(bytes);
		}
		return await Task.FromResult(val);
	}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BuildDelegates()
    {
        var infos = _proxyType.GetMethod("Serialize", types: new[] { typeof(T) });
        SerializeDynamic = infos.CreateDelegate<Serializer>();

        var infod = _proxyType.GetMethod("Deserialize", types: new[] { typeof(ReadOnlySpan<byte>) });
        DeserializeDynamic = infod.CreateDelegate<Deserializer>();

        var infosAsync = _proxyType.GetMethod("SerializeAsync", types: new[] { typeof(T) });
        SerializeDynamicAsync = infosAsync.CreateDelegate<SeriailzerAsync>();

        var infodAsync = _proxyType.GetMethod("DeserializeAsync", types: new[] { typeof(Memory<byte>) });
        DeserializeDynamicAsync = infodAsync.CreateDelegate<DeserializerAsync>();
    }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Generate()
	{
		var (length, serialize) = HyperSerializerGeneratorV2.Serialize<T>();
		var (length3, deserialize) = HyperSerializerGeneratorV2.Deserialize<T>();
		_generatedCode = string.Format(HyperSerializerCodeSnippetsV2.ClassTemplate, typeof(T).Name, typeof(T).ToString().Replace("+", "."),
			length, serialize, deserialize, HyperSerializerTypeSupport.IsSupportedType<T>() ? "default" : "new()");
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Compile()
	{
		var syntaxTree = CSharpSyntaxTree.ParseText(_generatedCode);

		string assemblyName = $"{_proxyTypeName}-{DateTime.Now.ToFileTimeUtc()}";
		var refPaths = HyperSerializerGeneratorV2.GetReferences<T>();
		MetadataReference[] references = refPaths.Select(r => MetadataReference.CreateFromFile(r)).ToArray();
		var compilation = CSharpCompilation.Create(
			assemblyName,
			new[] { syntaxTree },
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true,
				optimizationLevel: OptimizationLevel.Release)
					);

		_compilation = compilation;
		Emit();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static void Emit()
	{
		if (_proxyType != null) return;
		using (var ms = new MemoryStream())
		{
			var result = _compilation.Emit(ms);
			if (!result.Success)
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
#if NET46
            // Different in full .Net framework
           _generatedAssembly = Assembly.Load(ms.ToArray());
#endif

			_proxyType = _generatedAssembly.GetType(_proxyTypeName);
		}
	}

}


}