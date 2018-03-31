using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TribecaJam
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Mission")]
    public class Mission : ScriptableObject
    {

        public string text;
        public float fastTime;
        public float badTime;

        public string animationTrigger;


    }
}