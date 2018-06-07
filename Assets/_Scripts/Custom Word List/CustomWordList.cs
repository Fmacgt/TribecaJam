
using System.Collections;
using UnityEngine;


namespace Tribeca
{
	[CreateAssetMenu(menuName = "Scriptable Objects/Custom Word List")]
	public sealed class CustomWordList : ScriptableObject
	{
		public CustomWordDef[] wordList;
	}
}
