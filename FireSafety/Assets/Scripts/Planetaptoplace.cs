using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

// Attach this to your XR Origin (the object that has ARPlaneManager and ARRaycastManager on it).
// Lets the trainee tap a detected floor/table surface to place the training scenario prefab.
//
// Owner: Riddhi (AR)
// Depends on: AR Foundation package + ARCore XR Plugin already installed (see setup steps).

[RequireComponent(typeof(ARRaycastManager))]
public class PlaneTapToPlace : MonoBehaviour
{
    [Tooltip("The training scenario prefab to spawn (Fire module or Gas module root object).")]
    public GameObject scenarioPrefab;

    [Tooltip("Once placed, hide the plane visualizations so the scene looks cleaner.")]
    public ARPlaneManager planeManager;

    private ARRaycastManager raycastManager;
    private GameObject spawnedScenario;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        // Only allow placement if nothing has been placed yet
        if (spawnedScenario != null) return;

        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began) return;

        if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            PlaceScenario(hitPose);
        }
    }

    void PlaceScenario(Pose pose)
    {
        spawnedScenario = Instantiate(scenarioPrefab, pose.position, pose.rotation);

        // Hide plane visuals once the scenario is placed so it looks less cluttered
        if (planeManager != null)
        {
            planeManager.SetTrackablesActive(false);
            planeManager.enabled = false;
        }

        // Notify the training state machine that placement is done and training can begin
        var stateMachine = FindObjectOfType<TrainingStateMachine>();
        if (stateMachine != null)
        {
            stateMachine.OnScenarioPlaced(spawnedScenario);
        }
    }

    // Call this if you want to let the trainee reset placement (e.g. a "reposition" button)
    public void ResetPlacement()
    {
        if (spawnedScenario != null)
        {
            Destroy(spawnedScenario);
            spawnedScenario = null;
        }
        if (planeManager != null)
        {
            planeManager.enabled = true;
            planeManager.SetTrackablesActive(true);
        }
    }
}