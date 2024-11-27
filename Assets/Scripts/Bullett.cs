using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Bullett : NetworkBehaviour
{

    [SerializeField] private float _lifeTimer = 8F;
    [SerializeField] private AudioClip _shieldDamageAudio;
    private AudioSource _audio;


    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(Lifetime());
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!IsServer) return; // Only the server controls the collision

        var player = collision.gameObject.GetComponentInParent<Ability_Controller>();

        if (player != null)
        {
            Debug.Log("Bullet: I Touched the Player!");

            Vector3 hitPoint = collision.ClosestPoint(transform.position);
            player.BulletDamageServerRpc(hitPoint); // Damage to player

            Debug.Log("Bullet: I called the damage function to the player!");

            ReturnToPool();

            //if (GetComponent<NetworkObject>().IsSpawned)
            //{
            //    GetComponent<NetworkObject>().Despawn(); // NetworkObject'in despawn edilmesi
            //}
            //else
            //{
            //    Destroy(gameObject); // Fallback olarak normal yok etme
            //}
        }

        if (collision.gameObject.CompareTag("Shield"))
        {
            _audio.Stop();
            _audio.PlayOneShot(_shieldDamageAudio);

            Debug.Log("Bullet: I Touched the Shield!");

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        NetworkedObjectPool.Instance.ReturnToPool(GetComponent<NetworkObject>());
    }

    IEnumerator Lifetime() // Bullet life time
    {
        yield return new WaitForSeconds(_lifeTimer);

        ReturnToPool();

        //if (GetComponent<NetworkObject>().IsSpawned)
        //{
        //    GetComponent<NetworkObject>().Despawn(); // NetworkObject'in despawn edilmesi
        //}
        //else
        //{
        //    Destroy(gameObject); // Fallback olarak normal yok etme
        //}
    }

}
