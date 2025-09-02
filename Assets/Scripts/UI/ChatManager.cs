using Photon.Pun;
using UnityEngine;

public class ChatManager : MonoBehaviourPun
{
    #region Singleton
    // 싱글톤 패턴 구현: ChatManager의 단일 인스턴스를 보장
    private static ChatManager _instance;

    public static ChatManager Instance
    {
        get
        {
            // 인스턴스가 없으면 씬에서 ChatManager를 찾아 설정
            if (_instance == null)
                _instance = FindObjectOfType<ChatManager>();
            return _instance;
        }
    }
    #endregion

    // UIChat 컴포넌트 참조 (채팅 입력 UI를 제어)
    [SerializeField] private UIChat uiChat;

    // 채팅 메시지를 전송하는 함수
    public void Chat(string chat)
    {
        // 현재 플레이어의 ActorNumber와 닉네임 가져오기
        var actorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        var nickName = PhotonNetwork.NickName;

        // 네트워크를 통해 모든 클라이언트에 채팅 메시지 전송
        photonView.RPC(nameof(SendChat), RpcTarget.All, actorNumber, nickName, chat);        
    }

    // [PunRPC] 네트워크를 통해 호출되는 함수
    // 채팅 메시지를 특정 아바타에 표시
    [PunRPC]
    private void SendChat(int actorNumber, string nickName, string chat)
    {
        // 씬 내의 모든 PlayerAvatar를 찾음
        var allAvatars = FindObjectsByType<PlayerAvatar>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        PlayerAvatar targetAvatar = null;

        // ActorNumber를 기반으로 채팅을 표시할 대상 아바타를 찾음
        foreach (var avatar in allAvatars)
        {
            if (avatar.IsTargetAvatar(actorNumber))
            {
                targetAvatar = avatar;
                break;
            }
        }

        // 대상 아바타가 없으면 반환 (채팅 표시 중단)
        if (targetAvatar == null)
            return;

        // 대상 아바타에 채팅 메시지 표시
        targetAvatar.ShowChat(chat);
    }

    private void Update()
    {
        // 엔터키 입력을 감지
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // UIChat의 ToggleInputChat() 호출
            // 채팅 입력창의 활성화 상태를 토글
            uiChat.ToggleInputChat();
        }
    }
}
