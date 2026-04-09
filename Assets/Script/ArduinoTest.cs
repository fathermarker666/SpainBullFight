using System;
using System.Collections;
using System.Globalization;
using System.IO.Ports;
using UnityEngine;

public class ArduinoTest : MonoBehaviour
{
    [SerializeField] string portName = "COM6";
    [SerializeField] int baudRate = 115200;
    [SerializeField] float initialOpenDelay = 0.5f;
    [SerializeField] float reopenDelay = 1f;
    [SerializeField] float maxForceValue = 50f;
    [SerializeField] string calibrationCommand = "CAL";
    [SerializeField] float sensorSignalTimeout = 1.2f;
    [SerializeField] float phaseOneHoldEnterDistanceCm = 25f;
    [SerializeField] float phaseOneHoldExitDistanceCm = 32f;
    [SerializeField] float phaseOneHoldSignalTimeout = 0.5f;

    SerialPort sp;
    Coroutine reopenCoroutine;
    bool isQuitting;
    BullfightPlayerController playerController;
    float lastSensorMessageAt = -999f;
    float lastParsedForceAt = -999f;
    float lastUltrasonicDistanceCm = -1f;
    float lastUltrasonicMessageAt = -999f;
    bool isUltrasonicHoldingCloth;
    string pendingSerialData = string.Empty;
    string lastSensorMessage = string.Empty;
    string lastOpenError = string.Empty;

    public bool IsPortOpen => sp != null && sp.IsOpen;
    public bool IsSensorConnected => IsPortOpen && (Time.unscaledTime - lastSensorMessageAt) <= Mathf.Max(0.2f, sensorSignalTimeout);
    public bool HasRecentForcePacket => Time.unscaledTime - lastParsedForceAt <= Mathf.Max(0.2f, sensorSignalTimeout);
    public string LastSensorMessage => lastSensorMessage;
    public float LastUltrasonicDistanceCm => lastUltrasonicDistanceCm;
    public bool IsUltrasonicHoldingCloth => isUltrasonicHoldingCloth;
    public string CurrentConnectionStatus
    {
        get
        {
            if (IsSensorConnected)
                return string.Empty;

            if (!string.IsNullOrWhiteSpace(lastOpenError))
                return $"[未連接到感測器] {portName} 開啟失敗";

            if (IsPortOpen)
                return $"[未連接到感測器] {portName} 已開啟但未收到資料";

            return $"[未連接到感測器] 請確認 {portName}";
        }
    }

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
        lastSensorMessageAt = -999f;
        lastParsedForceAt = -999f;
        lastUltrasonicDistanceCm = -1f;
        lastUltrasonicMessageAt = -999f;
        pendingSerialData = string.Empty;
        lastSensorMessage = string.Empty;
        playerController?.SetPhaseTwoSensorCalibrationReady(false);

        sp = new SerialPort(portName, baudRate)
        {
            ReadTimeout = 100,
            NewLine = "\n"
        };

