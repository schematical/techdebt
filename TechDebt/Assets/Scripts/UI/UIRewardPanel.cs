using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UIRewardPanel : MonoBehaviour
    {
        public enum State
        {
            Closed,
            Opened
        };
        public State state = State.Closed;
        public Button openButton;
        public Image rewardImage;
        public Animator animator;
        private int count;
        public float timer = 0;
        public List<UILazerBeam> lazerBeams;
        public 
        void Start()
        {
            Init(5);
            openButton.onClick.AddListener(OnOpenClick);
        }
        public void Init(int _count = 1)
        {
            count = _count;
            timer = 0;
            state = State.Closed;
            if (lazerBeams != null)
            {
                foreach (UILazerBeam lazerBeam in lazerBeams)
                {
                    lazerBeam.gameObject.SetActive(false);
                }
            }
            lazerBeams.Clear();
        }

        void FixedUpdate()
        {
            if (state == State.Closed)
            {
                return;
            }

            timer += 0.02f;
       
            if (timer >= 2)
            {
               
                if (lazerBeams.Count < count)
                {
                    Vector2 position = rewardImage.transform.position - new Vector3(0, 30);
                    GameObject lazerGO =
                        GameManager.Instance.prefabManager.Create("UILazarBeamPanel", position, transform);

                    lazerGO.transform.SetAsFirstSibling();
                    UILazerBeam lazerBeam = lazerGO.GetComponent<UILazerBeam>();
                
                    float multiplier = 1;
                    int existingCount = lazerBeams.Count;
                    if (existingCount % 2 == 0)
                    {
                        multiplier = -1;
                    }

                    double rotationOffset = Math.Ceiling((double) existingCount / 2) * 20;
                    double rotation = rotationOffset * multiplier;
                    lazerBeam.Init((float) rotation);
                    lazerBeams.Add(lazerBeam);
                    timer = 0;
                }
            }
        }
        public void OnOpenClick()
        {
            
            // bool test = animator.GetBool("IsExploding");
            animator.SetBool("IsExploding", true);
            state = State.Opened;
         
            
        }
    }
}