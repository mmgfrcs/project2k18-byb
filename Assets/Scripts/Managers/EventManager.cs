using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EventRunMode
{
    Random, Bankruptcy, Defeat, Victory
}

public class EventManager : MonoBehaviour {
    public EventPanel panel;
    public BankruptEventPanel bankruptPanel;
    public VictoryDefeatEventPanel victoryDefeatPanel;
    public UnityEngine.UI.Text objectiveText, objectiveTitle;

    [Header("Clear Event")]
    public string clearEventTitle;
    [TextArea(10, 15)]
    public string clearEventDescription;

    [Header("Predefined Events")]
    public GameEventSystems.Event startEvent;
    public GameEventSystems.Event bankruptcyEvent;
    public GameEventSystems.Event defeatEvent;
    public GameEventSystems.Event victoryEvent;

    static EventManager instance;
    List<GameEventSystems.Event> events;
    GameEventSystems.Event eventToRun;

    internal bool clearEvent = false;

	// Use this for initialization
	void Start () {
        instance = this;
        
        events = new List<GameEventSystems.Event>();
        foreach(GameEventSystems.Event e in GetComponents<GameEventSystems.Event>())
        {
            events.Add(e);
        }
        Debug.Log("EventManager - Loaded Event: " + events.Count);
        GameManager.OnGameEnd += GameManager_OnGameEnd;
	}

    private void GameManager_OnGameEnd()
    {
        instance = null;
        GameManager.OnGameEnd -= GameManager_OnGameEnd;
    }

    // Update is called once per frame
    void Update () {
		
	}

    internal static void RunStartEvent()
    {
        instance.objectiveTitle.text = instance.startEvent.eventName;
        instance.objectiveText.text = instance.startEvent.eventDescription;
        instance.panel.eventTitle.text = instance.startEvent.eventName;
        instance.panel.eventDesc.text = instance.startEvent.eventDescription;
        instance.startEvent.Run();
        instance.panel.gameObject.SetActive(true);
    }

    internal static void RunEvent(EventRunMode runMode = EventRunMode.Random)
    {
        if (runMode == EventRunMode.Random)
        {
            print("EventManager: Requested Random Event");
            if (instance.eventToRun == null && !instance.clearEvent)
            {
                print("Rolling event...");
                RollEvent();
                RunEvent();
            }
            else if (instance.clearEvent)
            {
                //Nothing happening
                print("Event run: Clear");
                instance.panel.eventTitle.text = instance.clearEventTitle;
                instance.panel.eventDesc.text = instance.clearEventDescription;
                instance.panel.gameObject.SetActive(true);
            }
            else if (instance.eventToRun != null)
            {
                //Run that event
                print("Event run: " + instance.eventToRun.eventName);
                instance.panel.eventTitle.text = instance.eventToRun.eventName;
                instance.panel.eventDesc.text = instance.eventToRun.eventDescription;
                instance.eventToRun.Run();
                instance.panel.gameObject.SetActive(true);
            }
            else Debug.LogWarning("EventManager report bug - Clear:" + instance.clearEvent);
        }
        else
        {
            print("EventManager: Requested not Random Event");
            EndEvent();
            if (runMode == EventRunMode.Bankruptcy)
            {
                instance.bankruptPanel.eventTitle.text = instance.bankruptcyEvent.eventName;
                instance.bankruptPanel.eventDesc.text = instance.bankruptcyEvent.eventDescription;
                instance.bankruptcyEvent.Run();
                instance.bankruptPanel.gameObject.SetActive(true);
            }
            else {

                //Calculate scores
                float score = 0, victoryScore = 0;

                instance.victoryDefeatPanel.daysValue.text = GameManager.Days.ToString("n0");

                //Scores from Days
                float dayScore = GameManager.Days * 100;
                instance.victoryDefeatPanel.daysScore.text = dayScore.ToString("n0");
                score += dayScore;

                //TODO Scores from Net Worth

                //Scores from Victory, depends on whether the player loses or wins
                instance.victoryDefeatPanel.victoryValue.text = "x2";
                if (runMode == EventRunMode.Defeat)
                {
                    instance.victoryDefeatPanel.eventTitle.text = instance.defeatEvent.eventName;
                    instance.victoryDefeatPanel.eventDesc.text = instance.defeatEvent.eventDescription;
                    instance.victoryDefeatPanel.victoryLine.SetActive(false);
                    instance.defeatEvent.Run();
                }
                if (runMode == EventRunMode.Victory)
                {
                    instance.victoryDefeatPanel.eventTitle.text = instance.victoryEvent.eventName;
                    instance.victoryDefeatPanel.eventDesc.text = instance.victoryEvent.eventDescription;
                    instance.victoryDefeatPanel.victoryLine.SetActive(true);
                    instance.victoryEvent.Run();
                    victoryScore = score * 2;
                }
                instance.victoryDefeatPanel.victoryScore.text = victoryScore.ToString("n0");
                score += victoryScore;

                //Display total score
                instance.victoryDefeatPanel.totalScore.text = score.ToString("n0");

                instance.victoryDefeatPanel.gameObject.SetActive(true);
            }
        }
    }

    internal static void EndEvent()
    {
        //Revert event changes
        if (instance.eventToRun != null && !instance.eventToRun.everlasting)
        {
            instance.eventToRun.EndRun();
            /*if (instance.eventToRun.toEnd)*/ instance.eventToRun = null;
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
            print("EventManager: Rolling " + ev.eventName + ", Rollable:" + ev.rollable);
            if (!ev.rollable) continue;
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
