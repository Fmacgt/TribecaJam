
using System.Collections;
using UnityEngine;


namespace TribecaJam
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Target Text List")]
    public sealed class TargetTextList : ScriptableObject
    {
        public Mission[] list;

        /////////////////////////////////////////////////////////////////////////////////////

        public Mission this[int idx]
        {
            get
            {
                return list[idx];
            }
        }

        public int Count
        {
            get { return list.Length; }
        }
    }
}
