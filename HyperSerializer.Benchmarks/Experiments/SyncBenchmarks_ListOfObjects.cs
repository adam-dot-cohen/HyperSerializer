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
using HyperSerializer;
using HyperSerializer.Benchmarks.Experiments.HyperSerializer;
using MessagePack;
using ProtoBuf;
using Buffer = System.Buffer;
#if NET6_0_OR_GREATER && !NET8_0
using Apex.Serialization;
#endif

namespace Hyper.Benchmarks.Experiments;

[SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1,
	runtimeMoniker: RuntimeMoniker.Net60)]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
//[MeanColumn]
[MemoryDiagnoser]
//[AsciiDocExporter]
//[CsvMeasurementsExporter]
//[HtmlExporter]
public class SyncBenchmarks_ListOfObjects
{
	private List<Test> _test;
	[Params(1_000_000)] public int iterations;

	[GlobalSetup]
	public void Setup()
	{
		if (this._test == null) this._test = new List<Test>();

		for (var i = this._test.Count; i < this.iterations; i++)
		{
			this._test.Add(new Test()
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

	[Benchmark(Baseline = true, Description = "HyperSerializer")]
	public void HyperSerializerSync()
	{
		foreach (var obj in this._test)
		{
			var bytes = HyperSerializer<Test>.Serialize(obj);
			Test deserialize = HyperSerializer<Test>.Deserialize(bytes);
			Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
		}
	}

	[Benchmark(Description = "HyperExperimental")]
	public void HyperSerializerLegacySync()
	{
		foreach (var obj in this._test)
		{
			var bytes = HyperSerializerExperimental<Test>.Serialize(obj);
			Test deserialize = HyperSerializerExperimental<Test>.Deserialize(bytes);
			Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
		}
	}

	[Benchmark(Description = "HyperUnsafe")]
	public void HyperSerializerUnsafe()
	{
		foreach (var obj in this._test)
		{
			var bytes = HyperSerializerUnsafe<Test>.Serialize(obj);
			Test deserialize = HyperSerializerUnsafe<Test>.Deserialize(bytes);
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
			var deserialize = Serializer.Deserialize<Test>(stream);
			Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
		}
	}

	[Benchmark(Description = "MsgPack")]
	public void MessagePackSerializer()
	{
		foreach (var obj in this._test)
		{
			var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
			Test deserialize = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
			Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
		}
	}


	[Benchmark(Description = "MemoryPack")]
	public void MemoryPackSerializer()
	{
		foreach (var obj in this._test)
		{
			var serialize = MemoryPack.MemoryPackSerializer.Serialize(obj);
			Test deserialize = MemoryPack.MemoryPackSerializer.Deserialize<Test>(serialize);
			Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
		}
	}

#if NET6_0_OR_GREATER && !NET8_0
    [Benchmark(Description = "Apex")]
    public void ApexSerializer()
    {

        var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
        foreach (var obj in this._test)
        {
            using var stream = new MemoryStream();
            _binary.Write(obj, stream);
            stream.Position = 0;
            var deserialize = _binary.Read<Test>(stream);
            Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        }
    }
#endif
}