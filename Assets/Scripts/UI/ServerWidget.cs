using System.Threading.Tasks;
using Mirror;
using MirrorNetTest.Core.Messages;
using UnityEngine;
using UnityEngine.UI;

namespace MirrorNetTest.UI
{
    public class ServerWidget : WidgetBase
    {
        [SerializeField]
        private Button _startServerBtn;
        [SerializeField]
        private Button _stopServerBtn;

        [SerializeField]
        private Button _sendHelloBtn;

        [SerializeField]
        private Button _sendAltHelloBtn;

        private void OnStartServerBtnClick()
        {
            NetworkManager.singleton.StartServer();
        }

        private void OnStopServerBtnClick()
        {
            NetworkManager.singleton.StopServer();
        }

        private void OnSendHelloBtn()
        {
            NetworkServer.SendToAll(new HelloMessage() { Message = "Hello client!"});
        }

        private void OnSendAltHelloBtn()
        {
            NetworkServer.SendToAll(new AltHelloMessage() { Message = "Hello client ALT!"});
        }

        protected override void RegisterListeners()
        {
            _startServerBtn.onClick.AddListener(OnStartServerBtnClick);
            _stopServerBtn.onClick.AddListener(OnStopServerBtnClick);
            _sendHelloBtn.onClick.AddListener(OnSendHelloBtn);
            _sendAltHelloBtn.onClick.AddListener(OnSendAltHelloBtn);
        }

        protected override void RemoveListeners()
        {
            _startServerBtn.onClick.RemoveAllListeners();
            _stopServerBtn.onClick.RemoveAllListeners();
            _sendHelloBtn.onClick.RemoveAllListeners();
            _sendAltHelloBtn.onClick.RemoveAllListeners();
        }
    }
}