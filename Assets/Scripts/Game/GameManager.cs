using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance = null;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        private void Update()
        {
            // Hacks (for now)
            if (Keyboard.current.f11Key.wasPressedThisFrame)
            {
                GoToMainMenu();
            }
            else if (Keyboard.current.f12Key.wasPressedThisFrame)
            {
                RestartActiveScene();
            }
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(sceneBuildIndex: 0);
        }

        public void GoToGameplayScene()
        {
            SceneManager.LoadScene(sceneBuildIndex: 1);
        }

        public void RestartActiveScene()
        {
            int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(sceneBuildIndex: activeSceneIndex);
        }

        public void ExitGame()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}