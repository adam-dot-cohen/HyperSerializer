using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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
    public class SyncBenchmarks
    {
        private List<Test> _test;
        private int iterations = 10_0000;
        public SyncBenchmarks()
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
        public void HyperSerializer()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializerSafe<Test>.Serialize(obj);
                Test deserialize = HyperSerializerSafe<Test>.Deserialize(bytes);
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
                var deserialize = Serializer.Deserialize<Test>(stream);
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
                Test deserialize = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
                Debug.Assert(deserialize.E == obj.E);
            }
        }
        [Benchmark]
        public void DataContractSerializer()
        {
            foreach (var obj in _test)
            {
                using var stream = new MemoryStream();
                new System.Runtime.Serialization.DataContractSerializer(typeof(Test)).WriteObject(stream, obj);
                stream.Flush();
                stream.Position = 0;
                var deserialize = (Test)new System.Runtime.Serialization.DataContractSerializer(typeof(Test)).ReadObject(stream);
                Debug.Assert(deserialize.E == obj.E);
            }
        }
        [Benchmark]
        public void BinaryFormatterSerializer()
        {
            foreach (var obj in _test)
            {
                using var stream = new MemoryStream();
                new BinaryFormatter().Serialize(stream, obj);
                stream.Flush();
                stream.Position = 0;
                var deserialize = (Test)new BinaryFormatter().Deserialize(stream);
                Debug.Assert(deserialize.E == obj.E);
            }
        }
        //[Benchmark]
        //public void ZeroFormatterSerializer()
        //{
        //    foreach (var obj in _test)
        //    {
        //        var serialize = ZeroFormatter.ZeroFormatterSerializer.Serialize(obj);
        //        Test deserialize = ZeroFormatter.ZeroFormatterSerializer.Deserialize<Test>(serialize);
        //        Debug.Assert(deserialize.E == obj.E);

        //    }
        //}


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

