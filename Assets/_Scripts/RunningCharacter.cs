
using System.Collections;

using UnityEngine;
using UnityEngine.UI;


public sealed class RunningCharacter : MonoBehaviour
{
    public float initSpeed = 10f;
    public float slowRate = 1f;
    public float minSpeed = 5f;
    public float maxSpeed = 20f;

    public float maxDistance;
    public float timeLimit = 30f;

    public GameObject[] InGameUIGroup;
    public GameObject[] StartScreenUIGroup;
    public GameObject[] EndGameUIGroup;
	public GameObject winSplash;
    public ExampleStreaming recordingScript;

    public Text speedLabel;
    public Text distanceLabel;
    public Text remainingDistance;
    public Text remainTimerText;
    public Text ResultText;

    public Text countdownText;

    public Transform charTrans;
    public Transform UFOTrans;
    public ParticleSystem successParticle;

    public ScoreManager scoreManager;
    public Animator charAnim;
    public TrailCtrl trailScript;
    //==============================================================================

    private float _speed = 0f;
    private float _distance = 0f;
    private float _remainingDistance = 0f;
    private bool _gameEnded = false;
    private bool startGame = false;
    private bool starting = false;
    private float remainTime;
    private float timer = 0f;
    private float targetSpeed = 0;
    private Vector3 _origPos;
    private Vector3 _UFOOrigPos;

    /////////////////////////////////////////////////////////////////////////////////////

    public void boost(float amount)
    {
//        targetSpeed += amount;
        //targetSpeed = Mathf.Clamp(targetSpeed + amount, minSpeed, maxSpeed);
        _speed += amount;
    }


    public void successFX()
    {
        successParticle.Play();
    }

    /////////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        _origPos = charTrans.position;
        _UFOOrigPos = UFOTrans.position;
        hideDisplays();
        //_updateDisplay();
        toggleDisplays(InGameUIGroup, false);
        toggleDisplays(EndGameUIGroup, false);
        toggleDisplays(StartScreenUIGroup, true);
        targetSpeed = initSpeed;
    }

    private void Update()
    {
        if (startGame)
        {
            _speed = Mathf.Clamp(_speed - slowRate * Time.deltaTime, minSpeed, maxSpeed);
            //_speed = Mathf.Lerp(_speed, targetSpeed, Time.deltaTime * 3f);
            charAnim.SetFloat("runSpeed", _speed / 5f * 1.2f);
            charAnim.SetFloat("gear", _speed / 40f);

            _distance += _speed * Time.deltaTime;

            if (UFOTrans.position.x < maxDistance + 40f)
            {
                UFOTrans.Translate(_speed * Time.deltaTime, 0f, 0f);
            }
            if (UFOTrans.position.x > maxDistance + 40f)
            {
                UFOTrans.position = new Vector3(maxDistance + 40f, UFOTrans.position.y, UFOTrans.position.z);
            }

            charTrans.Translate(0f, 0f, _speed * Time.deltaTime);

            _remainingDistance = maxDistance - _distance;
            timer += Time.deltaTime;
            remainTime = timeLimit - timer;

			Debug.LogFormat("[LOG] distance: {0}", _remainingDistance);
            if (_remainingDistance <= 0f)
            {
                _remainingDistance = 0f;

                Win();
            }

            if (remainTime < 0f)
            {
                Fail();
            }
            if (!_gameEnded)
            {
                _updateDisplay();
            }
        }
        else if (starting)
        {
            UFOTrans.Translate(_speed * Time.deltaTime, 0f, 0f);
            charTrans.Translate(0f, 0f, _speed * Time.deltaTime);
        }
    }

    public float GetSpeed()
    {

        return _speed;
    }

    public void StartGame()
    {
        starting = true;
        StartCoroutine("GameStart");
        charAnim.Play("Default Run");
        reset();

        toggleDisplays(EndGameUIGroup, false);
        toggleDisplays(StartScreenUIGroup, false);
        toggleDisplays(InGameUIGroup, true);
        _updateDisplay();
        recordingScript.StartRecording();

    }

    IEnumerator GameStart()
    {
        countdownText.text = "3";
        yield return new WaitForSeconds(1f);
        countdownText.text = "2";
        yield return new WaitForSeconds(1f);
        countdownText.text = "1";
        yield return new WaitForSeconds(1f);
        countdownText.text = "GO";
        yield return new WaitForSeconds(1f);
        countdownText.text = "";
        recordingScript.StartGame();
        starting = false;
        startGame = true;
    }

    public void EndGame(string result)
    {
        startGame = false;
        recordingScript.StopGame();
        ResultText.text = result;

        toggleDisplays(InGameUIGroup, false);

        toggleDisplays(StartScreenUIGroup, false);
        toggleDisplays(EndGameUIGroup, true);
        recordingScript.StopRecording();
    }

    /////////////////////////////////////////////////////////////////////////////////////

    private void _updateDisplay()
    {
        speedLabel.text = string.Format("{0} m/s", System.Math.Round(_speed, 2).ToString());
        //distanceLabel.text = string.Format("{0} m", _distance.ToString());

        remainingDistance.text = string.Format("{0} m remaining", System.Math.Round(_remainingDistance, 2).ToString());
        remainTimerText.text = string.Format("{0} sec", System.Math.Round(remainTime, 2).ToString());
    }

    private void toggleDisplays(GameObject[] group, bool toggle)
    {
        foreach (GameObject obj in group)
        {
            obj.SetActive(toggle);
        }
    }


    public void hideDisplays()
    {
        speedLabel.text = "";
        distanceLabel.text = "";
        remainingDistance.text = "";
        ResultText.text = "";
        remainTimerText.text = "";
    }

    public void Fail()
    {
        EndGame("You Fail");
        _gameEnded = true;
        _speed = 0;
        charAnim.SetBool("exhausted", true);
    }

    public void Win()
    {
        EndGame("You Win");
        // Get Score
        _gameEnded = true;
        scoreManager.FinalScore(timer);

		winSplash.SetActive(true);
		// ScoreManager.Instance.FinalScore(GetTimer());
    }

    public void RestartGame()
    {
        hideDisplays();
		winSplash.SetActive(false);

        StartGame();
    }

    public float GetTimer()
    {
        return timer;
    }

    private void reset()
    {
        charAnim.SetBool("exhausted", false);
        timer = 0f;
        charAnim.SetFloat("gear", 0f);
        charAnim.SetFloat("runSpeed", 1f);
        _speed = initSpeed;
        _distance = 0f;
        _remainingDistance = maxDistance;
        targetSpeed = 0f;
        remainTime = timeLimit;
        charTrans.position = _origPos;
        UFOTrans.position = _UFOOrigPos;
        trailScript.Reset();
        _gameEnded = false;
    }
}
