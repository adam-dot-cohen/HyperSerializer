﻿namespace HyperSerializer.Benchmarks.Experiments.HyperSerializer;

internal class SnippetsSafeV2 : ISnippetsSafeV3
{
    public string PropertyTemplateSerialize => "var _{0} = ({1}) {2}; MemoryMarshal.Write(bytes.Slice(offset+=offsetWritten, offsetWritten = Unsafe.SizeOf<{1}>()), ref _{0});";
    public string PropertyTemplateDeserialize => "{0} = ({1}) MemoryMarshal.Read<{1}>(bytes.Slice(offset+=offsetWritten, offsetWritten = Unsafe.SizeOf<{1}>())));";
    public string PropertyTemplateDeserializeLocal => "var _{0} = ({1}) MemoryMarshal.Read<{1}>(bytes.Slice(offset+=offsetWritten, offsetWritten = Unsafe.SizeOf<{1}>())));";
    public string PropertyTemplateSerializeNullable => "var _{0} = {1} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{0}==null ? 1 : 0)) != 1)) MemoryMarshal.Write(bytes.Slice(offset, offsetWritten = Unsafe.SizeOf<{1}>()), ref _{0}); else offsetWritten = 0;";
    public string PropertyTemplateDeserializeNullable => "offset+=offsetWritten; if(bytes[offset++] != 1) {0} = ({1}?) MemoryMarshal.Read<{1}>(bytes.Slice(offset, offsetWritten = Unsafe.SizeOf<{1}>())); else offsetWritten = 0;";
    public string PropertyTemplateSerializeVarLenStr => "if(_{1} > 0) Utf8Encoding.GetBytes({0}.AsSpan(),bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
    public string PropertyTemplateDeserializeVarLenStr => "{0} = (_{1} >= 0) ? Utf8Encoding.GetString(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})) : null;";

    public string PropertyTemplateSerializeVarLenArr { get; }
    public string PropertyTemplateDeserializeVarLenArr { get; }
    public string PropertyTemplateDeserializeVarLenList { get; }
    public string PropertyTemplateSerializeListLen { get; }
    public string PropertyTemplateSerializeArrLen { get; }

    //public string PropertyTemplateSerializeVarLenStr { get { return "if(_{1} > 0) MemoryMarshal.AsBytes({0}.AsSpan(),bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));"; } }
    //public string PropertyTemplateDeserializeVarLenStr { get { return "{0} = (_{1} >= 0) ? MemoryMarshal.Cast<byte,char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;"; } }
    public string StringLength => "({0}?.Length ?? -1)";
    public string StringLengthSpan => "({0}?.Length ?? 0)";

    public string ClassTemplate =>
        @"
					namespace ProxyGen
					{{
						using System;
						using System.Runtime.CompilerServices;
						using System.Runtime.InteropServices;
						using System.Text;

						public static class SerializationProxy_{0}
						{{
								internal static readonly Encoding Utf8Encoding = new UTF8Encoding(false);
								private const int maxStackAlloc = 256;
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> Serialize({1} obj)
								{{
										return Heap(obj);
									
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								private static Span<byte> Stack({1} obj)
								{{
									var offset = 0;
									var offsetWritten = 0;
									var len = {2};
									Span<byte> bytes = stackalloc byte[len];	
                    {3}
									return bytes.ToArray();
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								private static Span<byte> Heap({1} obj)
								{{
									var offset = 0;
									var offsetWritten = 0;
									var len = {2};
									Span<byte> bytes = new byte[len];
                    {3}
									return bytes;
									
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static {1} Deserialize(ReadOnlySpan<byte> bytes)
								{{
									{1} obj = {5}; 
									var offset = 0;
									var offsetWritten = 0;
									int len0 = 0;
					{4}
									return obj;
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Memory<byte> SerializeAsync({1} obj)
								{{
									return Serialize(obj).ToArray();
								}}	
								
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static {1} DeserializeAsync(ReadOnlyMemory<byte> bytes)

								{{
									return Deserialize(bytes.Span);
								}}
						}}

		}}";
}