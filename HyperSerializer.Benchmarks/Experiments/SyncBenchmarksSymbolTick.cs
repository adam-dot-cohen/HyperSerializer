using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
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
//[MeanColumn]
[MemoryDiagnoser]
//[AsciiDocExporter]
//[CsvMeasurementsExporter]
//[HtmlExporter]
public class SyncBenchmarksSymbolTick
{
    private List<SymbolTick> _test;
    [Params(1_000_000)]
    public int iterations;
    [GlobalSetup]
    public void Setup()
    {
        if(this._test == null) this._test = new List<SymbolTick>();
            
        for (var i = this._test.Count; i < this.iterations; i++)
        {
            this._test.Add(new SymbolTick()
            {
                Timestamp = DateTime.Now,
                Bid = i,
                Ask = i,
                SymbolId = i
            });
        }
    }
    [Benchmark(Baseline = true, Description = "HyperSerializer")]
    public void HyperSerializerSync()
    {
        foreach (var obj in this._test)
        {
            var bytes = HyperSerializer<SymbolTick>.Serialize(obj);
            _ = HyperSerializer<SymbolTick>.Deserialize(bytes);
            
        }
    }


    [Benchmark(Description = "Protobuf")]
    public void ProtobufSerializer()
    {
        MemoryPool<byte>.Shared.Rent(1014);
        ArrayBufferWriter<byte> writer = new ArrayBufferWriter<byte>();
        foreach (var obj in this._test)
        {
            using var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);
            stream.Position = 0;
            _ = Serializer.Deserialize<SymbolTick>(stream);
            
        }
    }

    [Benchmark(Description = "MsgPack")]
    public void MessagePackSerializer()
    {
        foreach (var obj in this._test)
        {
            var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
            _ = MessagePack.MessagePackSerializer.Deserialize<SymbolTick>(serialize);
            
        }
    }

    
    [Benchmark(Description = "MemoryPack")]
    public void MemoryPackSerializer()
    {
        foreach (var obj in this._test)
        {
            var serialize = MemoryPack.MemoryPackSerializer.Serialize(obj);
            _ = MemoryPack.MemoryPackSerializer.Deserialize<SymbolTick>(serialize);
            
        }
    }
	//[Benchmark]
	//public void DataContractSerializer()
	//{
	//	var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(Test));
	//	using var stream = new MemoryStream();
	//	foreach (var obj in _test)
	//	{
	//		stream.Flush();
	//		stream.Position = 0;
	//		serializer.WriteObject(stream, obj);
	//		var deserialize = (Test)new System.Runtime.Serialization.DataContractSerializer(typeof(Test)).ReadObject(stream);
	//		
	//	}
	//}


	//[Benchmark]
	//public void ZeroFormatterSerializer()
	//{
	//    foreach (var obj in _test)
	//    {
	//        var serialize = ZeroFormatter.ZeroFormatterSerializer.Serialize(obj);
	//        Test deserialize = ZeroFormatter.ZeroFormatterSerializer.Deserialize<Test>(serialize);
	//        

	//    }
	//}


}
