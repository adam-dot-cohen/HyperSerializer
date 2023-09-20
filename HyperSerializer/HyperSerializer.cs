using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace HyperSerializer;

/// <summary>
/// HyperSerializer convenience class that wraps <see cref="HyperSerializer{T}"/> for syntactic convenience.  HyperSerializer supports class objects (object graph Level0 properties will be serialized including structs containing
///  properties...Level0 properties that are classes will be ignored).  HyperSerializer value types, strings, arrays and lists containing value types, and reference types (e.g. your DTO class).
/// Note that reference types containing properties that are complex types (i.e. a child object/class with properties) and Dictionaries are not yet supported.  Properties of these types will be ignored during serialization and deserialization.
/// </summary>
/// <typeparam name="T">The value type (e.g. int, Guid, string, decimal?,etc,; collection (e.g. arrays, lists etc..., or heap based ref type (e.g. DTO class/object) containing properties to be serialized/deserialized.
/// NOTE objects containing properties that are complex types (i.e. other objects with properties) and type Dictionary are ignored during serialization and deserialization.</typeparam>
public static class HyperSerializer
{
    /// <summary>
    /// Serialize <typeparam name="T"></typeparam> to binary non-async
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<byte> Serialize<T>(T obj)
        => HyperSerializer<T>.SerializeDynamic(obj);

    /// <summary>
    /// Deserialize binary to <typeparam name="T"></typeparam> non-async
    /// </summary>
    /// <param name="bytes"><seealso cref="ReadOnlySpan{byte}"/>, <seealso cref="Span{byte}"/> or byte[] to be deserialized</param>
    /// <returns><typeparam name="T"></typeparam></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Deserialize<T>(ReadOnlySpan<byte> bytes)
        => HyperSerializer<T>.DeserializeDynamic(bytes);

    /// <summary>
    /// Serialize <typeparam name="T"></typeparam> to binary async
    /// </summary>
    /// <param name="obj">object or value type to be serialized</param>
    /// <returns><seealso cref="Span{byte}"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<Memory<byte>> SerializeAsync<T>(T obj)
        => new(HyperSerializer<T>.Serialize(obj).ToArray());

    /// <summary>
    /// Deserialize binary to <typeparam name="T"></typeparam> async
    /// </summary>
    /// <param name="bytes"><seealso cref="ReadOnlyMemory{byte}"/>, <seealso cref="Memory{byte}"/> or byte[] array to be deserialized</param>
    /// <returns><typeparam name="T"></typeparam></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ValueTask<T> DeserializeAsync<T>(ReadOnlyMemory<byte> bytes)
        => new(HyperSerializer<T>.Deserialize(bytes.Span));
}