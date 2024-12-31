using UnityEngine;
using System;

public class ArrowProjectile : MonoBehaviour
{
    private Vector3 targetPosition;
    private Unit targetUnit;
    [SerializeField] private float movementSpeed = 20f;

    public event EventHandler<OnArrowHitEventArgs> OnArrowHit;


    private void Start()
    {
       Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.forward = moveDirection;
    }


    public class OnArrowHitEventArgs : EventArgs
    {
        public Unit targetUnit;
    }

    public void Setup(Vector3 targetPosition, Unit targetUnit)
    {
        this.targetPosition = targetPosition;
        this.targetUnit = targetUnit;

        transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
    }

    private void Update()
    {
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        transform.forward = moveDirection;
        
        float distanceBeforeMoving = Vector3.Distance(transform.position, targetPosition);
        transform.position += moveDirection * movementSpeed * Time.deltaTime;
        float distanceAfterMoving = Vector3.Distance(transform.position, targetPosition);

        if (distanceBeforeMoving < distanceAfterMoving)
        {
            OnArrowHit?.Invoke(this, new OnArrowHitEventArgs { targetUnit = targetUnit });
            Destroy(gameObject);
        }
    }
}