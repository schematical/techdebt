using UnityEngine;

namespace UI
{
    public class UIMainMenuCanvas: MonoBehaviour
    {
        private static UIMainMenuCanvas _instance;
        
   
        public static UIMainMenuCanvas Instance
        {
            get
            {
                return _instance;
            }
        }
        
        public MainMenu mainMenu;
        public MetaUnlockPanel metaUnlockPanel;
        public UIMetaChallengesPanel  uiMetaChallengesPanel;
        void Awake()
        {
            MetaCurrency.Load();
            _instance = this;
            ClosePanels();
            mainMenu.gameObject.SetActive(true);
        }

        public void ClosePanels()
        {
            mainMenu.gameObject.SetActive(false);
            metaUnlockPanel.gameObject.SetActive(false);
            uiMetaChallengesPanel.gameObject.SetActive(false);
        }
    }
}