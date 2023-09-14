using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Hyper;
using HyperSerializer.Benchmarks.Experiments.HyperSerializer;
using Buffer = System.Buffer;

namespace Hyper.Benchmarks.Experiments;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]

//|                Method |        Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
//|---------------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|
//|       HyperSerializer |    69.81 ms | 0.966 ms | 0.903 ms |  1.00 |    0.00 | 28000.0000 |    351 MB |
//| MessagePackSerializer | 1,311.43 ms | 2.462 ms | 2.303 ms | 18.79 |    0.23 | 25000.0000 |    313 MB |

public class AsyncBenchmarks
{
    private List<Test> _test;
    [Params(10_000_000, 100_000_000, 1_000_000_0000)]
    private int iterations = 1_000_000;
    public AsyncBenchmarks()
    {
        this._test = new List<Test>(); ;
        for (var i = 0; i < this.iterations; i++)
        {
            this._test.Add(new Test()
            {
                A = i,
                B = i,
                C = DateTime.Now.Date,
                D = (uint)i,
                E = i,
                F = DateTime.Now - DateTime.Now.AddDays(-1),
                G = Guid.NewGuid(),
                H = TestEnum.three,
                //  I = i.ToString()
            });
        }
    }


    [Benchmark(Baseline = true)]
    public void HyperSerializerAsync()
    {
        this._test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
        {
            var bytes = HyperSerializer<Test>.SerializeAsync(obj).GetAwaiter().GetResult();
            Test deserialize = HyperSerializer<Test>.DeserializeAsync(bytes).GetAwaiter().GetResult();
            Debug.Assert(deserialize.E == obj.E);
        });
    }
    [Benchmark]
    public void HyperSerializerUnsafeAsync()
    {
        this._test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
        {
            var bytes = HyperSerializerUnsafe<Test>.SerializeAsync(obj).GetAwaiter().GetResult();
            Test deserialize = HyperSerializerUnsafe<Test>.DeserializeAsync(bytes).GetAwaiter().GetResult();
            Debug.Assert(deserialize.E == obj.E);
        });
    }
    [Benchmark]
    public void ProtobufSerializerAsync()
    {
        this._test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
        {
            using var stream = new MemoryStream();
            ProtoBuf.Serializer.Serialize(stream, obj);
            stream.Position = 0;
            var deserialize = ProtoBuf.Serializer.Deserialize<Test>(stream);
            Debug.Assert(deserialize.
                E == obj.E);
        });
    }

    [Benchmark]
    public void MessagePackSerializerAsync()
    {
        this._test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
        {
            var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
            Test deserialize = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
            Debug.Assert(deserialize.E == obj.E);
        });
    }
}
//[Benchmark]
//public void FASTERSerializerHeap_Bench()
//{
//    foreach(var obj in _test)
//    {
//        HyperSerializer<Test>.SerializeAsync(out var bytes, obj);
//        Test deserialize = HyperSerializer<Test>.DeserializeAsync(bytes);
//        Debug.Assert(deserialize.E == obj.E);
//    }
//}

//[Benchmark]
//public void BinarySerializerV2Bench()
//{
//    foreach(var obj in _test)
//    {
//        HyperSerializerUnsafe<Test>.Serialize(out Span<byte> bytes, obj);
//        Test deserialize = HyperSerializerUnsafe<Test>.Deserialize(bytes);
//        Debug.Assert(deserialize.E == obj.E);
//    }
//}

//[Benchmark]
//public void BinarySerializerAsyncV2Bench()
//{
//    foreach(var obj in _test)
//    {
//        HyperSerializerUnsafe<Test>.SerializeAsync(out var bytes, obj);
//        Test deserialize = HyperSerializerUnsafe<Test>.DeserializeAsync(bytes);
//        Debug.Assert(deserialize.E == obj.E);
//    }
//}