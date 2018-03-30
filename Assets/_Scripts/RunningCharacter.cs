
using System.Collections;

using UnityEngine;
using UnityEngine.UI;


public sealed class RunningCharacter : MonoBehaviour
{
	public float initSpeed = 10f;
	public float slowRate = 1f;
	public float minSpeed = 5f;
	public float maxSpeed = 20f;

	public Text speedLabel;
	public Text distanceLabel;

	//==============================================================================

	private float _speed = 0f;
	private float _distance = 0f;

	/////////////////////////////////////////////////////////////////////////////////////

	public void boost(float amount)
	{
		_speed = Mathf.Clamp(_speed + amount, minSpeed, maxSpeed);
	}

	/////////////////////////////////////////////////////////////////////////////////////

	private void Start()
	{
		_speed = initSpeed;
		_distance = 0f;

		_updateDisplay();
	}

	private void Update()
	{
		_speed = Mathf.Clamp(_speed - slowRate * Time.deltaTime, minSpeed, maxSpeed);
		_distance += _speed * Time.deltaTime;

		_updateDisplay();
	}

	/////////////////////////////////////////////////////////////////////////////////////

	private void _updateDisplay()
	{
		speedLabel.text = _speed.ToString();
		distanceLabel.text = _distance.ToString();
	}
}
