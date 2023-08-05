    using UnityEngine;
    using UnityEngine.XR;
    using Leap.Unity;

/*
* Ce fichier a été fait en se basant sur la documentation de Varjo afin de définir automatiquement un offset pour la position des mains selon le type de casque utilisé.
* SOURCE: https://developer.varjo.com/docs/unity-xr-sdk/hand-tracking-with-varjo-xr-plugin
*/

[RequireComponent(typeof(LeapXRServiceProvider))]
    public class VarjoHandTrackingOffset : MonoBehaviour
    {
    private InputDevice hmd;
    private LeapXRServiceProvider xrServiceProvider;

    void Start()
    {
            hmd = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            xrServiceProvider = GetComponent<LeapXRServiceProvider>();

            switch (hmd.name)
            {
            case "XR-3":
            case "VR-3":
                    xrServiceProvider.deviceOffsetMode = LeapXRServiceProvider.DeviceOffsetMode.ManualHeadOffset;
                    xrServiceProvider.deviceOffsetYAxis = -0.0112f;
                    xrServiceProvider.deviceOffsetZAxis = 0.0999f;
                    xrServiceProvider.deviceTiltXAxis = 0f;
                    break;
            case "VR-2 Pro":
                    xrServiceProvider.deviceOffsetMode = LeapXRServiceProvider.DeviceOffsetMode.ManualHeadOffset;
                    xrServiceProvider.deviceOffsetYAxis = -0.025734f;
                    xrServiceProvider.deviceOffsetZAxis = 0.068423f;
                    xrServiceProvider.deviceTiltXAxis = 5f;
                    break;
            }
    }
    }