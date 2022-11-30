using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hyper.Test
{
    public class SerializerTestsAsync : TestBase
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test_Struct_Equality()
        {
            var testSruct = new TestStruct() { T1 = 4, T2 = 4 };
            RoundTripEquality<TestStruct>(testSruct);
        }

        [Test]
        public void Test_ObjectWithTestStruct_Equality()
        {
            var i = new Random().Next(int.MaxValue);
            var testObj = new TestObjectWithTestStruct()
            {
                A = i,
                B = i,
                C = DateTime.Now.Date,
                D = (uint)i,
                E = i,
                F = DateTime.Now - DateTime.Now.AddDays(-1),
                G = Guid.NewGuid(),
                H = TestEnum.three,
                I = i.ToString(),
                Ts = new TestStruct() { T1 = i, T2 = i }
            };
            RoundTripComplexTypeEquality(testObj);
        }

        [Test]
        public void Test_SimpleType_Nullable_Equality()
        {
            RoundTripEquality<Guid?>(null);
            RoundTripEquality<DateTime?>(null);
            RoundTripEquality<DateTimeOffset?>(null);
            RoundTripEquality<TimeSpan?>(null);

            RoundTripEquality<ushort?>(null);
            RoundTripEquality<uint?>(null);
            RoundTripEquality<ulong?>(null);

            RoundTripEquality<sbyte?>(null);
            RoundTripEquality<byte?>(null);

            RoundTripEquality<char?>(null);
            RoundTripEquality<string?>(null);

            RoundTripEquality<short?>(null);
            RoundTripEquality<int?>(null);
            RoundTripEquality<long?>(null);

            RoundTripEquality<float?>(null);
            RoundTripEquality<double?>(null);
            RoundTripEquality<decimal?>(null);
        }

        [Test]
        public void Test_SimpleType_Equality()
        {
            RoundTripEquality(Guid.NewGuid());

            RoundTripEquality(DateTime.MaxValue);
            RoundTripEquality(DateTimeOffset.MaxValue);
            RoundTripEquality(TimeSpan.MaxValue);

            RoundTripEquality(ushort.MaxValue);
            RoundTripEquality(uint.MaxValue);
            RoundTripEquality(ulong.MaxValue);

            RoundTripEquality(sbyte.MaxValue);
            RoundTripEquality(byte.MaxValue);

            RoundTripEquality(char.MaxValue);
            RoundTripEquality("Test string");
            RoundTripEquality(string.Empty);

            RoundTripEquality(short.MaxValue);
            RoundTripEquality(int.MaxValue);
            RoundTripEquality(long.MaxValue);

            RoundTripEquality(float.MaxValue);
            RoundTripEquality(double.MaxValue);
            RoundTripEquality(decimal.MaxValue);
        }

        [Test]
        public void Test_ComplexType_Equality()
        {
            var i = new Random().Next(int.MaxValue);
            var testObj = new TestWithStrings()
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
            };
            RoundTripComplexTypeEquality(testObj);
        }
        [Test]
        public void Test_ComplexType_TypeVersionConfict_Should_Fail()
        {
            var i = new Random().Next(int.MaxValue);
            var testObj = new Test.TestWithStrings()
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
            };
            var serialized = HyperSerializer<TestWithStrings>.SerializeAsync(testObj).GetAwaiter().GetResult();
            var deserialize = HyperSerializer<Incompatible.TestWithStrings>.DeserializeAsync(serialized).GetAwaiter().GetResult();
            Assert.False(AllCommonPropertiesAreEqual(testObj, deserialize));

        }

        [Test]
        public void Test_ComplexType_TypeVersions_With_Same_Parameter_Order_should_pass()
        {
            var i = new Random().Next(int.MaxValue);
            var testObj = new Test.TestWithStrings()
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
            };
            var serialized = HyperSerializer<TestWithStrings>.SerializeAsync(testObj).GetAwaiter().GetResult();
            var deserialize = HyperSerializer<TestWithStringsV2>.DeserializeAsync(serialized).GetAwaiter().GetResult() as ITestWithStrings;
            Assert.True(AllPropertiesAreEqual((ITestWithStrings)testObj, deserialize));
        }

        [Test]
        public void Test_AttemptedBufferOverflowShould_Throw_OutOfRangeException()
        {
            try
            {
                
                var i = 1L << 32;
                Span<byte> buffer = default;
                MemoryMarshal.Write(buffer, ref i);
                var deserialize = HyperSerializer<int>.DeserializeAsync(buffer.ToArray()).GetAwaiter().GetResult();
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }

        [Test]
        public void Test_AttemptedBufferOverflowShould_DateTime_Throw_OutOfRangeException()
        {
            try
            {

                var i = 1L << 32;
                Span<byte> buffer = default;
                MemoryMarshal.Write(buffer, ref i);
                var deserialize = HyperSerializer<DateTime>.DeserializeAsync(buffer.ToArray()).GetAwaiter().GetResult();
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
        [Test]
        public void Test_AttemptedBufferOverflowShould_short_Throw_OutOfRangeException()
        {
            try
            {

                var i = 1L << 32;
                Span<byte> buffer = default;
                MemoryMarshal.Write(buffer, ref i);
                var deserialize = HyperSerializer<short>.DeserializeAsync(buffer.ToArray()).GetAwaiter().GetResult();
            }
            catch (ArgumentOutOfRangeException)
            {
                Assert.Pass();
            }
            Assert.Fail();
        }
    }
}