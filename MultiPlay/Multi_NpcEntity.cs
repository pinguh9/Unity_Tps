using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multi_NpcEntity : MonoBehaviourPun, IInteraction
{
    public enum UIstate
    {
        Idle,//기본 유아이
        NpcUI//엔피시 유아이
    }
    
    public bool near { get; protected set; }
    public GameObject followCam;
    public GameObject NpcCam;
    protected GameObject target;
    protected UIstate uistate;

 
    private Animator animator;
    private Multi_PlayerMovement playerMovement;
    private Multi_PlayerShooter playerShooter;
   
    private Vector3 newPos;

    private void Start()
    {
        uistate = UIstate.Idle;
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;
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
        if(other.tag == "Player" && PhotonNetwork.IsMasterClient)
        {
            near = true;
            target = other.gameObject;
            playerMovement = other.GetComponent<Multi_PlayerMovement>();
            playerShooter = other.GetComponent<Multi_PlayerShooter>();
        }
        UIManager.Instance.SetActiveTalkButtonUI(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player" && PhotonNetwork.IsMasterClient)
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
        followCam.SetActive(false);
    }

    public virtual void Exit()
    {
        Cursor.visible = false;
        playerMovement.enabled = true;
        playerShooter.enabled = true;

        uistate = UIstate.Idle;
        UIManager.Instance.SetActiveGameplayUI(true);
        followCam.SetActive(true);
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
