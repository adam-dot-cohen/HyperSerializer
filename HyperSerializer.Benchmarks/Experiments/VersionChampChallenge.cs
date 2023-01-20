using System;
using System.Collections.Generic;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Hyper;
using HyperSerializer.Benchmarks.Experiments.HyperSerializer;
using Buffer = System.Buffer;

namespace Hyper.Benchmarks.Experiments
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]
    public class VersionChampChallenge
    {
        private List<TestWithStrings> _test;
        private int iterations = 1_000_000;
        public VersionChampChallenge()
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
        [Benchmark]
        public void HyperSerializer_Safe()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializer<TestWithStrings>.Serialize(obj);
                TestWithStrings deserialize = HyperSerializer<TestWithStrings>.Deserialize(bytes);
                Debug.Assert(deserialize.E == obj.E);
            }
        }
        [Benchmark(Baseline = true)]
        public void HyperSerializer_Unsafe()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializerExperimental<TestWithStrings>.Serialize(obj);
                TestWithStrings deserialize = HyperSerializerExperimental<TestWithStrings>.Deserialize(bytes);
                Debug.Assert(deserialize.E == obj.E);
            }
        }


    }
    public class VersionChampChallenge_SimpleType
    {
        private List<int?> _test;
        private int iterations = 1_000_000;
        public VersionChampChallenge_SimpleType()
        {
            _test = new List<int?>(); ;
            for (var i = 0; i < iterations; i++)
            {
                _test.Add(i);
            }
        }
        [Benchmark]
        public void HyperSerializer_Safe()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializer<int?>.Serialize(obj);
                var deserialize = HyperSerializer<int?>.Deserialize(bytes);
                Debug.Assert(obj == deserialize);
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

