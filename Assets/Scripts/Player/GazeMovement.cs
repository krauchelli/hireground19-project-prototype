using UnityEngine;

namespace HireGround.Core
{
    [RequireComponent(typeof(CharacterController))]
    public class GazeMovement : MonoBehaviour
    {
        [Header("Movement Settings")]
        [Tooltip("Kecepatan jalan player")]
        public float moveSpeed = 3.0f;
        
        [Tooltip("Sudut minimal kepala menunduk untuk mulai jalan (derajat)")]
        public float lookDownAngleThreshold = 25.0f; 
        
        [Tooltip("Batas maksimal menunduk (supaya tidak jalan kalau lihat tegak lurus ke bawah)")]
        public float lookDownAngleMax = 85.0f;

        [Header("References")]
        public Transform vrCamera; // Masukkan Main Camera di sini

        private CharacterController characterController;
        private bool isMoving = false;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            // Otomatis cari kamera jika belum diassign di inspector
            if (vrCamera == null) vrCamera = Camera.main.transform;
        }

        void Update()
        {
            HandleMovement();
        }

        void HandleMovement()
        {
            if (vrCamera == null) return;

            // Ambil rotasi X kamera
            float cameraX = vrCamera.eulerAngles.x;

            // Logika sederhana: jika x antara 25 dan 85 derajat (menunduk)
            isMoving = (cameraX >= lookDownAngleThreshold && cameraX < lookDownAngleMax);

            if (isMoving)
            {
                // Ambil arah depan kamera
                Vector3 forward = vrCamera.forward;
                
                // Matikan komponen Y supaya player tidak 'terbang' ke bawah tanah
                forward.y = 0; 
                
                // Gerakkan CharacterController
                characterController.SimpleMove(forward.normalized * moveSpeed);
            }
        }
    }
}