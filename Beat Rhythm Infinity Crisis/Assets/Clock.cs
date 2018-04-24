using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour {

    public Text clockLabel;

	// Use this for initialization
	void Start () {
        if (clockLabel == null) {
            clockLabel = GameObject.Find("Clock Label").GetComponent<Text>();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if(clockLabel == null) return;
        int minutes = calculateMinutes(Time.time);
        int seconds = calculateSeconds(Time.time);
        clockLabel.text = minutes + ":" + seconds;

    }

    private int calculateMinutes(float time)
    {
        return (int)(time / 60f);
    }

    private int calculateSeconds(float time)
    {
        return (int)(time % 60f);
    }
}
