using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Game
{
    public class GameMode : MonoBehaviour
    {
        private const int MAX_PLAYER = 3;
        private const int MAX_SCORE = 9999;
            
        [Header("Dependencies")]
        [SerializeField, Required] private UIPlayerScore uiPlayerScore;
        [SerializeField] private PlayerController[] players;

        private int[] playerScores = new int[MAX_PLAYER];

        private void Awake()
        {
            playerScores = new int[MAX_PLAYER];
                
            for (int i = 0; i < playerScores.Length; i++)
            {
                playerScores[i] = 0;
            }
            
            for (int i = 0; i < players.Length; i++)
            {
                int index = i;
                PlayerController player = players[index];

                if (player)
                {
                    player.OnDead += (playerIndex, causerIndex) =>
                    {
                        bool canUpdateScore = (causerIndex >= 0);
                        
                        if (!canUpdateScore)
                        {
                            return;
                        }
                        
                        int newScore = playerScores[causerIndex];
                        newScore = (newScore + 1) > MAX_SCORE ? MAX_SCORE : (newScore + 1);
                        playerScores[causerIndex] = newScore;

                        if (uiPlayerScore)
                        {
                            uiPlayerScore.UpdateUI(causerIndex, newScore);
                        }
                    };
                }
            }
        }
    }
}