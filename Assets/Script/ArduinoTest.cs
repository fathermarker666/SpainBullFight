using System;
using System.Collections;
using System.IO.Ports;
using UnityEngine;

public class ArduinoTest : MonoBehaviour
{
    [SerializeField] string portName = "COM6";
    [SerializeField] int baudRate = 4800;
    [SerializeField] float initialOpenDelay = 0.5f;
    [SerializeField] float reopenDelay = 1f;

    SerialPort sp;
    Coroutine reopenCoroutine;
    bool isQuitting;
    BullfightPlayerController playerController;

    void Start()
    {
        reopenCoroutine = StartCoroutine(OpenPortAfterDelay(initialOpenDelay));
    }

    IEnumerator OpenPortAfterDelay(float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        reopenCoroutine = null;
        OpenPort();
    }

    void OpenPort()
    {
        ClosePort();

        sp = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 100,
            NewLine = "\n"
        };

        try
        {
            sp.Open();
            Debug.Log($"Serial port {portName} opened.");
        }
        catch (Exception e)
        {
            Debug.LogError("Open failed: " + e.Message);
            ClosePort();
        }
    }

    void Update()
    {
        if (playerController == null)
            playerController = FindObjectOfType<BullfightPlayerController>(true);

        if (sp == null || !sp.IsOpen)
            return;

        try
        {
            string data = sp.ReadLine().Trim();
            if (string.IsNullOrEmpty(data))
                return;

            HandleSensorMessage(data);
        }
        catch (TimeoutException)
        {
        }
        catch (Exception e)
        {
            Debug.LogWarning("Serial connection lost, retrying: " + e.Message);
            ClosePort();

            if (!isQuitting && isActiveAndEnabled && reopenCoroutine == null)
                reopenCoroutine = StartCoroutine(OpenPortAfterDelay(reopenDelay));
        }
    }

    void OnDisable()
    {
        playerController?.SetPhaseTwoCalibrationSensorHeld(false);
        ClosePort();
    }

    void OnDestroy()
    {
        ClosePort();
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
        playerController?.SetPhaseTwoCalibrationSensorHeld(false);
        ClosePort();
    }

    void HandleSensorMessage(string data)
    {
        string message = data.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(message))
            return;

        if (message.Contains("BULL_START"))
        {
            Debug.Log("Bingo! Received bullfight start command.");
            return;
        }

        switch (message)
        {
            case "SWING":
            case "CAPA":
            case "PHASE1_SWING":
                playerController?.TriggerSensorSwing();
                break;

            case "PHASE2_CALIBRATION_START":
            case "PHASE2_CALIBRATE_START":
            case "CALIBRATION_START":
            case "CALIBRATE_ON":
                playerController?.SetPhaseTwoCalibrationSensorHeld(true);
                break;

            case "PHASE2_CALIBRATION_STOP":
            case "PHASE2_CALIBRATE_STOP":
            case "CALIBRATION_STOP":
            case "CALIBRATE_OFF":
                playerController?.SetPhaseTwoCalibrationSensorHeld(false);
                break;

            case "STAB":
            case "PHASE2_STAB":
                playerController?.TriggerPhaseTwoStab();
                break;
        }
    }

    void ClosePort()
    {
        if (reopenCoroutine != null)
        {
            StopCoroutine(reopenCoroutine);
            reopenCoroutine = null;
        }

        if (sp == null)
            return;

        try
        {
            if (sp.IsOpen)
                sp.Close();
        }
        catch (Exception e)
        {
            Debug.LogWarning("Close failed: " + e.Message);
        }
        finally
        {
            sp.Dispose();
            sp = null;
        }
    }
}
