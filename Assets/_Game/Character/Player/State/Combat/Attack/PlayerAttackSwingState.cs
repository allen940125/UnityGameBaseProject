namespace GameFramework.Actors
{
    public class PlayerAttackSwingState : PlayerCombatState
    {
        public PlayerAttackSwingState(PlayerStateContext stateContext) : base(stateContext)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();
            StateContext.ReusableData.AttackSwingFinished = false;

            // 1. 開啟傷害判定
            //StateContext.WeaponController.EnableHitbox();

            // 2. 鎖定移動與旋轉 (這時候不能再轉彎了！)
            StateContext.ReusableData.ActionMovementMultiplier = 0f;

            // 3. 開啟 Root Motion (讓動畫帶動角色往前踏步)
            //StateContext.Animator.applyRootMotion = true;
        }

        public override void OnLogic()
        {
            base.OnLogic();

            // 可選：每幀監測輸入（其實主要由 Transition 判斷，但這邊可預留回饋）
        }

        public override void PhysicsUpdate()
        {
            base.PhysicsUpdate();
        }
        
        public override void OnExit()
        {
            base.OnExit();
            //StateContext.WeaponController.DisableHitbox();
            //StateContext.Animator.applyRootMotion = false;
        }
    }
}