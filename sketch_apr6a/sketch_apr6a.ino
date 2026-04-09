#include <Adafruit_MPU6050.h>
#include <Adafruit_Sensor.h>
#include <Wire.h>

Adafruit_MPU6050 mpu;

float offset_y = 0;      
float noise_range = 0;   
bool isCalibrated = false;
float filtered_y = 0;    // 濾波後的數值
float alpha = 0.3;       // 濾波強度 (0.3 很適合刺擊反應)

// ===== Ultrasonic（新增）=====
const int trigPin = 5;
const int echoPin = 18;
unsigned long lastUltraRead = 0;
const unsigned long ultraInterval = 50; // 每 50ms 讀一次

void setup() {
  Serial.begin(115200);
  Wire.begin(21, 22);
  delay(1000);

  // ===== Ultrasonic 初始化（新增）=====
  pinMode(trigPin, OUTPUT);
  pinMode(echoPin, INPUT);
  digitalWrite(trigPin, LOW);

  if (!mpu.begin(0x68)) {
    Serial.println("MPU6050 Init Failed!");
  }
  
  mpu.setAccelerometerRange(MPU6050_RANGE_16_G);

  // --- 校準 3 秒 ---
  float sum_y = 0;
  float max_y = -999;
  float min_y = 999;
  int sample_count = 150; 

  for (int i = 0; i < sample_count; i++) {
    sensors_event_t a, g, temp;
    mpu.getEvent(&a, &g, &temp);
    float val = a.acceleration.y;
    sum_y += val;
    if (val > max_y) max_y = val;
    if (val < min_y) min_y = val;
    
    // 校準時輸出 0，讓 Plotter 有線但不亂跳
    Serial.println("0.0,35.0,50.0"); 
    delay(20);
  }

  offset_y = sum_y / sample_count;    
  noise_range = (max_y - min_y) / 2;  
  isCalibrated = true;
}

void loop() {
  sensors_event_t a, g, temp;
  mpu.getEvent(&a, &g, &temp);

  float raw_y = a.acceleration.y;
  float clean_y = raw_y - offset_y;

  // --- 加入濾波邏輯 ---
  filtered_y = (filtered_y * (1.0 - alpha)) + (clean_y * alpha);

  // ===== 原本 MPU 輸出（完全不動）=====
  Serial.print(filtered_y);  
  Serial.print(",");
  Serial.print(35.0);  
  Serial.print(",");
  Serial.println(50.0);  

  // ===== Ultrasonic（新增，不干擾原本）=====
  if (millis() - lastUltraRead > ultraInterval) {
    lastUltraRead = millis();

    digitalWrite(trigPin, LOW);
    delayMicroseconds(2);

    digitalWrite(trigPin, HIGH);
    delayMicroseconds(10);
    digitalWrite(trigPin, LOW);

    long duration = pulseIn(echoPin, HIGH, 30000);

    if (duration > 0) {
      float distanceCm = duration * 0.0343 / 2.0;

      // 過濾亂值
      if (distanceCm > 0 && distanceCm < 200) {
        Serial.print("DIST:");
        Serial.println(distanceCm, 1);
      }
    }
  }

  delay(20);
}
