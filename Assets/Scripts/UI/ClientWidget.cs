using Mirror;
using MirrorNetTest.Core.Messages;
using MirrorNetTest.Services.MessageProxyService;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MirrorNetTest.UI
{
    public class ClientWidget : WidgetBase
    {
        [SerializeField]
        private Toggle _toggle;

        [SerializeField]
        private Button _connectBtn;
        [SerializeField]
        private Button _disconnectBtn;
        private MessageProxyService _messageProxy;

        [Inject]
        public void Inject(MessageProxyService messageProxy)
        {
            _messageProxy = messageProxy;
        }

        private void OnConnectBtnClick()
        {
            if (_toggle.isOn)
            {
                NetworkClient.RegisterHandler<HelloMessage>(HelloMessageHandler);
            }
            else
            {
                NetworkClient.RegisterHandler<AltHelloMessage>(AltHelloMessageHandler);
            }

            // после всех подписок обновляем набор разрешённых msgId для фильтра
            MessageProxyService.UpdateActiveSubscriptions();

            NetworkManager.singleton.StartClient();
            _messageProxy.Init();
        }

        private void OnDisconnectBtnClick()
        {
            NetworkClient.UnregisterHandler<HelloMessage>();
            NetworkClient.UnregisterHandler<AltHelloMessage>();

            // после всех отписок обновляем набор разрешённых msgId для фильтра
            MessageProxyService.UpdateActiveSubscriptions();

            NetworkManager.singleton.StopClient();
            _messageProxy.Dispose();

        }

        public void HelloMessageHandler(HelloMessage message)
        {
            Debug.Log($"Received: {message.Message}");
        }
        public void AltHelloMessageHandler(AltHelloMessage message)
        {
            Debug.Log($"Received ALT: {message.Message}");
        }

        protected override void RegisterListeners()
        {
            _connectBtn.onClick.AddListener(OnConnectBtnClick);
            _disconnectBtn.onClick.AddListener(OnDisconnectBtnClick);
        }

        protected override void RemoveListeners()
        {
            _connectBtn.onClick.RemoveAllListeners();
            _disconnectBtn.onClick.RemoveAllListeners();
        }
    }
}