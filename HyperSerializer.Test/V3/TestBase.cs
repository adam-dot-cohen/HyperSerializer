using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hyper.Test;

public class TestBaseV3
{
    protected void RoundTripEquality<T>(T value)
    {
        var serialized = HyperSerializer<T>.Serialize(value);
        var deserialize = HyperSerializer<T>.Deserialize(serialized);

        if (value is null)
        {
            Assert.That(deserialize == null);
            return;
        }

        Assert.That(Equals(value, deserialize));
    }
    protected void RoundTripComplexTypeEquality<T>(T value)
    {
        var serialized = HyperSerializer<T>.Serialize(value);
        T deserialize = HyperSerializer<T>.Deserialize(serialized);
        Assert.That(this.AllPropertiesAreEqual(value, deserialize));
    }
    protected void RoundTripInequality<T>(T value)
    {
        var serialized = HyperSerializer<T>.Serialize(value);
        T deserialize = HyperSerializer<T>.Deserialize(serialized);
        Assert.That(!Equals(value, deserialize));
    }

    private bool Equals<T>(T val1, T val2)
        => EqualityComparer<T>.Default.Equals(val1, val2);

    protected bool AllPropertiesAreEqual<TObject, TObject2>(TObject obj, TObject2 value, params string[]? exclude)
    {
        foreach (var prop in typeof(TObject).GetProperties().Where(g => (exclude == null || exclude.All(q => q != g.Name))))
        {
            var prop2 = typeof(TObject2).GetProperty(prop.Name);
            if (!Equals(prop.GetValue(obj), prop2?.GetValue(value)) && (prop is Enumerable && !Enumerable.SequenceEqual((IEnumerable<object>)prop.GetValue(obj), (IEnumerable<object>)prop2.GetValue(value))))
                return false;
        }
        return true;
    }
    protected bool AllCommonPropertiesAreEqual<TObject, TObject2>(TObject obj, TObject2 value, params string[]? exclude)
    {
        foreach (var prop in typeof(TObject).GetProperties().Where(g => typeof(TObject2).GetProperties().Any(h => h.Name == g.Name) && (exclude == null || exclude.All(q => q != g.Name))))
        {
            var prop2 = typeof(TObject2).GetProperty(prop.Name);
            if (!prop.GetValue(obj)!.Equals(prop2!.GetValue(value)))
                return false;
        }
        return true;
    }
}