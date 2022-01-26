//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google LLC">
//
// Copyright 2020 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System;
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloARController : MonoBehaviour
    {
        /// <summary>
        /// The Depth Setting Menu.
        /// </summary>
        public DepthMenu DepthMenu;

        /// <summary>
        /// The Instant Placement Setting Menu.
        /// </summary>
        public InstantPlacementMenu InstantPlacementMenu;

        /// <summary>
        /// A prefab to place when an instant placement raycast from a user touch hits an instant
        /// placement point.
        /// </summary>
        public GameObject InstantPlacementPrefab;

        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR
        /// background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a vertical plane.
        /// </summary>
        public GameObject GameObjectVerticalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a horizontal plane.
        /// </summary>
        public GameObject GameObjectHorizontalPlanePrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a feature point.
        /// </summary>
        public GameObject GameObjectPointPrefab;

        /// <summary>
        /// A prefab to place when a raycast from a user touch hits a depth point.
        /// </summary>
        public GameObject GameObjectDepthPointPrefab;

        /// <summary>
        /// The rotation in degrees need to apply to prefab when it is placed.
        /// </summary>
        private const float _prefabRotation = 180.0f;

        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error,
        /// otherwise false.
        /// </summary>
        private bool _isQuitting = false;

        // Recording & Playback parameter
        ARCoreRecordingConfig record_test;
        public GameObject ARCoreDevice;
        ARCoreSession session;

        public int MenuRecordFlag = 0;
        public int MenuPlaybackFlag = 0;
        
        int playback_init = 0;
        public GameObject MenuRecordButton, MenuPlaybackButton;

        List<Track> track_list;
        byte[] meta_init = new byte[1];

        //RecordingResult recordingresult;
        PlaybackResult playbackresult;

        /////////////////////////////////

        /// <summary>
        /// The Unity Awake() method.
        /// </summary>
        public void Awake()
        {
            // Enable ARCore to target 60fps camera capture frame rate on supported devices.
            // Note, Application.targetFrameRate is ignored when QualitySettings.vSyncCount != 0.
            Application.targetFrameRate = 60;

            session = ARCoreDevice.GetComponent<ARCoreSession>();

            record_test = new ARCoreRecordingConfig();
            record_test.Mp4DatasetFilepath = Application.persistentDataPath + "/" + "test.mp4";

            meta_init[0] = (byte)0;

            // Track track_hit init(UUID, Metadata, MimeType)
            var track_hit = new Track
            {
                Id = Guid.Parse("de5ec7a4-09ec-4c48-b2c3-a98b66e71894") // from UUID generator
                ,
                Metadata = meta_init
                ,
                MimeType = ""
            };

            track_list = new List<Track> { track_hit };
            record_test.Tracks = track_list;
        }

        public void OnMenuRecordClick()
        {
            //record
            if (MenuRecordFlag == 0)
            {
                Session.StartRecording(record_test);

                MenuRecordButton.GetComponent<Image>().color = Color.red;
                MenuRecordFlag = 1;
            }
            //stop
            else if (MenuRecordFlag == 1)
            {
                Session.StopRecording();

                MenuRecordButton.GetComponent<Image>().color = Color.green;
                MenuRecordFlag = 0;
            }
        }

        public void OnMenuPlaybackClick()
        {
            //playback
            if (MenuPlaybackFlag == 0)
            {
                //MenuPlaybackButton.GetComponent<Image>().color = Color.red;
                MenuPlaybackFlag = 1;
            }
            //stop
            /*else if (MenuPlaybackFlag == 1)
            {
                MenuPlaybackButton.GetComponent<Image>().color = Color.green;
                MenuPlaybackFlag = 0;
                PlaybackFlag2 = 0;
            }*/
        }

        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            //Playback init
            if(MenuPlaybackFlag == 1){
                // Pause the current session when providing the path.
                if (playback_init == 0)
                {
                    session.enabled = false;
                }
                else if (playback_init == 1)
                {
                    // In the next frame, provide a filepath for the dataset you wish to play back.
                    playbackresult = Session.SetPlaybackDataset(Application.persistentDataPath + "/" + "test.mp4");
                    if(playbackresult == PlaybackResult.OK)
                    {
                        MenuPlaybackButton.GetComponent<Image>().color = Color.blue;
                    }
                    else{
                        MenuPlaybackButton.GetComponent<Image>().color = Color.magenta;
                    }
                }
                else if (playback_init == 2)
                {
                    // In the frame after that, resume the session.
                    session.enabled = true;
                }
                if (playback_init < 3)
                {
                    playback_init++;
                }
            }

            UpdateApplicationLifecycle();

            //Playback mode
            if (MenuPlaybackFlag == 1 && playback_init > 2)
            {
                //Get updated track data
                var trackDataList = Frame.GetUpdatedTrackData(Guid.Parse("de5ec7a4-09ec-4c48-b2c3-a98b66e71894"));

                // Extract the bytes of custom data from the list of track data.
                foreach (TrackData trackData in trackDataList)
                {
                    var byteArray4 = trackData.Data;
                    var floatArray4 = new float[byteArray4.Length * 1 / 4];
                    Buffer.BlockCopy(byteArray4, 0, floatArray4, 0, byteArray4.Length);

                    // Instantiate prefab at the hit pose.
                    var gameObject = Instantiate(GameObjectPointPrefab, new Vector3(floatArray4[0], floatArray4[1], floatArray4[2]),
                                                                        new Quaternion(floatArray4[3], floatArray4[4], floatArray4[5], floatArray4[6]));

                    // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                    gameObject.transform.Rotate(0, _prefabRotation, 0, Space.Self);
                }
            }


            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Should not handle input if the player is pointing on UI.
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            bool foundHit = false;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;
            // Allows the depth image to be queried for hit tests.
            raycastFilter |= TrackableHitFlags.Depth;
            foundHit = Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit);
            if (!foundHit && InstantPlacementMenu.IsInstantPlacementEnabled())
            {
                foundHit = Frame.RaycastInstantPlacement(
                    touch.position.x, touch.position.y, 1.0f, out hit);
            }

            if (foundHit)
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else
                {
                    if (DepthMenu != null)
                    {
                        // Show depth card window if necessary.
                        DepthMenu.ConfigureDepthBeforePlacingFirstAsset();
                    }

                    // Choose the prefab based on the Trackable that got hit.
                    GameObject prefab;
                    if (hit.Trackable is InstantPlacementPoint)
                    {
                        prefab = InstantPlacementPrefab;
                    }
                    else if (hit.Trackable is FeaturePoint)
                    {
                        prefab = GameObjectPointPrefab;
                    }
                    else if (hit.Trackable is DepthPoint)
                    {
                        prefab = GameObjectDepthPointPrefab;
                    }
                    else if (hit.Trackable is DetectedPlane)
                    {
                        DetectedPlane detectedPlane = hit.Trackable as DetectedPlane;
                        if (detectedPlane.PlaneType == DetectedPlaneType.Vertical)
                        {
                            prefab = GameObjectVerticalPlanePrefab;
                        }
                        else
                        {
                            prefab = GameObjectHorizontalPlanePrefab;
                        }
                    }
                    else
                    {
                        prefab = GameObjectHorizontalPlanePrefab;
                    }

                    // Record the hit data
                    if (MenuRecordFlag == 1)
                    {
                        var floatArray3 = new float[] { hit.Pose.position.x, hit.Pose.position.y, hit.Pose.position.z,
                                                hit.Pose.rotation.x, hit.Pose.rotation.y, hit.Pose.rotation.z, hit.Pose.rotation.w };

                        // create a byte array and copy the floats into it
                        var byteArray3 = new byte[floatArray3.Length * 4];
                        Buffer.BlockCopy(floatArray3, 0, byteArray3, 0, byteArray3.Length);

                        Frame.RecordTrackData(Guid.Parse("de5ec7a4-09ec-4c48-b2c3-a98b66e71894"), byteArray3);

                    }

                    // Instantiate prefab at the hit pose.
                    var gameObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    // Compensate for the hitPose rotation facing away from the raycast (i.e.
                    // camera).
                    gameObject.transform.Rotate(0, _prefabRotation, 0, Space.Self);

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of
                    // the physical world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                    // Make game object a child of the anchor.
                    gameObject.transform.parent = anchor.transform;

                    // Initialize Instant Placement Effect.
                    if (hit.Trackable is InstantPlacementPoint)
                    {
                        gameObject.GetComponentInChildren<InstantPlacementEffect>()
                            .InitializeWithTrackable(hit.Trackable);
                    }
                }
            }
        }

        /// <summary>
        /// Check and update the application lifecycle.
        /// </summary>
        private void UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                Screen.sleepTimeout = SleepTimeout.SystemSetting;
            }
            else
            {
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }

            if (_isQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to
            // appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                ShowAndroidToastMessage("Camera permission is needed to run this application.");
                _isQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                ShowAndroidToastMessage(
                    "ARCore encountered a problem connecting.  Please start the app again.");
                _isQuitting = true;
                Invoke("DoQuit", 0.5f);
            }
        }

        /// <summary>
        /// Actually quit the application.
        /// </summary>
        private void DoQuit()
        {
            Application.Quit();
        }

        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        private void ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity =
                unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>(
                            "makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
