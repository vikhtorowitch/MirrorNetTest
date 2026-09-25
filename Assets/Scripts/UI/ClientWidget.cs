using Mirror;
using MirrorNetTest.Core.Messages;
using MirrorNetTest.Services.MessageProxyService;
using TMPro;
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
        
        [SerializeField]
        private TMP_Text _receivedLabel;


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

            MessageProxyService.UpdateActiveSubscriptions();

            NetworkManager.singleton.StartClient();
            _messageProxy.Init();
        }

        private void OnDisconnectBtnClick()
        {
            NetworkClient.UnregisterHandler<HelloMessage>();
            NetworkClient.UnregisterHandler<AltHelloMessage>();

            MessageProxyService.UpdateActiveSubscriptions();

            NetworkManager.singleton.StopClient();
            _messageProxy.Dispose();
            SetMessage();
        }

        public void HelloMessageHandler(HelloMessage message)
        {
            SetMessage($"[HelloMessage] {message.Message}");
            Debug.Log($"Received: {message.Message}");
        }
        public void AltHelloMessageHandler(AltHelloMessage message)
        {
            SetMessage($"[AltHelloMessage] {message.Message}");
            Debug.Log($"Received ALT: {message.Message}");
        }

        private void SetMessage(string message = "")
        {
            if (string.IsNullOrEmpty(message))
            {
                _receivedLabel.text = "";
                return;
            }

            _receivedLabel.text = $"Received message: {message}";
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