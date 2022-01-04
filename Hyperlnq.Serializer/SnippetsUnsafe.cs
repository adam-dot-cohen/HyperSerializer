using System.Dynamic;

namespace HyperSerializer
{
    internal class SnippetsUnsafe : ISnippets
    {
        public string PropertyTemplateSerialize { get { return "var _{0} = ({1}) {2}; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({1}*)p) = _{0};"; } }
        public string PropertyTemplateDeserialize { get { return "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) {0} = *(({1}*)p);"; } }
        public string PropertyTemplateDeserializeLocal { get { return "var _{0} = ({1})default; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  _{0} = *(({1}*)p); "; } }
        public string PropertyTemplateSerializeNullable { get { return "var _{0} = {1}?.{0} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{0}==null ? 1 : 0)) != 1)) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) *(({3}*)(p)) = _{0}; else offsetWritten = 0;"; } }
        public string PropertyTemplateDeserializeNullable { get { return "offset+=offsetWritten; if(bytes[offset++] != 1) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  {0} = *(({1}*)p);  else offsetWritten = 0;"; } }
        public string PropertyTemplateSerializeVarLenStr { get { return "if(_{1} > -1) MemoryMarshal.AsBytes({0}.AsSpan()).CopyTo(bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));"; } }
        public string PropertyTemplateDeserializeVarLenStr { get { return "{0} = (_{1} > -1) ?  MemoryMarshal.Cast<byte, char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;"; } }
        //public string PropertyTemplateSerialize { get { return "System.Console.WriteLine({0}+\"_\"+1.ToString());var _{0} = ({1}) {2}; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({1}*)p) = _{0};"; } }
        //public string PropertyTemplateDeserialize { get { return "System.Console.WriteLine({0}+\"_\"+2.ToString());fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) {0} = *(({1}*)p);"; } }
        //public string PropertyTemplateDeserializeLocal { get { return "System.Console.WriteLine({0}+\"_\"+3.ToString());var _{0} = ({1})default; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  _{0} = *(({1}*)p); "; } }
        //public string PropertyTemplateSerializeNullable { get { return "System.Console.WriteLine({0}+\"_\"+4.ToString());var _{0} = {1}?.{0} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}?.{0}==null ? 1 : 0)) != 1)) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) *(({3}*)(p)) = _{0}; else offsetWritten = 0;"; } }
        //public string PropertyTemplateDeserializeNullable { get { return "System.Console.WriteLine({0}+\"_\"+5.ToString());offset+=offsetWritten; if(bytes[offset++] != 1) fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  {0} = *(({1}*)p);  else offsetWritten = 0;"; } }
        //public string PropertyTemplateSerializeVarLenStr { get { return "System.Console.WriteLine({0}+\"_\"+6.ToString());if(_{1} > 0) MemoryMarshal.AsBytes({0}.AsSpan()).CopyTo(bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));"; } }
        //public string PropertyTemplateDeserializeVarLenStr { get { return "System.Console.WriteLine({0}+\"_\"+7.ToString());{0} = (_{1} >= 0) ?  MemoryMarshal.Cast<byte, char>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToString() : null;"; } }
        public string StringLength { get { return "({0}?.Length * 2 ?? -1)"; } }
        //public string PropertyTemplateSerializeVarLenStr { get { return "if(_{1} > 0) Utf8Encoding.GetBytes({0}.AsSpan(),bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}));"; } }
        //public string PropertyTemplateDeserializeVarLenStr { get { return "{0} = (_{1} >= 0) ? Utf8Encoding.GetString(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})) : null;"; } }
        //public string StringLength { get { return "({0}?.Length ?? -1)"; } }
        public string ClassTemplate { get { return @"
					
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
						
		}}"; } }
    }
}
