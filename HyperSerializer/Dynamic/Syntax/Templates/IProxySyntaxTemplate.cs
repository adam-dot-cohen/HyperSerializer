﻿namespace HyperSerializer.Dynamic.Syntax.Templates;

internal interface IProxySyntaxTemplate
{
    string PropertyTemplateSerialize { get; }
    string PropertyTemplateDeserialize { get; }
    string PropertyTemplateDeserializeLocal { get; }
    string PropertyTemplateSerializeNullable { get; }
    string PropertyTemplateDeserializeNullable { get; }
    string PropertyTemplateSerializeVarLenStr { get; }
    string PropertyTemplateDeserializeVarLenStr { get; }
    string PropertyTemplateSerializeVarLenArr { get; }
    string PropertyTemplateDeserializeVarLenArr { get; }
    string PropertyTemplateDeserializeVarLenList { get; }
    string PropertyTemplateSerializeListLen { get; }
    string PropertyTemplateSerializeArrLen { get; }
    string StringLength { get; }
    string StringLengthSpan { get; }
    string ClassTemplate { get; }
}