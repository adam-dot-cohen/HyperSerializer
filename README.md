# HyperSerializer
Blazing fast binary serialization up to 18 times faster than MessagePack and Protobuf with roughly equivelant memory allocation (see Benchmarks below).  HyperSerializer uses the Span<T> and Memory<T> managed memory structs to acheive high speed and low memory allocation without unsafe code.  HyperSerializer is 100% thread-safe and comes with both sync and async serialization and deserialization methods.

The libary uses a customer binary format (aka wire format) and "bounded" serialization to prevent buffer overflows.  For more information see the Buffer Overfow section, and SerializerTest.cs contained in the test project for examples.  This doesn not mean binary serialition (or any form of serialization for that matter) does not come without risks. HyperSerializer is for use cases such as caching, and interservice communication behind firewalls or between known parites.
    
## Usage
HyperSerializer is a contract-less serializalizer that supports serializing primatives, structs and classes.  With respect to classes, only properties with public getters and setters will be serialied (fields and properties not matching the aforementioned crieria will be ignored)


If you have a class and need to exclude a property from being serialized for reasons other then performance (unless nanoseconds actually matter to you), presently your only option is a DTO.
    
Serialization of the following types and nested types is planned but not supported at this time (if you would like to contribute, fork away or reach out to collaborate):
    
    nested properties with complex data types (i.e. a class with a property of type ANY class) and serialization of collections are both planned but not supported at this time .
    
If you attempt to serialize a class that contains properties that are complex or collections, the properties will be ignored, but all other properties will be serialized.
    
Note, the HyperSerializer project contains an unsafe implementation of the serializer.  It is intended for benchmarking purposes only and  In most scenarios the "safe" default implemenation outperforms the the unsafe implementation.  To be clear: DO USE HyperSerializer<T>; DO NOT USE HyperSerializerUnsafe<T>. 
    
## Benchmarks
Benchmarks performed using BenchmarkDotNet follow the intended usage pattern of serializing and deserializing a single instance of an object at a time (as opposed to batch collection serialization used in the benchmarks published by other libraries such as Apex).  The benchmarks charts displayed below represent 1 million syncronous serialization and deserialization operations of the following object:

```csharp
public class Test {
    public Guid? Gn { get; set; }
    public int A { get; set; }
    public long B { get; set; }
    public DateTime C { get; set; }
    public uint D { get; set; }
    public decimal E { get; set; }
    public TimeSpan F { get; set; }
    public Guid G { get; set; }
    public TestEnum H { get; set; }
}
```
### Time in Milliseconds to Serialize and Deserialize 1M "Test" Objects (Lower is Better)
![Execution Duration](https://github.com/Hyperlnq/HyperSerializer/blob/main/BenchmarkAssets/Time.png)

|                    Method |         Mean |  Ratio |       Gen 0 |     Gen 1 | Allocated |
|-------------------------- |-------------:|-------:|------------:|----------:|----------:|
|           HyperSerializer |     5.461 ms |   1.00 |   1000.0000 |         - |     21 MB |
|     MessagePackSerializer |    89.076 ms |  16.36 |   1000.0000 |         - |     20 MB |
|        ProtobufSerializer |    97.729 ms |  17.97 |   3000.0000 |         - |     43 MB |
|    DataContractSerializer | 1,030.394 ms | 189.49 | 129000.0000 | 3000.0000 |  1,602 MB |
|           BinaryFormatter | 1,791.142 ms | 329.02 | 152000.0000 | 4000.0000 |  1,881 MB |


## Example Code
    
The "bounded" serialization to prevent buffer overflows.  As a result, attempting to deserialize binary that exceeds the size of the expected data type.  For example, the following code which can be found in the test project attempts to deserialize an 8 BYTE long message as a 4 BYTE int resulting in an ArgumentOutOfRangeException:

```csharp
long i = 1L << 32;
Span<byte> buffer = default;
MemoryMarshal.Write(buffer, ref i);
var deserialize = HyperSerializer<int>.Deserialize(buffer);
```
In the event the destiation data type were an 8 BYTES in length or an object containing properties that exceeded 8 BYTES one of the following would occur: (1) no exception if the bytes popluated are within the bounds the deserialized type(s), or (2) a data type specific excption, in most cases - ArguementOutOfRangeException.
