using UnityEngine;
using HireGround.Utils;

namespace HireGround.Managers
{
    [RequireComponent(typeof(SimpleAndroidVoice))]
    [RequireComponent(typeof(MainThreadDispatcher))]
    public class VoiceController : MonoBehaviour
    {
        public static VoiceController Instance;
        private SimpleAndroidVoice androidVoice;

        void Awake()
        {
            if (Instance == null) Instance = this;
            
            androidVoice = GetComponent<SimpleAndroidVoice>();
            if (androidVoice == null) androidVoice = gameObject.AddComponent<SimpleAndroidVoice>();
            if (GetComponent<MainThreadDispatcher>() == null) gameObject.AddComponent<MainThreadDispatcher>();
        }

        void OnEnable()
        {
            androidVoice.OnSttResult += HandleSttResult;
            androidVoice.OnTtsDone += HandleTtsFinished;
            androidVoice.OnSttError += HandleSttError;
        }

        void OnDisable()
        {
            androidVoice.OnSttResult -= HandleSttResult;
            androidVoice.OnTtsDone -= HandleTtsFinished;
            androidVoice.OnSttError -= HandleSttError;
        }

        public void StartListening()
        {
            Debug.Log("[VoiceController] Meminta Android mendengarkan...");
            androidVoice.StartListening();
        }

        void HandleSttResult(string textResult)
        {
            Debug.Log($"[VoiceController] Teks Diterima: {textResult}");
            DialogueManager.Instance.OnUserSpeechRecognized(textResult);
        }

        void HandleSttError(string errorMsg)
        {
            Debug.LogWarning($"[VoiceController] STT Error: {errorMsg}");
            DialogueManager.Instance.currentState = ConversationState.Idle; 
        }

        public void Speak(string textToSpeak)
        {
            Debug.Log($"[VoiceController] Meminta Android bicara: {textToSpeak}");
            androidVoice.Speak(textToSpeak);
        }

        void HandleTtsFinished()
        {
            Debug.Log("[VoiceController] Android selesai bicara.");
            DialogueManager.Instance.OnTTSFinished();
        }
    }
}