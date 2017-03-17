using UnityEngine;
using System.Collections;
using Tango;

public class BloonController : MonoBehaviour, ITangoDepth
{
	public GameObject m_marker;

	public MicHelper m_micHelper;

	public AudioSource m_balloonPopAudio;

	private BloonMarker m_currentMarker;

	private TangoApplication m_tangoApplication;

	bool m_findPlaneWaitingForDepth;

	public delegate void BalloonListener(GameObject obj);

	private BalloonListener m_balloonAddedListener, m_balloonPoppedListener;

	public void Init(TangoApplication app) {
		Debug.Log ("BloonController:Init()");
		m_tangoApplication = app;
		m_tangoApplication.Register(this);
	}

	public void SetOnBalloonAddedListener(BalloonListener listener) {
		m_balloonAddedListener = listener;
	}

	public void SetOnBalloonPoppedListener (BalloonListener listener)
	{
		m_balloonPoppedListener = listener;
	}

	// Use this for initialization
	void Start ()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.touchCount == 1)
		{
			Touch t = Input.GetTouch(0);
			_HandleTouch (t);
		}
	}

	private int m_touchCounter = 0;

	public void _HandleTouch (Touch t)
	{
		Camera cam = Camera.main;
		RaycastHit hitInfo;

		m_touchCounter++;

		if (t.phase == TouchPhase.Began) { // start touch
			Debug.Log (t.phase);

			m_touchCounter = 0;

//			bool hitObject = Physics.Raycast (cam.ScreenPointToRay (t.position), out hitInfo);
//			if (hitObject) {
//				GameObject tapped = hitInfo.collider.gameObject;
//				BloonMarker marker = tapped.GetComponent<BloonMarker> ();
//
//				Debug.LogFormat ("hitObject: {0}", tapped);
//
//				if (marker) {
//					_PlayBackBalloonAndPop (tapped.GetComponent<BloonMarker> ());
//					return;
//				}
//			}

			// tapped somewhere decent, add a balloon
			Debug.Log("Adding Balloon");
			StartCoroutine (_AddBalloon (t.position));

		} else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && m_currentMarker == null) {
			Debug.Log ("Waiting to ballon to be created...");
		} else if ((t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary) && m_currentMarker) {
			// TODO do cool shit while moving around/recording
			if (m_currentMarker.m_isRecording) {
				m_currentMarker.Grow ();
			}
		} else if (t.phase == TouchPhase.Ended && m_currentMarker ) { // end touch
			Debug.Log("Recording/TouchPhase complete");

			if(m_currentMarker.m_isRecording) {
				m_micHelper.StopRecording (m_currentMarker);
			}

//			bool hitObject = Physics.Raycast (cam.ScreenPointToRay (t.position), out hitInfo);
//			if (hitObject) {
//				GameObject tapped = hitInfo.collider.gameObject;
//				BloonMarker marker = tapped.GetComponent<BloonMarker> ();
//
//				Debug.LogFormat ("hitObject: {0}", tapped);
//
//				if (marker) {
//					_PlayBackBalloonAndPop (tapped.GetComponent<BloonMarker> ());
//					return;
//				}
//			}

			m_currentMarker = null;
		}
	}

	private void _PlayBackBalloonAndPop(BloonMarker marker) {
		Debug.Log (string.Format ("PlayBackBalloonAndPop: {0}", marker.m_audioRecordingFilename));

		m_balloonPopAudio.Play ();

		if(!string.IsNullOrEmpty(marker.m_audioRecordingFilename)) {
			m_micHelper.PlayRecording (marker.m_audioRecordingFilename);
		}

		marker.Pop ();
		m_balloonPoppedListener (marker.gameObject);

		if(m_currentMarker)
			m_currentMarker = null;
	}

	public GameObject AddMarkerByData (AreaLearningInGameController.MarkerData mark)
	{
		GameObject temp = Instantiate(m_marker,
			mark.m_position,
			mark.m_orientation) as GameObject;

		//re-enabled collider since we're not growing it
		temp.GetComponent<MeshCollider> ().enabled = true;

		return temp;
	}

	private IEnumerator _AddBalloon(Vector2 touchPosition) 
	{
		m_findPlaneWaitingForDepth = true;

		// Turn on the camera and wait for a single depth update.
		m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
		while (m_findPlaneWaitingForDepth)
		{
			yield return null;
		}

		m_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);

		// Find the plane.
		Camera cam = Camera.main;

		// Ensure the location is always facing the camera.  This is like a LookRotation, but for the Y axis.
		Vector3 up = Vector3.up; // plane.normal;
		Vector3 forward;
		if (Vector3.Angle(Vector3.up, cam.transform.forward) < 175)
		{
			Vector3 right = Vector3.Cross(up, cam.transform.forward).normalized;
			forward = Vector3.Cross(right, up).normalized;
		}
		else
		{
			// Normal is nearly parallel to camera look direction, the cross product would have too much
			// floating point error in it.
			forward = Vector3.Cross(up, cam.transform.right);
		}

		Vector3 inFront = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));

		// Instantiate marker object.
		GameObject newMarkObject = Instantiate(m_marker,
			inFront,
			Quaternion.LookRotation(forward, Vector3.up)
		) as GameObject;

		BloonMarker markerScript = newMarkObject.GetComponent<BloonMarker>();

//		markerScript.m_type = m_currentMarkType;
//		markerScript.m_timestamp = (float)m_poseController.m_poseTimestamp;

//		Matrix4x4 uwTDevice = Matrix4x4.TRS(m_poseController.m_tangoPosition,
//			m_poseController.m_tangoRotation,
//			Vector3.one);
//		Matrix4x4 uwTMarker = Matrix4x4.TRS(newMarkObject.transform.position,
//			newMarkObject.transform.rotation,
//			Vector3.one);
//		markerScript.m_deviceTMarker = Matrix4x4.Inverse(uwTDevice) * uwTMarker;

//		m_markerList.Add(newMarkObject);

		m_balloonAddedListener (newMarkObject);


		m_currentMarker = markerScript;

		Debug.LogFormat ("Balloon successfully Created: {0}", m_currentMarker);

		m_micHelper.StartRecording (m_currentMarker);
	}

	/// <summary>
	/// This is called each time new depth data is available.
	/// 
	/// On the Tango tablet, the depth callback occurs at 5 Hz.
	/// </summary>
	/// <param name="tangoDepth">Tango depth.</param>
	public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
	{
		// Don't handle depth here because the PointCloud may not have been updated yet.  Just
		// tell the coroutine it can continue.
		m_findPlaneWaitingForDepth = false;
	}



}

