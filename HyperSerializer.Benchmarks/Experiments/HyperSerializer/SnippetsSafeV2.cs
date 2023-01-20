namespace HyperSerializer.Benchmarks.Experiments.HyperSerializer;

internal class SnippetsSafeVe : ISnippetsSafeV3
{
    //public string PropertyTemplateSerialize
    //{
    //    get
    //    {
    //        return
    //            "var _{0} = ({1}) {2}; Unsafe.WriteUnaligned(ref bytes.Slice(offset+=offsetWritten, offsetWritten = {3})[0], _{0});";
    //    }
    //}

    //public string PropertyTemplateDeserialize
    //{
    //    get
    //    {
    //        return "{0} = ({1}) Unsafe.ReadUnaligned<{1}>(ref bytes.Slice(offset+=offsetWritten, offsetWritten = {2})[0]);";
    //    }
    //}

    //public string PropertyTemplateDeserializeLocal
    //{
    //    get
    //    {
    //        return
    //            "var _{0} = ({1}) Unsafe.ReadUnaligned<{1}>(ref bytes.Slice(offset+=offsetWritten, offsetWritten = {2})[0]);";
    //    }
    //}
    //public string PropertyTemplateSerialize => "var _{0} = ({1}) {2}; MemoryMarshal.Write(bytes.Slice(offset+=offsetWritten, offsetWritten = {3}), ref _{0});";
    //public string PropertyTemplateDeserialize => "{0} = ({1}) MemoryMarshal.Read<{1}>(bytes.Slice(offset+=offsetWritten, offsetWritten = {2}));";
    //public string PropertyTemplateDeserializeLocal => "var _{0} = ({1}) MemoryMarshal.Read<{1}>(bytes.Slice(offset+=offsetWritten, offsetWritten = {2}));";
    public string PropertyTemplateSerialize { get { return "var _{0} = ({1}) {2}; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {3})) *(({1}*)p) = _{0};"; } }
    public string PropertyTemplateDeserialize { get { return "fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2})) {0} = *(({1}*)p);"; } }
    public string PropertyTemplateDeserializeLocal { get { return "var _{0} = ({1})default; fixed (byte* p = bytes.Slice(offset+=offsetWritten, offsetWritten = {2}))  _{0} = *(({1}*)p); "; } }
    //public string PropertyTemplateSerializeNullable => "var _{0} = {1} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}==null ? 1 : 0)) != 1)) MemoryMarshal.Write(bytes.Slice(offset, offsetWritten = {2}), ref _{0}); else offsetWritten = 0;";
    //public string PropertyTemplateDeserializeNullable => "offset+=offsetWritten; if(bytes[offset++] != 1) {0} = ({1}?) MemoryMarshal.Read<{1}>(bytes.Slice(offset, offsetWritten = {2})); else offsetWritten = 0;";
    public string PropertyTemplateSerializeNullable { get { return "var _{0} = {1} ?? default; offset+=offsetWritten; if(((bytes[offset++] = (byte)({1}==null ? 1 : 0)) != 1)) MemoryMarshal.Write(bytes.Slice(offset, offsetWritten = {2}), ref _{0}); else offsetWritten = 0;"; } }
    public string PropertyTemplateDeserializeNullable { get { return "offset+=offsetWritten; if(bytes[offset++] != 1) {0} = ({1}?) MemoryMarshal.Read<{1}>(bytes.Slice(offset, offsetWritten = {2})); else offsetWritten = 0;"; } }
    public string PropertyTemplateSerializeVarLenStr => "if(_{1} > 0){{ Unsafe.WriteUnaligned(ref bytes.Slice(offset+=offsetWritten, offsetWritten = _{1})[0], {0}); }}";
    public string PropertyTemplateDeserializeVarLenStr => "{0} = (_{1} >= 0) ? Unsafe.ReadUnaligned<string>(ref bytes.Slice(offset += offsetWritten, offsetWritten = _{1})[0]) : null;";
    public string PropertyTemplateSerializeArrLen => "int _{0} = ({1}?.Length ?? -1)*Unsafe.SizeOf<{2}>(); MemoryMarshal.Write(bytes.Slice(offset+=offsetWritten, offsetWritten = 4), ref _{0});";
    public string PropertyTemplateSerializeListLen => "int _{0} = ({1}?.Count() ?? -1)*Unsafe.SizeOf<{2}>(); MemoryMarshal.Write(bytes.Slice(offset+=offsetWritten, offsetWritten = 4), ref _{0});";
    public string PropertyTemplateSerializeVarLenArr => "if(_{1} > 0){{ var b = bytes.Slice(offset+=offsetWritten, offsetWritten = _{1}); MemoryMarshal.Cast<{2},byte>({0}.AsSpan()).CopyTo(b); }}";
    public string PropertyTemplateDeserializeVarLenArr=> "{0} = (_{1} >= 0) ? MemoryMarshal.Cast<byte,{2}>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1})).ToArray() : null;";
    public string PropertyTemplateDeserializeVarLenList => "{0} = (_{1} >= 0) ? new List<{2}>(MemoryMarshal.Cast<byte,{2}>(bytes.Slice(offset += offsetWritten, offsetWritten = _{1}))) : null;";
    public string StringLength => "({0}?.Length ?? -1)"; 
    public string StringLengthSpan=>"({0}?.Length ?? 0)";
    public string ClassTemplate => @"
					namespace ProxyGen
					{{
						using System;
						using System.Runtime.CompilerServices;
						using System.Runtime.InteropServices;
						using System.Text;

						public unsafe static class SerializationProxy_{0}
						{{
								
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static Span<byte> Serialize({1} obj)
                                    => Heap(obj);	

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
								public static {1} Deserialize(Span<byte> bytes)
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
								    => Serialize(obj).ToArray();
								
								[MethodImpl(MethodImplOptions.AggressiveInlining)]
								public static {1} DeserializeAsync(Memory<byte> bytes)
                                   => Deserialize(bytes.Span);
						}}

		}}";
}