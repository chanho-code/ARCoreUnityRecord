# ARCoreUnityRecord
ARCore Recording &amp; Playback App in Unity (Android)
We have added the Recording function to the **HelloAR** project.

# Tested with
- Windows 10 OS
- Unity: 2019.4.27f1  
- ARCore-Unity-SDK: 1.24

# Try to build it !!
- Download the ARCoreUnityRecord project.
- Run the project in Unity.
- Try to find and open the HelloARRecord scene (GoogleARCore/Examples/HelloAR/Scenes/HelloARRecord).

![3](https://user-images.githubusercontent.com/68829425/151173098-305d2eba-e1a6-4a78-b000-5a644a8bb008.PNG)

- Switch Platform to Android. And build it.

# Demo App
- We are providing the apk file (HelloARCoreRecord_v0.1.apk) for quickly use.

## Recording mode
- When the Record button is pressed, the button changes to red.
- Save RGB, IMU, and the custom data (pose of virtual object placed by touch) in MP4 (path: Application.persistentDataPath + "/" + "test.mp4").
- To stop, press the Record button again.

## Playback mode
- After recording stops (or if there is an MP4 file in the path in advance), press the Playback button to load the saved data.
- In this demo, the custom data (pose of the virtual object placed in the Recording mode) is additionally loaded.

# Reference
- Introduction to Recording and Playback on Unity targeting Android: https://developers.google.com/ar/develop/unity/recording-and-playback/introduction
