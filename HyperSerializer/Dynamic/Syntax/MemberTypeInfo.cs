using System.Reflection;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.Serialization;
using Hyper;

namespace HyperSerializer.Dynamic.Syntax;

internal ref struct MemberTypeInfos<T>
{
    public Span<MemberTypeInfo> Members;
    public int Length;
    private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetField | BindingFlags.SetProperty;

    public ref MemberTypeInfo this[int index] => ref this.Members[index];

    public MemberTypeInfos()
    {
        var type = typeof(T);
        var members = type.GetFields(bindingFlags).Cast<MemberInfo>().ToArray();

		if(HyperSerializerSettings.SerializeFields)
			members = members.Concat(type.GetProperties(bindingFlags)).ToArray();

        this.Length = members.Length;

        this.Members = new MemberTypeInfo[members.Length];

        for (int i = 0; i < this.Length; i++)
        {
	        this.Members[i] = new MemberTypeInfo();

            if (members[i] is FieldInfo field)
            {
	            this.Members[i].PropertyType = field.FieldType;
	            this.Members[i].Name = field.Name;
	            this.Members[i].Ignore = members[i].IsDefined(typeof(IgnoreDataMemberAttribute));
                continue;
            }

            if (members[i] is PropertyInfo property)
            {
	            this.Members[i].PropertyType = property.PropertyType;
	            this.Members[i].Name = property.Name;
	            this.Members[i].Ignore = members[i].IsDefined(typeof(IgnoreDataMemberAttribute));
            }
        }
    }
}

internal class MemberTypeInfo
{
    public Type PropertyType;
    public bool Ignore;
    public string Name;
}