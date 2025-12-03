using UnityEngine;
using UnityEngine.SpatialTracking; 

namespace HireGround.Utils
{
    public class EditorCameraControl : MonoBehaviour
    {
#if UNITY_EDITOR

        [Header("Editor Simulation")]
        public float sensitivity = 2.0f;
        public bool requireAltKey = true;

        private float rotationX = 0f;
        private float rotationY = 0f;
        private TrackedPoseDriver driver;

        void Start()
        {
            driver = GetComponent<TrackedPoseDriver>();

            if (driver != null)
            {
                driver.enabled = false;
                Debug.Log("[EditorCameraControl] TrackedPoseDriver dimatikan sementara untuk simulasi Mouse.");
            }

            Vector3 startRot = transform.localEulerAngles;
            rotationX = startRot.x;
            rotationY = startRot.y;
        }

        void Update()
        {
            bool isInputActive = !requireAltKey || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            if (isInputActive)
            {
                float mouseX = Input.GetAxis("Mouse X") * sensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

                rotationY += mouseX;
                rotationX -= mouseY;
                rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Batasi agar tidak leher patah

                transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
            }
        }
#endif
    }
}