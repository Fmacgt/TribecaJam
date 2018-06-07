
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


public class TestSpeechRecognition : MonoBehaviour
{
    public TargetTextList targetTextList;

    public string username = "90b1d881-5177-4e18-9210-a9ad56cf93dd";
    public string password = "eZmE7JX3VapA";

    public string customModelId = "6df1f2ff-4248-49c2-8d63-9dc413b88956";

    public float spawnRadius = 5f;

    //==============================================================================

    private struct Candidate
    {
        public int wordCount;
        public string[] words;
        public GameObject prefab;
    }

    private List<Candidate> _candidates;
    private List<int> _recognizedIndices;

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
    private bool _gameRunning = false;

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
        _builder = new StringBuilder();

        _targetBuffer = new List<string>(16);
        _wordBuffer = new List<string>(32);

        _assembleCandidates(targetTextList.list);
    }

    void Start()
    {
        LogSystem.InstallDefaultReactors();
//        performanceDisplay.text = "";

        //  Create credential and instantiate service
        Credentials credentials = new Credentials(username, password, _url);

        _speechToText = new SpeechToText(credentials);
        Active = true;
        StartRecording();
        _textPtr = Random.Range(0, targetTextList.Count);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _recording == null)
        {
            Debug.Log("Stop recording");
            StopRecording();
        }
        else if (Input.GetKeyUp(KeyCode.R) && _recording != null)
        {
            Debug.Log("Start recording");
            StartRecording();
        }

        if (startMissionTimer)
        {
            missionTimer += Time.deltaTime;
        }
        if (thisMission && _gameRunning)
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
        _gameRunning = true;

        _pickNewText();
    }

    public void StopGame()
    {
        _gameRunning = false;
    }

    public bool Active
    {
        get { return _speechToText.IsListening; }
        set
        {
            if (value && !_speechToText.IsListening)
            {
                //_speechToText.CustomizationId = "d2097a62-9d08-47ce-aebb-bae11b7f27da";
                _speechToText.CustomizationId = customModelId;
                _speechToText.CustomizationWeight = 1f;                
//                _speechToText.AcousticCustomizationId = "4508215a-48e2-4e1f-aa4b-670cdef446e0";
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

    public void StartRecording()
    {
        if (_recordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            _recordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    public void StopRecording()
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
                    //string text = string.Format("[{3}] {0} ({1}, {2:0.00})\n", alt.transcript, res.final ? "Final" : "Interim", alt.confidence, result.result_index);
                    string text = string.Format("{0}", alt.transcript);
                    Log.Debug("ExampleStreaming.OnRecognize()", text);
//                    ResultsField.text = text;

                    var words = alt.transcript.Split(
                                    new string[] { " ", "-", "," },
                                    10, System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var word in words)
                    {
                        _wordBuffer.Add(word.ToLower());
                    }

                    _debugPrintBuffer(_wordBuffer);


//                    _tryToProcess();
                    _tryProcessMultiple();
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

    private void _assembleCandidates(Mission[] missionList)
    {
        int candidateCount = missionList.Length;
        _candidates = new List<Candidate>(candidateCount);
        for (int i = 0; i < candidateCount; i++) {
            var mission = targetTextList.list[i];

            var candidate = new Candidate();
            candidate.words = mission.text.Split(
                    new string[] { " ", "-", "," },
                    10, System.StringSplitOptions.RemoveEmptyEntries);
            candidate.wordCount = candidate.words.Length;
            candidate.prefab = mission.targetPrefab;

            for (int wordIdx = 0; wordIdx < candidate.wordCount; wordIdx++) {
                candidate.words[wordIdx] = candidate.words[wordIdx].ToLower();
            }
            _debugPrintBuffer("[INIT]", candidate.words);

            _candidates.Add(candidate);
        }

        _recognizedIndices = new List<int>(candidateCount);
        for (int i = 0; i < candidateCount; i++) {
            _recognizedIndices.Add(0);
        }
    }

    private void _pickNewText()
    {
        /**
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
        **/
    }

    private void _spawnItem(GameObject prefab)
    {
        var direct = Random.insideUnitCircle * spawnRadius;
        var pt = transform.position + new Vector3(direct.x, 0f, direct.y);

        var obj = Instantiate(prefab);
        obj.transform.position = pt;
        obj.transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
    }

    private void _lookForKeyword()
    {
        int targetCount = targetTextList.list.Length;
        int removePtr = 0;
        for (int i = 0; i < _wordBuffer.Count; i++) {
            string word = _wordBuffer[i];

            for (int targetPtr = 0; targetPtr < targetCount; targetPtr++) {
                string targetWord = targetTextList.list[targetPtr].text.ToLower();
                int compare = string.Compare(targetWord, word);// == 0
                Debug.LogFormat("[COMPARE] {0} == {1}? {2}", targetWord, word, compare);
                if (compare == 0) {
                    _spawnItem(targetTextList.list[targetPtr].targetPrefab);
                }
            }

            removePtr++;
        }

        for (int i = 0; i < removePtr; i++) {
            if (_wordBuffer.Count > 0) {
                _wordBuffer.RemoveAt(0);
            }
        }
    }

    private void _tryToProcess()
    {
        /**
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

                if(missionTimer > 0 && missionTimer > thisMission.fastTime)
                {
                }else
                {
                }


                // a complete match
                startMissionTimer = false;
                missionTimer = 0f;
            }
        }
        else
        {
            _wordBuffer.RemoveRange(0, Mathf.Min(startIdx, _wordBuffer.Count));
        }
        **/
    }

    private void _tryProcessMultiple()
    {
        if (_wordBuffer.Count == 0) {
            return;
        }

        // iterate each candidate and try to match a longest consecutive string with 
        // words in the word buffer
        int maxRecognizedWordCount = 0;
        int candidateCount = targetTextList.list.Length;
        for (int candidateIdx = 0; candidateIdx < candidateCount; candidateIdx++) {
            int recognizedWordCount = 0;

            int recognizedIndex = _recognizedIndices[candidateIdx];
            recognizedIndex = _processCandidate(candidateIdx, recognizedIndex, out recognizedWordCount);
            _recognizedIndices[candidateIdx] = recognizedIndex;

            if (recognizedWordCount > maxRecognizedWordCount) {
                maxRecognizedWordCount = recognizedWordCount;
            }
        }

        // remove words that are no longer used
        _wordBuffer.RemoveRange(0, maxRecognizedWordCount);


        for (int candidateIdx = 0; candidateIdx < candidateCount; candidateIdx++) {
            int recognizedIndex = _recognizedIndices[candidateIdx];
            if (recognizedIndex >= _candidates[candidateIdx].wordCount) {
                // handle fully recognized candidates
                _spawnItem(_candidates[candidateIdx].prefab);

                _recognizedIndices[candidateIdx] = 0;
            }
        }
    }

    private int _processCandidate(int candidateIdx, int recognizedIdx, out int recognizedWordCount)
    {
        var targetBuffer = _candidates[candidateIdx].words;
        int targetBufferLength = _candidates[candidateIdx].wordCount;
        int wordBufferLength = _wordBuffer.Count;

        int fromIdx = 0;
        int toIdx = recognizedIdx;

        // look for the next matching word in _targetBuffer
        while (fromIdx < wordBufferLength &&
                string.Compare(_wordBuffer[fromIdx], targetBuffer[toIdx]) != 0) {
            fromIdx++;
        }

        if (fromIdx < wordBufferLength) {
            // found the first matching character pair, start comparing remaining words
            // for more
            fromIdx++;
            toIdx++;
            while (fromIdx < wordBufferLength && toIdx < targetBufferLength &&
                    string.Compare(_wordBuffer[fromIdx], targetBuffer[toIdx]) == 0) {
                fromIdx++;
                toIdx++;
            }
        }

        recognizedWordCount = fromIdx;
        return toIdx;
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

//        targetTexts[_missionId].text = _builder.ToString();
    }

    private void generateRank(float missionTimer)
    {
        var thisMission = targetTextList[_textPtr];
        if (missionTimer > thisMission.fastTime)
        {
            //TODO: give a OK rank visual effect
//            performanceDisplay.text = "OK";
            if (SoundManager.instance) {
                SoundManager.instance.PlaySuccess ();
            }
        }
        else if (missionTimer < thisMission.fastTime)
        {
//            performanceDisplay.text = "Perfect";
            Debug.Log("Perfect");
            //TODO: give a FAST rank visual effect
            if (SoundManager.instance) {
                SoundManager.instance.PlaySuccess ();
                SoundManager.instance.PlayZoom ();
            }
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

    private void _debugPrintBuffer(string leadIn, string[] buffer)
    {
        _builder.Length = 0;
        _builder.Append(leadIn);
        _builder.Append(" ");

        for (int i = 0; i < buffer.Length; i++) {
            _builder.Append(buffer[i]);
            _builder.Append(", ");
        }

        Debug.Log(_builder.ToString());
    }
}