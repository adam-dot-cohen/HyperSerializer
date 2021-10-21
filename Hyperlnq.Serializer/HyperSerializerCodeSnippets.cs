namespace Hyperlnq.Serializer
{
    public static class HyperSerializerCodeSnippetsV2
    {
        public const string PropertyTemplateSerialize = "var _{0} = ({1}) {2}; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({1}*)(p)) = _{0};";
        public const string PropertyTemplateDeserialize = "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) {0} = *(({1}*)(p)); ";
        //public const string PropertyTemplateDeserialize = "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ {0} = ({1}) *(({2}*)(p)); }}";
        public const string PropertyTemplateDeserializeLocal = "var _{0} = ({1})default; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3}))  _{0} = *(({2}*)(p)); ";
        public const string PropertyTemplateSerializeNullable = "var _{0} = {1}?.{2} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{2}==null ? 1 : 0)) != 1)) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({4}*)(p)) = _{0}; else offsetWritten = 0;";
        public const string PropertyTemplateDeserializeNullable = "offset+=offsetWritten; if(bytes[offset++] != 1) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  {0} = *(({1}*)(p));  else offsetWritten = 0;";
        public const string PropertyTemplateSerializeVarLenStr = "if(_{1} > 0) MemoryMarshal.AsBytes({0}.AsSpan()).CopyTo(bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
        public const string PropertyTemplateDeserializeVarLenStr = "{0} = (_{1} >= 0) ?  MemoryMarshal.Cast<byte, char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;";
        public const string StringLength = "({0}?.Length * 2 ?? -1)";
        public const string ClassTemplate = @"

					
					using System;
					using System.Runtime.CompilerServices;
					using System.Runtime.InteropServices;
					using System.Text;
					namespace ProxyGen
					{{
						public static unsafe class SerializationProxy_{0}
						{{
								#if NET5_0
								internal static readonly Encoding UTF8Encoding => new UTF8Encoding(false);
								#else
								internal static readonly Encoding UTF8Encoding = new UTF8Encoding(false);
								#endif
								private const int maxStackAlloc = 256;
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> Serialize({1} obj)
								{{
									//var len = {2};	
									//if(len <= maxStackAlloc)
         //                           	return Stack(obj);
									//else
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
									byte[] bytes = new byte[len];
                    {3}
									return bytes;
									
								}}								
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static {1} Deserialize(ReadOnlySpan<byte> bytes)
								{{
									{1} obj = {5}; 
									var offset = 0;
									var offsetWritten = 0;
									//int len0 = 0;
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
						
		}}
		";

    }
    public static class HyperSerializerCodeSnippetsV3
    {
        public const string PropertyTemplateSerialize = "var _{0} = ({1}) {2}; fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptf, _{0}); }}";
        public const string PropertyTemplateDeserialize = "fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ {0} = ({1}) System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{2}>(ptf); }}";
        public const string PropertyTemplateDeserializeLocal = "var _{0} = ({1})default; fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ _{0} = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{2}>(ptf); }}";
        public const string PropertyTemplateSerializeNullable = "var _{0} = {1}?.{2} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{2}==null ? 1 : 0)) != 1)) fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptf, _{0}); }} else offsetWritten = 0;";
        public const string PropertyTemplateDeserializeNullable = "offset+=offsetWritten; if(bytes[offset++] != 1) fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ {0} = ({1}?) System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{2}>(ptf); }} else offsetWritten = 0;";
        public const string PropertyTemplateSerializeVarLenStr = "if(_{1} > 0) MemoryMarshal.AsBytes({0}.AsSpan()).CopyTo(bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
        public const string PropertyTemplateDeserializeVarLenStr = "{0} = (_{1} >= 0) ?  MemoryMarshal.Cast<byte, char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;";
        public const string StringLength = "({0}?.Length * 2 ?? 0)";
        public const string ClassTemplate = @"
					
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
						
		}}
		";
    }

    public static class HyperSerializerCodeSnippets_ByteArrachRefactor
    {
        public const string PropertyTemplateSerialize = "var _{0} = ({1}) {2}; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({1}*)p) = _{0};";
        public const string PropertyTemplateDeserialize = "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) {0} = *(({1}*)p); ";
        //public const string PropertyTemplateDeserialize = "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ {0} = ({1}) *(({2}*)p); }}";
        public const string PropertyTemplateDeserializeLocal = "var _{0} = ({1})default; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3}))  _{0} = *(({2}*)p); ";
        public const string PropertyTemplateSerializeNullable = "var _{0} = {1}?.{2} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{2}==null ? 1 : 0)) != 1)) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({4}*)p) = _{0}; else offsetWritten = 0;";
        public const string PropertyTemplateDeserializeNullable = "offset+=offsetWritten; if(bytes[offset++] != 1) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  {0} = *(({1}*)p);  else offsetWritten = 0;";
        public const string PropertyTemplateSerializeVarLenStr = "if(_{1} > 0) MemoryMarshal.AsBytes({0}.AsSpan()).CopyTo(bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
        public const string PropertyTemplateDeserializeVarLenStr = "{0} = (_{1} >= 0) ?  MemoryMarshal.Cast<byte, char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;";
        public const string StringLength = "({0}?.Length * 2 ?? -1)";
        public const string ClassTemplate = @"
					
					using System;
					using System.Runtime.CompilerServices;
					using System.Runtime.InteropServices;
					using System.Text;
					namespace ProxyGen
					{{
						public static unsafe class SerializationProxy_{0}
						{{
								#if NET5_0
								internal static readonly Encoding UTF8Encoding => new UTF8Encoding(false);
								#else
								internal static readonly Encoding UTF8Encoding = new UTF8Encoding(false);
								#endif
								private const int maxStackAlloc = 256;
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> Serialize({1} obj)
								{{
									//var len = {2};	
									//if(len <= maxStackAlloc)
         //                           	return Stack(obj);
									//else
										return Heap(obj);
									
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								private static byte[] Stack({1} obj)
								{{
									var offset = 0;
									var offsetWritten = 0;
									var len = {2};
									byte[] bytes = new byte[len];	
                    {3}
									return bytes.ToArray();
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								private static byte[] Heap({1} obj)
								{{
									var offset = 0;
									var offsetWritten = 0;
									var len = {2};
									byte[] bytes = new byte[len];
                    {3}
									return bytes;
									
								}}								
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static {1} Deserialize(ReadOnlySpan<byte> bytes)
								{{
									{1} obj = {5}; 
									var offset = 0;
									var offsetWritten = 0;
									//int len0 = 0;
					{4}
									return obj;
								}}
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static byte[] SerializeAsync({1} obj)
								{{
									return Serialize(obj);
								}}									
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static {1} DeserializeAsync(ReadOnlyMemory<byte> bytes)
								{{
									return Deserialize(bytes.Span);
								}}
						}}
						
		}}
		";

    }

    //    public static class HyperSerializerCodeSnippetsV2
    //    {
    //        public const string PropertyTemplateSerialize = "var _{0} = ({1}) {2}; fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptf, _{0}); }}";
    //        public const string PropertyTemplateDeserialize = "fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ {0} = ({1}) System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{2}>(ptf); }}";
    //        public const string PropertyTemplateDeserializeLocal = "var _{0} = ({1})default; fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ _{0} = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{2}>(ptf); }}";
    //        public const string PropertyTemplateSerializeNullable = "var _{0} = {1}?.{2} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{2}==null ? 1 : 0)) != 1)) fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptf, _{0}); }} else offsetWritten = 0;";
    //        public const string PropertyTemplateDeserializeNullable = "offset+=offsetWritten; if(bytes[offset++] != 1) fixed (void* ptf = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) {{ {0} = ({1}?) System.Runtime.CompilerServices.Unsafe.ReadUnaligned<{2}>(ptf); }} else offsetWritten = 0;";
    //        public const string PropertyTemplateSerializeVarLenStr = "if(_{1} > 0) UTF8.GetBytes({0}.AsSpan(),bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
    //        public const string PropertyTemplateDeserializeVarLenStr = "{0} = (_{1} >= 0) ? UTF8.GetString(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})) : null;";
    //        public const string StringLength = "({0}?.Length ?? 0)";
    //        public const string ClassTemplate = @"

    //					using System;
    //					using System.Runtime.CompilerServices;
    //					using System.Runtime.InteropServices;
    //					using System.Text;
    //					namespace ProxyGen
    //					{{
    //						using MemoryMarshal = ProxyGen.Facade.MemoryMarshal;
    //						public static unsafe class SerializationProxy_{0}
    //						{{
    //								#if NET5_0
    //								internal static Encoding Utf8Encoding => Encoding.UTF8;
    //								#else
    //								internal static  Encoding UTF8 = Encoding.UTF8;
    //								#endif
    //								private const int maxStackAlloc = 256;
    //								[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //								public static void Serialize(out Span<byte> byteSpan, {1} obj)
    //								{{
    //									var len = {2};	
    //									if(len <= maxStackAlloc)
    //                                    	SerializeStack(out byteSpan, obj);
    //									else
    //										SerializeHeap(out byteSpan, obj);

    //								}}	
    //								public static void SerializeStack(out Span<byte> byteSpan, {1} obj)
    //								{{
    //									var offset = 0;
    //									var offsetWritten = 0;
    //									var len = {2};
    //                                    byteSpan = new byte[len];
    //									Span<byte> bytes = stackalloc byte[len];	
    //{3}
    //									bytes.CopyTo(byteSpan);
    //								}}
    //								public static void SerializeHeap(out Span<byte> byteSpan, {1} obj)
    //								{{
    //									var offset = 0;
    //									var offsetWritten = 0;
    //									var len = {2};
    //									Span<byte> bytes = new byte[len];
    //{3}
    //									byteSpan = bytes;

    //								}}

    //								[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //								public static {1} Deserialize(Span<byte> bytes)
    //								{{
    //									{1} obj = {5}; 
    //									var offset = 0;
    //									var offsetWritten = 0;
    //									int len0 = 0;
    //					{4}
    //									return obj;
    //								}}
    //								[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //								public static void SerializeAsync(out Memory<byte> bytes, {1} obj)
    //								{{

    //									Serialize(out var sBytes, obj);
    //                                    bytes = sBytes.ToArray();
    //								}}	

    //								[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //								public static {1} DeserializeAsync(Memory<byte> bytes)
    //								{{
    //									return Deserialize(bytes.Span);
    //								}}
    //						}}

    //		}}
    //		namespace ProxyGen.Facade
    //		{{
    //						public static class MemoryMarshal
    //						{{
    //							[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //							public static unsafe void Write<T>(Span<byte> destination, ref T val)
    //							{{
    //								fixed (void* ptf = destination) System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptf, val);

    //							}}
    //							[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //							public static unsafe T Read<T>(Span<byte> span)
    //							{{
    //								fixed (void* ptf = span) return System.Runtime.CompilerServices.Unsafe.ReadUnaligned<T>(ptf);
    //							}}
    //						}}
    //		}}
    //		";

    //    }

    public static class HyperSerializerCodeSnippets
    {
        public const string PropertyTemplateSerialize = "var _{0} = ({1}) {2}; MemoryMarshal.Write(bytes.Slice(offset+=offsetWritten, offsetWritten = {3}), ref _{0});";
        public const string PropertyTemplateDeserialize = "{0} = ({1}) MemoryMarshal.Read<{2}>(bytes.Slice(offset+=offsetWritten, offsetWritten = {3}));";
        public const string PropertyTemplateDeserializeLocal = "var _{0} = ({1}) MemoryMarshal.Read<{2}>(bytes.Slice(offset+=offsetWritten, offsetWritten = {3}));";
        public const string PropertyTemplateSerializeNullable = "var _{0} = {1}?.{2} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{2}==null ? 1 : 0)) != 1)) MemoryMarshal.Write(bytes.Slice(offset, offsetWritten = {3}), ref _{0}); else offsetWritten = 0;";
        public const string PropertyTemplateDeserializeNullable = "offset+=offsetWritten; if(bytes[offset++] != 1) {0} = ({1}?) MemoryMarshal.Read<{2}>(bytes.Slice(offset, offsetWritten = {3})); else offsetWritten = 0;";
        public const string PropertyTemplateSerializeVarLenStr = "if(_{1} > 0) UTF8Encoding.GetBytes({0}.AsSpan(),bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));";
        public const string PropertyTemplateDeserializeVarLenStr = "{0} = (_{1} >= 0) ? UTF8Encoding.GetString(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})) : null;";
        public const string StringLength = "({0}?.Length ?? -1)";
        public const string ClassTemplate = @"
					namespace ProxyGen
					{{
						using System;
						using System.Runtime.CompilerServices;
						using System.Runtime.InteropServices;
						using System.Text;

						public static class SerializationProxy_{0}
						{{
								#if NET5_0
								internal static readonly Encoding UTF8Encoding => new UTF8Encoding(false);
								#else
								internal static readonly Encoding UTF8Encoding = new UTF8Encoding(false);
								#endif
								private const int maxStackAlloc = 256;
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> Serialize({1} obj)
								{{
									//var len = {2};	
									//if(len <= maxStackAlloc)
         //                           	return Stack(obj);
									//else
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
}
