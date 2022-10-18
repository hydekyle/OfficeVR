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
    public interface IAction
    {
        public async UniTask Resolve() { }
    }

    public interface IWaitable
    {
        [SerializeField]
        public bool WaitEnd { get; set; }
    }

    [Serializable]
    public class SetVariables : IAction
    {
        public VariableTableSet setVariables;

        public async UniTask Resolve()
        {
            RPGManager.GameData.ResolveSetVariables(setVariables);
        }
    }

    [Serializable]
    public class ShowCanvas : IAction
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
    public class CheckConditions : IAction
    {
        public VariableTableCondition conditionList;
        [SerializeReference]
        public List<IAction> onTrue = new(), onFalse = new();

        public async UniTask Resolve()
        {
            if (conditionList.IsAllConditionOK())
                foreach (var action in onTrue) await action.Resolve();
            else
                foreach (var action in onFalse) await action.Resolve();
        }
    }

    [Serializable]
    public class ShowText : IAction
    {
        public string text;
        public async UniTask Resolve()
        {
            Debug.Log(text);
        }
    }

    [Serializable]
    public class CallScript : IAction
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
    public class PlaySE : IAction, IWaitable
    {
        public AudioClip clip;
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
        public bool waitEnd = false;
        public bool WaitEnd { get => waitEnd; set => waitEnd = value; }

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
            await UniTask.Delay(TimeSpan.FromSeconds(clip.length), ignoreTimeScale: true);
        }
    }

    [Serializable]
    public class Await : IAction, IWaitable
    {
        public float seconds;
        public bool waitEnd = false;
        public bool WaitEnd { get => waitEnd; set => waitEnd = value; }

        public async UniTask Resolve()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(seconds), ignoreTimeScale: false);
        }
    }

    public enum TweenType { PunchScale, PunchRotation }

    [Serializable]
    public class Tween : IAction, IWaitable
    {
        public TweenType type;
        public Vector3 punch;
        public Transform targetTransform;
        public float duration, elasticity;
        public int vibrato;
        public bool waitEnd = false;
        public bool WaitEnd { get => waitEnd; set => waitEnd = value; }

        [Button()]
        public void TargetMyself()
        {
            targetTransform = Selection.activeGameObject.transform;
        }

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
            await UniTask.Delay(TimeSpan.FromSeconds(duration));
        }
    }

    [Serializable]
    public class AddItem : IAction
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
    public class ModifyTransform : IAction
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
            Debug.Log("menudo marron");
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
    public class ModifyMaterial : IAction, IWaitable
    {
        public MeshRenderer targetRenderer;
        public Color targetColor;
        public float transitionTime;
        [Tooltip("Set a new material. Leave it null if you want to keep the existing material")]
        public Material newMaterial;
        public bool waitEnd = false;
        public bool WaitEnd { get => waitEnd; set => waitEnd = value; }

        [Button()]
        public void TargetMyself()
        {
            targetRenderer = Selection.activeGameObject.GetComponent<MeshRenderer>();
        }

        public async UniTask Resolve()
        {
            if (newMaterial) targetRenderer.material = newMaterial;
            var material = targetRenderer.material;
            if (transitionTime > 0f)
            {
                var startTime = Time.time;
                var startColor = material.color;
                while (startTime + transitionTime > Time.time)
                {
                    var t = (Time.time - startTime) / transitionTime;
                    material.color = Color.Lerp(startColor, targetColor, t);
                    await UniTask.DelayFrame(1);
                }
            }
            material.color = targetColor;
        }
    }

}

