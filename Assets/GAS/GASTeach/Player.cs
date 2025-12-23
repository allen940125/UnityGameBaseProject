using GAS.Runtime;
using UnityEngine;
using UnityEngine.InputSystem; // 記得引用這個命名空間

public class Player : MonoBehaviour
{
    private Rigidbody _rb;
    private PlayerGAS _input;
    private AbilitySystemComponent _asc;
    
    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _asc = GetComponent<AbilitySystemComponent>();

        _input = new PlayerGAS();
        _input.Enable();
        _input.Player.Move.performed += OnActivateMove;
        _input.Player.Move.canceled += OnDeactivateMove;
        _input.Player.Fire.performed += OnFire;
        _input.Player.Skill1.performed += OnSweep;
    }

    private void OnDestroy()
    {
        _input.Disable();
        _input.Player.Move.performed -= OnActivateMove;
        _input.Player.Move.canceled -= OnDeactivateMove;
        _input.Player.Fire.performed -= OnFire;
        _input.Player.Skill1.performed -= OnSweep;
        
        // 防止關閉遊戲時 ASC 已經被銷毀導致報錯
        if (_asc != null && _asc.AttrSet<AS_Character>() != null)
        {
            _asc.AttrSet<AS_Character>().HP.UnregisterPostBaseValueChange(OnHpChange);
        }
        // 同理移除技能監聽...
        // _asc.AbilityContainer.AbilitySpecs()[GAbilityLib.Die.Name].UnregisterEndAbility(OnDie);
    }

    private void Start()
    {
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        // === 修正這裡：改用新版 Input System 獲取滑鼠位置 ===
        if (Mouse.current == null) return; // 防止沒有滑鼠設備時報錯

        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        
        // 3D 射線檢測
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
        
        // 建立一個在 Y=0 的平面
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = hitPoint - transform.position;
            dir.y = 0; // 忽略高度差
            
            if (dir != Vector3.zero)
            {
                transform.forward = dir.normalized; // 3D 用 forward 轉向
            }
        }
    }

    public void Init()
    {
        _asc.InitWithPreset(1);
        InitAttribute();
    }
    
    void InitAttribute()
    {
        _asc.AttrSet<AS_Character>().InitHP(100);
        _asc.AttrSet<AS_Character>().InitATK(10);
        _asc.AttrSet<AS_Character>().InitSPEED(8);
        
        _asc.AttrSet<AS_Character>().HP.RegisterPostBaseValueChange(OnHpChange);
        _asc.AbilityContainer.AbilitySpecs()[GAbilityLib.Die.Name].RegisterEndAbility(OnDie);
    }

    private void OnDie()
    {
        GameRunner.Instance.GameOver();
        Destroy(gameObject);
    }

    void OnActivateMove(InputAction.CallbackContext context)
    {
        var move = context.ReadValue<Vector2>();
        
        // Unity 6 使用 linearVelocity，舊版請改回 velocity
        var velocity = _rb.linearVelocity; 
        
        // 2D 輸入 (X, Y) 對應 3D 移動 (X, Z)
        velocity.x = move.x;
        velocity.z = move.y; 
        velocity.y = 0; // 確保不會飛起來

        velocity = velocity.normalized * _asc.AttrSet<AS_Character>().SPEED.CurrentValue;
        _rb.linearVelocity = velocity;
    }
    
    void OnDeactivateMove(InputAction.CallbackContext context)
    {
        _rb.linearVelocity = Vector3.zero;
    }
    
    void OnFire(InputAction.CallbackContext context)
    {
        _asc.TryActivateAbility(GAbilityLib.Fire.Name);
    }
    
    void OnSweep(InputAction.CallbackContext context)
    {
        _asc.TryActivateAbility(GAbilityLib.Skill1.Name);
    }
    
    void OnHpChange(AttributeBase attributeBase,float oldValue, float newValue)
    {
        // 這裡如果你沒有 UIManager_Test 可以先註解掉
        if (UIManager_Test.Instance != null)
        {
            UIManager_Test.Instance.SetHp((int)newValue);
        }
        
        if (newValue <= 0)
        {
            _asc.TryActivateAbility(GAbilityLib.Die.Name);
        }
    }
}