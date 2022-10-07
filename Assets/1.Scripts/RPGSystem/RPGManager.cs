using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPGSystem
{
    [Serializable]
    public class RPGManager
    {
        public static RPGManager Instance;
        public static bool isInteractAvailable = true;
        public static bool isMovementAvailable = true;
        public static GameData GameData { get => RPGManager.Instance.gameData; }
        public static AudioManager AudioManager { get => RPGManager.Instance.audioManager; }
        public GameData gameData = new();
        public AudioManager audioManager;

        public RPGManager()
        {
            Instance = this;
        }
    }

}