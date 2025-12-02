# HireGround 19 🎧🧑‍💼

**HireGround 19** is a VR job fair experience built with Unity for Android + VR Cardboard.  
Players explore a virtual hall, visit company booths, and converse with AI-assisted recruiter NPCs using voice (STT → text → LLM → text → TTS).

This project is developed as part of an AR/VR assignment.

---

## 🚀 Unity Version

This repository uses:

**Unity Version: `6000.0.58f2` (Unity 6)**  
_All collaborators must use this exact version to avoid compatibility issues._

---

## 🧩 Initial Tech Stack (Editable by Collaborators)

This is the baseline stack decided during initial planning.  
Collaborators may update these components as needed later.

### 🎮 Client (Unity / VR / Android)

- **Unity 6000.0.58f2** (3D Core or URP)
- **Target Platform:** Android (VR Cardboard)
- **Language:** C#
- **Core Client-Side Components:**
  - VR camera rig (split-screen Cardboard view)
  - Gyroscope-based gaze movement
  - Reticle + raycast interaction system
  - NPC interaction + Dialogue state machine
  - HTTP communication with backend via `UnityWebRequest`
- **Audio Processing (on device):**
  - **Speech-to-Text (STT):**
    - Android SpeechRecognizer (via Unity plugin)
    - or Unity STT plugin (flexible)
  - **Text-to-Speech (TTS):**
    - Android TextToSpeech engine
    - or Unity TTS plugin

### 🌐 Backend (LLM Layer)

Backend handles **text only** (no audio).

- **Language:** Node.js (Express) *or* Python (FastAPI)  
  _(final choice up to backend developer)_
- **LLM:** Google **Gemini (free tier)**  
  - Recommended model: `gemini-1.5-flash`
- **Responsibilities:**
  - Receive user text from Unity
  - Load NPC/booth profile
  - Build prompt for LLM
  - Send prompt to Gemini
  - Return structured answer text
- **API Contract:**
  - `POST /api/llm-query`  
  - JSON in → JSON out

---

## 🏛 High-Level Architecture

### 1. Unity (Client)
- Player speaks → STT converts speech → text
- Unity sends:

```json
{
  "npc_id": "booth_tech_01",
  "user_text": "What does a backend engineer do here?"
}
```

---

2. Backend (LLM)

- Reads NPC profile
- Builds Gemini prompt + sends to LLM
- Returns answer:
```json
{
  "npc_id": "booth_tech_01",
  "answer_text": "Our backend engineers design APIs, work with databases, and ensure system performance."
}
```
3. Unity (Client)

- Displays answer text
- Uses TTS to speak the answer aloud
- DialogueManager returns to idle

---

📁 Project Structure (Initial Draft)
```
HireGround19/
├─ Assets/
│  ├─ Scripts/
│  │  ├─ Player/
│  │  ├─ Interaction/
│  │  ├─ Dialogue/
│  │  ├─ Network/
│  │  └─ Voice/
│  ├─ Scenes/
│  ├─ Prefabs/
│  ├─ UI/
│  └─ Resources/
├─ Packages/
├─ ProjectSettings/
└─ README.md
```

(Structure may evolve during development.)

---

🧵 Git Workflow

Default branch: main

Recommended workflow:
```bash
git checkout -b "feature/<feature-name>"
git commit -m "feat: description"
git push origin "feature/<feature-name>"
```
Suggested feature branches

feature/vr-movement
- feature/npc-interaction
- feature/dialogue-ui
- feature/voice-stt
- feature/voice-tts
- feature/backend-llm
- feature/npc-content