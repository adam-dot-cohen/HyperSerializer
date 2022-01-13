using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Apex.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using HyperSerialize;
using MessagePack;
using ProtoBuf;
using Buffer = System.Buffer;

namespace HyperSerialize.Benchmarks.Experiments
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MemoryDiagnoser]

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
        public void HyperSerializerSync()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializer<TestWithStrings>.Serialize(obj);
                TestWithStrings deserialize = HyperSerializer<TestWithStrings>.Deserialize(bytes);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }
        [Benchmark]
        public void HyperSerializerUnsafe()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializerUnsafe<TestWithStrings>.Serialize(obj);
                TestWithStrings deserialize = HyperSerializerUnsafe<TestWithStrings>.Deserialize(bytes);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
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
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }

        [Benchmark]
        public void MessagePackSerializer()
        {
            foreach (var obj in _test)
            {
                var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
                TestWithStrings deserialize = MessagePack.MessagePackSerializer.Deserialize<TestWithStrings>(serialize);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }
        [Benchmark]
        public void ApexSerializer()
        {

            var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
            foreach (var obj in _test)
            {
                using var stream = new MemoryStream();
                _binary.Write(obj, stream);
                stream.Position = 0;
                var deserialize = _binary.Read<TestWithStrings>(stream);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }

    }
    //[Benchmark]
    //public void FASTERSerializerHeap_Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializer.SerializeAsync(out var bytes, obj);
    //        TestWithStrings deserialize = HyperSerializer.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializerUnsafe<TestWithStrings>.Serialize(out Span<byte> bytes, obj);
    //        TestWithStrings deserialize = HyperSerializerUnsafe<TestWithStrings>.Deserialize(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerAsyncV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializerUnsafe<TestWithStrings>.SerializeAsync(out var bytes, obj);
    //        TestWithStrings deserialize = HyperSerializerUnsafe<TestWithStrings>.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.E == obj.E);
    //    }
    //}

}

