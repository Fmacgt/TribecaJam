
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;
using IBM.Watson.DeveloperCloud.Connection;


namespace Tribeca
{
	public sealed class AddCustomWords : MonoBehaviour
	{
		public string modelId;
		public string username;
		public string password;

		public CustomWordDef[] wordList;

		//==============================================================================

		private string _serviceUrl = "https://stream.watsonplatform.net/speech-to-text/api";

		/////////////////////////////////////////////////////////////////////////////////////

		public void tryAddCustomWords()
		{
//			StopAllCoroutines();
//			StartCoroutine(_tryAddCustomWords());
			if (modelId.Length == 0 || username.Length == 0 || password.Length == 0 ||
					wordList.Length == 0) {
				return;
			} 

			_tryAddCustomWords();
		}

		/////////////////////////////////////////////////////////////////////////////////////

		private void _tryAddCustomWords()
		{
			// TODO: sign in with credentials
			var credentials = new Credentials(username, password, _serviceUrl);
			var speechToText = new SpeechToText(credentials);

			// TODO: create a Words based on the word list
			var wordData = new Words();
			int wordCount = wordList.Length;
			wordData.words = new Word[wordCount];

			int ptr = 0;
			foreach (var word in wordList) {
				var newWord = new Word();

				newWord.word = word.word;
				newWord.sounds_like = (string[])word.soundLikes.Clone();

				wordData.words[ptr] = newWord;
			}

			speechToText.AddCustomWords(_onAddWordSuccess, _onFailedToAddWord, 
					modelId, wordData);
		}

		/////////////////////////////////////////////////////////////////////////////////////

		private void _onAddWordSuccess(bool status, Dictionary<string, object> data)
		{
			Debug.LogFormat("[LOG] Successfully added a word? {0}", status);
		}

		private void _onFailedToAddWord(RESTConnector.Error error, Dictionary<string, object> data)
		{
		}
	}
}
