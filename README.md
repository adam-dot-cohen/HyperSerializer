# HyperSerializer
![]()
[![NuGet version (HyperSerializer)](https://img.shields.io/badge/nuget-v1.0.7-blue?style=flat-square)](https://www.nuget.org/packages/HyperSerializer/)

If you're looking for the fastest Microsoft.Net binary serializer, look no further - HyperSerializer is up to 18 times faster than MessagePack and Protobuf with roughly equivelant memory allocation (see Benchmarks below).  HyperSerializer was built as a champion/challenger (C++ vs C#) experiment to support the nanosecond latency requirements of high frequency trading.  HyperSerializer uses the managed Span\<T\> and Memory\<T\> structs to acheive extreme speed and low memory allocation without unsafe code.  HyperSerializer is 100% thread-safe and comes with both sync and async serialization and deserialization methods.  Out of the box support for .NETCoreApp 3.1, net5.0, net6.0.
    
HyperSerializer is intended for use cases such as caching and interservice communication behind firewalls or between known parites.  It is implemented using a customer binary format (aka wire format) and uses bounding techniques to protect against buffer overflows.  As a result, attempting to deserialize a message that exceeds the size of an expected data type will result in an exception in most cases as described later in this section.  For example, the following code which can be found in SerializerTests.cs in the test project attempts to deserialize an 8 BYTE buffer as a 4 BYTE int, which results in an ArgumentOutOfRangeException:

```csharp
long i = 1L << 32;
Span<byte> buffer = default;
MemoryMarshal.Write(buffer, ref i);
var deserialize = HyperSerializer<int>.Deserialize(buffer);
```
In the event the destiation data type was (1) 8 BYTES in length or (2) an object containing properties with an aggregate size exceeding 8 BYTES, one of the following would occur: (1) a data type specific execption, in most cases - ArguementOutOfRangeException, OR (2) no exception at all if the bytes happen to represent valid values for the destination type(s).

## Usage
HyperSerializer is a contract-less serializalizer that supports serializing primatives, structs, classes and record types.  As a result of being contract-less, changing the order or removing properties from a class that existed at the time of serialization will break deserialization.  If you ADD new properties to a class WITHOUT changing the names, types and order of preexisting properties, it will not break deserialization of previously serialized objects but should be tested throughly.  With respect to classes, only properties with public getters and setters will be serialied (fields and properties not matching the aforementioned crieria will be ignored).

```csharp
//Sync Example
Test obj = new();
Span<byte> bytes = HyperSerializer<Test>.Serialize(obj);
Test objDeserialized = HyperSerializer<Test>.Deserialize(bytes);
    
//Async Example
Test obj = new();
Memory<byte> bytes = HyperSerializer<Test>.SerializeAsync(obj);
Test objDeserialized = HyperSerializer<Test>.DeserializeAsync(bytes);

//Deserialize byte array...
byte[] bytes; // example - bytes you received over the wire, from cache etc...
Test objDeserialized = HyperSerializer<Test>.Deserialize(bytes);
Test objDeserialized = HyperSerializer<Test>.DeserializeAsync(bytes);
```
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
***Speed - Serializing and Deserializing 1M "Test" Objects***
_(Times in Milliseconds - Lower is Better)_ - HyperSerializer is roughly 3x faster than ApexSerializer and 18x faster than MessagePack and Protobuf...

![Execution Duration](https://github.com/Hyperlnq/HyperSerializer/blob/main/BenchmarkAssets/Time.png)

***Memory - Serializing and Deserializing 1M "Test" Objects***
_(Memory in Megabytes - Lower is Better)_ - HyperSerializer requres roughly the same memory allocation as MessagePack and half that of ApexSerializer and Protobuf...
    
![Execution Duration](https://github.com/Hyperlnq/HyperSerializer/blob/main/BenchmarkAssets/Space.png)

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1415 (21H2)
Intel Core i9-10980XE CPU 3.00GHz, 1 CPU, 36 logical and 18 physical cores
.NET SDK=6.0.101
  [Host]     : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT
  Job-XTVWKK : .NET 6.0.1 (6.0.121.56705), X64 RyuJIT

|                Method|       Mean|      Error|    StdDev|  Ratio|  RatioSD|       Gen 0|  Allocated
|       HyperSerializer|   49.95 ms|   0.543 ms|  0.508 ms|   1.00|     0.00|  17000.0000|     214 MB
| HyperSerializerUnsafe|   49.98 ms|   0.509 ms|  0.476 ms|   1.00|     0.01|  17000.0000|     214 MB
|        ApexSerializer|  155.18 ms|   0.884 ms|  0.827 ms|   3.11|     0.03|  35000.0000|     437 MB
| MessagePackSerializer|  899.12 ms|   0.994 ms|  0.881 ms|  18.02|     0.16|  16000.0000|     205 MB
|    ProtobufSerializer|  917.73 ms|  10.020 ms|  9.372 ms|  18.37|     0.17|  35000.0000|     435 MB
```
## HyperSerializerUnsafe\<T\>
The HyperSerializer project contains an unsafe implementation of the HyperSerializer\<T\>, named HyperSerializerUnsafe\<T\>.  It is an experimental version intended for benchmarking purposes and does not meaningfuly outperform HyperSerializer\<T\> in most scenarios, if at all (see table .  As such, it is not recommended for end user consumption.

## Limitations 
### Unsupported types
Serialization of the following types and nested types is planned but not supported at this time (if you would like to contribute, fork away or reach out to collaborate)...

- Complex type properties (i.e. a class with a property of type ANY class).  If a class contains a property that is a complex type, the class will still be serialized but the property will be ignored.
- Collections (e.g. List, Dictionary, etc.). If a class contains a property of type collection, the class will still be serialized but the property will be ignored.

### Property Exclusion
If you need to exclude a property from being serialized for reasons other then performance (unless nanoseconds actually matter to you), presently your only option is a DTO.  If you would like this feature added feel free to contribute or log an issue.
    
## Feedback, Suggestions and Contributions
Are all welcome!
