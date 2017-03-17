//-----------------------------------------------------------------------
// <copyright file="ARMarker.cs" company="Google">
//
// Copyright 2016 Google Inc. All Rights Reserved.
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
using System.Collections;
using UnityEngine;

/// <summary>
/// Location marker script to show hide/show animations.
///
/// Instead of calling destroy on this, send the "Hide" message.
/// </summary>
public class BloonMarker : MonoBehaviour
{
	/// <summary>
	/// The type of the location mark.
	/// 
	/// This field is used in the Area Learning example for identify the marker type.
	/// </summary>
	public int m_type = 0;

	/// <summary>
	/// The Tango time stamp when this object is created
	/// 
	/// This field is used in the Area Learning example, the timestamp is save for the position adjustment when the
	/// loop closure happens.
	/// </summary>
	public float m_timestamp = -1.0f;

	public bool m_isRecording = false;

	public bool m_hasRecording = false;

	public string m_audioRecordingFilename = "";

//	public float m_growthSteps = 0.8f / (20 * 60);

	/// <summary>
	/// The marker's transformation with respect to the device frame.
	/// </summary>
	public Matrix4x4 m_deviceTMarker = new Matrix4x4();

	/// <summary>
	/// The animation playing.
	/// </summary>
	private Animation m_anim;

	private const float START_SCALE = 0.4f;

	/// <summary>
	/// Awake this instance.
	/// </summary>
	private void Awake()
	{
		this.transform.localScale = new Vector3 (START_SCALE, START_SCALE, START_SCALE);

		// The animation should be started in Awake and not Start so that it plays on its first frame.
//		m_anim = GetComponent<Animation>();
//		m_anim.Play("ARMarkerShow", PlayMode.StopAll);
	}

	public void Grow() {
		Vector3 scale = this.transform.localScale;
		float nextScale = scale.x + ((Time.deltaTime / MicHelper.MAX_RECORDING_SECONDS) * 0.8f);

		Debug.LogFormat ("BloonMarker:Grow() increaseBy: {0}", nextScale);

		scale.Set (nextScale, nextScale, nextScale);
		this.transform.localScale = scale;
	}

	public void RecordingComplete ()
	{
		this.GetComponent<MeshCollider> ().enabled = true;
	}

	public void Pop ()
	{
		// TODO animate destruction
		HideDone ();
	}

	Vector3 m_lastCamPos = Vector3.zero;
	Vector3 m_balloonVelocity = Vector3.zero;

	void Update() {
		// float up and shit
		Vector3 lastPos = this.transform.position;
		float yPlusFloat = lastPos.y + (3.0f / (1000.0f * 15.0f));

//		lastPos.Set (lastPos.x, yPlusFloat, lastPos.z);
		lastPos.y = yPlusFloat;

		Vector3 camPos = Camera.main.transform.position;

		float dist = _GetCamDistance ();
		if (dist < .3) {
			Vector3 camVel = camPos - m_lastCamPos;
			Debug.LogFormat ("velocity: {0}", camVel);

			m_balloonVelocity += camVel;
		}

		lastPos += m_balloonVelocity;

		// set it and forget it
		this.transform.position = lastPos;

		// slow down and set shit
		m_lastCamPos = camPos;
		m_balloonVelocity.Scale (new Vector3(0.95f, 0.95f, 0.95f));

	}

	float _GetCamDistance ()
	{
		return Vector3.Distance (this.transform.position, Camera.main.transform.position);
	}

	/// <summary>
	/// Plays an animation, then destroys.
	/// </summary>
	private void Hide()
	{
//		m_anim.Play("ARMarkerHide", PlayMode.StopAll);
	}

	/// <summary>
	/// Callback for the animation system.
	/// </summary>
	private void HideDone()
	{
		Destroy(gameObject);
	}
}
