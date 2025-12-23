using System;
using GAS.Runtime;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

public class FireAsset : AbilityAsset
{
    public GameObject bulletPrefab;

    public override Type AbilityType() => typeof(Fire);// 下文对应Fire的Ability
    
}

public class Fire : AbstractAbility<FireAsset>
{
    // 这里的AbilityAsset是FireAsset类的变量。由abilityAsset转化而来。
    // AbilityAsset在AbstractAbility<T> 中定义。
    public GameObject bulletPrefab => AbilityAsset.bulletPrefab;

    // 修改處：加上 (abilityAsset as FireAsset)
    public Fire(AbilityAsset abilityAsset) : base(abilityAsset as FireAsset)
    {
    }
    
    public override AbilitySpec CreateSpec(AbilitySystemComponent owner)
    {
        return new FireSpec(this, owner); // 对应下文Fire的AbilitySpec 
    }
}

public class FireSpec : AbilitySpec<Fire>
{
    public FireSpec(Fire ability, AbilitySystemComponent owner) : base(ability, owner)
    {
    }

    public override void ActivateAbility(params object[] args)
    {
        // 生成子弹
        var bullet = Object.Instantiate(Data.bulletPrefab).GetComponent<Bullet>();
        var transform = Owner.transform;
        bullet.Init(transform.position, transform.up, 10, Owner.AttrSet<AS_Character>().ATK.CurrentValue);
        TryEndAbility();
    }

    public override void CancelAbility()
    {
    }

    public override void EndAbility()
    {
    }
}