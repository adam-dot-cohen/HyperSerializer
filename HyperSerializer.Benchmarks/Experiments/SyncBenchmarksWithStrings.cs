﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
#if NET5_0_OR_GREATER
using Apex.Serialization;
#endif
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Hyper;
using HyperSerializer;
using HyperSerializer.Benchmarks.Experiments.HyperSerializer;
using MessagePack;
using ProtoBuf;
using Buffer = System.Buffer;

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

    [Benchmark(Baseline = true)]
    public void HyperSerializerSync()
    {
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
            var bytes = HyperSerializer<TestWithStrings>.Serialize(obj);
            TestWithStrings deserialize = HyperSerializer<TestWithStrings>.Deserialize(bytes);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }

       
    [Benchmark]
    public void ProtobufSerializer()
    {
        MemoryPool<byte>.Shared.Rent(1014);
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
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
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
            var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
            TestWithStrings deserialize = MessagePack.MessagePackSerializer.Deserialize<TestWithStrings>(serialize);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }
    [Benchmark(Description = "MemoryPack")]
    public void MemoryPackSerializer()
    {
        foreach (var obj in this._test)
        {
            var serialize = MemoryPack.MemoryPackSerializer.Serialize(obj);
            var deserialize = MemoryPack.MemoryPackSerializer.Deserialize<TestWithStrings>(serialize);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }
#if NET5_0_OR_GREATER
    [Benchmark]
    public void ApexSerializer()
    {

        var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
        for (var i = 0; i < this.iterations; i++)
        {
            var obj = this._test[i];
            using var stream = new MemoryStream();
            _binary.Write(obj, stream);
            stream.Position = 0;
            var deserialize = _binary.Read<TestWithStrings>(stream);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }
#endif
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

//}

