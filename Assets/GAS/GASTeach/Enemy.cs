using GAS.Runtime;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private const float BoomDistance = 2.5f;
    private AbilitySystemComponent _asc;
    [SerializeField] private Player _player;
    private Rigidbody _rb; // Rigidbody2D -> Rigidbody

    private void Awake()
    {
        _asc = gameObject.GetComponent<AbilitySystemComponent>();
        _rb = gameObject.GetComponent<Rigidbody>(); // GetComponent<Rigidbody>()

        GameRunner.Instance.RegisterEnemy(this);

        Init(_player);
    }
    
    private void Update()
    {
        if (Chase()) Boom();
        
        if (_player != null)
        {
            var dir = _player.transform.position - transform.position;
            dir.y = 0; // 强制水平转向，不仰视或俯视
            if (dir != Vector3.zero)
            {
                transform.forward = dir.normalized; // up -> forward
            }
        }
    }

    private void OnDestroy()
    {
        GameRunner.Instance.UnregisterEnemy(this);
        // 注意检查 AttributeSet 是否为空，防止游戏关闭时报错
        if (_asc != null && _asc.AttrSet<AS_Character>() != null)
        {
            _asc.AttrSet<AS_Character>().HP.UnregisterPostBaseValueChange(OnHpChange);
        }
        // 同理 AbilityContainer 的检查... (这里省略，保持原样逻辑)
        // 你的原始代码这里有点风险，如果 ASC 被销毁了再调这个会空指针，不过先保持原样。
        // _asc.AbilityContainer.AbilitySpecs()[BombSkillName].UnregisterEndAbility(OnBombEnd);
        // _asc.AbilityContainer.AbilitySpecs()[GAbilityLib.Die.Name].UnregisterEndAbility(OnBombEnd);
    }

    public void Init(Player player)
    {
        _player = player;

        _asc.InitWithPreset(1);
        InitAttributes();

        _asc.AbilityContainer.AbilitySpecs()[BombSkillName].RegisterEndAbility(OnBombEnd);
        _asc.AbilityContainer.AbilitySpecs()[GAbilityLib.Die.Name].RegisterEndAbility(OnBombEnd);
    }

    private void OnBombEnd()
    {
        Destroy(gameObject);
    }

    private void InitAttributes()
    {
        _asc.AttrSet<AS_Character>().InitHP(10);
        _asc.AttrSet<AS_Character>().InitATK(20);
        _asc.AttrSet<AS_Character>().InitSPEED(5);

        _asc.AttrSet<AS_Character>().HP.RegisterPostBaseValueChange(OnHpChange);
    }

    private void OnHpChange(AttributeBase attributeBase, float oldValue, float newValue)
    {
        if (newValue <= 0) Die();
    }

    private bool Chase()
    {
        if (_player == null ||
            _asc.AttrSet<AS_Character>().HP.CurrentValue <= 0 ||
            _asc.HasTag(GTagLib.Ban_Motion))
        {
            _rb.linearVelocity = Vector3.zero; // Vector2 -> Vector3
            return false;
        }
        
        var delta = _player.transform.position - transform.position;
        delta.y = 0; // 忽略高度差
        
        var speed = _asc.AttrSet<AS_Character>().SPEED.CurrentValue;
        
        // 直接设置速度
        _rb.linearVelocity = delta.normalized * speed;
        
        var distance = delta.magnitude;
        return distance < BoomDistance;
    }

    protected virtual string BombSkillName => GAbilityLib.Bomb.Name;
    
    private void Boom()
    {
        _asc.TryActivateAbility(BombSkillName);
    }

    private void Die()
    {
        GameRunner.Instance.AddScore();
        _asc.TryActivateAbility(GAbilityLib.Die.Name);
    }
}