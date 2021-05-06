using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public AudioClip itemPickupClip;
    public Transform RespawnPoint;
    public int lifeRemains = 3;
    private AudioSource playerAudioPlayer;
    private PlayerHealth playerHealth;
    private PlayerMovement playerMovement;
    private PlayerShooter playerShooter;

    private void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
        playerHealth = GetComponent<PlayerHealth>();
        playerAudioPlayer = GetComponent<AudioSource>();

        playerHealth.OnDeath += HandleDeath;

        UIManager.Instance.UpdateLifeText(lifeRemains);
        Cursor.visible = false;
    }

    private void HandleDeath()
    {
        playerMovement.enabled = false;
        playerShooter.enabled = false;

        if (lifeRemains > 0)
        {
            lifeRemains--;
            UIManager.Instance.UpdateLifeText(lifeRemains);
            Invoke("Respawn", 3f);
        }
        else
        {
            GameManager.Instance.EndGame();
        }
        Cursor.visible = true;
    }

    public void Respawn()
    {
        gameObject.SetActive(false);


        transform.position = RespawnPoint.position;

        playerMovement.enabled = true;
        playerShooter.enabled = true;
        playerShooter.gun.ammoRemain = 120;//재장전

        gameObject.SetActive(true);

        Cursor.visible = false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (playerHealth.dead)
        {
            return;
        }
        var item = other.GetComponent<IItem>();

        if (item != null)
        {
            item.Use(gameObject);
            playerAudioPlayer.PlayOneShot(itemPickupClip);
        }
    }
}
