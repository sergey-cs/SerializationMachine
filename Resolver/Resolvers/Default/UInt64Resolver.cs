﻿using System.Xml.Linq;

namespace SerializationMachine.Resolver.Resolvers
{
    public sealed class UInt64Resolver : IResolver
    {
        public override void Serialize(XElement serialized, object obj)
        {
            serialized.Value = obj.ToString();
        }
        public override void Deserialzie(XElement serialized,ref object instance)
        {
            instance = System.UInt64.Parse(serialized.Value);
        }
        public UInt64Resolver() : base(TypeOf<ulong>.Type) { }
        protected internal override object GetTemplateInstance(XElement serializedObject)
        {
            return (ulong)0;
        }
    }
}
