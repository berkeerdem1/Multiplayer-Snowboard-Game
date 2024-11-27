using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Ability_Controller : NetworkBehaviour
{

    [Header("COMPONENTS")]
    [SerializeField] private Transform frontPoint;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private GameObject shield;


    [Header("DASH")]
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    private bool isDashing = false;


    [Header("AUDIOS")]
    [SerializeField] private AudioClip dashSudio;
    [SerializeField] private AudioClip shieldActiveAudio;
    [SerializeField] private AudioClip hurtAudio;


    [Header("components")]
    private Snowboard_Audios _myAudios;
    private Rigidbody _rb;
    private SnowboardController _mysnowboard;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myAudios = GetComponent<Snowboard_Audios>();
        _mysnowboard = GetComponent<SnowboardController>();
    }

    public void Shoot()
    {
        NetworkObject networkObject = NetworkedObjectPool.Instance.GetFromPool(firePoint.position, firePoint.rotation);

        Rigidbody rb = networkObject.GetComponent<Rigidbody>();
        rb.velocity = firePoint.rotation * Vector3.forward * bulletSpeed;
    }


    [ServerRpc(RequireOwnership = false)]
    public void BulletDamageServerRpc(Vector3 collisionPoint)
    {
        if (!IsOwner) return;

        _myAudios.PlayTemporarySound(hurtAudio);

        Debug.Log("ServerRpc has been called, the player will take damage!");

        Vector3 dir = (transform.position - collisionPoint).normalized;
        _rb.AddForce(dir * 100f, ForceMode.Impulse);

        Debug.Log("The player took damage");
    }


    public void Shield()
    {
        _myAudios.PlayTemporarySound(shieldActiveAudio);
        shield.SetActive(true);
        StartCoroutine(Coroutine());

        Debug.Log("Shield Activated");
    }

    IEnumerator Coroutine()
    {
        yield return new WaitForSeconds(10);
        shield.SetActive(false);
    }

    public void Dash()
    {
        _myAudios.PlayTemporarySound(dashSudio);

        isDashing = true;
        _rb.AddForce(frontPoint.forward * dashForce, ForceMode.Impulse);

        Invoke(nameof(StopDash), dashDuration);
    }

    private void StopDash()
    {
        isDashing = false;
        _rb.velocity = Vector3.zero;
    }

    public void HighJump()
    {
        _mysnowboard.jumpForce = 25;
    }

    public void InitialJump()
    {
        _mysnowboard.jumpForce = 10;
    }
}
