﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameEventSystems
{
    public enum LinkCondition
    {
        Any, All
    }
    public class Event : MonoBehaviour
    {
        public int eventId;
        public string eventName;
        [TextArea(10, 15)]
        public string eventDescription;
        public int goodWeight;
        [Range(0, 1)]
        public float chance;

        [Header("Guaranteed Event")]
        public bool guaranteed;
        public int guaranteeOnDay;

        [Header("Event Chain")]
        public LinkCondition linkCondition;
        public Event[] linkedEvent;

        internal bool toEnd;

        public virtual void Run()
        {

        }

        public virtual void EndRun()
        {
            toEnd = true;
        }

        // Use this for initialization
        protected virtual void Start()
        {

        }

        // Update is called once per frame
        protected virtual void Update()
        {

        }
    }
}