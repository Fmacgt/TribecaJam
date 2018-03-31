/**
* Copyright 2015 IBM Corp. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*
*/

using UnityEngine;
using System.Collections;
using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;

using TribecaJam;


public class ExampleStreaming : MonoBehaviour
{
    public RunningCharacter character;
    public TargetTextList targetTextList;

    public Text ResultsField;
    public Text[] targetTexts;
    public Text performanceDisplay;
    public Animator missionAnim;

    public float acceleration;
    public string username = "90b1d881-5177-4e18-9210-a9ad56cf93dd";
    public string password = "eZmE7JX3VapA";

	//==============================================================================

    private string _url = "https://stream.watsonplatform.net/speech-to-text/api";

    private int _recordingRoutine = 0;
    private string _microphoneID = null;
    private AudioClip _recording = null;
    private int _recordingBufferSize = 1;
    private int _recordingHZ = 22050;

    private int _missionId = 0;
    private SpeechToText _speechToText;
    private Mission thisMission;

	//==============================================================================

	private StringBuilder _builder;
    private int _textPtr = 0;

    private List<string> _targetBuffer;
    private List<string> _wordBuffer;
    private int _matchedCount = 0;
	private int _matchDisplayPtr = 0;
	private bool _missionCompleted = false;

    private float missionTimer = 0f;
    private bool startMissionTimer = false;

    /////////////////////////////////////////////////////////////////////////////////////

	private void Awake()
	{
        _targetBuffer = new List<string>(16);
        _wordBuffer = new List<string>(32);

		_builder = new StringBuilder();
	}

    void Start()
    {
        LogSystem.InstallDefaultReactors();
        performanceDisplay.text = "";

        //  Create credential and instantiate service
        Credentials credentials = new Credentials(username, password, _url);

        _speechToText = new SpeechToText(credentials);
        Active = true;

        _textPtr = Random.Range(0, targetTextList.Count);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _recording == null)
        {
            StartRecording();
        }
        else if (Input.GetKeyUp(KeyCode.R) && _recording != null)
        {
            StopRecording();
        }

        if (startMissionTimer)
        {
            missionTimer += Time.deltaTime;
        }
        //Debug.LogWarning(missionTimer);
        if (thisMission)
        {
            if (missionTimer > thisMission.badTime)
            {
                missionTimer = 0;
                startMissionTimer = false;
                _pickNewText();
            }
        }


