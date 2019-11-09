# DartboardRecognition
* The main idea to build classic steeltip dartboard-based stand with using four cameras and also write desktop application to work with.
* This system will be automaticly detects another throw and point of impact of a dart. 
* C# / .NET / EMGU.CV 
* Under heavy development now
* Main community, united this idea is [here](https://www.facebook.com/groups/281778298914107/)
* Version 1.0 released

## Stand details
* Now it is just debug-version of stand, but after all work complete and setup, it will be pretty-good made:

![Stand](https://user-images.githubusercontent.com/42347722/68527987-6e146300-02fe-11ea-8041-9981bda01664.jpg)

Something like that:
![model](https://user-images.githubusercontent.com/42347722/68069982-f8961900-fd78-11e9-9a5a-fb41a3806eb7.png)
![model](https://user-images.githubusercontent.com/42347722/68527996-a156f200-02fe-11ea-89de-dea217214385.png)
![model](https://user-images.githubusercontent.com/42347722/68069999-22e7d680-fd79-11e9-8698-0ee855457c36.png)

### To build stand, you need some necessary things:
#### 1. Cameras
* You need 4 cameras (In theory you can use only 2 cameras, but detection quality will be bad in nearly-sticked darts situations).
Cameras must be same model. Recomended resolution of cameras is 1280x720.
I use 4 camera modules (OV2710, 2MP 1080P HD, 85 HFOW, distortionless) on self-made handy standings:

![cams](https://user-images.githubusercontent.com/42347722/65386148-6f82cf80-dd40-11e9-8b0f-fef42072abd1.JPEG)

#### 2. Light
* For light I use 5 meters of 12V cold-light LED-strip

#### 3. Dartboard
* For debug-stand I use dummy plastic-foamed dartboard toy. I have regular Winmau dartboard, but it's on my wall now

#### 4. Some DIY things and tools
* Wood panels, carton panels, white paper, glue, scissors...

### Recomendations:
* Cams must be fixed tightly like this:

![Скриншот 2019-11-09 13 14 59](https://user-images.githubusercontent.com/42347722/68528391-2c39eb80-0303-11ea-8f1b-87896ffd8de6.jpg)

* To connect cams to PC I use this schema:

![Untitled Diagram](https://user-images.githubusercontent.com/42347722/68530994-a166ea00-031e-11ea-962b-d091dceefd94.jpg)

* Notice, that it can be situation, when all your connected cams will not be work together at same time - it can be if you trying to connect 4 cams in 1 usb hub and this hub in PC usb port (surprize! =)). It happens because there are not enough power in 1 PC usb port for working 4 cams at same time. You see all 4 cams connected via Control Panel but they don't work. To check this situation you can use CamCalibrator. Just run it 4 times at the same time and run in on all 4 cams. If not all cams translate image - you need to reconnect cams diffirent way.

## App details
### Get started
#### CamCalibrator
* CamCalibrator is simple project to presetup camera when you fix it on stand. The idea is to stick dart into bull and run calibrator. You need to set camera like next screenshot. Blue line goes throught dart, red line lies on dartboard surface. Then you fix camera tightly. This position of cameras is correct for main app work. Setup all your cameras this way.
![Скриншот 2019-11-09 18 38 54](https://user-images.githubusercontent.com/42347722/68531216-1e935e80-0321-11ea-8662-16a76d385552.png)

* All connected to PC cameras will be print in textbox. Each of camera have ID, which you can check this way:
![Скриншот 2019-11-09 18 38 55](https://user-images.githubusercontent.com/42347722/68531224-48e51c00-0321-11ea-8e47-c900ddbde39c.png)
* This ID's will be need soon.

### DartboardRecognition
* Main app also need some preSetup. Run DartboardRecognition, go to 'Setup' tab and fill Cams ID's boxes, Distance to Cam boxes, Cam FOV, Resolution boxes. Other boxes you can fill with defaul values like my:
![Скриншот 2019-11-09 18 53 04](https://user-images.githubusercontent.com/42347722/68531389-dffea380-0322-11ea-871f-99bd382fa156.png)
* All settings saves to DartboardRecognition.exe.config

* Then check this boxes:                                                     
![Скриншот 2019-11-09 18 53 05](https://user-images.githubusercontent.com/42347722/68531477-88146c80-0323-11ea-8acb-8a324c6f1742.png)
* Go to 'Projection' tab and hit 'Start'. Cam window will open and you will be able to setup last things:
* Threshold image. Move 'Threshold' slider to setup threshold.
![Скриншот 2019-11-09 19 04 51](https://user-images.githubusercontent.com/42347722/68531530-256fa080-0324-11ea-92b9-aae8e217ed59.png)
* You can see noise in the corners - we don't need it. Like this:
![Скриншот 2019-11-09 19 04 54](https://user-images.githubusercontent.com/42347722/68531573-c9594c00-0324-11ea-9212-efa79ace9c24.png)
* Stick dart into bull and setup ROI, surface and center like on image before
* Then hit 'Calibrate' button
* Do this instuctions with all of cams
* Hit 'Stop' button
* On 'Setup' tab check 'With detection' and uncheck 'Runtime capturing' and 'Show setup sliders'
![Скриншот 2019-11-09 19 20 56](https://user-images.githubusercontent.com/42347722/68531888-50f48a00-0328-11ea-85a7-7dad8c2570d1.png)
* All setup done, you can hit 'Start' and throw darts.
![Скриншот 2019-11-09 19 26 52](https://user-images.githubusercontent.com/42347722/68531772-13433180-0327-11ea-892c-0d8b42ad3c08.png)

### The main idea of app working process:
  * ![process](https://user-images.githubusercontent.com/42347722/65386705-ea9bb400-dd47-11e9-9c09-b78dc986e84b.jpg)
  * You throw next dart in dartboard
  * App takes image from every camera
  * From ROI gets tresholded-grayscaled image to find dart contour
  * From this images calculates dart rectangle contour (orange)
  * On this rectangle, through middle points calculates ray (blue) to surface (red)
  * Intersection of this ray and surface gives us point of impact (white) from one camera
  * Using information about POI from all 4 cameras, we can calculate actual dart POI on dartboard if draw rays from cameras setup points to cameras POIs
  
* Thats how it works. Sounds simple, but there are many things to stuck with...
