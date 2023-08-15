using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine;
using Varjo.XR;
using static VarjoMarkerManager;

/*
 * Ce fichier a été fait en se basant sur un exemple de la documentation officielle de Varjo permettant d'implémenter différente façon d'utiliser les marqueurs.
 * SOURCE: https://developer.varjo.com/docs/v3.1.0/unity-xr-sdk/varjo-markers-with-varjo-xr-plugin
 */

public class VarjoMarkerManager : MonoBehaviour
{
    // Serializable struct to make it easy to add tracked objects in the Inspector. 
    [Serializable]
    public struct TrackedObject
    {
        public List<long> ids;
        public GameObject gameObject;
        public bool dynamicTracking;
        public IDictionary<int, Vector3> markersPositions;
        public IDictionary<int, int> markersLastSeen;
        public Quaternion rotation;
    }

    // An public array for all the tracked objects. 
    public TrackedObject[] trackedObjects = new TrackedObject[1];

    // A list for found markers.
    private List<VarjoMarker> markers = new List<VarjoMarker>();

    // A list for IDs of removed markers.
    private List<long> removedMarkerIds = new List<long>();

    [SerializeField]
    public Transform RTS;
    public Leap.Unity.LeapRTS leapRTS;

    private void OnEnable()
    {
        // Enable Varjo Marker tracking.
        VarjoMarkers.EnableVarjoMarkers(true);
    }

    private void OnDisable()
    {
        // Disable Varjo Marker tracking.
        VarjoMarkers.EnableVarjoMarkers(false);
    }

    public float scale = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        for (var i = 0; i < trackedObjects.Length; i++)
        {
            trackedObjects[i].markersPositions = new Dictionary<int, Vector3>();
            trackedObjects[i].ids = new List<long> { 103, 104, 108, 109 };
            trackedObjects[i].markersLastSeen = new Dictionary<int, int>();
            trackedObjects[i].markersLastSeen.Add(0, 0);
            trackedObjects[i].markersLastSeen.Add(1, 0);
            trackedObjects[i].markersLastSeen.Add(2, 0);
            trackedObjects[i].markersLastSeen.Add(3, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {

        for (var i = 0; i < trackedObjects.Length; i++)
        {
            trackedObjects[i].markersPositions = new Dictionary<int, Vector3>();
            trackedObjects[i].markersLastSeen = new Dictionary<int, int>();
        }
        
        // Check if Varjo Marker tracking is enabled and functional.
        if (VarjoMarkers.IsVarjoMarkersEnabled())
        {
            // Get a list of markers with up-to-date data.
            VarjoMarkers.GetVarjoMarkers(out markers);

            for (var i = 0; i < trackedObjects.Length; i++)
            {
                for (var j = 0; j < trackedObjects[i].markersLastSeen.Count; j++)
                {
                    trackedObjects[i].markersLastSeen[j]++;
                }
            }

            foreach (var marker in markers)
            {
                for (var i = 0; i < trackedObjects.Length; i++)
                {
                    for (var j = 0; j < trackedObjects[i].ids.Count; j++)
                    {
                        if (trackedObjects[i].ids[j] == marker.id)
                        {
                            trackedObjects[i].markersLastSeen[j] = 0;
                            trackedObjects[i].markersPositions[j] = marker.pose.position;
                            trackedObjects[i].rotation = Quaternion.RotateTowards(trackedObjects[i].rotation, marker.pose.rotation, 0.1f);
                            if ((marker.flags == VarjoMarkerFlags.DoPrediction) != trackedObjects[i].dynamicTracking)
                            {
                                if (trackedObjects[i].dynamicTracking)
                                {
                                    VarjoMarkers.AddVarjoMarkerFlags(marker.id, VarjoMarkerFlags.DoPrediction);
                                }
                                else
                                {
                                    VarjoMarkers.RemoveVarjoMarkerFlags(marker.id, VarjoMarkerFlags.DoPrediction);
                                }
                            }
                        }
                    }
                }
            }
            foreach (var trackedObject in trackedObjects)
            {

                if (leapRTS.resetTransformations)
                {
                    RTS.rotation = trackedObject.rotation;
                    RTS.localScale = Vector3.one;
                }

                trackedObject.gameObject.transform.position = Vector3.MoveTowards(trackedObject.gameObject.transform.position, getCenter(trackedObject), (trackedObject.gameObject.transform.position - (getCenter(trackedObject))).magnitude);
                trackedObject.gameObject.transform.rotation = RTS.rotation * trackedObject.rotation;
                trackedObject.gameObject.transform.localScale = RTS.localScale;
            }
        }
    }

    private Vector3 getCenter(TrackedObject trackedObject)
    {
        Vector3 center = Vector3.zero;

        for (var i = 0; i < trackedObject.ids.Count; i++)
        {
            if (trackedObject.markersPositions.ContainsKey(0) && trackedObject.markersPositions.ContainsKey(2))
            {
                if (i % 2 == 0)
                {
                    center += trackedObject.markersPositions[i] / 2.0f;
                }
            }
            else if (trackedObject.markersPositions.ContainsKey(1) && trackedObject.markersPositions.ContainsKey(3))
            {
                if (i % 2 != 0)
                {
                    center += trackedObject.markersPositions[i] / 2.0f;
                }
            }
            else if (trackedObject.markersPositions.Count == trackedObject.ids.Count)
            {
                center += trackedObject.markersPositions[i] / trackedObject.markersPositions.Count;
            }
        }
        return center;

    }
}