using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ManualStartMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject startMenuRoot;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private Button startButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button closeControlsButton;

    [Header("Cameras")]
    [SerializeField] private Camera startMenuCamera;
    [SerializeField] private Camera playerCamera;

    [Header("Audio")]
    [SerializeField] private AudioListener startMenuAudioListener;
    [SerializeField] private AudioListener playerAudioListener;

    [Header("Game Flow")]
    [SerializeField] private BullfightGameFlow gameFlow;

    private bool listenersBound;

    private void Start()
    {
        BindButtonListeners();
        ReturnToMenu();
    }

    private void OnDestroy()
    {
        UnbindButtonListeners();
    }

    public void ReturnToMenu()
    {
        if (startMenuRoot != null)
            startMenuRoot.SetActive(true);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        SetCameraState(startMenuCamera, true);
        SetCameraState(playerCamera, false);
        SetAudioListenerState(startMenuAudioListener, true);
        SetAudioListenerState(playerAudioListener, false);

        gameFlow?.audioController?.StopAllAudio();
        gameFlow?.SetMainMenuGameplayLocked(true);
    }

    public void StartGame()
    {
        SetCameraState(startMenuCamera, false);
        SetCameraState(playerCamera, true);
        SetAudioListenerState(startMenuAudioListener, false);
        SetAudioListenerState(playerAudioListener, true);

        if (startMenuRoot != null)
            startMenuRoot.SetActive(false);

        if (controlsPanel != null)
            controlsPanel.SetActive(false);

        gameFlow?.SetMainMenuGameplayLocked(false);
        gameFlow?.StartPhaseOneDirect();
    }

    public void ShowControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void HideControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BindButtonListeners()
    {
        if (listenersBound)
            return;

        startButton?.onClick.AddListener(StartGame);
        controlsButton?.onClick.AddListener(ShowControls);
        quitButton?.onClick.AddListener(QuitGame);
        closeControlsButton?.onClick.AddListener(HideControls);
        listenersBound = true;
    }

    private void UnbindButtonListeners()
    {
        if (!listenersBound)
            return;

        startButton?.onClick.RemoveListener(StartGame);
        controlsButton?.onClick.RemoveListener(ShowControls);
        quitButton?.onClick.RemoveListener(QuitGame);
        closeControlsButton?.onClick.RemoveListener(HideControls);
        listenersBound = false;
    }

    private static void SetCameraState(Camera cameraTarget, bool enabledState)
    {
        if (cameraTarget != null)
            cameraTarget.enabled = enabledState;
    }

    private static void SetAudioListenerState(AudioListener audioListenerTarget, bool enabledState)
    {
        if (audioListenerTarget != null)
            audioListenerTarget.enabled = enabledState;
    }
}

