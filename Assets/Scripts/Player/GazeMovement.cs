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
        public Transform vrCamera;

        private CharacterController characterController;
        private bool isMoving = false;

        void Start()
        {
            characterController = GetComponent<CharacterController>();
            if (vrCamera == null) vrCamera = Camera.main.transform;
        }

        void Update()
        {
            HandleMovement();
        }

        void HandleMovement()
        {
            if (vrCamera == null) return;

            float cameraX = vrCamera.eulerAngles.x;

            isMoving = (cameraX >= lookDownAngleThreshold && cameraX < lookDownAngleMax);

            if (isMoving)
            {
                Vector3 forward = vrCamera.forward;
                
                forward.y = 0; 
                
                characterController.SimpleMove(forward.normalized * moveSpeed);
            }
        }
    }
}