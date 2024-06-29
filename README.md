# Overview

Binary Serializer.  Serialization up to 18x faster than Protobuf and MessagePack

![]()
[![NuGet version (HyperSerializer)](https://img.shields.io/badge/nuget-v1.5-blue?style=flat-square)](https://www.nuget.org/packages/HyperSerializer/)

If you're looking for the fastest binary serializer for DotNet known to Git-kind, look no further.  HyperSerializer is up to ***16x faster than [MessagePack](https://github.com/neuecc/MessagePack-CSharp) and [Protobuf](https://github.com/protocolbuffers/protobuf), and 2x faster than [MemoryPack](https://github.com/Cysharp/MemoryPack)***, with roughly equivelant or better memory allocation. Simply install the [Nuget package (Install-Package HyperSerializer)](https://www.nuget.org/packages/HyperSerializer/) and serialize/deserialize with just 2 lines of code.

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
See the test project for additional examples.

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
BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22621.3296)
Intel Core i9-10980XE CPU 3.00GHz, 1 CPU, 36 logical and 18 physical cores
.NET SDK=8.0.203
  [Host]     : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2
  Job-PSJYJC : .NET 8.0.3 (8.0.324.11423), X64 RyuJIT AVX2

Jit=RyuJit  Runtime=.NET 8.0  Arguments=/p:Optimize=true
InvocationCount=1  LaunchCount=1  RunStrategy=Throughput
UnrollFactor=1

|            Method | iterations |      Mean | Ratio | Allocated | Alloc Ratio |
|------------------ |----------- |----------:|------:|----------:|------------:|
|   HyperSerializer |    1000000 |  46.24 ms |  1.00 | 205.99 MB |        1.00 |
|        MemoryPack |    1000000 |  98.81 ms |  2.14 | 213.62 MB |        1.04 |
|       MessagePack |    1000000 | 768.58 ms | 16.62 | 197.86 MB |        0.96 |
|          Protobuf |    1000000 | 769.34 ms | 16.64 | 427.25 MB |        2.07 |
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