        try
        {
            sp.Open();
            lastOpenError = string.Empty;
            Debug.Log($"Serial port {portName} opened.");
        }
        catch (Exception e)
        {
            lastOpenError = e.Message;
            Debug.LogError("Open failed: " + e.Message);
            ClosePort();
        }
    }

    void Update()
    {
        if (playerController == null)
            playerController = FindObjectOfType<BullfightPlayerController>(true);

        if (sp == null || !sp.IsOpen)
        {
            UpdateUltrasonicHoldTimeout();
            return;
        }

        try
        {
            const int maxLinesPerFrame = 24;
            if (sp.BytesToRead > 0)
                pendingSerialData += sp.ReadExisting();

            int linesReadThisFrame = 0;
            while (linesReadThisFrame < maxLinesPerFrame)
            {
                int newlineIndex = pendingSerialData.IndexOf('\n');
                if (newlineIndex < 0)
                    break;

                string data = pendingSerialData.Substring(0, newlineIndex).Trim();
                pendingSerialData = pendingSerialData.Substring(newlineIndex + 1);
                if (!string.IsNullOrEmpty(data))
                    HandleSensorMessage(data);

                linesReadThisFrame++;

                if (sp != null && sp.IsOpen && sp.BytesToRead > 0)
                    pendingSerialData += sp.ReadExisting();
            }
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

        UpdateUltrasonicHoldTimeout();
    }

    void OnDisable()
    {
        playerController?.SetPhaseTwoCalibrationSensorHeld(false);
        playerController?.ResetPhaseTwoSensorState();
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
        playerController?.ResetPhaseTwoSensorState();
        ClosePort();
    }

    public void BeginPhaseTwoCalibration()
    {
        if (playerController == null)
            playerController = FindObjectOfType<BullfightPlayerController>(true);

        if (sp == null || !sp.IsOpen)
            OpenPort();

        playerController?.SetPhaseTwoSensorCalibrationReady(false);
        TryWriteLine(calibrationCommand);
    }

    void HandleSensorMessage(string data)
    {
        string message = data.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(message))
            return;

        lastSensorMessageAt = Time.unscaledTime;
        lastSensorMessage = data.Trim();

        if (TryHandleUltrasonicDistanceMessage(data))
            return;

        if (TryHandleForceMessage(data))
            return;

        if (message.Contains("BULL_START"))
        {
            Debug.Log("Bingo! Received bullfight start command.");
            return;
        }

        if (message.Contains("READY"))
        {
            playerController?.SetPhaseTwoSensorCalibrationReady(true);
            return;
        }

        if (message.Contains("THRUST") || message.Contains("STAB"))
        {
            playerController?.TriggerPhaseTwoStab();
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

            case "CAL_RESET":
            case "PHASE2_CAL_RESET":
            case "CALIBRATION_RESET":
                playerController?.SetPhaseTwoSensorCalibrationReady(false);
                break;
        }
    }

    bool TryHandleUltrasonicDistanceMessage(string rawData)
    {
        if (playerController == null)
            return false;

        string data = rawData.Trim();
        if (string.IsNullOrEmpty(data))
            return false;

        const string prefix = "DIST:";
        if (!data.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return false;

        string valueText = data.Substring(prefix.Length).Trim();
        if (!float.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out float distanceCm))
            return false;

        lastUltrasonicDistanceCm = distanceCm;
        lastUltrasonicMessageAt = Time.unscaledTime;

        bool newHoldState = isUltrasonicHoldingCloth;

        if (!isUltrasonicHoldingCloth && distanceCm <= phaseOneHoldEnterDistanceCm)
            newHoldState = true;
        else if (isUltrasonicHoldingCloth && distanceCm >= phaseOneHoldExitDistanceCm)
            newHoldState = false;

        if (newHoldState != isUltrasonicHoldingCloth)
        {
            isUltrasonicHoldingCloth = newHoldState;
            playerController.SetUltrasonicHoldActive(isUltrasonicHoldingCloth);
        }

        return true;
    }

    bool TryHandleForceMessage(string rawData)
    {
        if (playerController == null)
            return false;

        string data = rawData.Trim();
        if (string.IsNullOrEmpty(data))
            return false;

        if (TryHandleCsvSensorMessage(data))
            return true;

        string[] separators = { ":", "=", "," };
        for (int i = 0; i < separators.Length; i++)
        {
            string separator = separators[i];
            int splitIndex = data.IndexOf(separator, StringComparison.Ordinal);
            if (splitIndex < 0)
                continue;

            string label = data.Substring(0, splitIndex).Trim();
            string valueText = data.Substring(splitIndex + separator.Length).Trim();
            if (!label.Equals("FORCE", StringComparison.OrdinalIgnoreCase) &&
                !label.Equals("POWER", StringComparison.OrdinalIgnoreCase) &&
                !label.Equals("THRUST", StringComparison.OrdinalIgnoreCase))
                continue;

            if (!float.TryParse(valueText, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedForce))
                return false;

            lastParsedForceAt = Time.unscaledTime;
            playerController.SetPhaseTwoSensorCalibrationReady(true);
            playerController.SetPhaseTwoSensorReading(parsedForce, Mathf.Clamp(Mathf.Abs(parsedForce), 0f, Mathf.Max(1f, maxForceValue)));
            return true;
        }

        if (!float.TryParse(data, NumberStyles.Float, CultureInfo.InvariantCulture, out float rawForce))
            return false;

        lastParsedForceAt = Time.unscaledTime;
        playerController.SetPhaseTwoSensorCalibrationReady(true);
        playerController.SetPhaseTwoSensorReading(rawForce, Mathf.Clamp(Mathf.Abs(rawForce), 0f, Mathf.Max(1f, maxForceValue)));
        return true;
    }

    bool TryHandleCsvSensorMessage(string data)
    {
        string[] parts = data.Split(',');
        if (parts.Length < 2)
            return false;

        if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float signedSignal))
            return false;

        float displayedForce = Mathf.Abs(signedSignal);
        if (parts.Length >= 3 &&
            float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float reportedCap) &&
            reportedCap > 0.01f)
        {
            displayedForce = Mathf.Clamp(displayedForce, 0f, reportedCap);
        }

        lastParsedForceAt = Time.unscaledTime;
        playerController.SetPhaseTwoSensorCalibrationReady(true);
        playerController.SetPhaseTwoSensorReading(signedSignal, Mathf.Clamp(displayedForce, 0f, Mathf.Max(1f, maxForceValue)));
        return true;
    }

    void TryWriteLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line) || sp == null || !sp.IsOpen)
            return;

        try
        {
            sp.WriteLine(line);
        }
        catch (Exception e)
        {
            lastOpenError = e.Message;
            Debug.LogWarning("Serial write failed: " + e.Message);
        }
    }

    void UpdateUltrasonicHoldTimeout()
    {
        if (!isUltrasonicHoldingCloth)
            return;

        if (Time.unscaledTime - lastUltrasonicMessageAt <= Mathf.Max(0.1f, phaseOneHoldSignalTimeout))
            return;

        isUltrasonicHoldingCloth = false;
        playerController?.SetUltrasonicHoldActive(false);
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
            lastSensorMessageAt = -999f;
            lastParsedForceAt = -999f;
            pendingSerialData = string.Empty;
            lastSensorMessage = string.Empty;
            lastUltrasonicDistanceCm = -1f;
            lastUltrasonicMessageAt = -999f;
        }
    }
}