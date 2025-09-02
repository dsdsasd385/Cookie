using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHider : MonoBehaviour
{

    private bool isLock = true;
    void Start()
    {
        isLock = true;
        LockCursor(); // 게임 시작 시 마우스 잠금
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isLock)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }     
    }
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isLock = true;
        Debug.Log("마우스 잠금됨");
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isLock = false;
        Debug.Log("마우스 잠금 해제됨");
    }
}
