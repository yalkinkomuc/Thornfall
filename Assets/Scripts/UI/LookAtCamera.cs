using UnityEngine;

namespace UI
{
   public class LookAtCamera : MonoBehaviour
   {
      [SerializeField] private bool invert;
   
      private Transform cameraTransform;

      private void Awake()
      {
         cameraTransform = Camera.main.transform;
      }

      private void LateUpdate()
      {
         if (invert)
         {
            Debug.Log("sdfsd");
            Vector3 dirToCamera = (cameraTransform.position - transform.position).normalized;
            transform.LookAt(transform.position + dirToCamera * -1);
         }
         else
         {
            transform.LookAt(cameraTransform);
         }
      }
   }
}
