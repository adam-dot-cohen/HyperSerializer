using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using HyperSerializer;
using MessagePack;
using ProtoBuf;
using Buffer = System.Buffer;

namespace HyperSerializer.Benchmarks.Experiments
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]

    //|                Method |        Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
    //|---------------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|
    //|      FASTERSerializer |    69.81 ms | 0.966 ms | 0.903 ms |  1.00 |    0.00 | 28000.0000 |    351 MB |
    //| MessagePackSerializer | 1,311.43 ms | 2.462 ms | 2.303 ms | 18.79 |    0.23 | 25000.0000 |    313 MB |
    public class SyncBenchmarksWithStrings
    {
        private List<TestWithStrings> _test;
        private int iterations = 1_000_000;
        public SyncBenchmarksWithStrings()
        {
            _test = new List<TestWithStrings>(); ;
            for (var i = 0; i < iterations; i++)
            {
                _test.Add(new TestWithStrings()
                {
                    A = i,
                    B = i,
                    C = DateTime.Now.Date,
                    D = (uint)i,
                    E = i,
                    F = DateTime.Now - DateTime.Now.AddDays(-1),
                    G = Guid.NewGuid(),
                    H = TestEnum.three,
                    I = i.ToString()
                });
            }
        }

        [Benchmark(Baseline = true)]
        public void HyperSerializer()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializerSafe<TestWithStrings>.Serialize(obj);
                var deserialize = HyperSerializerSafe<TestWithStrings>.Deserialize(bytes);
                Debug.Assert(deserialize.E == obj.E);
            }
        }
        [Benchmark]
        public void ProtobufSerializer()
        {
            MemoryPool<byte>.Shared.Rent(1014);
            foreach (var obj in _test)
            {
                using var stream = new MemoryStream();
                Serializer.Serialize(stream, obj);
                stream.Position = 0;
                var deserialize = Serializer.Deserialize<TestWithStrings>(stream);
                Debug.Assert(deserialize.
                    E == obj.E);
            }
        }

        [Benchmark]
        public void MessagePackSerializer()
        {
            foreach (var obj in _test)
            {
                var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
                var deserialize = MessagePack.MessagePackSerializer.Deserialize<TestWithStrings>(serialize);
                Debug.Assert(deserialize.E == obj.E);

            }
        }


    }
    //[Benchmark]
    //public void FASTERSerializerHeap_Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializerSafe<Test>.SerializeAsync(out var bytes, obj);
    //        Test deserialize = HyperSerializerSafe<Test>.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializer<Test>.Serialize(out Span<byte> bytes, obj);
    //        Test deserialize = HyperSerializer<Test>.Deserialize(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerAsyncV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializer<Test>.SerializeAsync(out var bytes, obj);
    //        Test deserialize = HyperSerializer<Test>.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

}

