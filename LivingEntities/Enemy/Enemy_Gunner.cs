using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Gunner : Enemy
{
    public Gun gun;
    private float TimeBetFire = 5f;
    private float lastFireTime = 0f;

    private void FixedUpdate()
    {
        if (dead) return;

        if (state == State.Attacking)
        {
            var lookRotation = Quaternion.LookRotation(targetEntity.transform.position - transform.position);
            //현재 위치에서 타겟을 바라보는 방향을 향하는 방향 벡터를 구한 다음 그 방향을 바라보는 회전 벡터 생성
            var targetAngleY = lookRotation.eulerAngles.y; //y축 기준으로 회전하므로 y축값만 가져옴

            targetAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.eulerAngles = Vector3.up * targetAngleY;

            
            if (Time.time >= lastFireTime + TimeBetFire)
            {
                lastFireTime = Time.time;
                if (gun.Fire(targetEntity.transform.position))
                {
                    animator.SetTrigger("Shoot");
                }
            }

            DisableAttack();
        }
    }

    public override void BeginAttack()
    {
       state = State.Attacking;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (gun == null || gun.state == Gun.State.Reloading) return;//총이 없거나 재장전 중인 상태에는 왼손 위치를 갱신해주지않음

        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1.0f);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1.0f);//왼손의 위치와 회전을 100퍼센트로

        animator.SetIKPosition(AvatarIKGoal.LeftHand, gun.leftHandMount.position);
        animator.SetIKRotation(AvatarIKGoal.LeftHand, gun.leftHandMount.rotation);
    }

}
