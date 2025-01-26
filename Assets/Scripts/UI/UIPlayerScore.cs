using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    public class UIPlayerScore : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TextMeshProUGUI[] lblPlayerScores;

        public void UpdateUI(int playerIndex, int score)
        {
            bool isValid = (lblPlayerScores != null) && (playerIndex < lblPlayerScores.Length);

            if (isValid)
            {
                TextMeshProUGUI lblScore = lblPlayerScores[playerIndex];
                lblScore.text = $"P{playerIndex + 1}: {score}";
            }
        }
    }
}