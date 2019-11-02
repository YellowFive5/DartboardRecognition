# DartboardRecognition
* The main idea to build classic steeltip dartboard-based stand with using four cameras and also write desktop application to work with.
* This system will be automaticly detects another throw and point of impact of a dart. 
* C# / .NET / EMGU.CV 
* Under heavy development now !
* Main community, united this idea is [here](https://www.facebook.com/groups/281778298914107/)

## Stand details
* Now it is just debug-version of stand, but after all work complete and setup, it will be pretty-good made:

![debug-stand](https://user-images.githubusercontent.com/42347722/65386080-a1476680-dd3f-11e9-8bd3-38284bfed99a.jpg)
![model](https://user-images.githubusercontent.com/42347722/68069982-f8961900-fd78-11e9-9a5a-fb41a3806eb7.png)
![model](https://user-images.githubusercontent.com/42347722/68069995-1794ab00-fd79-11e9-9542-80249c80e0f7.png)
![model](https://user-images.githubusercontent.com/42347722/68069999-22e7d680-fd79-11e9-8698-0ee855457c36.png)

* I use 4 camera modules (OV2710 2MP 1080P HD 100 degree distortionless) on self-made handy standings:

![cams](https://user-images.githubusercontent.com/42347722/65386148-6f82cf80-dd40-11e9-8b0f-fef42072abd1.JPEG)

* For light I use 5 meters of 12V cold-light LED-strip

## App details
### PreSetup using CamCalibrator
* CamCalibrator is simple project to presetup camera when you fix it on stand. The idea is to stick dart into bull and run calibrator. You need to set camera like next screenshot. Blue line goes throught dart, red line lies on dartboard surface. Then you fix camera tightly. This position of cameras is correct for main app work. 
* Each of 4 cameras, connected to PC, have index. [0,1,2,3] to setup each of it. All connected to PC cameras will be print in textbox.
![CamCalibrator](https://user-images.githubusercontent.com/42347722/66772929-84c5c680-eec6-11e9-8888-f73d5b29745f.png)
* **Notice, that it can be situation, when all your connected cams will not be work together at same time - it can be if you trying to connect 4 cams in 1 usb hub and this hub in PC usb port. It happens because there are not enough power in 1 PC usb port for working 4 cams at same time. You see all 4 cams connected via Control Panel but they don't work. To check this situation you can use CamCalibrator. Just run it 4 times at the same time and run in on all 4 cams. If not all cams translate image - you need to reconnect cams.**
### Main app

* Main app also need presetup each camera. Using sliders you need to set up surface, center point, and region of interesting (ROI)

![MainApp](https://user-images.githubusercontent.com/42347722/65386514-5892ac00-dd45-11e9-8c98-9e46d4d82473.jpg)

* The main idea of app working process:
  * ![process](https://user-images.githubusercontent.com/42347722/65386705-ea9bb400-dd47-11e9-9c09-b78dc986e84b.jpg)
  * You throw next dart in dartboard
  * App takes image from every camera
  * From ROI gets tresholded-grayscaled image to find dart contour
  * From this images calculates dart rectangle contour (orange)
  * On this rectangle, through middle points calculates ray (blue) to surface (red)
  * Intersection of this ray and surface gives us point of impact (white) from one camera
  * Using information about POI from all 4 cameras, we can calculate actual dart POI on dartboard if draw rays from cameras setup points to cameras POIs
  
  ![MainApp2jpg](https://user-images.githubusercontent.com/42347722/67162119-d7413000-f369-11e9-9c1d-e222ccac6ef0.png)
  
* Thats how it works. Sounds simple, but there are many things to stuck with...
