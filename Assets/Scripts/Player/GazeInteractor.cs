using UnityEngine;
using UnityEngine.UI;

namespace HireGround.Core
{
    public class GazeInteractor : MonoBehaviour
    {
        [Header("Raycast Settings")]
        public float interactDistance = 10.0f;
        public LayerMask interactableLayer;

        [Header("Dwell Settings (Timer)")]
        public float dwellTime = 1.5f;
        public Image reticleLoadingImage;
        private float timer = 0f;
        private IInteractable currentInteractable;
        private bool isInteracting = false; 

        void Update()
        {
            PerformRaycast();
        }

        void PerformRaycast()
        {
            Ray ray = new Ray(transform.position, transform.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance, interactableLayer))
            {
                IInteractable hitInteractable = hit.collider.GetComponent<IInteractable>();

                if (hitInteractable != null)
                {
                    if (currentInteractable != hitInteractable)
                    {
                        ResetSelection();
                        currentInteractable = hitInteractable;
                        currentInteractable.OnGazeEnter();
                    }
                        
                    if (!isInteracting)
                    {
                        timer += Time.deltaTime;
                        UpdateReticleUI(timer / dwellTime);

                        if (timer >= dwellTime)
                        {
                            isInteracting = true;
                            currentInteractable.OnGazeTrigger();
                            ResetReticle();
                        }
                    }
                }
            }
            else
            {
                if (currentInteractable != null)
                {
                    ResetSelection();
                }
            }
        }

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