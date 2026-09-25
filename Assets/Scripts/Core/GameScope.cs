using MirrorNetTest.Services.MessageProxyService;
using MirrorNetTest.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MirrorNetTest.Core
{
    public class GameScope : LifetimeScope
    {
        [SerializeField]
        private ClientWidget _clientWidget;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<MessageProxyService>(Lifetime.Singleton);
            builder.RegisterComponent(_clientWidget);
        }
    }
}