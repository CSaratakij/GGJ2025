using System;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Game.UI
{
    public class UIMainMenu : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Button btnStartGame;
        [SerializeField] private Button btnExitGame;

        private void Awake()
        {
            if (btnStartGame)
            {
                btnStartGame.onClick.AddListener(() =>
                {
                    if (GameManager.Instance)
                    {
                        GameManager.Instance.GoToGameplayScene();
                    }
                });
            }
            
            if (btnExitGame)
            {
                btnExitGame.onClick.AddListener(() =>
                {
                     if (GameManager.Instance)
                     {
                         GameManager.Instance.ExitGame();
                     }                   
                });
            }
        }
    }
}
