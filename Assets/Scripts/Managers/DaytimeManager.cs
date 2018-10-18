using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaytimeManager : MonoBehaviour {

    public int startHour = 17, endHour = 17;
    public float timeSpeed = 60;
    public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe(6, 0), new Keyframe(12, 1), new Keyframe(18, 0));
    public Transform lightTransform;

    public static int TimeHour { get { return instance.time.Hour; } }
    public static float TimeMinute { get { return instance.time.Minute; } }

    static DaytimeManager instance;
    bool paused = false;

    public static event System.Action OnDayEnd;

    System.DateTime time;
	// Use this for initialization
	void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
        time = new System.DateTime(2017, 12, 31, startHour, 0, 0, System.DateTimeKind.Utc);
        lightTransform.rotation = Quaternion.Euler((startHour - 6) * 15f, lightTransform.rotation.y, lightTransform.rotation.z);
	}
	
	// Update is called once per frame
	void Update () {
        if (!paused)
        {
            time = time.AddSeconds(Time.deltaTime * timeSpeed);
            RotateSun();
            if (time.Hour >= endHour && OnDayEnd != null) OnDayEnd();
        }
    }

    void RotateSun()
    {
        //Hour 6 is 0, hour 18 is 180
        //RenderSettings.ambientIntensity = intensityCurve.Evaluate(time.Hour + (time.Minute / 60f) + (time.Second * 3600));
        //RenderSettings.reflectionIntensity = intensityCurve.Evaluate(time.Hour + (time.Minute / 60f) + (time.Second * 3600));
        //RenderSettings.ba
        lightTransform.Rotate(Vector3.right * 15f * Time.deltaTime * timeSpeed / 3600);
    }

    public static void PauseDaytime()
    {
        instance.paused = true;
    }

    public static void UnpauseDaytime()
    {
        instance.paused = false;
    }

    public static void AdvanceTimeTo(int h)
    {
        var targetDate = new System.DateTime(instance.time.Year, instance.time.Month, instance.time.Day, h, 0, 0);
        //while (targetDate < instance.time) targetDate.AddDays(1);
        targetDate.AddDays(1);

        instance.time = targetDate;
        instance.lightTransform.rotation = Quaternion.Euler((h - 6) * 15f, instance.lightTransform.rotation.y, instance.lightTransform.rotation.z);
    }
}
