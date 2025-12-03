using UnityEngine;
using UnityEngine.Android;
using System.Collections;

namespace HireGround.Utils
{
    public class RuntimePermissionRequest : MonoBehaviour
    {
        void Start()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            StartCoroutine(AskPermissionsRoutine());
#endif
        }

        IEnumerator AskPermissionsRoutine()
        {
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                Debug.Log("[Permission] Meminta izin Kamera...");
                Permission.RequestUserPermission(Permission.Camera);
                yield return new WaitForSeconds(0.5f); 
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            {
                Debug.Log("[Permission] Meminta izin Microphone...");
                Permission.RequestUserPermission(Permission.Microphone);
                yield return new WaitForSeconds(0.5f);
            }

            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageRead);
                yield return new WaitForSeconds(0.5f);
            }
            
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
            }
        }
        
        public void ForceRequest()
        {
            StartCoroutine(AskPermissionsRoutine());
        }
    }
}