using UnityEngine;

namespace Game
{
    public class CameraShake : MonoBehaviour
    {
        [SerializeField] private float shakeDuration = 0.5f;
        [SerializeField] private float shakeMagnitude = 0.2f;

        private Vector3 originalPosition;
        private float shakeTimer;

        private void Start()
        {
            originalPosition = transform.localPosition;
        }

        private void Update()
        {
            if (shakeTimer > 0)
            {
                transform.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;
                shakeTimer -= Time.deltaTime;
            }
            else
            {
                transform.localPosition = originalPosition;
            }
        }

        public void Shake()
        {
             shakeTimer = shakeDuration;           
        }
        
        public void Shake(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
            shakeTimer = duration;
        }
    }
}
