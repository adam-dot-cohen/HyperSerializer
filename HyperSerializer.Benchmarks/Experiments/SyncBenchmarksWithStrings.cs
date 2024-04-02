using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Hyper;
using HyperSerializer;
using HyperSerializer.Benchmarks.Experiments.HyperSerializer;
using MessagePack;
using ProtoBuf;

namespace Hyper.Benchmarks.Experiments;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1, runtimeMoniker:RuntimeMoniker.Net60)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[MemoryDiagnoser]

public class SyncBenchmarksWithStrings
{
    private List<TestWithStrings> _test;
    private int iterations = 1_000_000;
    public SyncBenchmarksWithStrings()
    {
        this._test = new List<TestWithStrings>(); ;
        for (var i = 0; i < this.iterations; i++)
        {
            this._test.Add(new TestWithStrings()
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

    [Benchmark(Baseline = true, Description = "HyperSerializer")]
    public void HyperSerializerSync()
    {
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
            var bytes = HyperSerializer<TestWithStrings>.Serialize(obj);
            _ = HyperSerializer<TestWithStrings>.Deserialize(bytes);
            
        }
    }

       
    [Benchmark(Description = "Protobuf")]
    public void ProtobufSerializer()
    {
        MemoryPool<byte>.Shared.Rent(1014);
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            stream.Position = 0;
            _ = Serializer.Deserialize<TestWithStrings>(stream);
            
        }
    }

    [Benchmark(Description = "MessagePack")]
    public void MessagePackSerializer()
    {
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
            var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
            _ = MessagePack.MessagePackSerializer.Deserialize<TestWithStrings>(serialize);
            
        }
    }
    [Benchmark(Description = "MemoryPack")]
    public void MemoryPackSerializer()
    {
        foreach (var obj in this._test)
        {
            var serialize = MemoryPack.MemoryPackSerializer.Serialize(obj);
            var deserialize = MemoryPack.MemoryPackSerializer.Deserialize<TestWithStrings>(serialize);
            
        }
    }
}