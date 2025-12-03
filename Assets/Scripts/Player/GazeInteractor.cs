using UnityEngine;
using UnityEngine.UI;

namespace HireGround.Core
{
    public class GazeInteractor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public float interactDistance = 10.0f; // Jarak pandang maksimal
        public LayerMask interactableLayer;    // Layer khusus NPC (Layer 6)

        [Header("Dwell Settings (Timer)")]
        public float dwellTime = 1.5f; // Berapa lama harus menatap NPC
        public Image reticleLoadingImage; // Image UI tipe Filled Radial

        private float timer = 0f;
        private IInteractable currentInteractable;
        private bool isInteracting = false; 

        void Update()
        {
            PerformRaycast();
        }

        void PerformRaycast()
        {
            // Tembak sinar dari posisi kamera ke arah depan
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            // Cek apakah sinar menabrak sesuatu di layer Interactable
            if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
            {
                // Coba ambil komponen IInteractable dari objek yang tertabrak
                IInteractable hitInteractable = hit.collider.GetComponent<IInteractable>();

                if (hitInteractable != null)
                {
                    // Jika melihat objek baru (berbeda dari frame sebelumnya)
                    if (currentInteractable != hitInteractable)
                    {
                        ResetSelection();
                        currentInteractable = hitInteractable;
                        currentInteractable.OnGazeEnter();
                    }
                    
                    // Logika Dwell (Menunggu trigger)
                    if (!isInteracting)
                    {
                        timer += Time.deltaTime;
                        UpdateReticleUI(timer / dwellTime); // Update progress bar lingkaran

                        // Jika waktu habis, trigger interaksi
                        if (timer >= dwellTime)
                        {
                            isInteracting = true;
                            currentInteractable.OnGazeTrigger();
                            ResetReticle(); // Reset timer visual tapi tetap di state interacting
                        }
                    }
                }
            }
            else
            {
                // Jika tidak melihat apapun atau melihat objek bukan interactable
                if (currentInteractable != null)
                {
                    ResetSelection();
                }
            }
        }

        // Reset saat player memalingkan muka
        void ResetSelection()
        {
            if (currentInteractable != null)
            {
                currentInteractable.OnGazeExit();
                currentInteractable = null;
            }
            ResetReticle();
        }

        void ResetReticle()
        {
            timer = 0f;
            isInteracting = false;
            UpdateReticleUI(0f);
        }

        void UpdateReticleUI(float fillAmount)
        {
            if (reticleLoadingImage != null)
                reticleLoadingImage.fillAmount = fillAmount;
        }
    }
}