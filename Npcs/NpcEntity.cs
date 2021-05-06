using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcEntity : MonoBehaviour, IInteraction
{
    public enum UIstate
    {
        Idle,//기본 유아이
        NpcUI//엔피시 유아이
    }
    
    public bool near { get; protected set; }
    protected GameObject target;
    protected UIstate uistate;

    private Animator animator;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;
   
    private Vector3 newPos;

    private void Start()
    {
        uistate = UIstate.Idle;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if(Input.GetButtonDown("Talk"))
        {
            if (near && uistate == UIstate.Idle)
            {
                Talk();
            }
        }

        if (Input.GetButtonDown("Cancel"))
        {
            if (uistate == UIstate.NpcUI)
            {
                Exit();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            near = true;
            target = other.gameObject;
            playerMovement = other.GetComponent<PlayerMovement>();
            playerShooter = other.GetComponent<PlayerShooter>();
        }
        UIManager.Instance.SetActiveTalkButtonUI(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player")
        {
            near = false;
            UIManager.Instance.SetActiveTalkButtonUI(false);
        }
    }
    public virtual void Talk()
    {
        Cursor.visible = true;
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        uistate = UIstate.NpcUI;
        UIManager.Instance.SetActiveGameplayUI(false);
        CameraManager.Instance.SetActiveFollowCam(false);
    }

    public virtual void Exit()
    {
        Cursor.visible = false;
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        uistate = UIstate.Idle;
        UIManager.Instance.SetActiveGameplayUI(true);
        CameraManager.Instance.SetActiveFollowCam(true);
    }

    void OnAnimatorIK()
    {
        if (near && target != null)
        {
            animator.SetLookAtWeight(1.0f);
            newPos = new Vector3(target.transform.position.x, target.transform.position.y + 1.5f, target.transform.position.z);
            animator.SetLookAtPosition(newPos);
        }
    }
}
