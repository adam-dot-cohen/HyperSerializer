namespace HyperSerializer.Dynamic.Syntax.Templates;

internal class ProxySyntaxTemplateUnsafe : IProxySyntaxTemplate
{
    public string PropertyTemplateSerialize => "var _{0} = ({1}) {2}; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({1}*)p) = _{0};";
    public string PropertyTemplateDeserialize => "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) {0} = *(({1}*)p);";
    public string PropertyTemplateDeserializeLocal => "var _{0} = ({1})default; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  _{0} = *(({1}*)p); ";
    public string PropertyTemplateSerializeNullable => "var _{0} = {1} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}==null ? 1 : 0)) != 1)) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) *(({3}?*)(p)) = _{0}; else offsetWritten = 0;";
    public string PropertyTemplateDeserializeNullable => "offset+=offsetWritten; if(bytes[offset++] != 1) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  {0} = *(({1}?*)p);  else offsetWritten = 0;";
    public string PropertyTemplateSerializeVarLenStr => "if(_{1} >= 0 && {0} != null) MemoryMarshal.AsBytes({0}.AsSpan()).CopyTo(bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
    public string PropertyTemplateDeserializeVarLenStr => "{0} = (_{1} >= 0) ? MemoryMarshal.Cast<byte, char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;";
    public string PropertyTemplateSerializeVarLenArr { get; }
    public string PropertyTemplateDeserializeVarLenArr { get; }
    public string PropertyTemplateDeserializeVarLenList { get; }
    public string PropertyTemplateSerializeListLen { get; }
    public string PropertyTemplateSerializeArrLen { get; }
    public string StringLength => "({0}?.Length * 2 ?? -1)";
    public string StringLengthSpan => "({0}?.Length * 2 ?? 0)";
    public string ClassTemplate =>
        @"
					
					using System;
					using System.Runtime.CompilerServices;
					using System.Runtime.InteropServices;
					using System.Text;
					namespace ProxyGen
					{{
						public static unsafe class SerializationProxy_{0}
						{{
								#if NET5_0
								internal static readonly Encoding Utf8Encoding => new UTF8Encoding(false);
                                #elif NET6_0
                                internal static readonly Encoding Utf8Encoding => new UTF8Encoding(false);                                
								#else
								internal static readonly Encoding Utf8Encoding = new UTF8Encoding(false);
								#endif
								private const int maxStackAlloc = 128;
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> Serialize({1} obj)
								{{
									//var len = {2};	
									//if(len <= maxStackAlloc)
         //                           	return SerializeStack(obj);
									//else
										return SerializeHeap(obj);
									
								}}	
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> SerializeStack({1} obj)
								{{
									var offset = 0;
									var offsetWritten = 0;
									var len = {2};
									Span<byte> bytes = stackalloc byte[len];	
{3}
									return bytes.ToArray();
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> SerializeHeap({1} obj)
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