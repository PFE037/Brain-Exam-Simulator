using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Varjo.XR;
using System.Collections.Specialized;


/*
 * Ce code a ete fait a partir du fichier EyeTrackingExample fournit par le plugin Varjo lors de l'installation de leur API sur Unity.
 * Celui-ci permet d'acceder aux differentes informations de la fonctionnalite du suivi des yeux et de pouvoir extraire ces informations.
 * Nous l'avons utiliser afin d'extraire la direction vers laquelle regarde l'utilisateur.
 * 
 * SOURCE: https://github.com/varjocom/VarjoUnityXRPlugin
 */


public class EyeTracking : MonoBehaviour
{
    [Header("Gaze calibration settings")]
    public VarjoEyeTracking.GazeCalibrationMode gazeCalibrationMode = VarjoEyeTracking.GazeCalibrationMode.Fast;
    public KeyCode calibrationRequestKey = KeyCode.Space;

    [Header("Gaze output filter settings")]
    public VarjoEyeTracking.GazeOutputFilterType gazeOutputFilterType = VarjoEyeTracking.GazeOutputFilterType.Standard;
    public KeyCode setOutputFilterTypeKey = KeyCode.RightShift;

    [Header("Gaze data output frequency")]
    public VarjoEyeTracking.GazeOutputFrequency frequency;

    [Header("Debug Gaze")]
    public KeyCode checkGazeAllowed = KeyCode.PageUp;
    public KeyCode checkGazeCalibrated = KeyCode.PageDown;


    [Header("Visualization Transforms")]
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;

    [Header("XR camera")]
    public Camera xrCamera;
    private float gazeRadius = 0.01f;

    public Transform eyesTransform;
    private List<InputDevice> devices = new List<InputDevice>();
    private InputDevice device;
    private Eyes eyes;
    private VarjoEyeTracking.GazeData gazeData;
    private List<VarjoEyeTracking.GazeData> dataSinceLastUpdate;
    private List<VarjoEyeTracking.EyeMeasurements> eyeMeasurementsSinceLastUpdate;
    private Vector3 leftEyePosition;
    private Vector3 rightEyePosition;
    private Quaternion leftEyeRotation;
    private Quaternion rightEyeRotation;
    private Vector3 direction;
    private Vector3 rayOrigin;
    private RaycastHit hit;
    private float distance;


    [Header("Eyes direction")]
    public Vector3 eyesDirection = Vector3.zero;


    void GetDevice()
    {
        InputDevices.GetDevicesAtXRNode(XRNode.CenterEye, devices);
        device = devices.FirstOrDefault();
    }

    void OnEnable()
    {
        if (!device.isValid)
        {
            GetDevice();
        }
    }

    private void Start()
    {
        VarjoEyeTracking.SetGazeOutputFrequency(frequency);
    }

    void Update()
    {
        // Request gaze calibration
        if (Input.GetKeyDown(calibrationRequestKey))
        {
            VarjoEyeTracking.RequestGazeCalibration(gazeCalibrationMode);
        }

        // Get gaze data if gaze is allowed and calibrated
        if (VarjoEyeTracking.IsGazeAllowed() && VarjoEyeTracking.IsGazeCalibrated())
        {
            //Get device if not valid
            if (!device.isValid)
            {
                GetDevice();
            }

            // Show gaze target
            gazeData = VarjoEyeTracking.GetGaze();

            if (gazeData.status != VarjoEyeTracking.GazeStatus.Invalid)
            {
                // GazeRay vectors are relative to the HMD pose so they need to be transformed to world space
                if (gazeData.leftStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
                {
                    leftEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.left.origin);
                    leftEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.left.forward));
                }

                if (gazeData.rightStatus != VarjoEyeTracking.GazeEyeStatus.Invalid)
                {
                    rightEyeTransform.position = xrCamera.transform.TransformPoint(gazeData.right.origin);
                    rightEyeTransform.rotation = Quaternion.LookRotation(xrCamera.transform.TransformDirection(gazeData.right.forward));
                }

                // Set gaze origin as raycast origin
                rayOrigin = xrCamera.transform.TransformPoint(gazeData.gaze.origin);

                // Set gaze direction as raycast direction
                direction = xrCamera.transform.TransformDirection(gazeData.gaze.forward);
            }
        }


        // Raycast to world from VR Camera position towards fixation point
        if (Physics.SphereCast(rayOrigin, gazeRadius, direction, out hit))
        {

            // Prefer layers or tags to identify looked objects in your application
            // This is done here using GetComponent for the sake of clarity as an example
            RotateWithGaze rotateWithGaze = hit.collider.gameObject.GetComponent<RotateWithGaze>();
            if (rotateWithGaze != null)
            {
                rotateWithGaze.RayHit();
            }
        }

        int dataCount = VarjoEyeTracking.GetGazeList(out dataSinceLastUpdate, out eyeMeasurementsSinceLastUpdate);

        for (int i = 0; i < dataCount; i++)
        {
            eyesDirection = dataSinceLastUpdate[i].gaze.forward;
            eyesTransform.localPosition = new Vector3(eyesDirection.x, eyesDirection.y, gazeData.focusDistance);
        }
    }
}