using UnityEngine;
// Menggunakan namespace UnityEngine.SpatialTracking untuk mengakses TrackedPoseDriver
using UnityEngine.SpatialTracking; 

namespace HireGround.Utils
{
    // Script ini HANYA akan berjalan di Unity Editor.
    // Saat dibuild ke HP Android, script ini otomatis mati/hilang.
    public class EditorCameraControl : MonoBehaviour
    {
#if UNITY_EDITOR

        [Header("Editor Simulation")]
        public float sensitivity = 2.0f;
        public bool requireAltKey = true; // Harus tahan Alt untuk gerak?

        private float rotationX = 0f;
        private float rotationY = 0f;
        private TrackedPoseDriver driver;

        void Start()
        {
            // 1. Cari component TrackedPoseDriver
            driver = GetComponent<TrackedPoseDriver>();

            // 2. Matikan Driver supaya kamera tidak terkunci (bisa digerakkan script ini)
            if (driver != null)
            {
                driver.enabled = false;
                Debug.Log("[EditorCameraControl] TrackedPoseDriver dimatikan sementara untuk simulasi Mouse.");
            }

            // Inisialisasi rotasi awal
            Vector3 startRot = transform.localEulerAngles;
            rotationX = startRot.x;
            rotationY = startRot.y;
        }

        void Update()
        {
            // Cek apakah tombol Alt ditekan (jika requireAltKey = true)
            // Atau klik kanan mouse (opsional, tergantung selera)
            bool isInputActive = !requireAltKey || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

            if (isInputActive)
            {
                // Ambil input mouse
                float mouseX = Input.GetAxis("Mouse X") * sensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

                // Hitung rotasi
                rotationY += mouseX;
                rotationX -= mouseY;
                rotationX = Mathf.Clamp(rotationX, -90f, 90f); // Batasi agar tidak leher patah

                // Terapkan rotasi ke kamera
                transform.localEulerAngles = new Vector3(rotationX, rotationY, 0);
            }
        }
#endif
    }
}