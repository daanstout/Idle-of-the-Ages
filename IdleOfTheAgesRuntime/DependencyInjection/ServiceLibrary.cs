﻿// <copyright file="ServiceLibrary.cs" company="DaanStout">
// Copyright (c) DaanStout. All rights reserved.
// </copyright>

using IdleOfTheAgesLib;
using IdleOfTheAgesLib.DependencyInjection;

using System;
using System.Collections.Generic;

namespace IdleOfTheAgesRuntime.DependencyInjection {
    /// <summary>
    /// A library where services that are registered through a <see cref="IServiceRegistry"/> can be obtained from.
    /// </summary>
    public class ServiceLibrary : IServiceLibrary, IServiceRegistry {
        private struct ResolverKey : IEquatable<ResolverKey> {
            public Type Type { get; set; }

            public string? Key { get; set; }

            public ResolverKey(Type type, string? key) {
                Type = type;
                Key = key;
            }

            public override readonly string ToString() => $"Type: {Type.FullName} - Key: {Key}";

            public override readonly bool Equals(object? obj) => obj is ResolverKey key && Equals(key);

            public readonly bool Equals(ResolverKey other) => EqualityComparer<Type>.Default.Equals(Type, other.Type) && Key == other.Key;

            public override readonly int GetHashCode() => HashCode.Combine(Type, Key);

            public static bool operator ==(ResolverKey left, ResolverKey right) => left.Equals(right);

            public static bool operator !=(ResolverKey left, ResolverKey right) => !(left == right);

            public static implicit operator ResolverKey((Type, string?) tuple) => new ResolverKey(tuple.Item1, tuple.Item2);
        }

        private readonly Dictionary<ResolverKey, IResolver> resolvers = new Dictionary<ResolverKey, IResolver>();
        private readonly IServiceLibrary? parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceLibrary"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="IServiceLibrary"/>.</param>
        public ServiceLibrary(IServiceLibrary? parent) {
            this.parent = parent;

            Bind<IServiceLibrary>().ToInstance(this);
        }

        /// <inheritdoc/>
        public IResolver Bind(Type type, string? key = null) {
            if (type is null) {
                throw new ArgumentNullException(nameof(type));
            }

            ResolverKey resolverKey = (type, key);

            if (!resolvers.TryGetValue(resolverKey, out var resolver)) {
                resolver = (IResolver)Activator.CreateInstance(typeof(Resolver<>).MakeGenericType(type));
                resolvers[resolverKey] = resolver;
            }

            return resolver;
        }

        /// <inheritdoc/>
        public IResolver<TType> Bind<TType>(string? key = null)
                where TType : class {
            return (IResolver<TType>)Bind(typeof(TType), key);
        }

        /// <inheritdoc/>
        public TType Get<TType>(string? key = null)
            where TType : class {
            return (TType)Get(typeof(TType), key);
        }

        /// <inheritdoc/>
        public object Get(Type type, string? key = null) {
            if (type is null) {
                throw new ArgumentNullException(nameof(type));
            }

            if (!resolvers.TryGetValue((type, key), out var resolver)) {
                return parent?.Get(type, key) ?? throw new ArgumentException($"No resolver for type {type.FullName} with key {key} exists!");
            }

            return resolver.Resolve(this);
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetAllServiceNames() {
            foreach (var key in resolvers.Keys)
                yield return key.Type.FullName;

            if (parent != null)
                foreach (var serviceName in parent.GetAllServiceNames())
                    yield return serviceName;
        }

        /// <inheritdoc/>
        public Result RegisterService<TInterface, TImplementation>(string? key = null)
                where TInterface : class
                where TImplementation : class {
            return RegisterService(typeof(TInterface), typeof(TImplementation), key);
        }

        /// <inheritdoc/>
        public Result RegisterService<TInterface, TImplementation>(Func<IServiceLibrary, TInterface>? factory, string? key = null)
                where TInterface : class
                where TImplementation : class {
            return RegisterService(typeof(TInterface), typeof(TImplementation), factory, key);
        }

        /// <inheritdoc/>
        public Result RegisterService<TInterface>(Func<IServiceLibrary, TInterface> factory, string? key = null) where TInterface : class {
            return RegisterService(typeof(TInterface), factory, key);
        }

        /// <inheritdoc/>
        public Result RegisterService(Type interfaceType, Func<IServiceLibrary, object> factory, string? key = null) {
            return RegisterService(interfaceType, null, factory, key);
        }

        /// <inheritdoc/>
        public Result RegisterService(Type interfaceType, Type implementationType, string? key = null) {
            return RegisterService(interfaceType, implementationType, null, key);
        }

        /// <inheritdoc/>
        public Result RegisterService(Type interfaceType, Type? implementationType, Func<IServiceLibrary, object>? factory, string? key = null) {
            if (interfaceType is null) {
                return (false, "Interface type is null!", new ArgumentNullException(nameof(interfaceType)));
            }

            if (!interfaceType.IsInterface && !interfaceType.IsAbstract) {
                return (false, $"{interfaceType.FullName} is not an interface or abstract class!", new ArgumentException($"{nameof(interfaceType)} of {interfaceType.FullName} is not an interface or an abstract class!"));
            }

            if (implementationType != null && (implementationType.IsInterface || implementationType.IsAbstract)) {
                return (false, $"{implementationType.FullName} is not an instantiable class!", new ArgumentException($"{nameof(implementationType)} of {implementationType.FullName} is an interface or an abstract class!"));
            }

            ResolverKey resolverKey = (interfaceType, key);

            if (resolvers.ContainsKey(resolverKey)) {
                return (false, $"Service with key ({resolverKey}) already exists!");
            }

            var resolver = (IResolver)Activator.CreateInstance(typeof(Resolver<>).MakeGenericType(interfaceType), implementationType);
            resolver.Factory = factory;
            resolvers[resolverKey] = resolver;
            return true;
        }

        /// <inheritdoc/>
        public virtual bool ContainsService<TType>(string? key = null) {
            return ContainsService(typeof(TType), key);
        }

        /// <inheritdoc/>
        public virtual bool ContainsService(Type type, string? key = null) {
            return resolvers.ContainsKey((type, key)) || (parent?.ContainsService(type, key) ?? false);
        }
    }
}
