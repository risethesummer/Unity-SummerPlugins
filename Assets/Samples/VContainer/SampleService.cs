using System;
using System.Collections.Generic;
using VContainer.Unity;

namespace Samples.VContainer
{
    
    public class SampleService : IDisposable
    {
        private readonly List<LifetimeScope> _scopes = new();
        private readonly IEnumerable<Tester> _testers;
        public SampleService(LifetimeScope lifetimeScope, IEnumerable<Tester> testers)
        {
            _testers = testers;
            _scopes.Add(lifetimeScope);
        }
        public void SampleDo()
        {
            var lastScope = _scopes[^1];
            var newScope = lastScope.CreateChild();
            _scopes.Add(newScope);
        }
        public void Dispose()
        {
            foreach (var scope in _scopes)
                scope.Dispose();
            _scopes.Clear();
        }
    }
}