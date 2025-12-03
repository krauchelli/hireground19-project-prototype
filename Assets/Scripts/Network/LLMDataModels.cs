using System;

namespace HireGround.Network
{
    [Serializable]
    public class ChatRequest
    {
        public string session_id;
        public string npc_id;
        public string user_text;
    }

    [Serializable]
    public class ChatResponse
    {
        public string npc_id;
        public string answer_text;
    }

    [Serializable]
    public class SessionResponse
    {
        public string session_id;
        public string message;
    }
}