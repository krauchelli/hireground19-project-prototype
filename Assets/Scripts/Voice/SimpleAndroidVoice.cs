using UnityEngine;
using System;

namespace HireGround.Utils
{
    public class SimpleAndroidVoice : MonoBehaviour
    {
        public static SimpleAndroidVoice Instance;

        // Action Events
        public Action<string> OnSttResult;
        public Action<string> OnSttError;
        public Action OnTtsStart;
        public Action OnTtsDone;

        // Android Objects
        private AndroidJavaObject speechRecognizer;
        private AndroidJavaObject speechIntent;
        private AndroidJavaObject ttsObject;
        
        private bool isTtsInitialized = false;
        private const string INDONESIAN_LOCALE = "id_ID"; // Kode bahasa Indonesia

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            InitTTS();
            InitSTT();
#else
            Debug.LogWarning("[SimpleAndroidVoice] Native Voice hanya berjalan di Device Android fisik.");
#endif
        }

        void InitTTS()
        {
            try
            {
                TTSInitListener ttsListener = new TTSInitListener(this);
                
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    ttsObject = new AndroidJavaObject("android.speech.tts.TextToSpeech", activity, ttsListener);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[TTS Init Error] {e.Message}");
            }
        }

        public void OnTTSInitialized(int status)
        {
            if (status == 0) 
            {
                using (AndroidJavaClass localeClass = new AndroidJavaClass("java.util.Locale"))
                using (AndroidJavaObject indoLocale = new AndroidJavaObject("java.util.Locale", "id", "ID"))
                {
                    int result = ttsObject.Call<int>("setLanguage", indoLocale);
                    if (result >= 0) 
                    {
                        isTtsInitialized = true;
                        Debug.Log("[TTS] Bahasa Indonesia berhasil diset.");
                    }
                    else
                    {
                        Debug.LogError("[TTS] Bahasa Indonesia tidak didukung atau belum didownload di HP ini.");
                    }
                }
            }
            else
            {
                Debug.LogError("[TTS] Gagal inisialisasi engine TTS.");
            }
        }

        public void Speak(string text)
        {
            if (!isTtsInitialized || ttsObject == null) 
            {
                Debug.LogWarning("TTS belum siap.");
                return;
            }

            ttsObject.Call<int>("speak", text, 0, null, "UtteranceId");
            OnTtsStart?.Invoke();
            
            StartCoroutine(WaitSpeaking(text));
        }

        System.Collections.IEnumerator WaitSpeaking(string text)
        {
            float duration = Mathf.Max(2.0f, text.Length * 0.1f);
            yield return new WaitForSeconds(duration);
            OnTtsDone?.Invoke();
        }

        void InitSTT()
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                using (AndroidJavaClass speechClass = new AndroidJavaClass("android.speech.SpeechRecognizer"))
                {
                    speechRecognizer = speechClass.CallStatic<AndroidJavaObject>("createSpeechRecognizer", activity);
                    SpeechListener listener = new SpeechListener(this);
                    speechRecognizer.Call("setRecognitionListener", listener);
                }

                using (AndroidJavaClass recognizerIntentClass = new AndroidJavaClass("android.speech.RecognizerIntent"))
                {
                    speechIntent = new AndroidJavaObject("android.content.Intent");
                    speechIntent.Call<AndroidJavaObject>("setAction", recognizerIntentClass.GetStatic<string>("ACTION_RECOGNIZE_SPEECH"));
                    speechIntent.Call<AndroidJavaObject>("putExtra", recognizerIntentClass.GetStatic<string>("EXTRA_LANGUAGE_MODEL"), recognizerIntentClass.GetStatic<string>("LANGUAGE_MODEL_FREE_FORM"));
                    speechIntent.Call<AndroidJavaObject>("putExtra", recognizerIntentClass.GetStatic<string>("EXTRA_LANGUAGE"), "id-ID"); // PENTING: Set Bahasa Indonesia
                    speechIntent.Call<AndroidJavaObject>("putExtra", recognizerIntentClass.GetStatic<string>("EXTRA_PARTIAL_RESULTS"), true);
                    speechIntent.Call<AndroidJavaObject>("putExtra", recognizerIntentClass.GetStatic<string>("EXTRA_MAX_RESULTS"), 1);
                }
            }));
        }

        public void StartListening()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (speechRecognizer != null)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    speechRecognizer.Call("startListening", speechIntent);
                }));
            }
#else
            Debug.Log("Editor Mode: Listening...");
            Invoke("SimulateEditorResult", 2.0f);
#endif
        }

        void SimulateEditorResult()
        {
            OnSttResult?.Invoke("Ini adalah contoh suara dari editor");
        }

        public void StopListening()
        {
            if (speechRecognizer != null)
            {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    speechRecognizer.Call("stopListening");
                }));
            }
        }

        class TTSInitListener : AndroidJavaProxy
        {
            private SimpleAndroidVoice manager;
            public TTSInitListener(SimpleAndroidVoice m) : base("android.speech.tts.TextToSpeech$OnInitListener") { manager = m; }
            
            public void onInit(int status)
            {
                manager.OnTTSInitialized(status);
            }
        }

        class SpeechListener : AndroidJavaProxy
        {
            private SimpleAndroidVoice manager;
            public SpeechListener(SimpleAndroidVoice m) : base("android.speech.RecognitionListener") { manager = m; }

            public void onResults(AndroidJavaObject bundle)
            {
                if (bundle == null) return;
                
                using (AndroidJavaClass speechClass = new AndroidJavaClass("android.speech.SpeechRecognizer"))
                {
                    string key = speechClass.GetStatic<string>("RESULTS_RECOGNITION");
                    AndroidJavaObject list = bundle.Call<AndroidJavaObject>("getStringArrayList", key);
                    
                    if (list != null)
                    {
                        string result = list.Call<string>("get", 0);
                        if (!string.IsNullOrEmpty(result))
                        {
                            MainThreadDispatcher.Execute(() => manager.OnSttResult?.Invoke(result));
                        }
                    }
                }
            }

            public void onError(int error)
            {
                string msg = "Error Code: " + error;
                MainThreadDispatcher.Execute(() => manager.OnSttError?.Invoke(msg));
            }

            public void onReadyForSpeech(AndroidJavaObject paramsBundle) { }
            public void onBeginningOfSpeech() { }
            public void onRmsChanged(float rmsdB) { }
            public void onBufferReceived(byte[] buffer) { }
            public void onEndOfSpeech() { }
            public void onPartialResults(AndroidJavaObject partialResults) { }
            public void onEvent(int eventType, AndroidJavaObject paramsBundle) { }
        }
    }

    public class MainThreadDispatcher : MonoBehaviour
    {
        private static System.Collections.Generic.Queue<Action> _executionQueue = new System.Collections.Generic.Queue<Action>();

        public void Update()
        {
            lock (_executionQueue)
            {
                while (_executionQueue.Count > 0)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        public static void Execute(Action action)
        {
            if (action == null) return;
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}