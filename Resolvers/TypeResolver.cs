﻿using System;
using System.Xml.Linq;
using SerializeMachine.Utility;

namespace SerializeMachine.Resolvers
{
    public class TypeResolver : Core.IResolver
    {
        private readonly Core.Serializator Serializator;

        public TypeResolver(Core.Serializator serializator)
            :base(TypeOf<Type>.Type)
        {
            this.Serializator = serializator; 
        }

        public override void Deserialzie(XElement serializedObject, ref object instance){ }

        public override void Serialize(XElement serialized, object resolveObject)
        {
            string convention;
            var value = resolveObject as Type;

            if(Serializator.TypeManager.Dictionary.TryGetConvention(value,out convention))
                serialized.Value = "@" + convention;
            else
                serialized.Value = value.AssemblyQualifiedName;

        }

        protected internal override object ManagedObjectOf(XElement serializedObject)
        {
            if (serializedObject.Value[0] == '@')
                return Serializator.TypeManager.Dictionary.TypeOf(serializedObject.Value.Remove(0, 1));
            else
                return Type.GetType(serializedObject.Value);
        }
    }
}