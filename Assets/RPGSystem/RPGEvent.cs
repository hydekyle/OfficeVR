using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace RPGSystem
{
    public class RPGEvent : MonoBehaviour
    {
        [OnValueChanged("OnValuePageChanged", true)]
        public List<PageEvent> pages = new();
        int activePageIndex = -1;
        List<int> _subscribedLocalVariableList = new();
        List<int> _subscribedSwitchList = new();
        List<int> _subscribedVariableList = new();
        SpriteRenderer spriteRenderer;

        void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void Start()
        {
            SubscribeToRequiredValueConditions();
            CheckAllPageCondition();
        }

        private void OnDestroy()
        {
            UnSubscribeToRequiredConditions();
        }

        void OnValidate()
        {
            foreach (var page in pages)
            {
                page.RPGEventParent = this;
                if (page.conditions != null) page.conditions.Refresh();
                if (page.actionList != null)
                    foreach (var action in page.actionList)
                    {
                        var actionType = action.GetType();
                        if (actionType == typeof(SetVariables))
                        {
                            SetVariables sv = (SetVariables)action;
                            sv.setVariables?.Refresh();
                        }
                        else if (actionType == typeof(CheckConditions))
                        {
                            CheckConditions sv = (CheckConditions)action;
                            sv.conditionList?.Refresh();
                        }
                    }
            }
        }

        // Called when the component is added for first time
        void Reset()
        {
            pages.Add(new PageEvent()
            {
                sprite = TryGetComponent<SpriteRenderer>(out SpriteRenderer s) ? s.sprite : null
            });
        }

        void OnValuePageChanged()
        {
            UIShowSprite();
            UIShowBoxCollider();
        }

        void UIShowSprite()
        {
            for (var x = 0; x < pages.Count; x++)
            {
                if (pages[x].sprite != null)
                {
                    if (TryGetComponent<SpriteRenderer>(out SpriteRenderer spriteRenderer))
                    {
                        spriteRenderer.sprite = pages[x].sprite;
                        spriteRenderer.sortingOrder = 3;
                    }
                    else
                    {
                        try
                        {
                            var newRenderer = gameObject.AddComponent<SpriteRenderer>();
                            newRenderer.sprite = pages[x].sprite;
                            newRenderer.sortingLayerName = "Outside";
                        }
                        catch
                        {
                            Debug.LogError("Remove MeshFilter and MeshRenderer from the GameObject first in order to add a Sprite");
                        }
                    }
                    return;
                }
            }
            // If pages has no sprite
            DestroyImmediate(GetComponent(typeof(SpriteRenderer)));
        }

        void UIShowBoxCollider2D()
        {
            if (pages.Exists(page => page.trigger == TriggerType.PlayerInteraction || page.trigger == TriggerType.PlayerTouch))
            {
                if (!TryGetComponent<BoxCollider2D>(out BoxCollider2D boxCollider))
                {
                    var newCollider = gameObject.AddComponent<BoxCollider2D>();
                    newCollider.isTrigger = true;
                }
            }
            else
            {
                if (TryGetComponent<BoxCollider2D>(out BoxCollider2D boxCollider))
                    if (boxCollider.isTrigger) DestroyImmediate(boxCollider);
            }
        }

        void UIShowBoxCollider()
        {
            if (pages.Exists(page => page.trigger == TriggerType.PlayerInteraction || page.trigger == TriggerType.PlayerTouch))
            {
                if (!TryGetComponent<MeshCollider>(out MeshCollider boxCollider))
                {
                    var newCollider = gameObject.AddComponent<MeshCollider>();
                    newCollider.convex = true;
                    newCollider.isTrigger = true;
                }
            }
            else
            {
                if (TryGetComponent<MeshCollider>(out MeshCollider boxCollider))
                    if (boxCollider.isTrigger) DestroyImmediate(boxCollider);
            }
        }

        public PageEvent GetActivePage()
        {
            return pages[activePageIndex];
        }

        void ApplyPage(int pageIndex)
        {
            var page = pages[pageIndex];
            if (spriteRenderer) spriteRenderer.sprite = page.sprite;
            if (pageIndex != activePageIndex)
            {
                if (page.trigger == TriggerType.Autorun && page.actionList.Count > 0) page.ResolveActionList(this.GetCancellationTokenOnDestroy()).Forget();
                activePageIndex = pageIndex;
                gameObject.SetActive(true);
                if (page.playSFXOnEnabled) RPGManager.AudioManager.PlaySound(page.playSFXOnEnabled, page.soundOptions, gameObject);
            }
        }

        public void TriggerPageActionList()
        {
            if (activePageIndex == -1) return;
            var page = GetActivePage();
            if (page.conditions.IsAllConditionOK()) page.ResolveActionList(this.GetCancellationTokenOnDestroy()).Forget();
        }

        // Called every time a required switch or variable changes the value
        void CheckAllPageCondition()
        {
            for (var x = pages.Count - 1; x >= 0; x--)
            {
                var page = pages[x];
                var isAllOK = page.conditions.IsAllConditionOK();
                if (isAllOK && activePageIndex == x) return;
                else if (isAllOK)
                {
                    ApplyPage(x);
                    return;
                }
            }
            gameObject.SetActive(false);
            activePageIndex = -1;
        }

        void SubscribeToRequiredValueConditions()
        {
            foreach (var page in pages) page.conditions.SubscribeToConditionTable(ref _subscribedSwitchList, ref _subscribedVariableList, ref _subscribedLocalVariableList, CheckAllPageCondition);
        }

        void UnSubscribeToRequiredConditions()
        {
            foreach (var page in pages) page.conditions.UnsubscribeConditionTable(ref _subscribedSwitchList, ref _subscribedVariableList, ref _subscribedLocalVariableList, CheckAllPageCondition);
        }
    }

}