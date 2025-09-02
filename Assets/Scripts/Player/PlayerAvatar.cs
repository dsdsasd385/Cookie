using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAvatar : MonoBehaviourPun
{
    // 닉네임을 표시하는 UI 텍스트
    [SerializeField] private Text txtNickname;

    // 채팅 내용을 표시하는 UI 텍스트
    [SerializeField] private Text txtChat;

    // 채팅 말풍선 오브젝트
    [SerializeField] private GameObject chatBubble;


    // 플레이어의 움직임을 제어하는 AvatarController 컴포넌트
    private CustomAvatarController _controller;

    // 플레이어의 닉네임
    public string _nickname;

    // 플레이어의 Photon ActorNumber (네트워크 상의 고유 번호)
    public int _actorNumber;

    private Rigidbody _rb;


    RaycastHit hit;

    [SerializeField] private float MaxDistance = 15f;

    private void Awake()
    {
        // AvatarController 컴포넌트를 가져옴
        _controller = GetComponent<CustomAvatarController>();

        // 채팅 말풍선을 비활성화하여 초기 상태를 숨김
        chatBubble.SetActive(false);

        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        _rb.velocity = Vector3.zero;
    }


    // [PunRPC] 네트워크를 통해 호출되는 메서드
    // 플레이어의 닉네임과 ActorNumber를 설정
    [PunRPC]
    public void SetNickname(string nickname , int actorNumber)
    {
        _nickname = nickname;
        _actorNumber = actorNumber;

        // 닉네임 UI 텍스트에 표시
        if (photonView.IsMine)
        {
            txtNickname.text = nickname;
        }
        else
        {
            txtNickname.text = ""; // 다른 사람에게는 닉네임 숨기기
        }
    }

    // 플레이어의 움직임 가능 여부를 설정
    public void SetMovable(bool value)
    {
        _controller.canMove = value;
    }

    // 특정 ActorNumber와 현재 아바타의 ActorNumber를 비교
    // 동일한 아바타인지 확인하는 메서드
    public bool IsTargetAvatar(int actorNumber) => _actorNumber == actorNumber;

    // 채팅을 표시하는 메서드
    // 이미 채팅 코루틴이 실행 중이라면 중지하고 새 채팅을 표시
    public void ShowChat(string chat)
    {
        if (_chatCoroutine != null)
            StopCoroutine(_chatCoroutine); // 기존 코루틴 중지

        _chatCoroutine = ChatCoroutine(chat); // 새로운 코루틴 생성
        StartCoroutine(_chatCoroutine); // 코루틴 실행
    }

    // 채팅을 일정 시간 동안 표시하고 숨기는 코루틴
    private IEnumerator _chatCoroutine;
    private IEnumerator ChatCoroutine(string chat)
    {
        // 채팅 말풍선을 활성화
        chatBubble.SetActive(true);

        // 채팅 내용을 텍스트에 표시
        txtChat.text = chat;

        // 3초 동안 채팅을 표시w
        yield return new WaitForSeconds(3f);

        // 채팅 내용을 지우고 말풍선을 숨김
        txtChat.text = null;
        chatBubble.SetActive(false);
    }
}
