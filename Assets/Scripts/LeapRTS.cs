
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

namespace Leap.Unity
{

    /*
     * Ce fichier a été fait en se basant sur le fichier initial qui a été fourni lors de l'importation d'un exemple dans lequel on pouvait appliquer des transformations à un objet.
     * SOURCE: https://www.developer.leapmotion.com/releases/detection-examples-103-hbe4t-mwykd
     */

    public class LeapRTS : MonoBehaviour
    {

        public enum RotationMethod
        {
            None,
            Single,
            Full
        }

        [Header("Hands Detector")]
        [SerializeField]
        private PinchDetector _pinchDetectorA;
        public PinchDetector PinchDetectorA
        {
            get
            {
                return _pinchDetectorA;
            }
            set
            {
                _pinchDetectorA = value;
            }
        }


        [SerializeField]
        private PinchDetector _pinchDetectorB;
        public PinchDetector PinchDetectorB
        {
            get
            {
                return _pinchDetectorB;
            }
            set
            {
                _pinchDetectorB = value;
            }
        }

        public HandPoseDetector resetTransformationGesture;


        [Header("Types of rotation")]
        [SerializeField]
        private RotationMethod _oneHandedRotationMethod;

        [SerializeField]
        private RotationMethod _twoHandedRotationMethod;

        [SerializeField]
        private bool _allowScale = true;

        public Transform _anchor;

        private float _defaultNearClip;

        public bool isPinching = false;

        private float previousPinchDistance;
        private float minimumPinchChangeDistance;
        public bool resetTransformations = false;


        void Start()
        {
            if (_pinchDetectorA == null || _pinchDetectorB == null)
            {
                Debug.LogWarning("Both Pinch Detectors of the LeapRTS component must be assigned. This component has been disabled.");
                enabled = false;
            }

            GameObject pinchControl = new GameObject("RTS Anchor");
            _anchor = pinchControl.transform;
            _anchor.transform.parent = transform.parent;
            transform.parent = _anchor;
            previousPinchDistance = 0;
            minimumPinchChangeDistance = 0.005f;
        }

        void Update()
        {
            isPinching = false;
            bool didUpdate = false;
            if (_pinchDetectorA != null)
                didUpdate |= _pinchDetectorA.DidChangeFromLastFrame;
            if (_pinchDetectorB != null)
                didUpdate |= _pinchDetectorB.DidChangeFromLastFrame;

            if (didUpdate)
            {
                transform.SetParent(null, true);
            }

            if (_pinchDetectorA != null && _pinchDetectorA.IsActive &&
                _pinchDetectorB != null && _pinchDetectorB.IsActive)
            {
                    isPinching = true;
                    transformDoubleAnchor();
            }
            else if (_pinchDetectorA != null && _pinchDetectorA.IsActive)
            {
                transformSingleAnchor(_pinchDetectorA);
            }
            else if (_pinchDetectorB != null && _pinchDetectorB.IsActive)
            {
                transformSingleAnchor(_pinchDetectorB);
            }

            if (didUpdate)
            {
                transform.SetParent(_anchor, true);
            }


            HandPoseScriptableObject detectedPose = resetTransformationGesture.GetCurrentlyDetectedPose();
            if (detectedPose != null)
            {
                resetTransformations = true;
            }
            else
            {
                resetTransformations = false;
            }
        }

        private void transformDoubleAnchor()
        {
            switch (_twoHandedRotationMethod)
            {
                case RotationMethod.None:
                    break;
                case RotationMethod.Single:
                    Vector3 p = _pinchDetectorA.Position;
                    p.y = _anchor.position.y;
                    _anchor.LookAt(p);
                    break;
                case RotationMethod.Full:
                    Quaternion pp = Quaternion.Lerp(_pinchDetectorA.Rotation, _pinchDetectorB.Rotation, 0.5f);
                    Vector3 u = pp * Vector3.up;
                    _anchor.LookAt(_pinchDetectorA.Position, u);
                    break;
            }

            if (_allowScale)
            {
                float currentPitchDistance = Vector3.Distance(_pinchDetectorA.Position, _pinchDetectorB.Position);
                if(currentPitchDistance > previousPinchDistance + minimumPinchChangeDistance)
                {
                    transform.localScale = Vector3.MoveTowards(transform.localScale, transform.localScale + Vector3.one * currentPitchDistance, 10f);
                }
                else if (currentPitchDistance < previousPinchDistance - minimumPinchChangeDistance)
                {
                    transform.localScale = Vector3.MoveTowards(transform.localScale, transform.localScale - Vector3.one * currentPitchDistance, 10f);
                }
                previousPinchDistance = currentPitchDistance;
            }
        }

        private void transformSingleAnchor(PinchDetector singlePinch)
        {
            switch (_oneHandedRotationMethod)
            {
                case RotationMethod.None:
                    _anchor.position = singlePinch.Position;
                    break;
                case RotationMethod.Single:
                    Vector3 p = singlePinch.Rotation * Vector3.right;
                    p.y = _anchor.position.y;
                    _anchor.LookAt(p);
                    break;
                case RotationMethod.Full:
                    _anchor.rotation = singlePinch.Rotation;
                    break;
            }
        }
    }
}