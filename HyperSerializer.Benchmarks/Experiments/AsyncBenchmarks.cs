using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using HyperSerializer;
using Buffer = System.Buffer;

namespace HyperSerializer.Benchmarks.Experiments
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]

    //|                Method |        Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
    //|---------------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|
    //|      FASTERSerializer |    69.81 ms | 0.966 ms | 0.903 ms |  1.00 |    0.00 | 28000.0000 |    351 MB |
    //| MessagePackSerializer | 1,311.43 ms | 2.462 ms | 2.303 ms | 18.79 |    0.23 | 25000.0000 |    313 MB |

    //    |                Method |        Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
    //    |---------------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|
    //    |      FASTERSerializer |    55.48 ms | 0.576 ms | 0.511 ms |  1.00 |    0.00 | 27000.0000 |    343 MB |
    //    |    ProtobufSerializer | 1,032.05 ms | 2.701 ms | 2.394 ms | 18.60 |    0.17 | 41000.0000 |    511 MB |
    //    | MessagePackSerializer | 1,147.54 ms | 3.251 ms | 2.882 ms | 20.68 |    0.18 | 24000.0000 |    305 MB |
    public class AsyncBenchmarks
    {
        private List<Test> _test;
        private int iterations = 1_000_000;
        public AsyncBenchmarks()
        {
            _test = new List<Test>(); ;
            for (var i = 0; i < iterations; i++)
            {
                _test.Add(new Test()
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
            _test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
            {
                var bytes = HyperSerializerUnsafe<Test>.Serialize(obj);
                Test deserialize = HyperSerializerUnsafe<Test>.Deserialize(bytes);
                Debug.Assert(deserialize.E == obj.E);
            });
        }
        [Benchmark]
        public void ProtobufSerializerAsync()
        {

            _test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
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

            _test.AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount).ForAll((obj) =>
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

}

