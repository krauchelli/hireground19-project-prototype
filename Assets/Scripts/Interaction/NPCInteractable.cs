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
            Debug.Log($"[Gaze] Melihat NPC: {npcName}");
            if (highlightMesh) highlightMesh.SetActive(true);
            if (talkPromptUI) talkPromptUI.SetActive(true);
        }

        public void OnGazeExit()
        {
            Debug.Log($"[Gaze] Berhenti melihat NPC: {npcName}");
            if (highlightMesh) highlightMesh.SetActive(false);
            if (talkPromptUI) talkPromptUI.SetActive(false);
        }

        public void OnGazeTrigger()
        {
            Debug.Log($"[Gaze] TRIGGER -> Mulai Percakapan dengan {npcName}...");
            
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.StartConversation(this);
            }
            else
            {
                Debug.LogError("[NPCInteractable] DialogueManager tidak ditemukan di Scene! Pastikan GameObject 'Managers' sudah ada.");
            }
            
            if (talkPromptUI) talkPromptUI.SetActive(false);
        }
    }
}