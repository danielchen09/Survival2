using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour {
    private Rigidbody rb;

    private GameObject target;
    public Item item;
    public int count;

    public float moveSpeed = 10f;

    public float _interactRadius;
    public float interactRadius {
        get => _interactRadius;
        set {
            _interactRadius = value;
            GetComponent<SphereCollider>().radius = value;
        }
    }

    public float pickupRadius;

    private float _lifeTime;
    public float lifeTime {
        get => Time.time - _lifeTime;
    }

    private void Start() {
        rb = GetComponent<Rigidbody>();

        _lifeTime = Time.time;
    }

    private void Update() {
        if (!target)
            return;
        Inventory inventory = target.GetComponent<Inventory>();

        if (Vector3.Distance(target.transform.position, transform.position) <= pickupRadius) {
            int remaining = inventory.AddItem(item, count);
            if (remaining > 0) {
                count = remaining;
                return;
            }
            Destroy(gameObject);
            return;
        }

        Vector3 direction = Vector3.Normalize(target.transform.position - transform.position);
        rb.velocity = direction * moveSpeed;
    }

    private void OnTriggerEnter(Collider other) {
        if (target)
            return;
        Inventory inventory = other.GetComponent<Inventory>();
        if (inventory) {
            target = other.gameObject;
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _interactRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
