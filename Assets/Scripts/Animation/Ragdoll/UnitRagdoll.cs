using UnityEngine;

public class UnitRagdoll : MonoBehaviour
{
    [SerializeField] private Transform ragdollRootBone;

    public void Setup(Transform originalRootBone, Vector3 hitDirection, float hitForce)
    {
        MatchAllChildTransforms(originalRootBone, ragdollRootBone);
        
        // Explosion yerine hit direction'a göre kuvvet uygula
        ApplyDirectionalForceToRagdoll(ragdollRootBone, hitDirection * hitForce);
    }

    private void MatchAllChildTransforms(Transform root , Transform clone )
    {
        foreach (Transform child in root)
        {
          Transform cloneChild = clone.Find(child.name);

          if (cloneChild != null)
          {
              cloneChild.position = child.position;
              cloneChild.rotation = child.rotation;
              
              MatchAllChildTransforms(child, cloneChild);
          }
        }
    }

    private void ApplyDirectionalForceToRagdoll(Transform root, Vector3 force)
    {
        foreach (Transform child in root)
        {
            if (child.TryGetComponent(out Rigidbody childRigidbody))
            {
                childRigidbody.AddForce(force, ForceMode.Impulse);
                // Biraz da yukarı kuvvet ekleyelim
                childRigidbody.AddForce(Vector3.up * force.magnitude * 0.3f, ForceMode.Impulse);
            }

            ApplyDirectionalForceToRagdoll(child, force);
        }
    }

    public void ApplyForceToRagdoll(Vector3 force, Vector3 hitPoint, float radius)
    {
        void ApplyForceRecursive(Transform bone)
        {
            if (bone.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                // Kemikten vuruş noktasına olan mesafeyi hesapla
                float distance = Vector3.Distance(hitPoint, bone.position);
                
                // Mesafeye göre kuvveti azalt
                float forceMultiplier = 1f - (distance / radius);
                forceMultiplier = Mathf.Max(forceMultiplier, 0); // Negatif olmasın
                
                // Kuvveti uygula
                rb.AddForce(force * forceMultiplier, ForceMode.Impulse);
                
                // Biraz da tork ekleyelim gerçekçilik için
                rb.AddTorque(Random.insideUnitSphere * force.magnitude * 0.5f, ForceMode.Impulse);
            }

            // Alt kemiklere de uygula
            foreach (Transform child in bone)
            {
                ApplyForceRecursive(child);
            }
        }

        ApplyForceRecursive(ragdollRootBone);
    }

}
