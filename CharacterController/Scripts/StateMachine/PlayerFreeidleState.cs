using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFreeidleState : PlayerBaseState 
{
    public PlayerFreeidleState(PlayerStateMachine currentContext, PlayerStateFactory playerStateFactory)
    : base(currentContext, playerStateFactory) { }

    int randomNumber;

    public override void EnterState()
    {
        //Debug.Log("Free Idle State!");
        SpawnRandomNumber();                                         // ランダムな方忖を伏撹して、その方忖でアイドル�啝�の�N��を�Qめる
        Ctx.Animator.SetBool(Ctx.IsFreeidlingHash, true);            // Free Idle彜�Bに秘ったことを幣すパラメ�`タ�`
        Ctx.Animator.SetInteger(Ctx.RandomFreeidle, randomNumber);   // ランダムな方忖をパラメ�`タ�`に嚥える
    }

    public override void UpdateState()
    {
        CheckSwitchStates();
        
    }

    public override void ExitState() {
        Ctx.Animator.SetInteger(Ctx.RandomFreeidle, 0);               // アイドル�啝�の�N��を�Qめるパラメ�`タ�`を0で兜豚晒する
    }

    public override void CheckSwitchStates()
    {
        if (Ctx.IsMovementPressed || Ctx.IsJumpPressed) {
            SwitchState(Factory.Idle());
        } else {
            MotionLoop();
        }
    }

    public override void InitializeSubState() { }

    public void SpawnRandomNumber() {
        randomNumber = Random.Range(1, 7); // 伏撹1欺6岻寂議昧字屁方
        //randomNumber = 6;
        //Debug.Log("Random Number: " + randomNumber);
    }

    private void MotionLoop()
    {
        if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Sad Idle")) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                SwitchState(Factory.Idle());
            }
        } 
        else if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Happy Idle")) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                SwitchState(Factory.Idle());
            }
        }
        // Sit Up StateMachine
        else if (randomNumber == 3) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Sit Up")) {
                if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                    Ctx.Animator.Play("Sit Up To Idle", 0, 0f);
                }
            } else if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Sit Up To Idle")) {
                  if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                      SwitchState(Factory.Idle());
                  }
            }
        }
        // Push Up StateMachine
        else if (randomNumber == 4) {  
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Push Up")) {
                if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 3) {
                    Ctx.Animator.Play("Push Up To Idle", 0, 0f);
                }
            } else if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).IsTag("Push Up To Idle")) {
                if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                    SwitchState(Factory.Idle());
                }
            }
        }
        // Look Around 1 & 2
        else if (randomNumber == 5 || randomNumber == 6) {
            if (Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1) {
                SwitchState(Factory.Idle());
            }
        }
        
    }
}
