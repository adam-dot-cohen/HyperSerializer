using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using MessagePack;
using ProtoBuf;
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
    public  class SyncBenchmarks
    {
        private List<Test> _test;
        private int iterations = 1_000_000;
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
                    E = (decimal)i,
                    F = (DateTime.Now - DateTime.Now.AddDays(-1)),
                    G = Guid.NewGuid(),
                    H = TestEnum.three,
                    //  I = i.ToString()
                });
            }
        }

        [Benchmark(Baseline = true)]
        public void FASTERSerializer()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializer<Test>.Serialize(obj);
                Test deserialize = HyperSerializer<Test>.Deserialize(bytes);
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
                ProtoBuf.Serializer.Serialize(stream, obj);
                stream.Position = 0;
                var deserialize = ProtoBuf.Serializer.Deserialize<Test>(stream);
                Debug.Assert(deserialize.
                    E == obj.E);
            }
        }

        [Benchmark]
        public void MessagePackSerializer()
        {
            foreach (var obj in _test)
            {
                var serialize = MessagePack.MessagePackSerializer.Serialize<Test>(obj);
                Test deserialize = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
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
  [MessagePackObject(), ProtoContract()]
        public class Test
        {
            [Key(1), ProtoMember(1)]
            public int A { get; set; }
            [Key(2), ProtoMember(2)]
            public long B { get; set; }
            [Key(3), ProtoMember(3)]
            public DateTime C { get; set; }
            [Key(4), ProtoMember(4)]
            public uint D { get; set; }
            [Key(5), ProtoMember(5)]
            public decimal E { get; set; }
            [Key(6), ProtoMember(6)]
            public TimeSpan F { get; set; }
            [Key(7), ProtoMember(7)]
            public Guid G { get; set; }
            [Key(8), ProtoMember(8)]
            public TestEnum H { get; set; }
            //[Key(9), ProtoMember(9)]
            //public string I { get; set; }
            [Key(10), ProtoMember(11)]
            public int? An { get; set; }
            [Key(12), ProtoMember(12)]
            public long? Bn { get; set; }
            [Key(13), ProtoMember(13)]
            public DateTime Cn { get; set; }
            [Key(14), ProtoMember(14)]
            public uint? Dn { get; set; }
            [Key(15), ProtoMember(15)]
            public decimal? En { get; set; }
            [Key(16), ProtoMember(16)]
            public TimeSpan? Fn { get; set; }
            [Key(17), ProtoMember(17)]
            public Guid? Gn { get; set; }
            //[Key(18), ProtoMember(18)]
            //public TestEnum? Hn { get; set; }
            //[Key(19), ProtoMember(19)]
            //public string In { get; set; }
        }

        public enum TestEnum
        {
            one, two, three
        }
}

