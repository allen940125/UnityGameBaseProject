using UnityEngine;

namespace GameFramework.Actors
{
    public class PlayerAttackWindupState : PlayerCombatState
    {
        public PlayerAttackWindupState(PlayerStateContext stateContext) : base(stateContext)
        {
        }

        public override void OnEnter()
        {
            base.OnEnter();

            // 1. 消耗輸入 (表示系統已響應玩家按鍵)
            StateContext.ReusableData.WantsToAttack = false;
            StateContext.ReusableData.AttackWindupFinished = false; // 重置旗標

            //SetAnimationBool(StateContext.AnimationData.WantsToAttackParameterName, true);

            // 2. 根據 ComboIndex 播放對應動畫
            // 假設你的動畫名是 "Attack_0", "Attack_1", "Attack_2"


            string animName = "Attack_" + StateContext.ReusableData.ComboIndex;

            // 使用 CrossFade 讓動作銜接更滑順 (0.1f 是過渡時間)
            StateContext.Animator.CrossFade(animName, 0.1f);

            // 3. 減速 (前搖時允許微動，但要很慢)
            StateContext.ReusableData.ActionMovementMultiplier = 0f;
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
            SetAnimationBool(StateContext.AnimationData.WantsToAttackParameterName, false);
        }
        
        public override void OnAnimationExitEvent()
        {
            StateContext.ReusableData.AttackComboWindowFinished = false;
            StateContext.ReusableData.AttackWindupFinished = false;
            StateContext.ReusableData.AttackSwingFinished = false;
        }
    }
}