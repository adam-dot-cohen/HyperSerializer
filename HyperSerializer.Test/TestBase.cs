using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Hyper.Test;

public class TestBase
{
	private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField | BindingFlags.SetProperty;
    protected void RoundTripEquality<T>(T value)
    {
        var serialized = HyperSerializer<T>.Serialize(value);
        var deserialize = HyperSerializer<T>.Deserialize(serialized);
        Assert.AreEqual(value, deserialize);
    }
    protected void RoundTripComplexTypeEquality<T>(T value)
    {
        var serialized = HyperSerializer<T>.Serialize(value);
        T deserialize = HyperSerializer<T>.Deserialize(serialized);
        Assert.True(this.AllPropertiesAreEqual(value, deserialize));
    }
    protected void RoundTripInequality<T>(T value)
    {
        var serialized = HyperSerializer<T>.Serialize(value);
        T deserialize = HyperSerializer<T>.Deserialize(serialized);
        Assert.AreNotEqual(value, deserialize);
    }

    protected bool AllPropertiesAreEqual<TObject, TObject2>(TObject obj, TObject2 value, params string[]? exclude)
    {
		var props = typeof(TObject).GetFields(bindingFlags).Cast<MemberInfo>()
		    .Concat(typeof(TObject).GetProperties(bindingFlags)).ToList();

        foreach (var prop in props
            .Where(g => (exclude == null || exclude.All(q => q != g.Name))))
        {
	        if (prop is FieldInfo field)
	        {
		        var prop2 = typeof(TObject2).GetField(prop.Name);
		        if (!Equals(field.GetValue(obj), prop2?.GetValue(value)))
			        return false;
	        }
	        if (prop is PropertyInfo property)
	        {
		        var prop2 = typeof(TObject2).GetProperty(prop.Name);
		        if (!Equals(property.GetValue(obj), prop2?.GetValue(value)))
			        return false;
	        }
        }
        return true;
    }
    protected bool AllCommonPropertiesAreEqual<TObject, TObject2>(TObject obj, TObject2 value, params string[]? exclude)
    {
	    var props = typeof(TObject).GetFields(bindingFlags).Cast<MemberInfo>()
		    .Concat(typeof(TObject).GetProperties(bindingFlags)).ToList();

	    var props2 = typeof(TObject2).GetFields(bindingFlags).Cast<MemberInfo>()
		    .Concat(typeof(TObject2).GetProperties(bindingFlags)).ToList();

        foreach (var prop in props
            .Where(g => props2.Any(h => h.Name == g.Name) && (exclude == null || exclude.All(q => q != g.Name))))
        {
	        if (prop is FieldInfo field)
	        {
		        var prop2 = typeof(TObject2).GetField(prop.Name);
		        if (!Equals(field.GetValue(obj), prop2?.GetValue(value)))
			        return false;
	        }
	        if (prop is PropertyInfo property)
	        {
		        var prop2 = typeof(TObject2).GetProperty(prop.Name);
		        if (!Equals(property.GetValue(obj), prop2?.GetValue(value)))
			        return false;
	        }
        }
        return true;
    }
}