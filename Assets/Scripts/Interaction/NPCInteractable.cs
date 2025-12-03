using UnityEngine;

namespace HireGround.Core
{
    // =========================================================================
    // 1. INTERFACE (Kontrak Interaksi)
    // Ditaruh di atas sini supaya satu file saja
    // =========================================================================
    public interface IInteractable
    {
        void OnGazeEnter();   // Saat reticle masuk
        void OnGazeExit();    // Saat reticle keluar
        void OnGazeTrigger(); // Saat dwell timer selesai (terpilih)
    }

    // =========================================================================
    // 2. NPC CLASS (Implementasi di Object)
    // Pasang script ini di GameObject NPC
    // =========================================================================
    public class NPCInteractable : MonoBehaviour, IInteractable
    {
        [Header("NPC Profile")]
        public string npcID = "booth_tech_01";
        public string npcName = "Recruiter Google";
        
        [Header("Visual Feedback")]
        public GameObject highlightMesh; // Opsional: Outline/Mesh renderer
        public GameObject talkPromptUI;  // Canvas Worldspace: "Tatap untuk bicara"

        // Dipanggil saat mata mulai melihat NPC
        public void OnGazeEnter()
        {
            Debug.Log($"[Gaze] Melihat NPC: {npcName}");
            if (highlightMesh) highlightMesh.SetActive(true);
            if (talkPromptUI) talkPromptUI.SetActive(true);
        }

        // Dipanggil saat mata berpaling dari NPC
        public void OnGazeExit()
        {
            Debug.Log($"[Gaze] Berhenti melihat NPC: {npcName}");
            if (highlightMesh) highlightMesh.SetActive(false);
            if (talkPromptUI) talkPromptUI.SetActive(false);
        }

        // Dipanggil saat loading lingkaran reticle penuh (Dwell time selesai)
        public void OnGazeTrigger()
        {
            Debug.Log($"[Gaze] TRIGGER -> Mulai Percakapan dengan {npcName}...");
            
            // Nanti di sini kita sambungkan ke DialogueManager
            // Contoh: DialogueManager.Instance.StartConversation(this);
            
            // Matikan prompt UI agar tidak menghalangi wajah NPC saat bicara
            if (talkPromptUI) talkPromptUI.SetActive(false);
        }
    }
}