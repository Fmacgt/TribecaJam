using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreadmillCtrl : MonoBehaviour {
    public float speed;
    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
