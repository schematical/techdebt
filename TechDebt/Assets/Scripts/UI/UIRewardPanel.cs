using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
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
        private State state = State.Closed;
        public Button openButton;
        public Image rewardImage;
        public Animator animator;
        private int count;
        private float timer = 0;
        private List<UILazerBeam> lazerBeams = new List<UILazerBeam>();
        private UnityAction onDone;
        void Start()
        {
            openButton.onClick.AddListener(OnOpenClick);
        }
        public void Show(UnityAction _onDone, int _count = 1)
        {
           Debug.Log("UIRewardPanel.Show");
            GameManager.Instance.UIManager.SetTimeScalePause();
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
            onDone = _onDone;
            gameObject.SetActive(true);
            openButton.gameObject.SetActive(true);
        }

        void Update()
        {
            if (state == State.Closed)
            {
                // Debug.Log(state);
                return;
            }

            timer += Time.unscaledDeltaTime;
            if (timer >= 1)
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

            if (timer > 4)
            {
                foreach (UILazerBeam lazerBeam in lazerBeams)
                {
                    lazerBeam.Shutdown();
                }

            }

            if (timer > 6)
            {
                onDone.Invoke();
                gameObject.SetActive(false);
            }
           
        }
        public void OnOpenClick()
        {
            
            // bool test = animator.GetBool("IsExploding");
            animator.SetBool("IsExploding", true);
            state = State.Opened;
            openButton.gameObject.SetActive(false);
         
            
        }
    }
}