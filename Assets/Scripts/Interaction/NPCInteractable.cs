using UnityEngine;
using HireGround.Managers; 

namespace HireGround.Core
{
    public interface IInteractable
    {
        void OnGazeEnter();   
        void OnGazeExit();    
        void OnGazeTrigger(); 
    }

    public class NPCInteractable : MonoBehaviour, IInteractable
    {
        [Header("NPC Profile")]
        public string npcID = "booth_tech_01";
        public string npcName = "Recruiter Google";
        
        [Header("Visual Feedback")]
        public GameObject highlightMesh; 
        public GameObject talkPromptUI; 

        public void OnGazeEnter()
        {
            if (highlightMesh) highlightMesh.SetActive(true);
            
            if (DialogueManager.Instance != null && !DialogueManager.Instance.IsConversationActive)
            {
                if (talkPromptUI) talkPromptUI.SetActive(true);
            }
        }

        public void OnGazeExit()
        {
            if (highlightMesh) highlightMesh.SetActive(false);
            if (talkPromptUI) talkPromptUI.SetActive(false);

            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.EndConversation();
            }
        }

        public void OnGazeTrigger()
        {
            if (DialogueManager.Instance != null)
            {
                if (!DialogueManager.Instance.IsConversationActive)
                {
                    Debug.Log($"[Gaze] Start Chat with {npcName}");
                    DialogueManager.Instance.StartConversation(this);
                    
                    if (talkPromptUI) talkPromptUI.SetActive(false);
                }
                else
                {
                }
            }
        }
    }
}