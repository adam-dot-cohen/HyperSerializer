# Overview

Binary Serializer.  Serialization up to 18x faster than Protobuf and MessagePack

![]()
[![NuGet version (HyperSerializer)](https://img.shields.io/badge/nuget-v1.5-blue?style=flat-square)](https://www.nuget.org/packages/HyperSerializer/)

If you're looking for the fastest binary serializer for DotNet known to Git-kind, look no further.  HyperSerializer is up to ***18x faster than [MessagePack](https://github.com/neuecc/MessagePack-CSharp) and [Protobuf](https://github.com/protocolbuffers/protobuf), and 11x faster than [BinaryPack](https://github.com/Sergio0694/BinaryPack)***, with roughly equivelant or better memory allocation. Simply install the [Nuget package (Install-Package HyperSerializer)](https://www.nuget.org/packages/HyperSerializer/) and serialize/deserialize with just 2 lines of code.

```csharp
//Sync Example
Test obj = new();
Span<byte> bytes = HyperSerializer.Serialize(obj);
Test objDeserialized = HyperSerializer.Deserialize<Test>(bytes);
```
***Version 1.4 Field Support Added***

Version 1.4 adds support for fields (in addition to properties). To turn off for backwards compatibility with previously serialized types that contain PUBLIC fields (private fields will not impact deserialization of previously serialized types), add the following setting to the Program.cs file or prior to use of the serializer...

```csharp
//Turn off field serialization
HyperSerializerSettings.SerializeFields = false;
```
***Head to Head Speed Test***

BenchmarkDotNet experiment serializing and deserializing 1M "Test" classes. Times in Milliseconds - Lower is Better.  See "Benchmarks" for serialized test object definiiton and additional stats on memory utiltization.

![Execution Duration](http://raw.githubusercontent.com/Hyperlnq/HyperSerializer/main/BenchmarkAssets/Time.png)
 
# Implementation and Framework Support
HyperSerializer was built as a champion/challenger (C++ vs C#) experiment to support the microsecond latency requirements of high frequency trading.  HyperSerializer leverages Span\<T\> and Memory\<T\> structs that deliver near performance parity with C++ for this use case.  HyperSerializer is 100% thread-safe and comes with both sync and async serialization and deserialization methods.  Out of the box support for net6.0, net7.0, net8.0.
    
HyperSerializer is intended for use cases such as caching and interservice communication behind firewalls.  It serializes and deserializes the fields and properites of stack (i.e. struct) and heap (i.e. class) based types in sequential order without a contract (i.e. no inclusive attributes such `[DataMember]` required or supported...however `[IgnoreDataMember]` is supported).  So long as the source and destination types have same layout (i.e. fields/properities that share the same data type and order), serialization and deserialization will succeed.  For example, the following types may be serialized and deserialized interchangably

As with all forms of binary serialization, care must be takenFor example, the following code which can be found in SerializerTests.cs in the test project attempts to deserialize an 8 BYTE buffer as a 4 BYTE int, which results in an ArgumentOutOfRangeException:

```csharp
//Simulate foreign serialization
long i = 1L << 32;
Span<byte> buffer = default;
MemoryMarshal.Write(buffer, ref i);

//Simulate attempting to deserialize Int64 (8 bytes) to Int32 (4 bytes)
var deserialize = HyperSerializer.Deserialize<int>(buffer);

//Result: ArgumentOutOfRangeException
```
In the event the destiation data type was (1) 8 BYTES in length or (2) an object containing properties with an aggregate size exceeding 8 BYTES, one of the following would occur: (1) a data type specific execption, in most cases - ArguementOutOfRangeException, OR (2) no exception at all if the bytes happen to represent valid values for the destination type(s).

## Usage
HyperSerializer is a contract-less serializalizer that supports serializing primatives, structs, classes and record types.  As a result of being contract-less, changing the order or removing properties from a class that existed at the time of serialization will break deserialization.  If you ADD new properties to a class WITHOUT changing the names, types and order of preexisting properties, it will not break deserialization of previously serialized objects but should be tested throughly.  With respect to classes, only properties with public getters and setters will be serialied (fields and properties not matching the aforementioned crieria will be ignored).

```csharp
//Sync Example
Test obj = new();
Span<byte> bytes = HyperSerializer.Serialize(obj);
Test objDeserialized = HyperSerializer.Deserialize<Test>(bytes);
    
//Async Example
Test obj = new();
Memory<byte> bytes = HyperSerializer.SerializeAsync(obj);
Test objDeserialized = HyperSerializer.Deserialize<Test>Async(bytes);

//Deserialize byte array...
byte[] bytes; // example - bytes you received over the wire, from cache etc...
Test objDeserialized = HyperSerializer.Deserialize<Test>(bytes);
Test objDeserialized = HyperSerializer.Deserialize<Test>Async(bytes);
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
    public string I { get; set; }
}
```
***Speed - Serializing and Deserializing 1M "Test" Objects***
_(Times in Milliseconds - Lower is Better)_ - HyperSerializer is roughly 3x faster than ApexSerializer and 18x faster than MessagePack and Protobuf...

![Execution Duration](http://raw.githubusercontent.com/Hyperlnq/HyperSerializer/main/BenchmarkAssets/Time.png)

***Memory - Serializing and Deserializing 1M "Test" Objects***
_(Memory in Megabytes - Lower is Better)_ - HyperSerializer requres roughly the same memory allocation as MessagePack and half that of ApexSerializer and Protobuf...
    
![Execution Duration](http://raw.githubusercontent.com/Hyperlnq/HyperSerializer/main/BenchmarkAssets/Space.png)

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

## Limitations 
### Unsupported types
Serialization of the following types and nested types is planned but not supported at this time (if you would like to contribute, fork away or reach out to collaborate)...

- Complex type properties (i.e. a class with a property of type ANY class).  If a class contains a property that is a complex type, the class will still be serialized but the property will be ignored.
- Dictionaries are not supported at this type (arrays, generic lists, etc. are supported). If a class contains a property of type Dictionary, the class will still be serialized but the property will be ignored.

### Property Exclusion
If you need to exclude a property from being serialized for reasons other then performance (unless nanoseconds actually matter to you), decorate with the `[IngoreDataMember]` attribute from `System.Runtime.Serialization`.

```csharp
[IgnoreDataMember]
public int MyProperty { get; set; }
```

## Feedback, Suggestions and Contributions
Are all welcome!
