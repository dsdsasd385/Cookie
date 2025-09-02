using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    void Start()
    {
        // 해상도를 1920x1080으로 고정하고 전체 화면으로 설정
        Screen.SetResolution(1600, 900, false);
    }
}
