using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Buffer = System.Buffer;

namespace Hyperlnq.Serializer.Benchmarks.Experiments
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]

    //|                Method |        Mean |    Error |   StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
    //|---------------------- |------------:|---------:|---------:|------:|--------:|-----------:|----------:|
    //|      FASTERSerializer |    69.81 ms | 0.966 ms | 0.903 ms |  1.00 |    0.00 | 28000.0000 |    351 MB |
    //| MessagePackSerializer | 1,311.43 ms | 2.462 ms | 2.303 ms | 18.79 |    0.23 | 25000.0000 |    313 MB |
    public  class VersionChampChallenge
    {
        private List<Test> _test;
        private int iterations = 1_000_000;
        public VersionChampChallenge()
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
                    E = (decimal)i,
                    F = (DateTime.Now - DateTime.Now.AddDays(-1)),
                    G = Guid.NewGuid(),
                    H = TestEnum.three,
                    //  I = i.ToString()
                });
            }
        }

        [Benchmark(Baseline = true)]
        public void HyperSerializer_V2()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializerV2<Test>.Serialize(obj);
                Test deserialize = HyperSerializerV2<Test>.Deserialize(bytes);
                Debug.Assert(deserialize.E == obj.E);
            }
        }
        [Benchmark]
        public void HyperSerializer_V1()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializer<Test>.Serialize(obj);
                Test deserialize = HyperSerializer<Test>.Deserialize(bytes);
                Debug.Assert(deserialize.E == obj.E);
            }
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
    //        HyperSerializerV2<Test>.Serialize(out Span<byte> bytes, obj);
    //        Test deserialize = HyperSerializerV2<Test>.Deserialize(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerAsyncV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializerV2<Test>.SerializeAsync(out var bytes, obj);
    //        Test deserialize = HyperSerializerV2<Test>.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

}

