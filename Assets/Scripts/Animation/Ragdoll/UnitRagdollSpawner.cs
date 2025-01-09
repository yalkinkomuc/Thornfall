#pragma warning disable CS0114, CS0618, CS0414, CS0067
using System;
using UnityEngine;
using UnityEngine.Serialization;

public class UnitRagdollSpawner : MonoBehaviour
{
   [SerializeField] private Transform ragdollPrefab;
   [SerializeField] private Transform originalRootBone;
   
   private HealthSystem healthSystem;
   private bool hasSpawnedRagdoll = false;
   private Vector3 lastHitDirection;
   private float lastHitForce;

   public void SetLastHitInfo(Vector3 direction, float force)
   {
       lastHitDirection = direction;
       lastHitForce = force;
   }

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
      unitRagdoll.Setup(originalRootBone, lastHitDirection, lastHitForce);
   }
}
