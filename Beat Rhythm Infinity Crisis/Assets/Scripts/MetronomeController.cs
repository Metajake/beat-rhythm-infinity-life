using SonicBloom.Koreo;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MetronomeController : MonoBehaviour {

    public List<KoreographyEvent> allEvents;
    private SonicBloom.Koreo.Demos.RhythmGameController gameController;

    public Color extentNormalColor;
    public Color extentLitUpColor;
    public int extentLitUpFrameCount = 5;

    private bool isLeftActive { get { return (elapsedBeats() % 2 == 0); } }      //this is the state of the metronome - if true, the current destination of the needle is the left extent, otherwise right.
    private GameObject leftExtent;
    private GameObject rightExtent;
    private GameObject needle;
    double progress;

    // Use this for initialization
    void Start() {
        leftExtent = GameObject.Find("MetronomeLeftExtent");
        rightExtent = GameObject.Find("MetronomeRightExtent");
        needle = GameObject.Find("MetronomeNeedle");
        //should get allEvents populated at RhythmGameController's Start
        gameController = GameObject.FindObjectOfType<SonicBloom.Koreo.Demos.RhythmGameController>();
        
    }

    // Update is called once per frame
    void Update() {
        
        MoveNeedle();
    }

    private void MoveNeedle()
    {
            MoveNeedleOnBeat();
    }

    double calculateBeatsPerSecond() {
        try {
            return Koreographer.Instance.GetKoreographyAtIndex(0).GetBPM(Math.Max(0,gameController.DelayedSampleTime)) / 60d;
        } catch (Exception e) {
            Debug.Log("Could not get BPM");
            return -1d;
        }
    }

    double elapsedSeconds() {
       return Math.Max(0d, (double)gameController.DelayedSampleTime / (double)gameController.SampleRate);
    }

    private void MoveNeedleOnBeat() {
        double bps = calculateBeatsPerSecond();
        double secondsPerBeat = 1d / bps;
        progress = (elapsedSeconds() % secondsPerBeat) / secondsPerBeat;
        //needle.transform.position = Vector3.Lerp((isLeftActive ? rightExtent : leftExtent).transform.position, getCurrentTarget().transform.position, (float) progress);
        float newX = isLeftActive ? rightExtent.transform.position.x - 6f * (float)progress : leftExtent.transform.position.x + 6f * (float)progress;
        needle.transform.position = new Vector3(newX, needle.transform.position.y, needle.transform.position.z);

    }

    GameObject getCurrentTarget() {
        if (isLeftActive) {
            return leftExtent;
        }
        return rightExtent;
    }

    MetronomeNeedle getNeedle() {
        return this.needle.GetComponent<MetronomeNeedle>();
    }

    int elapsedBeats() {
        return (int)(calculateBeatsPerSecond() * elapsedSeconds());
    }
}
