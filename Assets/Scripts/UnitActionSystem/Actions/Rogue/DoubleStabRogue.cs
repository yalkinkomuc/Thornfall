using UnityEngine;

public class DoubleStabRogue : BaseMeleeAction
{
    [Header("Double Stab Settings")]
    [SerializeField] private int baseDamageAmount = 25;
    [SerializeField] private int actionPointsCost = 2;
    [SerializeField] private float backstabAngle = 90f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override string GetActionName()
    {
        return "Double Stab";
    }

    public override int GetActionPointsCost()
    {
        return actionPointsCost;
    }

    protected override void OnStartAttack()
    {
        animator.SetTrigger("BackStab");
    }

    protected override int GetDamageAmount()
    {
        if (IsBackstab())
        {
            return Mathf.RoundToInt(baseDamageAmount * 1.5f);
        }
        return baseDamageAmount;
    }

    private bool IsBackstab()
    {
        if (targetUnit == null) return false;

        // Hedefin yönü ile saldıran arasındaki açıyı hesapla
        Vector3 targetForward = targetUnit.transform.forward;
        Vector3 toAttacker = (transform.position - targetUnit.transform.position).normalized;

        // Açıyı hesapla (0-180 derece arası)
        float angle = Vector3.Angle(targetForward, toAttacker);

        // Eğer açı backstabAngle'dan küçükse arkadan vuruş sayılır
        return angle <= backstabAngle / 2f;
    }

    protected override StatusEffect GetStatusEffect(Unit target)
    {
        return null;
    }

    public override string GetActionDescription()
    {
        return $"Sneaky double stab attack that deals {baseDamageAmount} damage.\nDeals 50% more damage when attacking from behind!";
    }
} 