using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UINPCDetailPanel: UIPanel
    {
        public Button followButton;
        private NPCBase _selectedNPC;
        public TextMeshProUGUI _npcDetailText;

        void Start()
        {
            followButton.onClick.AddListener(() =>
            {
                GameManager.Instance.cameraController.StartFollowing(_selectedNPC.transform);
            });
            /*
             * TODO: Add assignable NPC tasks:
             * AddButton("Follow", () =>
        {
            var cameraController = FindObjectOfType<CameraController>();
            if (cameraController != null && _selectedNPC != null)
            {
                cameraController.StartFollowing(_selectedNPC.transform);
            }
        })
             */
        }
        public void Show(NPCBase npc)
        {
            _selectedNPC = npc;
            GameManager.Instance.UIManager.Close();
            gameObject.SetActive(true);
        }

        void Update()
        {
            if (_selectedNPC == null) return;

            if (_npcDetailText == null)
            {
                Debug.LogError("_npcDetailText is not assigned. Cannot update NPC Detail Panel.");
                return;
            }

           
            _npcDetailText.text = _selectedNPC.GetDetailText();
        }
    }
}