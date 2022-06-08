using UnityEngine;

public interface IDamageable
{
    void OnDamage(float damage, Vector3 hitPoins, Vector3 hitNormal);
}
