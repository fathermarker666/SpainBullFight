using UnityEngine;

public class StartMenuCameraSwitcher : MonoBehaviour
{
    [Header("Cameras")]
    public Camera startMenuCamera;
    public Camera playerCamera;

    void Start()
    {
        // 一開始顯示首頁相機
        if (startMenuCamera != null)
            startMenuCamera.enabled = true;

        if (playerCamera != null)
            playerCamera.enabled = false;
    }

    public void SwitchToPlayerCamera()
    {
        if (startMenuCamera != null)
            startMenuCamera.enabled = false;

        if (playerCamera != null)
            playerCamera.enabled = true;
    }
}