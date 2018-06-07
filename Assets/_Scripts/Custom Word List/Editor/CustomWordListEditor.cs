
using System.Collections;
using System.IO;

using UnityEngine;
using UnityEditor;


namespace Tribeca
{
	[System.Serializable]
	public sealed class WordListExport
	{
		public WordExport[] words;
	}

	[System.Serializable]
	public sealed class WordExport
	{
		public string word;
		public string[] sounds_like;
//		public string display_as;
	}

	/////////////////////////////////////////////////////////////////////////////////////

	[CustomEditor(typeof(CustomWordList))]
	public sealed class CustomWordListEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("Export")) {
				_exportJson();
			}
		}

		/////////////////////////////////////////////////////////////////////////////////////

		private void _exportJson()
		{
			var wordList = target as CustomWordList;

			var exportData = new WordListExport();
			int wordCount = wordList.wordList.Length;
			exportData.words = new WordExport[wordCount];
			for (int i = 0; i < wordCount; i++) {
				var newData = new WordExport();

				newData.word = wordList.wordList[i].word;
				newData.sounds_like = (string[])wordList.wordList[i].soundLikes.Clone();

				exportData.words[i] = newData;
			}

			string json = JsonUtility.ToJson(exportData, true);
			var savePath = string.Format("{0}/../temp_files/{1}.json", Application.dataPath, target.name);
			File.WriteAllText(savePath, json);
		}
	}
}
