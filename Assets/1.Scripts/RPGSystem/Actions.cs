using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RPGSystem
{
    public interface RPGAction
    {
        public async UniTask Resolve() { }
    }

    [Serializable]
    public class SetVariables : RPGAction
    {
        public VariableTableSet setVariables;

        public async UniTask Resolve()
        {
            RPGManager.GameData.ResolveSetVariables(setVariables);
        }
    }

    // [Serializable]
    // public class FlashScreen : RPGAction
    // {
    //     public Color flashColor;
    //     public float duration;
    //     public bool waitToEnd;

    //     public  async UniTask Resolve()
    //     {
    //         var flashScreen = GameManager.refs.flashScreen;
    //         var initTime = Time.time;
    //         flashScreen.color = flashColor;
    //         do
    //         {
    //             var t = (Time.time - initTime) / duration;
    //             flashScreen.color = new Color(flashColor.r, flashColor.g, flashColor.b, 1 - t);
    //             await UniTask.Yield();
    //         }
    //         while (initTime + duration >= Time.time);
    //         flashScreen.color = new Color(0, 0, 0, 0);
    //     }
    // }

    [Serializable]
    public class ShowCanvas : RPGAction
    {
        [ValueDropdown("UIGetCanvasList")]
        public Transform canvasT;
        public bool isVisible;

        public async UniTask Resolve()
        {
            canvasT.gameObject.SetActive(isVisible);
        }

        IEnumerable UIGetCanvasList()
        {
            foreach (Transform child in GameObject.Find("Canvas").transform)
                yield return child;
        }
    }

    [Serializable]
    public class CheckConditions : RPGAction
    {
        public VariableTableCondition conditionList;
        public RPGAction[] onTrue, onFalse;

        public async UniTask Resolve()
        {
            if (conditionList.IsAllConditionOK())
                foreach (var action in onTrue) await action.Resolve();
            else
                foreach (var action in onFalse) await action.Resolve();
        }
    }

    [Serializable]
    public class ShowText : RPGAction
    {
        public string text;
        public async UniTask Resolve()
        {
            Debug.Log(text);
        }
    }

    [Serializable]
    public class CallScript : RPGAction
    {
        public UnityEvent unityEvent;
        public async UniTask Resolve()
        {
            unityEvent.Invoke();
        }
    }

    [Serializable]
    public class PlaySE : RPGAction
    {
        public AudioClip clip;
        public bool waitEnd;
        public bool isLoop;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Tooltip("0 -> 2D Sound (global)\n1 -> 3D Sound (from position)")]
        [Range(0f, 1f)]
        public float spatialBlend = 1f;
        [Range(-1f, 1f)]
        public float stereoPan = 0f;
        [Range(-3f, 3f)]
        public float pitch = 1f;
        [Tooltip("If null we use MainCamera as emitter")]
        public GameObject emitter;

        [Button("Set myself as emitter")]
        public void EmitterMyself()
        {
            emitter = Selection.activeGameObject;
        }

        [Button("Global Emitter")]
        public void EmitterGlobal()
        {
            emitter = null;
        }

        public async UniTask Resolve()
        {
            RPGManager.AudioManager.PlaySound(clip, emitter, isLoop, volume, spatialBlend, stereoPan, pitch);
            await UniTask.Delay(TimeSpan.FromSeconds(waitEnd ? clip.length : 0), ignoreTimeScale: true);
        }
    }

    [Serializable]
    public class Await : RPGAction
    {
        public float seconds;
        public async UniTask Resolve()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: false);
        }
    }

    public enum TweenType { PunchScale, PunchRotation }

    // [Serializable]
    // public class Tween : RPGAction
    // {
    //     public TweenType type;
    //     public Transform targetTransform;
    //     public Vector3 punch;
    //     public float duration, elasticity;
    //     public int vibrato;
    //     public bool waitToEnd;
    //     public  async UniTask Resolve()
    //     {
    //         UniTask task;
    //         switch (type)
    //         {
    //             case TweenType.PunchScale:
    //                 task = targetTransform.DOPunchScale(punch, duration, vibrato, elasticity).AwaitForComplete(); break;
    //             case TweenType.PunchRotation:
    //                 task = targetTransform.DOPunchRotation(punch, duration, vibrato, elasticity).AwaitForComplete(); break;
    //             default:
    //                 return;
    //         }
    //         await task;
    //     }
    // }

    [Serializable]
    public class AddItem : RPGAction
    {
        public ScriptableItem item;
        [ShowIf("@item && item.isStackable")]
        public int amount;
        public async UniTask Resolve()
        {
            RPGManager.GameData.AddItem(item, amount);
        }
    }

    // [Serializable]
    // public class TeleportPlayer : RPGAction
    // {
    //     [ValueDropdown("GetSceneNameList")]
    //     public string mapName;
    //     [Range(0, 10)]
    //     public int mapSpawnIndex;
    //     public bool changeFaceDirection;
    //     [ShowIf("changeFaceDirection")]
    //     public FaceDirection newFaceDirection;

    //     public  async UniTask Resolve()
    //     {
    //         var gameData = GameManager.GameData;
    //         var playerEntity = GameManager.refs.player;
    //         gameData.savedMapSpawnIndex = mapSpawnIndex;
    //         gameData.savedFaceDir = changeFaceDirection ? newFaceDirection : playerEntity.faceDirection;
    //         if (SceneManager.GetActiveScene().name == mapName)
    //             GameManager.Instance.SpawnPlayer();
    //         else
    //             SceneManager.LoadScene(mapName);
    //     }

    //     public IEnumerable<string> GetSceneNameList()
    //     {
    //         var path = Application.dataPath + "/Scenes";
    //         var directoryInfo = new DirectoryInfo(path);
    //         var fileList = directoryInfo.GetFiles();
    //         foreach (var file in fileList)
    //             if (!file.Name.Contains(".meta")) yield return file.Name.Split(".unity")[0];
    //     }
    // }

}

