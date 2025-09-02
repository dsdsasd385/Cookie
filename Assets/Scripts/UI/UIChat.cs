using UnityEngine;
using UnityEngine.UI;

public class UIChat : MonoBehaviour
{
    [SerializeField] private InputField inputChat;

    private void Awake()
    {
        // 메인 씬이 시작될때 인풋필드를 비활성화하여
        // 화면에 출력되지 않게 함
        inputChat.gameObject.SetActive(false);
    }

    public void ToggleInputChat()
    {
        // 인풋필드 게임오브젝트가 활성화되어있으면,
        if (inputChat.gameObject.activeSelf)
        {
            // 인풋필드에 텍스트가 적혀있으면(채팅이 입력되어 있으면)
            if (string.IsNullOrEmpty(inputChat.text) == false)
                ChatManager.Instance.Chat(inputChat.text);

            // 아바타가 움직일 수 있게
            GameManager.Instance.MyAvatar.SetMovable(true);

            inputChat.text = null;
            inputChat.gameObject.SetActive(false);
        }
        // 인풋필드 게임오브젝트가 비활성화 되어있으면,
        else
        {
            inputChat.gameObject.SetActive(true);

            // 아바타 움직이지 못하게
            GameManager.Instance.MyAvatar.SetMovable(false);

            // 인풋필드에 포커스를 줌 (바로 텍스트를 입력할 수 있게)
            inputChat.Select();
            inputChat.ActivateInputField();
        }
    }
}
