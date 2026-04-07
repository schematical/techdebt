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
            text.color = Color.white;
        }

        public float Tick(float time)
        { 
           duration -= time;
           if (duration < 5f)
           {
               text.color = new Color(
                   text.color.r,
                   text.color.g,
                   text.color.b,
                   duration / 5
                );
           }
           return duration;
        }
    }
}