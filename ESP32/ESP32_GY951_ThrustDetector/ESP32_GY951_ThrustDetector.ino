#include <Wire.h>

/*
  ESP32 DevKit V1 + GY-521 / MPU6050

  Wiring:
  - VCC -> 3V3
  - GND -> GND
  - SDA -> GPIO21
  - SCL -> GPIO22
  - AD0 -> GND (I2C address 0x68)

  Serial commands:
  - "CAL" : capture a new resting baseline

  Serial output:
  - "READY"
  - "FORCE:<0-50>"
  - "THRUST_DETECTED"
*/

namespace Config {
constexpr uint8_t kSensorAddress = 0x68;
constexpr uint8_t kSdaPin = 21;
constexpr uint8_t kSclPin = 22;
constexpr uint32_t kSerialBaud = 4800;
constexpr uint32_t kI2cClock = 400000;
constexpr uint32_t kSampleIntervalMs = 50;        // 20 Hz keeps 4800 baud stable.
constexpr uint32_t kCalibrationDurationMs = 350;  // Fast zeroing; Unity handles the 5 s hold.
constexpr float kAccelScaleLsbPerG = 16384.0f;    // +-2g
constexpr float kMinimumNoiseBandG = 0.015f;
constexpr float kForceScale = 40.0f;
constexpr float kForceCap = 50.0f;
constexpr float kThrustThreshold = 35.0f;
constexpr float kReleaseThreshold = 18.0f;
}

namespace MpuReg {
constexpr uint8_t kWhoAmI = 0x75;
constexpr uint8_t kPwrMgmt1 = 0x6B;
constexpr uint8_t kPwrMgmt2 = 0x6C;
constexpr uint8_t kConfig = 0x1A;
constexpr uint8_t kSampleRateDiv = 0x19;
constexpr uint8_t kAccelConfig = 0x1C;
constexpr uint8_t kAccelConfig2 = 0x1D;
constexpr uint8_t kAccelXoutH = 0x3B;
}

struct CalibrationStats {
  float sumY = 0.0f;
  float minY = 0.0f;
  float maxY = 0.0f;
  uint32_t sampleCount = 0;
};

enum class RunMode {
  Idle,
  Calibrating,
  Monitoring
};

RunMode gMode = RunMode::Idle;
CalibrationStats gCalibration;
unsigned long gLastSampleMs = 0;
unsigned long gCalibrationStartMs = 0;
float gAccelYOffset = 0.0f;
float gNoiseBandY = Config::kMinimumNoiseBandG;
float gLastForce = 0.0f;
bool gSensorReady = false;
bool gThrustLatched = false;

bool writeRegister(uint8_t reg, uint8_t value) {
  Wire.beginTransmission(Config::kSensorAddress);
  Wire.write(reg);
  Wire.write(value);
  return Wire.endTransmission() == 0;
}

bool readRegisters(uint8_t startReg, uint8_t* buffer, size_t length) {
  Wire.beginTransmission(Config::kSensorAddress);
  Wire.write(startReg);
  if (Wire.endTransmission(false) != 0) {
    return false;
  }

  const size_t received = Wire.requestFrom(
      static_cast<int>(Config::kSensorAddress), static_cast<int>(length), static_cast<int>(true));
  if (received != length) {
    return false;
  }

  for (size_t i = 0; i < length; ++i) {
    buffer[i] = Wire.read();
  }

  return true;
}

bool readAccelY(float& accelYG) {
  uint8_t buffer[6];
  if (!readRegisters(MpuReg::kAccelXoutH, buffer, sizeof(buffer))) {
    return false;
  }

  const int16_t rawY = static_cast<int16_t>((buffer[2] << 8) | buffer[3]);
  accelYG = static_cast<float>(rawY) / Config::kAccelScaleLsbPerG;
  return true;
}

bool initSensor() {
  uint8_t whoAmI = 0;
  if (!readRegisters(MpuReg::kWhoAmI, &whoAmI, 1)) {
    return false;
  }

  if (!writeRegister(MpuReg::kPwrMgmt1, 0x01)) {
    return false;
  }
  if (!writeRegister(MpuReg::kPwrMgmt2, 0x00)) {
    return false;
  }
  if (!writeRegister(MpuReg::kConfig, 0x03)) {
    return false;
  }
  if (!writeRegister(MpuReg::kSampleRateDiv, 0x09)) {
    return false;
  }
  if (!writeRegister(MpuReg::kAccelConfig, 0x00)) {
    return false;
  }
  if (!writeRegister(MpuReg::kAccelConfig2, 0x03)) {
    return false;
  }

  return whoAmI != 0x00 && whoAmI != 0xFF;
}

