using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGSystem
{
    [Serializable]
    public class PageEvent
    {
        [PreviewField(50, ObjectFieldAlignment.Center)]
        public Sprite sprite;
        [GUIColor(0, 1, 1)]
        public VariableTableCondition conditions;
        [GUIColor(1, 1, 0)]
        [ListDrawerSettings(Expanded = true)]
        [SerializeReference]
        public List<RPGAction> actionList = new();
        [ShowIf("@this.actionList.Count > 0")]
        public TriggerType trigger = TriggerType.Autorun;
        [ShowIf("@this.actionList.Count > 0 && trigger == TriggerType.Autorun")]
        public bool isLoop;
        [ShowIf("@this.actionList.Count > 0")]
        public FreezeType freezePlayerAtRun;
        [Space(25)]
        public AudioClip playSFXOnEnabled;
        [ShowIf("@playSFXOnEnabled != null")]
        public SoundOptions soundOptions = new SoundOptions()
        {
            isLoop = false,
            volume = 1f,
            spatialBlend = 1f,
            stereoPan = 0f,
            pitch = 1f
        };
        bool isResolvingActionList = false;

        public async UniTaskVoid ResolveActionList(CancellationToken cts)
        {
            if (isResolvingActionList) return;
            isResolvingActionList = true;
            DoFreezeWhile();
            do
            {
                for (var x = 0; x < actionList.Count; x++)
                {
                    var action = actionList[x];
                    await action.Resolve().AttachExternalCancellation(cts);
                }
                await UniTask.Yield();
            } while (isLoop && conditions.IsAllConditionOK());
            UnfreezeWhile();
            isResolvingActionList = false;
        }

        void DoFreezeWhile()
        {
            switch (freezePlayerAtRun)
            {
                case FreezeType.FreezeAll: RPGManager.isInteractAvailable = RPGManager.isMovementAvailable = false; break;
                case FreezeType.FreezeInteraction: RPGManager.isInteractAvailable = false; break;
                case FreezeType.FreezeMovement: RPGManager.isMovementAvailable = false; break;
            }
        }

        void UnfreezeWhile()
        {
            RPGManager.isInteractAvailable = RPGManager.isMovementAvailable = true;
        }
    }
}