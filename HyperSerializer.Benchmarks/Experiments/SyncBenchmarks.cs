
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using Hyper;
using Hyper.Benchmarks.Experiments;
using HyperSerializer;
using HyperSerializer.Benchmarks.Experiments.HyperSerializer;
using MessagePack;
using ProtoBuf;
using Buffer = System.Buffer;

namespace HyperSerializer.Benchmarks.Experiments;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1, runtimeMoniker: RuntimeMoniker.Net60)]
[HideColumns(Column.Error, Column.StdDev, Column.RatioSD)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
//[MeanColumn]
[MemoryDiagnoser]
//[AsciiDocExporter]
//[CsvMeasurementsExporter]
//[HtmlExporter]
public class SyncBenchmarks
{
    private Test _test;
    [Params(1_000_000)]
    public int iterations = 1_000_000;
    [GlobalSetup]
    public void Setup()
    {
        _test = new()
        {
            A = iterations,
            B = iterations,
            C = DateTime.Now.Date,
            D = (uint)iterations,
            E = iterations,
            F = DateTime.Now - DateTime.Now.AddDays(-1),
            G = Guid.NewGuid(),
            H = TestEnum.three,
            //  I = i.ToString()
        };
    }
    [Benchmark(Baseline = true, Description = "HyperSerializer")]
    public void HyperSerializerSync()
    {
        var bytes = HyperSerializer<Test>.Serialize(_test);

        _ = HyperSerializer<Test>.Deserialize(bytes);
    }


    [Benchmark(Description = "Protobuf")]
    public void ProtobufSerializer()
    {
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, _test);
        stream.Position = 0;
        _ = Serializer.Deserialize<Test>(stream);
    }

    [Benchmark(Description = "MsgPack")]
    public void MessagePackSerializer()
    {
        var serialize = MessagePack.MessagePackSerializer.Serialize(_test);

        _ = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
    }


    [Benchmark(Description = "MemoryPack")]
    public void MemoryPackSerializer()
    {
        var serialize = MemoryPack.MemoryPackSerializer.Serialize(_test);

        _ = MemoryPack.MemoryPackSerializer.Deserialize<Test>(serialize);
    }
}