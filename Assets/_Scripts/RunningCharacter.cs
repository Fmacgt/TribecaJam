
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


    public Text speedLabel;
    public Text distanceLabel;
    public Text remainingDistance;
    public Text remainTimerText;
    public Text ResultText;
    public Transform charTrans;
    public ParticleSystem successParticle;

    //==============================================================================

    private float _speed = 0f;
    private float _distance = 0f;
    private float _remainingDistance = 0f;
    private bool startGame = false;
    private float remainTime;
    private float timer = 0f;
    private float targetSpeed = 0;

    /////////////////////////////////////////////////////////////////////////////////////

    public void boost(float amount)
    {
        targetSpeed += amount;
        //_speed = Mathf.Clamp(_speed + amount, minSpeed, maxSpeed);
    }


    public void successFX()
    {
        successParticle.Play();
    }
    /////////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
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
            _speed = Mathf.Lerp(_speed, targetSpeed, Time.deltaTime * 3f);
            _distance += _speed * Time.deltaTime;
            charTrans.Translate(0f, 0f, _speed * Time.deltaTime);
            _remainingDistance = maxDistance - _distance;
            timer += Time.deltaTime;
            remainTime = timeLimit - timer;

            if (_remainingDistance <= 0f)
            {
                _remainingDistance = 0f;
                Win();
            }

            if (remainTime < 0f)
            {
                Fail();
            }
            _updateDisplay();

        }
    }

    public void StartGame()
    {
        startGame = true;
        reset();

        toggleDisplays(EndGameUIGroup, false);
        toggleDisplays(StartScreenUIGroup, false);
        toggleDisplays(InGameUIGroup, true);
        _updateDisplay();


    }

    public void EndGame(string result)
    {
        startGame = false;

        ResultText.text = result;

        toggleDisplays(InGameUIGroup, false);

        toggleDisplays(StartScreenUIGroup, false);
        toggleDisplays(EndGameUIGroup, true);
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

    }

    public void Win()
    {
        EndGame("You Win");

    }

    public void RestartGame()
    {
        hideDisplays();

        StartGame();
    }

    private void reset()
    {
        timer = 0f;
        _speed = initSpeed;
        _distance = 0f;
        _remainingDistance = maxDistance;
        targetSpeed = 0f;
        remainTime = timeLimit;
    }
}
