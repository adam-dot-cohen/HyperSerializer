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
#if NET6_0_OR_GREATER && !NET8_0
using Apex.Serialization;
#endif

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
            SymbolTick deserialize = HyperSerializer<SymbolTick>.Deserialize(bytes);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
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
            SymbolTick deserialize = Serializer.Deserialize<SymbolTick>(stream);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }

    [Benchmark(Description = "MsgPack")]
    public void MessagePackSerializer()
    {
        foreach (var obj in this._test)
        {
            var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
            SymbolTick deserialize = MessagePack.MessagePackSerializer.Deserialize<SymbolTick>(serialize);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }

    
    [Benchmark(Description = "MemoryPack")]
    public void MemoryPackSerializer()
    {
        foreach (var obj in this._test)
        {
            var serialize = MemoryPack.MemoryPackSerializer.Serialize(obj);
            SymbolTick deserialize = MemoryPack.MemoryPackSerializer.Deserialize<SymbolTick>(serialize);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }

#if NET6_0_OR_GREATER && !NET8_0
    [Benchmark(Description="Apex")]
    public void ApexSerializer()
    {

        var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
        foreach (var obj in this._test)
        {
            using var stream = new MemoryStream();
            _binary.Write(obj, stream);
            stream.Position = 0;
            var deserialize = _binary.Read<SymbolTick>(stream);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }
#endif
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
	//		Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
	//	}
	//}


	//[Benchmark]
	//public void ZeroFormatterSerializer()
	//{
	//    foreach (var obj in _test)
	//    {
	//        var serialize = ZeroFormatter.ZeroFormatterSerializer.Serialize(obj);
	//        Test deserialize = ZeroFormatter.ZeroFormatterSerializer.Deserialize<Test>(serialize);
	//        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());

	//    }
	//}


}
