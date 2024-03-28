
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


#if NET6_0_OR_GREATER && !NET8_0
using Apex.Serialization;
#endif

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

        Test deserialize = HyperSerializer<Test>.Deserialize(bytes);
    }


    [Benchmark(Description = "Protobuf")]
    public void ProtobufSerializer()
    {
        MemoryPool<byte>.Shared.Rent(1024);
        ArrayBufferWriter<byte> writer = new ArrayBufferWriter<byte>();
        using var stream = new MemoryStream();
        Serializer.Serialize(stream, _test);
        stream.Position = 0;
        Test deserialize = Serializer.Deserialize<Test>(stream);
    }

    [Benchmark(Description = "MsgPack")]
    public void MessagePackSerializer()
    {
        var serialize = MessagePack.MessagePackSerializer.Serialize(_test);

        Test deserialize = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
    }


    [Benchmark(Description = "MemoryPack")]
    public void MemoryPackSerializer()
    {
        var serialize = MemoryPack.MemoryPackSerializer.Serialize(_test);

        Test deserialize = MemoryPack.MemoryPackSerializer.Deserialize<Test>(serialize);
    }

#if NET6_0_OR_GREATER && !NET8_0
    [Benchmark(Description = "Apex")]
    public void ApexSerializer()
    {
        var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
        using var stream = new MemoryStream();
        _binary.Write(_test, stream);
        stream.Position = 0;
        var deserialize = _binary.Read<Test>(stream);
    }
#endif
}