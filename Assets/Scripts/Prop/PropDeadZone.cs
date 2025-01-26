using System;
using UnityEngine;

namespace Game
{
    public class PropDeadZone : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var playerController = other.gameObject.GetComponent<PlayerController>();
                playerController.ResetCharacterState();
            }
        }
    }
}
