
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace Tribeca
{
	public sealed class StartSequence : MonoBehaviour
	{
		public ExampleStreaming control;


		public Transform ufoTr;
		public Transform camTr;

		public Transform initUfoTr;
		public Transform introUfoTr;
		public Transform runningUfoTr;

		public Transform initCamTr;
		public Transform lookUpCamTr;
		public Transform runningCamTr;

		public AnimationCurve panCurve;


		public Image fadingMask;

		//==============================================================================

		private Color _fadeFromColor;
		private Color _fadeToColor;

		private Transform _rotateFromTr;
		private Transform _rotateToTr;

		private Vector3 _moveFromPos;
		private Vector3 _moveToPos;

		/////////////////////////////////////////////////////////////////////////////////////

		public void play()
		{
			StopAllCoroutines();
			StartCoroutine(_startSequence());
		}

		/////////////////////////////////////////////////////////////////////////////////////

		private IEnumerator _startSequence()
		{
			/**
			fadingMask.gameObject.SetActive(true);
			fadingMask.color = Color.clear;
			_fadeFromColor = Color.clear;
			_fadeToColor = Color.black;
			LeanTween.value(gameObject, 0f, 1f, 0.5f).setOnUpdate(_fadeMask);

			yield return new WaitForSeconds(1.0f);

			ufoTr.position = initUfoTr.position;
			camTr.rotation = initCamTr.rotation;
			camTr.position = initCamTr.position;

			_fadeFromColor = Color.black;
			_fadeToColor = Color.clear;
			fadingMask.color = Color.black;
			LeanTween.value(gameObject, 0f, 1f, 0.5f).setOnUpdate(_fadeMask);

			yield return new WaitForSeconds(0.5f);
			fadingMask.gameObject.SetActive(false);
			**/

			LeanTween.move(ufoTr.gameObject, introUfoTr.position, 1f);

			/**
			_rotateFromTr = initCamTr;
			_rotateToTr = lookUpCamTr;
			LeanTween.value(gameObject, 0f, 1f, 1.5f).setOnUpdate(_rotateCamera);
			**/
			yield return new WaitForSeconds(1.5f);

			// move down back
			_rotateFromTr = lookUpCamTr;
			_rotateToTr = runningCamTr;
			LeanTween.value(gameObject, 0f, 1f, 1f).setOnUpdate(_rotateCamera);

			_moveFromPos = camTr.position;
			_moveToPos = runningCamTr.position;
			LeanTween.value(gameObject, 0f, 1f, 1f).setOnUpdate(_moveCamera).setEase(panCurve);

			LeanTween.move(ufoTr.gameObject, runningUfoTr.position, 0.8f).setEase(panCurve);


			yield return new WaitForSeconds(1f);
			control.StartGame();
		}


		private void Awake()
		{
			camTr.rotation = lookUpCamTr.rotation;
			camTr.position = lookUpCamTr.position;

			ufoTr.position = initUfoTr.position;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.P)) {
				play();
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////

		private void _fadeMask(float t)
		{
			fadingMask.color = Color.Lerp(_fadeFromColor, _fadeToColor, t);
		}

		private void _rotateCamera(float t)
		{
			camTr.rotation = Quaternion.Slerp(_rotateFromTr.rotation, _rotateToTr.rotation, t);
		}

		private void _moveCamera(float t)
		{
			camTr.position = Vector3.Lerp(_moveFromPos, _moveToPos, t);
		}
	}
}
