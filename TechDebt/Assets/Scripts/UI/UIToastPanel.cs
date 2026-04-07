using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class UIToastPanel: MonoBehaviour
    {
        public TextMeshProUGUI text;
        protected float duration;

        public void Init(string s, float duration)
        {
            this.duration = duration;
            text.text = s;
        }

        public float Tick(float time)
        { 
           duration -= time;
           return duration;
        }
    }
}