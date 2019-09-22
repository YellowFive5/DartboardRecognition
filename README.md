# DartboardRecognition
* The main idea to build classic steeltip dartboard-based stand with using four cameras and also write desktop application to work with.
* This system will be automaticly detects another throw and point of impact of a dart. 
* C# / .NET / EMGU.CV 
* Under heavy development now !
* Main community, united this idea is [here](https://www.facebook.com/groups/281778298914107/)

## Stand details
* Now it is just debug-version of stand, but after all work complete and setup, it will be pretty-good made:

![debug-stand](https://user-images.githubusercontent.com/42347722/65386080-a1476680-dd3f-11e9-8bd3-38284bfed99a.jpg)

* I use 4 camera modules (OV2710 2MP 1080P HD 100 degree distortionless) on self-made handy standings:

![cams](https://user-images.githubusercontent.com/42347722/65386148-6f82cf80-dd40-11e9-8b0f-fef42072abd1.JPEG)

* For light I use 5 meters of 12V cold-light LED-strip

## App details
### PreSetup using CamCalibrator
* CamCalibrator is simple project to presetup camera when you fix it on stand. The idea is to stick dart into bull and run calibrator. You need to set camera like next screenshot. Blue line goes throught dart, red line lies on dartboard surface. Then you fix camera tightly. This position of cameras is correct for main app work. 
* Each of 4 cameras, connected to PC, have index. [0,1,2,3] to setup each of it. 

![CamCalibrator](https://user-images.githubusercontent.com/42347722/65386346-284a0e00-dd43-11e9-865b-a90d0066a61e.jpg)

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
  
![MainApp2jpg](https://user-images.githubusercontent.com/42347722/65386812-cd1b1a00-dd48-11e9-9bb1-57a17373da27.jpg)

* Thats how it works. Sounds simple, but there are many things to stuck with...