		if (_matchDisplayPtr < _matchedCount) {
			_highlightMatchedWords();

			if (_missionCompleted && _matchDisplayPtr == _matchedCount) {
				_missionCompleted = false;

                generateRank(missionTimer);

				// wait and pick the next word
				LeanTween.delayedCall(gameObject, 0.5f, _pickNewText);
			}
		}
    }

    public void StartGame()
    {
        _pickNewText();
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                _speechToText.CustomizationId = "d2097a62-9d08-47ce-aebb-bae11b7f27da";
                _speechToText.CustomizationWeight = 1f;                
                _speechToText.AcousticCustomizationId = "4508215a-48e2-4e1f-aa4b-670cdef446e0";
                _speechToText.DetectSilence = true;
                _speechToText.EnableWordConfidence = true;
                _speechToText.EnableTimestamps = true;
                _speechToText.SilenceThreshold = 0.01f;
                _speechToText.MaxAlternatives = 0;
                _speechToText.EnableInterimResults = true;
                _speechToText.OnError = OnError;
                _speechToText.InactivityTimeout = -1;
                _speechToText.ProfanityFilter = false;
                _speechToText.SmartFormatting = true;
                _speechToText.SpeakerLabels = false;
                _speechToText.WordAlternativesThreshold = null;
                _speechToText.StartListening(OnRecognize, OnRecognizeSpeaker);

            }
            else if (!value && _speechToText.IsListening)
            {
                _speechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (_recordingRoutine != 0)
        {
            Microphone.End(_microphoneID);
            Runnable.Stop(_recordingRoutine);
            _recordingRoutine = 0;

            _recording = null;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming.OnError()", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        Log.Debug("ExampleStreaming.RecordingHandler()", "devices: {0}", Microphone.devices);
        _recording = Microphone.Start(_microphoneID, true, _recordingBufferSize, _recordingHZ);
        yield return null;      // let _recordingRoutine get set..

        if (_recording == null)
        {
            StopRecording();
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = _recording.samples / 2;
        float[] samples = null;

        while (_recordingRoutine != 0 && _recording != null)
        {
            int writePos = Microphone.GetPosition(_microphoneID);
            if (writePos > _recording.samples || !Microphone.IsRecording(_microphoneID))
            {
                Log.Error("ExampleStreaming.RecordingHandler()", "Microphone disconnected.");

                StopRecording();
                yield break;
            }

            if ((bFirstBlock && writePos >= midPoint)
                || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                _recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(Mathf.Abs(Mathf.Min(samples)), Mathf.Max(samples));
                record.Clip = AudioClip.Create("Recording", midPoint, _recording.channels, _recordingHZ, false);
                record.Clip.SetData(samples, 0);

                _speechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (_recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)_recordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }

    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = string.Format("[{3}] {0} ({1}, {2:0.00})\n",
                                      alt.transcript, res.final ? "Final" : "Interim", alt.confidence, result.result_index);
                    Log.Debug("ExampleStreaming.OnRecognize()", text);
                    ResultsField.text = text;

                    var words = alt.transcript.Split(
                                    new string[] { " ", "-", "," },
                                    10, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        _wordBuffer.Add(word.ToLower());
                    }

                    _debugPrintBuffer(_wordBuffer);


                    _tryToProcess();
                }
            }
        }
    }

    private void OnRecognizeSpeaker(SpeakerRecognitionEvent result)
    {
        if (result != null)
        {
            foreach (SpeakerLabelsResult labelResult in result.speaker_labels)
            {
                Log.Debug("ExampleStreaming.OnRecognize()", string.Format("speaker result: {0} | confidence: {3} | from: {1} | to: {2}", labelResult.speaker, labelResult.from, labelResult.to, labelResult.confidence));
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////

    private void _pickNewText()
    {
        _textPtr = (_textPtr + Random.Range(1, targetTextList.Count)) % targetTextList.Count;
        thisMission = targetTextList[_textPtr];

        _missionId ++;
        _missionId = _missionId % targetTexts.Length;

        targetTexts[_missionId].text = targetTextList[_textPtr].text;
        missionAnim.Play("SwitchTwister" + _missionId.ToString());
        missionTimer = 0f;
        startMissionTimer = true;
        performanceDisplay.text = "";


        var words = targetTextList[_textPtr].text.Split(
                        new string[] { " ", "," },
                        10, System.StringSplitOptions.RemoveEmptyEntries);
        _targetBuffer.Clear();
        foreach (var word in words)
        {
            _targetBuffer.Add(word.ToLower());
        }
        _debugPrintBuffer(_targetBuffer);

        _matchedCount = 0;
        _matchDisplayPtr = 0;
    }

    private void _tryToProcess()
    {
        if (_wordBuffer.Count == 0 || _missionCompleted || _matchedCount >= _targetBuffer.Count)
        {
            return;
        }

        // iterate _wordBuffer, to look for the first word in _targetBuffer
        int startIdx = 0;
        while (startIdx < _wordBuffer.Count &&
               string.Compare(_wordBuffer[startIdx], _targetBuffer[_matchedCount]) != 0)
        {
            startIdx++;
        }

        if (startIdx < _wordBuffer.Count &&
            string.Compare(_wordBuffer[startIdx], _targetBuffer[_matchedCount]) == 0)
        {
            // found a matching pair, start comparing remaining words
            int matchFrom = startIdx + 1;
            int matchTo = _matchedCount + 1;
            _matchedCount++;
            while (matchFrom < _wordBuffer.Count && matchTo < _targetBuffer.Count &&
                   string.Compare(_wordBuffer[matchFrom], _targetBuffer[matchTo]) == 0)
            {
                matchFrom++;
                matchTo++;

                _matchedCount++;
            }

            // end of matching, check remaining words
            _wordBuffer.RemoveRange(0, matchFrom);



			
            if (!_missionCompleted && _matchedCount >= _targetBuffer.Count)
            {
				_missionCompleted = true;

                if(missionTimer > 0)
                {
                    character.boost(acceleration * Mathf.Min(thisMission.badTime / missionTimer, 4f));
                }else
                {
                    character.boost(acceleration * 3);
                }

                character.successFX();

                // a complete match
                startMissionTimer = false;
                missionTimer = 0f;
            }
        }
        else
        {
            _wordBuffer.RemoveRange(0, Mathf.Min(startIdx, _wordBuffer.Count));
        }
    }

	private void _highlightMatchedWords()
	{
		_matchDisplayPtr++;


		_builder.Length = 0;

		_builder.Append("<color='red'>");
		for (int i = 0; i < _matchDisplayPtr; i++)
		{
			_builder.Append(_targetBuffer[i]);
			_builder.Append(" ");
		}
		_builder.Append("</color>");

		for (int i = _matchDisplayPtr; i < _targetBuffer.Count; i++)
		{
			_builder.Append(_targetBuffer[i]);
			_builder.Append(" ");
		}

        targetTexts[_missionId].text = _builder.ToString();
	}

    private void generateRank(float missionTimer)
    {
        var thisMission = targetTextList[_textPtr];
        if (missionTimer > thisMission.fastTime)
        {
            //TODO: give a OK rank visual effect
            performanceDisplay.text = "OK";
            character.boost(8f);
        }
        else if (missionTimer < thisMission.fastTime)
        {
            character.boost(15f);
            performanceDisplay.text = "Perfect";
            Debug.Log("Perfect");
            //TODO: give a FAST rank visual effect
        }

        if (thisMission.animationTrigger != null)
        {
            //TODO: animator.setTrigger(thisMission.animationTrigger);
        }

    }


    private void _debugPrintBuffer(List<string> buffer)
    {
		_builder.Length = 0;
        foreach (var word in buffer)
        {
            _builder.Append(word);
            _builder.Append(", ");
        }

        Debug.Log(_builder.ToString());
    }
}
