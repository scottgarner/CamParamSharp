# CamParam

I was shocked to find no simple way to change basic webcam settings (gain, exposure, etc.) from the command line on Windows. After much time spent looking at existing approaches, I finally gave in and wrote this little program.

## Usage

```
// List available devices.

CamParam.exe              

// Output

Available devices:
0 HD Pro Webcam C920     
1 OBS Virtual Camera
```

```
// List properties for a device.

CamParam.exe device=0

// Output

Configuring Device: HD Pro Webcam C920
Pan=0
Tilt=0
Roll=0
Zoom=100
Exposure=-5
Iris=-5
Focus=15
Brightness=128
Contrast=128
Hue=128
Saturation=128
Sharpness=128
Gamma=128
ColorEnable=128
WhiteBalance=4200
BacklightCompensation=0
Gain=64
```

```
// Set exposure and gain for device "0".

CamParam.exe device=0 gain=64 exposure=-5  

// Output

Configuring Device: HD Pro Webcam C920
Setting exposure to -5
Setting gain to 64
```