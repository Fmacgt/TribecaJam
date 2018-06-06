using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailCtrl : MonoBehaviour
{

    public GameObject[] trailObjs;
    public RunningCharacter runScript;
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < trailObjs.Length; i++)
        {
            trailObjs[i].SetActive(false);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        float speed = runScript.GetSpeed();
        if (speed > 10f)
        {

            int maxTrail = Mathf.RoundToInt(trailObjs.Length * speed / 170f);
            for (int i = 0; i < trailObjs.Length; i++)
            {
                if (i <= maxTrail)
                {
                    trailObjs[i].SetActive(true);
                }else
                {
                    trailObjs[i].SetActive(false);
                }
            }
        }
    }

    public void Reset()
    {
        for (int i = 0; i < trailObjs.Length; i++)
        {
            trailObjs[i].SetActive(false);
        }
    }
}
