using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Samples.VContainer
{
    public class CompositeTester : Tester
    {
        private readonly IEnumerable<Tester> _testers;
        public CompositeTester(string name, IEnumerable<Tester> testers) : base(name)
        {
            _testers = testers;
        }
    }
    public class Tester
    {
        public Tester(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }

    public class SampleLifetimeScope : LifetimeScope
    {
        [SerializeField] private SampleView view;

        // public static void RegisterFacade<TFacade, TInstaller>(this IContainerBuilder containerBuilder, 
        //     Lifetime lifetime,
        //     LifetimeScope scope, TInstaller installer, LifetimeScope childPrefab = null) where TInstaller : IInstaller where TFacade : class
        // {
        //     containerBuilder.Register(_ =>
        //     {
        //         var childScope = childPrefab is not null ? scope.CreateChildFromPrefab(childPrefab, installer) : scope.CreateChild(installer);
        //         var facade = childScope.Container.Resolve<TFacade>();
        //         return facade;
        //     }, lifetime);
        // }
        //
        // public static void RegisterComposite<TType, TChild, TBind>(this IContainerBuilder containerBuilder, 
        //     Lifetime lifetime, Func<>) 
        //     where TType : class, TChild where TChild : class
        // {
        //     containerBuilder.Register<TType>(b =>
        //     {
        //         var children = b.Resolve<TChild>();
        //         b.Resolve()
        //         return b.
        //     }, lifetime).As<TBind>();
        // }
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<SamplePresenter>();
            CreateChild(e => {
                e.Register<Tester>(Lifetime.Singleton);
            }).CreateChild(e => {
                e.Register<Tester>(Lifetime.Singleton);
            });
            builder.Register(_ => new Tester("A"), Lifetime.Scoped);
            builder.Register(_ => new Tester("B"), Lifetime.Scoped);
            builder.Register(b =>
            {
                var children = b.Resolve<IEnumerable<Tester>>();
                return new CompositeTester("C", children);
            }, Lifetime.Scoped).As<Tester>();
            builder.Register<SampleService>(Lifetime.Singleton);
            builder.RegisterComponent(view);
        }
    }
}
