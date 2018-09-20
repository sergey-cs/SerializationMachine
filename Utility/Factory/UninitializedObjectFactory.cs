﻿using System;

namespace SerializationMachine.Utility
{
    internal sealed class UninitializedObjectFactory : IFactory
    {
        private readonly Type TypeObject;

        public object Instantiate()
        {
            return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(TypeObject);
        }

        internal UninitializedObjectFactory(Type typeObject)
        {
            this.TypeObject = typeObject;
        }
    }
}
