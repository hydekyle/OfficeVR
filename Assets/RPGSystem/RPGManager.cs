using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGSystem
{
    [Serializable]
    public class RPGManager : MonoBehaviour
    {
        public static RPGManager Instance;
        public static bool isInteractAvailable = true;
        public static bool isMovementAvailable = true;
        public static GameData GameData { get => RPGManager.Instance.gameData; }
        public static AudioManager AudioManager { get => RPGManager.Instance.audioManager; }
        public GameData gameData = new();
        public AudioManager audioManager;

        private void Awake()
        {
            if (Instance != null) Destroy(this.gameObject);
            else
            {
                Instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }
    }

}