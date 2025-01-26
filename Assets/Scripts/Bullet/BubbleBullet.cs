using System;
using UnityEngine;

namespace Game
{
    public class BubbleBullet : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private float lifeTime = 3f;

        private bool isMoveable = false;
        private float moveSpeed = 0.25f;
        private float lifeTimeTimer = 0.0f;
        private Vector3 moveDirection;
        private Rigidbody rigid;
        private GameObject owner = null;
        
        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        /*
        private void Start()
        {
            StartMove(Vector3.forward, moveSpeed: 0.3f, lifeTime: lifeTime);
        }
        */

        private void Update()
        {
            bool isExpired = (Time.time > lifeTimeTimer);

            if (isExpired)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (isMoveable)
            {
                Vector3 newPosition = rigid.position + moveDirection * (moveSpeed * Time.fixedTime);
                rigid.MovePosition(newPosition);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                bool shouldHit = (owner != other.gameObject);

                if (shouldHit)
                {
                    IImmobilize immobilizeAffector = other.GetComponent<IImmobilize>();

                    if (immobilizeAffector != null)
                    {
                        immobilizeAffector.Immobilize();
                    }
                    
                    lifeTimeTimer = 0.0f;
                }
            }
        }

        public void StartMove(Vector3 moveDirection, float moveSpeed, float lifeTime, GameObject owner = null)
        {
            this.moveDirection = moveDirection;
            this.moveSpeed = moveSpeed;
            this.owner = owner;
            this.lifeTime = lifeTime;
            lifeTimeTimer = (Time.time + lifeTime);
            isMoveable = true;
        }
    }
}
