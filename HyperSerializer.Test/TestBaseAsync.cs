using System;
using System.Linq;
using NUnit.Framework;

namespace Hyper.Test;

public class TestBaseAsync
{
    protected void RoundTripEquality<T>(T value)
    {
        var serialized = HyperSerializer<T>.SerializeAsync(value).GetAwaiter().GetResult();
        var deserialize = HyperSerializer<T>.DeserializeAsync(serialized).GetAwaiter().GetResult();
        Assert.AreEqual(value, deserialize);
    }
    protected void RoundTripComplexTypeEquality<T>(T value)
    {
        var serialized = HyperSerializer<T>.SerializeAsync(value).GetAwaiter().GetResult();
        var deserialize = HyperSerializer<T>.DeserializeAsync(serialized).GetAwaiter().GetResult();
        Assert.True(this.AllPropertiesAreEqual(value, deserialize));
    }
    protected void RoundTripInequality<T>(T value)
    {
        var serialized = HyperSerializer<T>.SerializeAsync(value).GetAwaiter().GetResult();
        var deserialize = HyperSerializer<T>.DeserializeAsync(serialized).GetAwaiter().GetResult();
        Assert.AreNotEqual(value, deserialize);
    }

    protected bool AllPropertiesAreEqual<TObject, TObject2>(TObject obj, TObject2 value, params string[]? exclude)
    {
        foreach (var prop in typeof(TObject).GetProperties().Where(g => (exclude == null || exclude.All(q => q != g.Name))))
        {
            var prop2 = typeof(TObject2).GetProperty(prop.Name);
            if (!Object.Equals(prop.GetValue(obj), prop2?.GetValue(value)))
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