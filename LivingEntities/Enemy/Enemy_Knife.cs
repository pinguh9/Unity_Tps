using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Knife : Enemy
{
    private List<LivingEntity> lastAttackedTargets = new List<LivingEntity>();

    private void FixedUpdate()
    {
        if (dead) return;

        if (state == State.AttackBegin || state == State.Attacking)
        {
            var lookRotation = Quaternion.LookRotation(targetEntity.transform.position - transform.position);
            //현재 위치에서 타겟을 바라보는 방향을 향하는 방향 벡터를 구한 다음 그 방향을 바라보는 회전 벡터 생성
            var targetAngleY = lookRotation.eulerAngles.y; //y축 기준으로 회전하므로 y축값만 가져옴

            targetAngleY = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngleY, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.eulerAngles = Vector3.up * targetAngleY;
        }

        if (state == State.Attacking)
        {
            var direction = transform.forward;
            var deltaDistance = agent.velocity.magnitude * Time.deltaTime;//공격의 궤적을 나타냄

            var size = Physics.SphereCastNonAlloc(attackRoot.position,
                attackRadius, direction, hits, deltaDistance, whatIsTarget);//이미 가지고 있는 hit배열에 결과를 받음
            //움직이면서 공격할 때 감지의 정확도를 높임

            for (var i = 0; i < size; i++)
            {
                var attackTargetEntity = hits[i].collider.GetComponent<LivingEntity>();//감지한 상대방이 LivingEntity 오브젝트인지
                if (attackTargetEntity != null && !lastAttackedTargets.Contains(attackTargetEntity))
                {//직전까지 공격을 가했던 대상이 아니라면 (공격 도중에 또 공격하는 것 방지)
                    var message = new DamageMessage();
                    message.amount = damage;
                    message.damager = gameObject;

                    if (hits[i].distance <= 0f)
                    {//시작하자마자 이미 겹친 콜라이더가 있을 경우 hit.point가 0이됨
                        message.hitPoint = attackRoot.position;
                    }
                    else
                    {
                        message.hitPoint = hits[i].point;
                    }
                    message.hitNormal = hits[i].normal;

                    attackTargetEntity.ApplyDamage(message);
                    lastAttackedTargets.Add(attackTargetEntity);
                    break;
                }
            }
        }
    }

    public override void BeginAttack()
    {
        state = State.AttackBegin;
        base.BeginAttack();
        animator.SetTrigger("Attack");
    }

    public void EnableAttack()
    {
        state = State.Attacking;
        lastAttackedTargets.Clear();
    }

}
