
using System.Collections;
using UnityEngine;


namespace Tribeca
{
	[CreateAssetMenu(menuName="Scriptable Objects/Custom Word Definition")]
	public sealed class CustomWordDef : ScriptableObject
	{
		public string word;
		public string[] soundLikes;
	}
}
