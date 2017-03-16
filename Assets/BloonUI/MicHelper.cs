using UnityEngine;
using System.Collections;

public class MicHelper : MonoBehaviour
{
	//A boolean that flags whether there's a connected microphone  
	private bool micConnected = false;  

	//The maximum and minimum available recording frequencies  
	private int minFreq;  
	private int maxFreq;  

	// TODO make this 
	private int MAX_RECORDING_SECONDS = 20;

	//A handle to the attached AudioSource  
	private AudioSource goAudioSource;  

	//Use this for initialization  
	void Start()   
	{  
		foreach (string device in Microphone.devices) {
			Debug.Log("NameOfMics: " + device);
		}

		//Check if there is at least one microphone connected  
		if(Microphone.devices.Length <= 0)  
		{  
			//Throw a warning message at the console if there isn't  
			Debug.LogWarning("Microphone not connected!");  
		}  
		else //At least one microphone is present  
		{  
			//Set 'micConnected' to true  
			micConnected = true;  

			//Get the default microphone recording capabilities  
			Microphone.GetDeviceCaps(null, out minFreq, out maxFreq);  

			//According to the documentation, if minFreq and maxFreq are zero, the microphone supports any frequency...  
			if(minFreq == 0 && maxFreq == 0)  
			{  
				//...meaning 44100 Hz can be used as the recording sampling rate  
				maxFreq = 44100;  
			}  

			//Get the attached AudioSource component  
			goAudioSource = this.GetComponent<AudioSource>();  
		}  
	}  

	public void StartRecording(BloonMarker marker) {
		if(micConnected)  
		{  
			Debug.Log ("StartRecording: connected");
			//If the audio from any microphone isn't being captured  
			if(!Microphone.IsRecording(null))  
			{  
				Debug.Log ("StartRecording: isRecording:false");
				//Start recording and store the audio captured from the microphone at the AudioClip in the AudioSource  
				marker.m_isRecording = true;
				goAudioSource.clip = Microphone.Start(null, false, MAX_RECORDING_SECONDS, maxFreq);  
			}  
		}  
		else // No microphone  
		{  
			//Print a red "Microphone not connected!" message at the center of the screen  
			Debug.LogError("No Microphone connected!");
		}  
	}

	public void StopRecording(BloonMarker marker) {
		Debug.Log ("StopRecording");

		if (Microphone.IsRecording (null)) {
			Debug.Log ("StopRecording: End()");
			Microphone.End (null);
		}

		marker.m_isRecording = false;

		// test audio
		goAudioSource.Play(); //Playback the recorded audio  

		_StoreAudioFile (marker);
	}

	void _StoreAudioFile (BloonMarker marker)
	{
		string filename = string.Format(@"{0}.wav", System.DateTime.Now.Ticks);

		Debug.Log ("StoreAudioFile Starting...");
		bool complete = SavWav.Save (filename, goAudioSource.clip);

		if (complete) {
			Debug.Log (string.Format("StoreAudioFile Complete: {0}", filename));
			marker.m_audioRecordingFilename = filename;
		}
		else
			Debug.LogError ("Error saving file...");
	}

	public void PlayRecording (string filepath)
	{
		Debug.Log (string.Format ("File exists: {0}", System.IO.File.Exists (filepath)));

//		Debug.Log (System.IO.Path.GetDirectoryName (filepath));
		string url = "file://" + filepath;

		Debug.Log (string.Format ("PlayRecording() {0}", url));

		WWW www = new WWW(url);
		while (!www.isDone) 
		{
			//Wait untill it's done
			Debug.Log("downloading");
		}

		goAudioSource.clip = WWWAudioExtensions.GetAudioClip (www, true, false, AudioType.WAV);
//		goAudioSource.clip.LoadAudioData ();
		Debug.Log (goAudioSource.clip.loadState);
		goAudioSource.Play ();
	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}
}