void resetCalibrationStats() {
  gCalibration = CalibrationStats{};
}

void startCalibration() {
  resetCalibrationStats();
  gCalibrationStartMs = millis();
  gMode = RunMode::Calibrating;
  gThrustLatched = false;
  gLastForce = 0.0f;
}

void finishCalibration() {
  if (gCalibration.sampleCount == 0) {
    gMode = RunMode::Idle;
    return;
  }

  gAccelYOffset = gCalibration.sumY / static_cast<float>(gCalibration.sampleCount);
  const float positiveNoise = gCalibration.maxY - gAccelYOffset;
  const float negativeNoise = gAccelYOffset - gCalibration.minY;
  gNoiseBandY = max(max(positiveNoise, negativeNoise), Config::kMinimumNoiseBandG);
  gMode = RunMode::Monitoring;
  gThrustLatched = false;
  gLastForce = 0.0f;
  Serial.println("READY");
}

float computeForce(float accelYG) {
  const float delta = fabsf(accelYG - gAccelYOffset);
  const float adjustedDelta = max(0.0f, delta - gNoiseBandY);
  const float scaledForce = adjustedDelta * Config::kForceScale;
  return min(Config::kForceCap, scaledForce);
}

void emitForce(float force) {
  Serial.print("FORCE:");
  Serial.println(force, 1);
}

void processMonitoringSample(float accelYG) {
  const float force = computeForce(accelYG);
  gLastForce = force;
  emitForce(force);

  if (!gThrustLatched && force >= Config::kThrustThreshold) {
    gThrustLatched = true;
    Serial.println("THRUST_DETECTED");
  } else if (gThrustLatched && force <= Config::kReleaseThreshold) {
    gThrustLatched = false;
  }
}

void updateSensorTask() {
  const unsigned long now = millis();
  if (now - gLastSampleMs < Config::kSampleIntervalMs) {
    return;
  }
  gLastSampleMs = now;

  float accelYG = 0.0f;
  if (!readAccelY(accelYG)) {
    return;
  }

  if (gMode == RunMode::Calibrating) {
    if (gCalibration.sampleCount == 0) {
      gCalibration.minY = accelYG;
      gCalibration.maxY = accelYG;
    } else {
      gCalibration.minY = min(gCalibration.minY, accelYG);
      gCalibration.maxY = max(gCalibration.maxY, accelYG);
    }

    gCalibration.sumY += accelYG;
    ++gCalibration.sampleCount;

    if (now - gCalibrationStartMs >= Config::kCalibrationDurationMs) {
      finishCalibration();
    }
    return;
  }

  if (gMode == RunMode::Monitoring) {
    processMonitoringSample(accelYG);
  }
}

void handleSerialCommandChar(char ch) {
  static char commandBuffer[8];
  static uint8_t index = 0;

  if (ch == '\r' || ch == '\n' || ch == ' ' || ch == '\t') {
    index = 0;
    return;
  }

  if (index >= sizeof(commandBuffer) - 1) {
    index = 0;
  }

  if (ch >= 'a' && ch <= 'z') {
    ch = ch - 'a' + 'A';
  }

  commandBuffer[index++] = ch;
  commandBuffer[index] = '\0';

  if (index >= 3 &&
      commandBuffer[index - 3] == 'C' &&
      commandBuffer[index - 2] == 'A' &&
      commandBuffer[index - 1] == 'L') {
    index = 0;
    startCalibration();
  }
}

void handleSerialInput() {
  while (Serial.available() > 0) {
    const char ch = static_cast<char>(Serial.read());
    handleSerialCommandChar(ch);
  }
}

void setup() {
  Serial.begin(Config::kSerialBaud);
  Wire.begin(Config::kSdaPin, Config::kSclPin);
  Wire.setClock(Config::kI2cClock);

  gSensorReady = initSensor();
  gLastSampleMs = millis();

  if (!gSensorReady) {
    Serial.println("SENSOR_INIT_FAILED");
    return;
  }

  startCalibration();
}

void loop() {
  handleSerialInput();

  if (!gSensorReady) {
    return;
  }

  updateSensorTask();
}
