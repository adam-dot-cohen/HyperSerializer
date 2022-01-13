using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using Apex.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Order;
using HyperSerialize;
using MessagePack;
using ProtoBuf;
using Buffer = System.Buffer;

namespace HyperSerialize.Benchmarks.Experiments
{
    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1)]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [MeanColumn]
    [MemoryDiagnoser]
    [AsciiDocExporter]
    [CsvMeasurementsExporter]
    [HtmlExporter]
    [RPlotExporter]
    public class SyncBenchmarks
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
                    E = i,
                    F = DateTime.Now - DateTime.Now.AddDays(-1),
                    G = Guid.NewGuid(),
                    H = TestEnum.three,
                    //  I = i.ToString()
                });
            }
        }
        [Benchmark(Baseline = true, Description = "Hyper")]
        public void HyperSerializerSync()
        {
            foreach (var obj in _test)
            {
                var bytes = HyperSerializer<Test>.Serialize(obj);
                Test deserialize = HyperSerializer<Test>.Deserialize(bytes);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }
        //[Benchmark(Description = "HyperUnsafe")]
        //public void HyperSerializerUnsafe()
        //{
        //    foreach (var obj in _test)
        //    {
        //        var bytes = HyperSerializerUnsafe<Test>.Serialize(obj);
        //        Test deserialize = HyperSerializerUnsafe<Test>.Deserialize(bytes);
        //        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        //    }
        //}
       
        [Benchmark(Description = "Protobuf")]
        public void ProtobufSerializer()
        {
            MemoryPool<byte>.Shared.Rent(1014);
            foreach (var obj in _test)
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
            foreach (var obj in _test)
            {
                var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
                Test deserialize = MessagePack.MessagePackSerializer.Deserialize<Test>(serialize);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }
        [Benchmark(Description="Apex")]
        public void ApexSerializer()
        {

            var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
            foreach (var obj in _test)
            {
                using var stream = new MemoryStream();
                _binary.Write(obj, stream);
                stream.Position = 0;
                var deserialize = _binary.Read<Test>(stream);
                Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
            }
        }
        //[Benchmark]
        //public void DataContractSerializer()
        //{
        //    foreach (var obj in _test)
        //    {
        //        using var stream = new MemoryStream();
        //        new System.Runtime.Serialization.DataContractSerializer(typeof(Test)).WriteObject(stream, obj);
        //        stream.Flush();
        //        stream.Position = 0;
        //        var deserialize = (Test)new System.Runtime.Serialization.DataContractSerializer(typeof(Test)).ReadObject(stream);
        //        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        //    }
        //}
        //[Benchmark]
        //public void BinaryFormatterSerializer()
        //{
        //    foreach (var obj in _test)
        //    {
        //        using var stream = new MemoryStream();
        //        new BinaryFormatter().Serialize(stream, obj);
        //        stream.Flush();
        //        stream.Position = 0;
        //        var deserialize = (Test)new BinaryFormatter().Deserialize(stream);
        //        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
        //    }
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
    //[Benchmark]
    //public void FASTERSerializerHeap_Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializer<Test>.SerializeAsync(out var bytes, obj);
    //        Test deserialize = HyperSerializer<Test>.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializerUnsafe<Test>.Serialize(out Span<byte> bytes, obj);
    //        Test deserialize = HyperSerializerUnsafe<Test>.Deserialize(bytes);
    //        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
    //    }
    //}

    //[Benchmark]
    //public void BinarySerializerAsyncV2Bench()
    //{
    //    foreach(var obj in _test)
    //    {
    //        HyperSerializerUnsafe<Test>.SerializeAsync(out var bytes, obj);
    //        Test deserialize = HyperSerializerUnsafe<Test>.DeserializeAsync(bytes);
    //        Debug.Assert(deserialize.GetHashCode() == obj.GetHashCode());
    //    }
    //}

}

