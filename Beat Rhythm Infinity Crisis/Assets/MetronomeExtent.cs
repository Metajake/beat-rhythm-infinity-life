using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetronomeExtent : MonoBehaviour {
    private MetronomeController controller;
    public int lightupTimeRemaining;

	// Use this for initialization
	void Start () {
        this.controller = GetComponentInParent<MetronomeController>();
        lightupTimeRemaining = -1;
	}
	
	// Update is called once per frame
	void Update () {
        HandleLightupMaintenance();
	}

    public void LightUp() {
        this.lightupTimeRemaining = controller.extentLitUpFrameCount;
        GetComponent<SpriteRenderer>().color = this.controller.extentLitUpColor;
        //Rotate();
    }

    void Rotate() {
        this.transform.Rotate(Vector3.forward, 45f);
    }

    void HandleLightupMaintenance()
    {
        if (lightupTimeRemaining == -1) {
            return;
        }
        
        if (lightupTimeRemaining <= 0)
        {
            lightupTimeRemaining = -1;
            GetComponent<SpriteRenderer>().color = this.controller.extentNormalColor;
        }
        else
        {
            lightupTimeRemaining--;
        }

    }
}
