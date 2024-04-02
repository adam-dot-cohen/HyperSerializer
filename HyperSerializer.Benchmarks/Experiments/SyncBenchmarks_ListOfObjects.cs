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
				H = TestEnum.three
			});
		}
	}

	[Benchmark(Baseline = true, Description = "HyperSerializer")]
	public void HyperSerializerSync()
	{
		foreach (var obj in this._test)
		{
			var bytes = HyperSerializer<Test>.Serialize(obj);
			_ = HyperSerializer<Test>.Deserialize(bytes);
		}
	}

	[Benchmark(Description = "HyperExperimental")]
	public void HyperSerializerLegacySync()
	{
		foreach (var obj in this._test)
		{
			var bytes = HyperSerializerExperimental<Test>.Serialize(obj);
			_ = HyperSerializerExperimental<Test>.Deserialize(bytes);
		}
	}

	[Benchmark(Description = "HyperUnsafe")]
	public void HyperSerializerUnsafe()
	{
		foreach (var obj in this._test)
		{
			var bytes = HyperSerializerUnsafe<Test>.Serialize(obj);
			_ = HyperSerializerUnsafe<Test>.Deserialize(bytes);
		}
	}

	[Benchmark(Description = "Protobuf")]
	public void ProtobufSerializer()
	{
		foreach (var obj in this._test)
		{
			using var stream = new MemoryStream();
			Serializer.Serialize(stream, obj);
			stream.Position = 0;
			_ = Serializer.Deserialize<Test>(stream);
		}
	}

	[Benchmark(Description = "MessagePack")]
	public void MessagePackSerializer()
	{
		foreach (var obj in this._test)
		{
			var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
			_ = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
		}
	}


	[Benchmark(Description = "MemoryPack")]
	public void MemoryPackSerializer()
	{
		foreach (var obj in this._test)
		{
			var serialize = MemoryPack.MemoryPackSerializer.Serialize(obj);
			_ = MemoryPack.MemoryPackSerializer.Deserialize<Test>(serialize);
		}
	}
}