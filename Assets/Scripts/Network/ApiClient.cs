using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using HireGround.Managers;

namespace HireGround.Network
{
    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance;

        [Header("Server Configuration")]
        public string serverIp = "192.168.1.5"; 
        // public string port = "5000";
        
        private string baseUrl;
        private string currentSessionId = ""; 

        [Header("Debug")]
        public bool useMockServer = false;

        void Awake()
        {
            if (Instance == null) Instance = this;
            // Susun URL
            // baseUrl = $"http://{serverIp}:{port}/api";
            Debug.Log($"attempting to set baseUrl with serverIp: {serverIp}");
            baseUrl = $"https://{serverIp}/api";
        }

        void Start()
        {
            if (!useMockServer)
            {
                StartCoroutine(CreateSessionRoutine());
            }
        }

        IEnumerator CreateSessionRoutine()
        {
            string url = $"{baseUrl}/create-session";
            Debug.Log($"[ApiClient] Creating session at {url}...");

            using (UnityWebRequest request = new UnityWebRequest(url, "GET"))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    SessionResponse data = JsonUtility.FromJson<SessionResponse>(json);
                    currentSessionId = data.session_id;
                    Debug.Log($"[ApiClient] Session ID didapat: {currentSessionId}");
                }
                else
                {
                    Debug.LogError($"[ApiClient] Gagal buat session: {request.error}");
                }
            }
        }

        public void SendQuery(string npcId, string userText, System.Action<string> onSuccess, System.Action<string> onError)
        {
            if (useMockServer)
            {
                StartCoroutine(MockServerResponse(userText, onSuccess));
                return;
            }

            if (string.IsNullOrEmpty(currentSessionId))
            {
                Debug.LogWarning("Session ID belum ada, mencoba buat ulang...");
                StartCoroutine(CreateSessionAndRetry(npcId, userText, onSuccess, onError));
                return;
            }

            StartCoroutine(PostChatRequest(npcId, userText, onSuccess, onError));
        }

        IEnumerator CreateSessionAndRetry(string npcId, string userText, System.Action<string> onSuccess, System.Action<string> onError)
        {
            yield return StartCoroutine(CreateSessionRoutine());
            if (!string.IsNullOrEmpty(currentSessionId))
            {
                StartCoroutine(PostChatRequest(npcId, userText, onSuccess, onError));
            }
            else
            {
                onError?.Invoke("Gagal membuat sesi server.");
            }
        }

        IEnumerator PostChatRequest(string npcId, string userText, System.Action<string> onSuccess, System.Action<string> onError)
        {
            string url = $"{baseUrl}/llm-query";

            ChatRequest requestData = new ChatRequest
            {
                session_id = currentSessionId,
                npc_id = npcId,
                user_text = userText
            };

            string jsonBody = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    try 
                    {
                        ChatResponse responseData = JsonUtility.FromJson<ChatResponse>(jsonResponse);
                        onSuccess?.Invoke(responseData.answer_text);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Parse Error: {e.Message}");
                        onError?.Invoke("Error data server.");
                    }
                }
                else
                {
                    Debug.LogError($"[ApiClient] Chat Error: {request.error}");
                    onError?.Invoke("Gagal terhubung ke AI.");
                }
            }
        }

        void OnApplicationQuit()
        {
            if (!string.IsNullOrEmpty(currentSessionId) && !useMockServer)
            {
                StartCoroutine(DeleteSessionRoutine());
            }
        }

        IEnumerator DeleteSessionRoutine()
        {
            string url = $"{baseUrl}/delete-session/{currentSessionId}";
            using (UnityWebRequest request = UnityWebRequest.Delete(url))
            {
                yield return request.SendWebRequest();
                Debug.Log("[ApiClient] Session Deleted.");
            }
        }

        IEnumerator MockServerResponse(string question, System.Action<string> callback)
        {
            yield return new WaitForSeconds(1.0f);
            callback?.Invoke($"[MOCK] Jawaban untuk: {question}");
        }
    }
}