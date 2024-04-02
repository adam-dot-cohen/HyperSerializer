//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//#if NET5_0_OR_GREATER
//using Apex.Serialization;
//#endif
//using BenchmarkDotNet.Attributes;
//using BenchmarkDotNet.Engines;
//using BenchmarkDotNet.Order;
//using Hyper;
//using MessagePack;
//using ProtoBuf;
//using Buffer = System.Buffer;

//namespace Hyper.Benchmarks.Experiments
//{
//    [SimpleJob(runStrategy: RunStrategy.Throughput, launchCount: 1, invocationCount: 1)]
//    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
//    [MemoryDiagnoser]

//    public class SyncBenchmarksWithStringsAndArrays
//    {
//        private List<TestObjectWithStringsAndArray> _test;
//        [Params(10, 100, 1_000, 10_000, 100_000, 1_000_000)]
//        public int iterations;
//        [GlobalSetup]
//        public void Setup()
//        {
//            if(_test == null)
//                _test = new List<TestObjectWithStringsAndArray>();
            
//            for (var i = _test.Count; i < iterations; i++)
//            {
//                _test.Add(new TestObjectWithStringsAndArray()
//                {
//                    A = i,
//                    B = i,
//                    C = DateTime.Now.Date,
//                    D = (uint)i,
//                    E = i,
//                    F = DateTime.Now - DateTime.Now.AddDays(-1),
//                    G = Guid.NewGuid(),
//                    H = TestEnum.three,
//                    I = i.ToString(),
//                    ArrayTest = new int[] { 1, 2, 3 },
//                    ListTest = new List<int> { 1, 2, 3 }
//                });
//            }
//        }

//        [Benchmark(Baseline = true)]
//        public void HyperSerializerSync()
//        {
//            foreach (var obj in _test)
//            {
//                var bytes = HyperSerializer<TestObjectWithStringsAndArray>.Serialize(obj);
//                var deserialize = HyperSerializer<TestObjectWithStringsAndArray>.Deserialize(bytes);
//                
//            }
//        }
       
//        [Benchmark]
//        public void ProtobufSerializer()
//        {
//            MemoryPool<byte>.Shared.Rent(1014);
//            foreach (var obj in _test)
//            {
//                using var stream = new MemoryStream();
//                Serializer.Serialize(stream, obj);
//                stream.Position = 0;
//                var deserialize = Serializer.Deserialize<TestObjectWithStringsAndArray>(stream);
//                
//            }
//        }

//        [Benchmark]
//        public void MessagePackSerializer()
//        {
//            foreach (var obj in _test)
//            {
//                var serialize = MessagePack.MessagePackSerializer.Serialize(obj);
//                var deserialize = MessagePack.MessagePackSerializer.Deserialize<TestObjectWithStringsAndArray>(serialize);
//                
//            }
//        }
//#if NET5_0_OR_GREATER
//        [Benchmark]
//        public void ApexSerializer()
//        {

//            var _binary = Binary.Create(new Settings { UseSerializedVersionId = false }.MarkSerializable(x => true));
//            foreach (var obj in _test)
//            {
//                using var stream = new MemoryStream();
//                _binary.Write(obj, stream);
//                stream.Position = 0;
//                var deserialize = _binary.Read<TestObjectWithStringsAndArray>(stream);
//                
//            }
//        }

//    }
//    #endif
//    //[Benchmark]
//    //public void FASTERSerializerHeap_Bench()
//    //{
//    //    foreach(var obj in _test)
//    //    {
//    //        HyperSerializer.SerializeAsync(out var bytes, obj);
//    //        TestWithStrings deserialize = HyperSerializer.DeserializeAsync(bytes);
//    //        Debug.Assert(deserialize.E == obj.E);
//    //    }
//    //}

//    //[Benchmark]
//    //public void BinarySerializerV2Bench()
//    //{
//    //    foreach(var obj in _test)
//    //    {
//    //        HyperSerializerUnsafe<TestWithStrings>.Serialize(out Span<byte> bytes, obj);
//    //        TestWithStrings deserialize = HyperSerializerUnsafe<TestWithStrings>.Deserialize(bytes);
//    //        Debug.Assert(deserialize.E == obj.E);
//    //    }
//    //}

//    //[Benchmark]
//    //public void BinarySerializerAsyncV2Bench()
//    //{
//    //    foreach(var obj in _test)
//    //    {
//    //        HyperSerializerUnsafe<TestWithStrings>.SerializeAsync(out var bytes, obj);
//    //        TestWithStrings deserialize = HyperSerializerUnsafe<TestWithStrings>.DeserializeAsync(bytes);
//    //        Debug.Assert(deserialize.E == obj.E);
//    //    }
//    //}

//}

