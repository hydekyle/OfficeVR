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
        [SerializeReference]
        public List<RPGAction> onTrue = new(), onFalse = new();

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

    /// <summary>
    /// Plays a sound effect
    ///</summary>
    [Serializable]
    public class PlaySE : RPGAction
    {
        public AudioClip clip;
        public bool waitEnd;
        public SoundOptions soundOptions = new SoundOptions()
        {
            keepPlayingWhenDisabled = false,
            soundLoop = false,
            volume = 1f,
            spatialBlend = 1f,
            stereoPan = 0f,
            pitch = 1f
        };
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
            RPGManager.AudioManager.PlaySound(clip, soundOptions, emitter);
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

    [Serializable]
    public class Tween : RPGAction
    {
        public TweenType type;
        public Vector3 punch;
        public Transform targetTransform;
        [Button()]
        public void TargetMyself()
        {
            targetTransform = Selection.activeGameObject.transform;
        }
        public float duration, elasticity;
        public int vibrato;
        public bool waitToEnd;

        public async UniTask Resolve()
        {
            switch (type)
            {
                case TweenType.PunchScale:
                    targetTransform.DOPunchScale(punch, duration, vibrato, elasticity); break;
                case TweenType.PunchRotation:
                    targetTransform.DOPunchRotation(punch, duration, vibrato, elasticity); break;
                default:
                    return;
            }
            if (waitToEnd) await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }
    }

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

    [Serializable]
    public class ModifyTransform : RPGAction
    {
        public OperationType operationType;
        public Vector3 targetPosition, targetRotation, targetScale;
        public Transform targetTransform;
        [Tooltip("Sum values instead replacing them")]

        [Button()]
        public void TargetMyself()
        {
            targetTransform = Selection.activeGameObject.transform;
        }

        [Button()]
        public void CopyTargetValues()
        {
            targetPosition = targetTransform.position;
            targetRotation = targetTransform.rotation.eulerAngles;
            targetScale = targetTransform.localScale;
        }

        public async UniTask Resolve()
        {
            if (operationType == OperationType.Add)
            {
                targetTransform.position += targetPosition;
                targetTransform.rotation = Quaternion.Euler(targetRotation + targetTransform.rotation.eulerAngles);
                targetTransform.localScale += targetScale;
            }
            else if (operationType == OperationType.Replace)
            {
                targetTransform.position = targetPosition;
                targetTransform.rotation = Quaternion.Euler(targetRotation);
                targetTransform.localScale = targetScale;
            }

        }
    }

    [Serializable]
    public class ModifyMaterialColor : RPGAction
    {
        public MeshRenderer targetRenderer;
        public Color targetColor;
        public float transitionTime;
        public bool waitToEnd;

        [Button()]
        public void TargetMyself()
        {
            targetRenderer = Selection.activeGameObject.GetComponent<MeshRenderer>();
        }

        public async UniTask Resolve()
        {
            var material = targetRenderer.material;
            if (waitToEnd)
            {
                var startTime = Time.time;
                while (Time.time - startTime < transitionTime)
                {
                    material.color = Color.Lerp(material.color, targetColor, Time.time - startTime / transitionTime);
                    Debug.LogWarning(Time.time - startTime / transitionTime);
                    await UniTask.DelayFrame(1);
                }
            }
            material.color = targetColor;
        }
    }

}

