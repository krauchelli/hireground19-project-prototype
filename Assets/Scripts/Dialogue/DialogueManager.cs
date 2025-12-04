using UnityEngine;
using HireGround.Core;
using HireGround.Network;
using TMPro;

namespace HireGround.Managers
{
    public enum ConversationState
    {
        Idle,
        Listening,
        Processing,
        Responding
    }

    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance;

        [Header("UI References")]
        public GameObject dialogueCanvas; 
        public TextMeshProUGUI statusText; 
        public TextMeshProUGUI npcNameText; 
        public TextMeshProUGUI dialogueText; 

        // tambahan header untuk jarak interaksi
        public float exitDistance = 3.5f; // max distance to keep conversation
        private Transform playerTransform; // reference to player position

        [Header("State")]
        public ConversationState currentState = ConversationState.Idle;

        private NPCInteractable currentNPC;

        public bool IsConversationActive => dialogueCanvas.activeSelf && currentState != ConversationState.Idle;

        void Awake()
        {
            if (Instance == null) Instance = this;
            if (dialogueCanvas) dialogueCanvas.SetActive(false);

            // ambil referensi player transform
            if (Camera.main != null) playerTransform = Camera.main.transform; // assuming main camera is the player
        }

        void Update()
        {
            // cek jarak
            if (IsConversationActive && currentNPC != null && playerTransform != null)
            {
                float distance = Vector3.Distance(playerTransform.position, currentNPC.transform.position);

                // akhiri percakapan
                if (distance > exitDistance)
                {
                    EndConversation();
                }
            }
        }

        public void StartConversation(NPCInteractable npc)
        {
            // if (IsConversationActive) return;
            if (IsConversationActive && currentNPC == npc) return; // prevent restarting same conversation, pengganti di atas

            // handler kalau sudah ada percakapan aktif dengan NPC lain
            // tutup percakapan sebelumnya
            if (IsConversationActive && currentNPC != npc) EndConversation();
            
            currentNPC = npc;
            dialogueCanvas.SetActive(true);
            
            npcNameText.text = npc.npcName;
            dialogueText.text = "Halo! Silakan bicara...";

            EnterListeningState();
        }

        void EnterListeningState()
        {
            if (currentState == ConversationState.Listening) return;

            currentState = ConversationState.Listening;
            statusText.text = "Mendengarkan...";
            statusText.color = Color.red;

            VoiceController.Instance.StartListening();
        }

        public void OnUserSpeechRecognized(string userText)
        {
            if (currentState != ConversationState.Listening) return;

            currentState = ConversationState.Processing;
            statusText.text = "Sedang Berpikir...";
            statusText.color = Color.yellow;
            dialogueText.text = $"You: {userText}";

            ApiClient.Instance.SendQuery(currentNPC.npcID, userText, OnBackendSuccess, OnBackendError);
        }

        void OnBackendSuccess(string answer)
        {
            currentState = ConversationState.Responding;
            statusText.text = "Menjawab...";
            statusText.color = Color.green;
            
            dialogueText.text = answer;

            VoiceController.Instance.Speak(answer);
        }

        void OnBackendError(string error)
        {
            statusText.text = "Error Connection";
            dialogueText.text = error;
            currentState = ConversationState.Idle;
        }

        public void OnTTSFinished()
        {
            currentState = ConversationState.Idle;
            statusText.text = "Selesai. Tatap lagi untuk lanjut.";
            statusText.color = Color.white;
        }

        public void EndConversation()
        {            
            currentState = ConversationState.Idle;
            currentNPC = null; // clear current NPC
            if (dialogueCanvas) dialogueCanvas.SetActive(false);
        }
    }
}