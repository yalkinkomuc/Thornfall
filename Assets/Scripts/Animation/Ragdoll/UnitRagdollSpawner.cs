using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitRagdollSpawner : MonoBehaviour
{
   [SerializeField] private Transform ragdollPrefab;
   [SerializeField] private Transform originalRootBone;
   
   private HealthSystem healthSystem;
   private bool hasSpawnedRagdoll = false;

   private void Awake()
   {
      healthSystem = GetComponent<HealthSystem>();
      
      if (healthSystem != null)
      {
         healthSystem.OnDead += HealthSystemOnOnDead;
      }
      else
      {
         Debug.LogError("HealthSystem not found on " + gameObject.name);
      }
   }

   private void OnDestroy()
   {
      if (healthSystem != null)
      {
         healthSystem.OnDead -= HealthSystemOnOnDead;
      }
   }

   private void HealthSystemOnOnDead(object sender, EventArgs e)
   {
      if (hasSpawnedRagdoll) return;
      
      hasSpawnedRagdoll = true;
      Transform ragdollTransform = Instantiate(ragdollPrefab, transform.position, transform.rotation);
      UnitRagdoll unitRagdoll = ragdollTransform.GetComponent<UnitRagdoll>();
      unitRagdoll.Setup(originalRootBone);
   }
}
