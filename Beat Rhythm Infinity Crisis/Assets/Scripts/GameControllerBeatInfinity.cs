﻿//----------------------------------------------
//            	   Koreographer                 
//    Copyright © 2014-2017 Sonic Bloom, LLC    
//----------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using SonicBloom.Koreo.Demos;
using SonicBloom.Koreo;
using System;

public class GameControllerBeatInfinity : MonoBehaviour
	{
		#region Fields

		[Tooltip("The Event ID of the track to use for target generation.")]
		[EventID]
		public string eventID;

		[Tooltip("The number of milliseconds (both early and late) within which input will be detected as a Hit.")]
		[Range(8f, 150f)]
		public float hitWindowRangeInMS = 80;

		[Tooltip("The number of units traversed per second by Note Objects.")]
		public float noteSpeed = 1f;

		[Tooltip("The archetype (blueprints) to use for generating notes.  Can be a prefab.")]
		public NoteObjectBeatInfinity noteObjectArchetype;

		[Tooltip("The list of Lane Controller objects that represent a lane for an event to travel down.")]
		public List<LaneControllerBeatInfinity> noteLanes = new List<LaneControllerBeatInfinity>();

		[Tooltip("The amount of time in seconds to provide before playback of the audio begins.  Changes to this value are not immediately handled during the lead-in phase while playing in the Editor.")]
		public float leadInTime;

		[Tooltip("The Audio Source through which the Koreographed audio will be played.  Be sure to disable 'Auto Play On Awake' in the Music Player.")]
		public AudioSource audioCom;

        [Tooltip("The list of types of 'hit' a player can achieve on a note, from best at the top to worst at the bottom.")]
        public string[] hitQualityTypes;

		// The amount of leadInTime left before the audio is audible.
		float leadInTimeLeft;

		// The amount of time left before we should play the audio (handles Event Delay).
		float timeLeftToPlay;

		// Local cache of the Koreography loaded into the Koreographer component.
		Koreography playingKoreo;

		// Koreographer works in samples.  Convert the user-facing values into sample-time.  This will simplify
		//  calculations throughout.
		int hitWindowRangeInSamples;	// The sample range within which a viable event may be hit.
		
		// The pool for containing note objects to reduce unnecessary Instantiation/Destruction.
		Stack<NoteObjectBeatInfinity> noteObjectPool = new Stack<NoteObjectBeatInfinity>();

        protected Animator animationTarget;

        public Text pointsUIText;
        protected int points = 0;
        protected int Points { get { return points; } private set { points = value; pointsUIText.text = points.ToString(); } }

    #endregion
    #region Properties

    // Public access to the hit window.
    public int HitWindowSampleWidth
		{
			get
			{
				return hitWindowRangeInSamples;
			}
		}

		// Access to the current hit window size in Unity units.
		public float WindowSizeInUnits
		{
			get
			{
				return noteSpeed * (hitWindowRangeInMS * hitQualityTypes.Length * 0.001f);
			}
		}

		// The Sample Rate specified by the Koreography.
		public int SampleRate
		{
			get
			{
				return playingKoreo.SampleRate;
			}
		}

		// The current sample time, including any necessary delays.
		public int DelayedSampleTime
		{
			get
			{
				// Offset the time reported by Koreographer by a possible leadInTime amount.
				return playingKoreo.GetLatestSampleTime() - (int)(audioCom.pitch * leadInTimeLeft * SampleRate);
			}
		}

		#endregion
		#region Methods

		void Start()
    {

        //Find the animator for the dancing man.
        this.animationTarget = GameObject.FindObjectOfType<Animator>();

        InitializeLeadIn();

        // Initialize all the Lanes.
        for (int i = 0; i < noteLanes.Count; ++i)
        {
            noteLanes[i].Initialize(this);
        }

        // Initialize events.
        playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0);
        // Grab all the events out of the Koreography.
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID);
        List<KoreographyEvent> rawEvents = rhythmTrack.GetAllEvents();
        InitializeMetronomeIfPresent(rawEvents);
        for (int i = 0; i < rawEvents.Count; ++i)
        {
            KoreographyEvent evt = rawEvents[i];
            string payload = evt.GetTextValue();

            // Find the right lane.
            for (int j = 0; j < noteLanes.Count; ++j)
            {
                LaneControllerBeatInfinity lane = noteLanes[j];
                if (lane.DoesMatchPayload(payload))
                {
                    // Add the object for input tracking.
                    lane.AddEventToLane(evt);

                    // Break out of the lane searching loop.
                    break;
                }
            }
        }
    }

    private static void InitializeMetronomeIfPresent(List<KoreographyEvent> rawEvents)
    {
        MetronomeController metronome = GameObject.FindObjectOfType<MetronomeController>();
        if (metronome != null) {
            metronome.allEvents = rawEvents;
        }
    }

    public void ReportHitIndex(int hitQualityIndex)
    {
        Debug.Log(this.hitQualityTypes[hitQualityIndex]);
        int danceNumber = this.hitQualityTypes.Length - hitQualityIndex;
        Points += danceNumber;
        this.animationTarget.SetInteger("Dance", danceNumber);
    }

    public void ReportMiss() {
        Debug.Log("Missed!");
        this.animationTarget.SetInteger("Dance", 0);    //should probably do this on the last animation finishing, instead
    }

    // Sets up the lead-in-time.  Begins audio playback immediately if the specified lead-in-time is zero.
    void InitializeLeadIn()
		{
			// Initialize the lead-in-time only if one is specified.
			if (leadInTime > 0f)
			{
				// Set us up to delay the beginning of playback.
				leadInTimeLeft = leadInTime;
				timeLeftToPlay = leadInTime - Koreographer.Instance.EventDelayInSeconds;
			}
			else
			{
				// Play immediately and handle offsetting into the song.  Negative zero is the same as
				//  zero so this is not an issue.
				audioCom.time = -leadInTime;
				audioCom.Play();
			}
		}

		void Update()
		{
			// This should be done in Start().  We do it here to allow for testing with Inspector modifications.
			UpdateInternalValues();

			// Count down some of our lead-in-time.
			if (leadInTimeLeft > 0f)
			{
				leadInTimeLeft = Mathf.Max(leadInTimeLeft - Time.unscaledDeltaTime, 0f);
			}

			// Count down the time left to play, if necessary.
			if (timeLeftToPlay > 0f)
			{
				timeLeftToPlay -= Time.unscaledDeltaTime;

				// Check if it is time to begin playback.
				if (timeLeftToPlay <= 0f)
				{
					audioCom.time = -timeLeftToPlay;
					audioCom.Play();

					timeLeftToPlay = 0f;
				}
			}
		}

		// Update any internal values that depend on externally accessible fields (public or Inspector-driven).
		void UpdateInternalValues()
		{
			hitWindowRangeInSamples = (int)(0.001f * hitWindowRangeInMS * SampleRate);
		}

		// Retrieves a frehsly activated Note Object from the pool.
		public NoteObjectBeatInfinity GetFreshNoteObject()
		{
			NoteObjectBeatInfinity retObj;

			if (noteObjectPool.Count > 0)
			{
				retObj = noteObjectPool.Pop();
			}
			else
			{
				retObj = GameObject.Instantiate<NoteObjectBeatInfinity>(noteObjectArchetype);
			}
			
			retObj.gameObject.SetActive(true);
			retObj.enabled = true;

			return retObj;
		}

		// Deactivates and returns a Note Object to the pool.
		public void ReturnNoteObjectToPool(NoteObjectBeatInfinity obj)
		{
			if (obj != null)
			{
				obj.enabled = false;
				obj.gameObject.SetActive(false);

				noteObjectPool.Push(obj);
			}
		}

		// Restarts the game, causing all Lanes and any active Note Objects to reset or otherwise clear.
		public void Restart()
		{
			// Reset the audio.
			audioCom.Stop();
			audioCom.time = 0f;

			// Flush the queue of delayed event updates.  This effectively resets the Koreography and ensures that
			//  delayed events that haven't been sent yet do not continue to be sent.
			Koreographer.Instance.FlushDelayQueue(playingKoreo);

			// Reset the Koreography time.  This is usually handled by loading the Koreography.  As we're simply
			//  restarting, we need to handle this ourselves.
			playingKoreo.ResetTimings();

			// Reset all the lanes so that tracking starts over.
			for (int i = 0; i < noteLanes.Count; ++i)
			{
				noteLanes[i].Restart();
			}

			// Reinitialize the lead-in-timing.
			InitializeLeadIn();
		}

		#endregion
	}
