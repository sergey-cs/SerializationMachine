﻿using System;

namespace SerializationMachine.Resolver
{
    public sealed class ResolverBank
    {
        private readonly ResolverStorage ResolverStorage;
        private readonly ResolverFacrory ResolverFactory;

        public IResolver GetResolver(string convention)
        {
            var result = ResolverStorage.GetResolver(convention);
            if (result == null)
            {
                result = ResolverFactory.CreateResolver(convention);
                ResolverStorage.SetResolver(result, convention);
            }
            return result;
        }

        public ResolverBank(ResolverStorage resolverStorage, ResolverFacrory resolverFactory)
        {
            this.ResolverStorage = resolverStorage;
            this.ResolverFactory = resolverFactory;
        }

        public ResolverBank(Serializator serializator)
        {
            ResolverStorage = new ResolverStorage(serializator.GetTypeManager().UsingDictionary);
            ResolverFactory = new ResolverFacrory(serializator);
        }

        public void SetResolver(string convention,IResolver resolver)
        {
            ResolverStorage.SetResolver(resolver, convention);
        }
        public ResolverFacrory Factory { get { return ResolverFactory; } }
        public ResolverStorage Storage { get { return ResolverStorage; } }
    }
}
