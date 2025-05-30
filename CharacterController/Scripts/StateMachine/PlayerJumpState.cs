using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState, IRootState
{
    public PlayerJumpState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) {
        IsRootState = true;
    }

    public override void EnterState() {
        InitializeSubState();
        HandleJump();
    }

    public override void UpdateState() {
        HandleGravity();
        CheckSwitchStates();
    }

    public override void ExitState() {
        Ctx.Animator.SetBool(Ctx.IsJumpingHash, false);
        if (Ctx.IsJumpPressed) {
            Ctx.RequireNewJumpPress = true;
        }
        Ctx.CurrentJumpResetRoutine = Ctx.StartCoroutine(JumpResetRoutine());
        if (Ctx.JumpCount == 3) {
            Ctx.JumpCount = 0;
            Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
        }
    }

    public override void CheckSwitchStates() { 
        if (Ctx.CharacterController.isGrounded) {
            SwitchState(Factory.Grounded());
        }
    }

    public override void InitializeSubState() {
        if (!Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Idle());
        } else if (Ctx.IsMovementPressed && !Ctx.IsRunPressed) {
            SetSubState(Factory.Walk());
        } else {
            SetSubState(Factory.Run());
        }
    }

    void HandleJump() {
        if (Ctx.JumpCount < 3 && Ctx.CurrentJumpResetRoutine != null) {
            Ctx.StopCoroutine(Ctx.CurrentJumpResetRoutine);
        }
        Ctx.Animator.SetBool(Ctx.IsJumpingHash, true);
        Ctx.IsJumping = true;
        Ctx.JumpCount += 1;
        Ctx.Animator.SetInteger(Ctx.JumpCountHash, Ctx.JumpCount);
        Ctx.CurrentMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount];
        Ctx.AppliedMovementY = Ctx.InitialJumpVelocities[Ctx.JumpCount]; 
    }

    private IEnumerator JumpResetRoutine()
    {
        yield return new WaitForSeconds(.5f);
        Ctx.JumpCount = 0;
    }

    public void HandleGravity()
    {
        bool isFalling = Ctx.CurrentMovementY <= 0.0f || !Ctx.IsJumpPressed;
        float fallMultiplier = 2.0f;

       if (isFalling) //  角色在空中下落的重力系数
       {                                
            float previousYVelocity = Ctx.CurrentMovementY;
            Ctx.CurrentMovementY = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * fallMultiplier * Time.deltaTime);  // 如果是下落过程中，则重力乘以2加速下落    
            Ctx.AppliedMovementY = Mathf.Max((previousYVelocity + Ctx.CurrentMovementY) * .5f, -20.0f);     // 防止从高空急速下坠，设置重力系数的下限         
       } else {                                              // 角色在空中上升的重力系数
           float previousYVelocity = Ctx.CurrentMovementY;
           Ctx.CurrentMovementY  = Ctx.CurrentMovementY + (Ctx.JumpGravities[Ctx.JumpCount] * Time.deltaTime);  // 这里如果不乘以 deltaTime 就不会进行跳跃(至少视觉上)   
           Ctx.AppliedMovementY = (previousYVelocity + Ctx.CurrentMovementY) * .5f;
       }
    }
}
