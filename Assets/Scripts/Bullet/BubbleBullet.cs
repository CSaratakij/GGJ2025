using System;
using UnityEngine;

namespace Game
{
    public class BubbleBullet : MonoBehaviour
    {
        [Header("Setting")]
        [SerializeField] private float lifeTime = 3f;

        public bool IsStartOperate => isStartOperate;
        public GameObject Owner => owner;
        
        private bool isMoveable = false;
        private float moveSpeed = 0.25f;
        private float lifeTimeTimer = 0.0f;
        private Vector3 moveDirection;
        private Rigidbody rigid;
        private GameObject owner = null;
        private bool isStartOperate = false;
        
        private void Awake()
        {
            rigid = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (!isStartOperate)
            {
                return;
            }
            
            bool isExpired = (Time.time > lifeTimeTimer);

            if (isExpired)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (!isStartOperate)
            {
                return;
            }
            
            if (isMoveable)
            {
                Vector3 newPosition = rigid.position + moveDirection * (moveSpeed * Time.deltaTime);
                rigid.MovePosition(newPosition);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isStartOperate)
            {
                return;
            }
            
            if (other.CompareTag("Player"))
            {
                bool shouldHit = (owner != other.gameObject);

                if (shouldHit)
                {
                    IImmobilize immobilizeAffector = other.GetComponent<IImmobilize>();
                    bool shouldApplyImmobilizeEffect = (immobilizeAffector != null) && (immobilizeAffector.CanImmobilize());
                    
                    if (shouldApplyImmobilizeEffect)
                    {
                        immobilizeAffector.Immobilize();
                    }
                    
                    lifeTimeTimer = 0.0f;
                }
            }
        }

        public void StartOperation(Vector3 moveDirection, float moveSpeed, float lifeTime, GameObject owner = null)
        {
            this.moveDirection = moveDirection;
            this.moveSpeed = moveSpeed;
            this.owner = owner;
            this.lifeTime = lifeTime;
            
            lifeTimeTimer = (Time.time + lifeTime);
            isMoveable = true;
            isStartOperate = true;
        }

        public void ForceStopOperation()
        {
            isStartOperate = false;
            lifeTimeTimer = 0.0f;
            Destroy(gameObject);
        }
    }
}
