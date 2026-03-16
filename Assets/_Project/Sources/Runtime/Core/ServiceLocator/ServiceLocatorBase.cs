using System;
using System.Collections.Generic;

namespace Sources.Runtime.Core.ServiceLocator
{
    public abstract class ServiceLocatorBase : IServiceLocator, IDisposable
    {
        protected readonly Dictionary<Type, IService> _services = new();

        public T GetService<T>() where T : IService
        {
            if (_services.TryGetValue(typeof(T), out var service) == true)
                return (T)service;

            return default;
        }

        public bool TryRegisterService<TContract, TImplementation>(TImplementation service)
            where TContract : class, IService
            where TImplementation : class, TContract
        {
            var contractType = typeof(TContract);

            if (_services.TryAdd(contractType, service) == false)
                return false;

            var implementationType = typeof(TImplementation);

            if (implementationType != contractType)
            {
                if (_services.TryAdd(implementationType, service) == false)
                {
                    _services.Remove(contractType);
                    
                    return false;
                }
            }

            return true;
        }
        
        public bool TryRegisterService<T>(T service) where T : class, IService => TryRegisterService<T, T>(service);

        public bool TryUnregisterService<TContract, TImplementation>(TImplementation service)
            where TContract : class, IService
            where TImplementation : class, TContract
        {
            var contractType = typeof(TContract);
            var isRegistrationExists = _services.TryGetValue(contractType, out var existing);

            if (isRegistrationExists == false || ReferenceEquals(existing, service) == false)
                return false;

            var isContractRemoved = _services.Remove(contractType);
            var implementationType = typeof(TImplementation);

            if (isContractRemoved == false && implementationType != contractType)
            {
                if (TryUnregisterService<TContract, TImplementation>(service) == false)
                {
                    _services.Add(contractType, service);

                    return false;
                }
            }

            return true;
        }
        
        public bool TryUnregisterService<T>(T service) where T : class, IService => TryUnregisterService<T, T>(service);
        
        public void Dispose()
        {
            var services = new HashSet<IService>(_services.Values);

            foreach (var service in services)
            {
                if (service is IDisposable disposable)
                    disposable.Dispose();
            }

            _services.Clear();
        }
    }
}