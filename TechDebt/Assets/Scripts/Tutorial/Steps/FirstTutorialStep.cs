using System.Collections.Generic;
using NPCs;
using UnityEngine;

namespace Tutorial.Steps
{
    public class FirstTutorialStep: TutorialStep
    {
        public FirstTutorialStep(TutorialStepId id, string name, string description) : base(id, name, description)
        {
            onTrigger = () =>
            {
                GameManager.Instance.HideAllAttentionIcons();
                InfrastructureInstance server = GameManager.Instance.GetInfrastructureInstanceByID("server1");
                GameObject sGO = GameManager.Instance.prefabManager.Create("SchematicalBot",
                    server.transform.position + new Vector3(4, 0));
                NPCSchematicalBot schematicalBot = sGO.GetComponent<NPCSchematicalBot>();
                schematicalBot.Initialize();
                NPCBase bossNPC = GameManager.Instance.AllNpcs.Find((npc) => npc.GetComponent<BossNPC>() != null);
                bossNPC.transform.position = GameManager.Instance.GetInfrastructureInstanceByID("boss-desk")
                    .GetInteractionPosition();
                bossNPC.gameObject.SetActive(true);
            };
        }

        public override List<DialogButtonOption> GetDialogOptions()
        {
            return new List<DialogButtonOption>()
            {
                new DialogButtonOption() { Text = "Start Tutorial", OnClick = () =>
                    {
             
                        Next();
                    } 
                },
                new DialogButtonOption() { Text = "Just Get Started", OnClick = () => GameManager.Instance.TutorialManager.End() },
            };
        }
    }
}