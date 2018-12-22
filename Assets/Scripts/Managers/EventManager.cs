using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {
    public EventPanel panel;

    [Header("Clear Event")]
    public string clearEventTitle;
    [TextArea(10, 15)]
    public string clearEventDescription;

    [Header("Start Event")]
    public GameEventSystems.Event startEvent;

    static EventManager instance;
    List<GameEventSystems.Event> events;
    GameEventSystems.Event eventToRun;

    internal bool clearEvent = false;

	// Use this for initialization
	void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
        events = new List<GameEventSystems.Event>();
        foreach(GameEventSystems.Event e in GetComponents<GameEventSystems.Event>())
        {
            events.Add(e);
        }
        Debug.Log("EventManager - Loaded Event: " + events.Count);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal static void RunStartEvent()
    {
        instance.panel.eventTitle.text = instance.startEvent.eventName;
        instance.panel.eventDesc.text = instance.startEvent.eventDescription;
        instance.startEvent.Run();
        instance.panel.gameObject.SetActive(true);
    }

    internal static void RunEvent()
    {
        if (instance.eventToRun == null && !instance.clearEvent)
        {
            RollEvent();
            RunEvent();
        }
        else if (instance.clearEvent)
        {
            //Nothing happening
            instance.panel.eventTitle.text = instance.clearEventTitle;
            instance.panel.eventDesc.text = instance.clearEventDescription;
            instance.panel.gameObject.SetActive(true);
        }
        else if (instance.eventToRun != null)
        {
            //Run that event
            instance.panel.eventTitle.text = instance.eventToRun.eventName;
            instance.panel.eventDesc.text = instance.eventToRun.eventDescription;
            instance.eventToRun.Run();
            instance.panel.gameObject.SetActive(true);
        }
    }

    internal static void EndEvent()
    {
        //Revert event changes
        if (instance.eventToRun != null)
        {
            instance.eventToRun.EndRun();
            if (instance.eventToRun.toEnd) instance.eventToRun = null;
        }
    }

    internal static GameEventSystems.Event RollEvent()
    {
        foreach (var ev in instance.events)
        {
            if (ev.guaranteed && ev.guaranteeOnDay == GameManager.Days)
            {
                ev.guaranteed = false;
                instance.eventToRun = ev;
                instance.clearEvent = false;
                Debug.Log("EventManager - Guaranteed Event " + ev.eventName + " on Day " + GameManager.Days);
                return ev;
            }
        }

        foreach (var ev in instance.events)
        {
            if (ev == instance.startEvent) continue;
            if (MathRand.WeightedPick(new float[] { ev.chance, 1 - ev.chance }) == 0)
            {
                instance.eventToRun = ev;
                instance.clearEvent = false;
                Debug.Log("EventManager - Rolled " + ev.eventName);
                return ev;
            }
        }
        instance.clearEvent = true;
        Debug.Log("EventManager - Rolled Clear event");
        return null;
    }
}
