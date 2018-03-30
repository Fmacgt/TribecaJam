
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

    //==============================================================================

    private float _speed = 0f;
    private float _distance = 0f;
    private float _remainingDistance = 0f;
    private bool startGame = false;
    private float remainTime;
    private float timer = 0f;

    /////////////////////////////////////////////////////////////////////////////////////

    public void boost(float amount)
    {
        _speed = Mathf.Clamp(_speed + amount, minSpeed, maxSpeed);
    }

    /////////////////////////////////////////////////////////////////////////////////////

    private void Start()
    {
        hideDisplays();
        //_updateDisplay();
        toggleDisplays(InGameUIGroup, false);
        toggleDisplays(EndGameUIGroup, false);
        toggleDisplays(StartScreenUIGroup, true);
    }

    private void Update()
    {
        if (startGame)
        {
            _speed = Mathf.Clamp(_speed - slowRate * Time.deltaTime, minSpeed, maxSpeed);
            _distance += _speed * Time.deltaTime;
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
        speedLabel.text = _speed.ToString();
        distanceLabel.text = _distance.ToString();

        remainingDistance.text = _remainingDistance.ToString();
        remainTimerText.text = remainTime.ToString();
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
        reset();
    }

    public void Win()
    {
        EndGame("You Win");
        reset();
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

        remainTime = timeLimit;
    }
}
