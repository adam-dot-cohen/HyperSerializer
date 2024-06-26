using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Hyper.Test.Incompatible;
using HyperSerializer;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Hyper.Test;

public class SerializerTests : TestBase
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test_String_Equality()
    {
        this.RoundTripEquality<string>("test");
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
    public void Test_Class_Equality()
    {
        var i = new Random().Next(int.MaxValue);
        var testObj = new PersonClass()
        {
            Name = i.ToString(),
            Age = i
        };
        this.RoundTripComplexTypeEquality(testObj);
    }

    [Test]
    public void Test_Class_1000_Iterations_Equality()
    {
        var rand = new Random();

        for (int i = 0; i < 1000; i++)
        {
            var val = rand.Next(int.MaxValue);
            var testObj = new PersonClass()
            {
                Name = val.ToString(),
                Age = val
            };
            this.RoundTripComplexTypeEquality(testObj);
        }
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
    public void Test_ComplexType_With_IngoreDataMember_Equality()
    {
	    var i = new Random().Next(int.MaxValue);
	    var testObj = new TestWithStrings_IntoreDataMember()
	    {
		    A = i,
		    B = i,
		    C = 1,
		    D = DateTime.Now.Date,
		    E = i,
		    F = DateTime.Now - DateTime.Now.AddDays(-1),
		    G = Guid.NewGuid(),
		    H = TestEnum.three
	    };
	    var serialized = HyperSerializer.Serialize(testObj);
	    var deserialize = HyperSerializer.Deserialize<TestWithStrings_IntoreDataMember>(serialized);
		
		Assert.That(testObj.C != deserialize.C);
    }

    [Test]
    public void Test_ComplexType_Equality_With_Stream()
    {
        MemoryStream ms = new MemoryStream();
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
        //HyperSerializer.HyperBinarySerializer<TestWithStrings>.Serialize(ms, testObj);
        //ms.Position = 0;
        //var obj2 = HyperBinarySerializer<TestWithStrings>.Deserialize(ms);
        //Assert.AreEqual(testObj.A, obj2.A);
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
        var serialized = HyperSerializer<TestWithStrings>.Serialize(testObj);
        var deserialize = HyperSerializer<Incompatible.TestWithStrings>.Deserialize(serialized);
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
        var serialized = HyperSerializer<TestWithStrings>.Serialize(testObj);
        var deserialize = HyperSerializer<TestWithStringsV2>.Deserialize(serialized) as ITestWithStrings;
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
            var deserialize = HyperSerializer<int>.Deserialize(buffer);
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
            var deserialize = HyperSerializer<DateTime>.Deserialize(buffer);
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
            var deserialize = HyperSerializer<short>.Deserialize(buffer);
        }
        catch (ArgumentOutOfRangeException)
        {
            Assert.Pass();
        }
        Assert.Fail();
    }


    [Test]
    public void Test_Class_With_Nullable_Byte_Array_Should_Serialize()
    {
        var content = new ScreenshotContent
        {
            Width = 100,
            Height = 200,
            ImageBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7 }
        };

        var serialized = HyperSerializer.Serialize(content);

        var deserialized = HyperSerializer.Deserialize<ScreenshotContent>(serialized);

        Assert.That(deserialized.ImageBytes.SequenceEqual(content.ImageBytes));
    }

    [Test]
    public void Test_Struct_Array_Should_Serialize()
    {

	    HyperSerializerSettings.WriteProxyToConsoleOutput = true;

        var numRecords = 1000;

        SymbolTick[] ticks = new SymbolTick[numRecords];

        for (var i = 0; i < numRecords; i++)
            ticks[i] = new SymbolTick
            {
                Timestamp = i,
                Bid = 1.23456,
                Ask = 1.12345,
                SymbolId = 12
            };

        var tickArray = HyperSerializer.Serialize(ticks);

        var hsOut = HyperSerializer.Deserialize<SymbolTick[]>(tickArray);

        for (var i = 0; i < numRecords; i++) Assert.That(base.AllCommonPropertiesAreEqual(ticks[i], hsOut[i]));
    }

}