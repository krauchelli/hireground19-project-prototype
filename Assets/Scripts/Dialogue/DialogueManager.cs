using UnityEngine;
using UnityEngine.UI;
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

        [Header("State")]
        public ConversationState currentState = ConversationState.Idle;

        private NPCInteractable currentNPC;

        void Awake()
        {
            if (Instance == null) Instance = this;
            if (dialogueCanvas) dialogueCanvas.SetActive(false);
        }

        public void StartConversation(NPCInteractable npc)
        {
            currentNPC = npc;
            dialogueCanvas.SetActive(true);
            
            npcNameText.text = npc.npcName;
            dialogueText.text = "Halo! Silakan bicara...";

            EnterListeningState();
        }

        void EnterListeningState()
        {
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
            dialogueText.text = "Maaf, saya tidak bisa terhubung ke server.";
            currentState = ConversationState.Idle;
        }

        public void OnTTSFinished()
        {
            currentState = ConversationState.Idle;
            statusText.text = "Selesai. Tatap lagi untuk bicara.";
            
        }
    }
}