using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Hyper.Test;

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
        this.RoundTripEquality<TestStruct>(testSruct);
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
        this.RoundTripComplexTypeEquality(testObj);
    }

    [Test]
    public void Test_SimpleType_Nullable_Equality()
    {
        this.RoundTripEquality<Guid?>(null);
        this.RoundTripEquality<DateTime?>(null);
        this.RoundTripEquality<DateTimeOffset?>(null);
        this.RoundTripEquality<TimeSpan?>(null);

        this.RoundTripEquality<ushort?>(null);
        this.RoundTripEquality<uint?>(null);
        this.RoundTripEquality<ulong?>(null);

        this.RoundTripEquality<sbyte?>(null);
        this.RoundTripEquality<byte?>(null);

        this.RoundTripEquality<char?>(null);
        this.RoundTripEquality<string?>(null);

        this.RoundTripEquality<short?>(null);
        this.RoundTripEquality<int?>(null);
        this.RoundTripEquality<long?>(null);

        this.RoundTripEquality<float?>(null);
        this.RoundTripEquality<double?>(null);
        this.RoundTripEquality<decimal?>(null);
    }

    [Test]
    public void Test_SimpleType_Equality()
    {
        this.RoundTripEquality(Guid.NewGuid());

        this.RoundTripEquality(DateTime.MaxValue);
        this.RoundTripEquality(DateTimeOffset.MaxValue);
        this.RoundTripEquality(TimeSpan.MaxValue);

        this.RoundTripEquality(ushort.MaxValue);
        this.RoundTripEquality(uint.MaxValue);
        this.RoundTripEquality(ulong.MaxValue);

        this.RoundTripEquality(sbyte.MaxValue);
        this.RoundTripEquality(byte.MaxValue);

        this.RoundTripEquality(char.MaxValue);
        this.RoundTripEquality("Test string");
        this.RoundTripEquality(string.Empty);

        this.RoundTripEquality(short.MaxValue);
        this.RoundTripEquality(int.MaxValue);
        this.RoundTripEquality(long.MaxValue);

        this.RoundTripEquality(float.MaxValue);
        this.RoundTripEquality(double.MaxValue);
        this.RoundTripEquality(decimal.MaxValue);
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
        this.RoundTripComplexTypeEquality(testObj);
    }
    [Test]
    public void Test_ComplexType_TypeVersionConfict_Should_Fail()
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
        var serialized = HyperSerializer<TestWithStrings>.SerializeAsync(testObj).GetAwaiter().GetResult();
        var deserialize = HyperSerializer<Incompatible.TestWithStrings>.DeserializeAsync(serialized).GetAwaiter().GetResult();
        Assert.That(!this.AllCommonPropertiesAreEqual(testObj, deserialize));

    }

    [Test]
    public void Test_ComplexType_TypeVersions_With_Same_Parameter_Order_should_pass()
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
        var serialized = HyperSerializer<TestWithStrings>.SerializeAsync(testObj).GetAwaiter().GetResult();
        var deserialize = HyperSerializer<TestWithStringsV2>.DeserializeAsync(serialized).GetAwaiter().GetResult() as ITestWithStrings;
        Assert.That(this.AllPropertiesAreEqual((ITestWithStrings)testObj, deserialize));
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