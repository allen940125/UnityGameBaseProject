using GAS.Runtime;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private GameplayEffectAsset geBulletDamage;
    private AbilitySystemComponent _asc;
    private Rigidbody _rb; // Rigidbody2D -> Rigidbody
    private GameplayEffect _geBulletDamage;
    
    private void Awake()
    {
        _asc = gameObject.GetComponent<AbilitySystemComponent>();
        _rb = gameObject.GetComponent<Rigidbody>(); // Rigidbody
        _geBulletDamage = new GameplayEffect(geBulletDamage);
    }

    // 参数从 Vector2 改为 Vector3
    public void Init(Vector3 position, Vector3 direction, float speed, float damage)
    {
        // 设置出生点，速度
        transform.position = position;
        
        // 确保方向没有 Y 轴分量（如果是水平射击）
        direction.y = 0; 
        direction.Normalize();

        _rb.linearVelocity = direction * speed;
        
        // 也可以让子弹朝向飞行方向
        if(direction != Vector3.zero) transform.forward = direction;

        // 设置伤害
        _asc.InitWithPreset(1);
        _asc.AttrSet<AS_Bullet>().InitATK(damage);
    }

    // 3D 碰撞检测：OnTriggerEnter
    // 参数类型：Collider
    private void OnTriggerEnter(Collider other)
    {
        // 伤害生效
        if(other.gameObject.TryGetComponent(out AbilitySystemComponent enemy))
        {
            if (enemy.HasTag(GTagLib.Faction_Enemy))
            {
                _asc.ApplyGameplayEffectTo(_geBulletDamage, enemy);
                Destroy(gameObject);
            }
        }
        // 注意：这里可能需要判断一下是不是撞墙了，或者撞到 Player 需不需要销毁
        // 为了保持和你原代码一致，非 Enemy 也销毁
        else
        {
            Destroy(gameObject);
        }
    }
}