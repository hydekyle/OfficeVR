using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityObservables;

namespace RPGSystem
{
    public enum CollisionType { player, other, any }
    public enum FaceDirection { North, West, East, South }
    public enum Conditionality { Equals, GreaterThan, LessThan }
    public enum VariableSetType { Set, Add, Sub, Multiply, Random }
    public enum TriggerType { PlayerInteraction, PlayerTouch, Autorun }
    public enum FreezeType { None, FreezeMovement, FreezeInteraction, FreezeAll }

    [Serializable]
    public struct SoundOptions
    {
        public bool keepPlayingWhenDisabled;
        public bool soundLoop;
        [Range(0f, 1f)]
        public float volume;
        [Tooltip("0 -> 2D Sound (global)\n1 -> 3D Sound (from position)")]
        [Range(0f, 1f)]
        public float spatialBlend;
        [Range(-1f, 1f)]
        public float stereoPan;
        [Range(-3f, 3f)]
        public float pitch;
    }

    [Serializable]
    public class VariableTableCondition
    {
        [TableList]
        public List<UISwitch> switchTable = new();
        [Space]
        [TableList]
        public List<UIVariableCondition> variableTable = new();
        [Space]
        [TableList]
        public List<UILocalVariableCondition> localVariableTable = new();

        /// <summary>Refresh variable names if they have changed in .txt</summary>
        public void Refresh()
        {
            if (switchTable.Count > 0)
            {
                var switchLineList = new List<string>();
                foreach (var line in UIReadSwitchesFromTXT())
                {
                    switchLineList.Add(line);
                }
                foreach (var sw in switchTable)
                {
                    if (sw.switchID == null || sw.switchID == "") return;
                    var ID = sw.switchID[..4];
                    var txtID = switchLineList[int.Parse(ID)];
                    if (txtID != sw.switchID)
                    {
                        sw.switchID = txtID;
                    }
                }
            }

            if (variableTable.Count > 0)
            {
                var variableLineList = new List<string>();
                foreach (var line in UIReadVariablesFromTXT())
                {
                    variableLineList.Add(line);
                }
                foreach (var vr in variableTable)
                {
                    if (vr.variableID == null) return;
                    var ID = vr.variableID[..4];
                    var txtID = variableLineList[int.Parse(ID)];
                    if (txtID != vr.variableID)
                    {
                        vr.variableID = txtID;
                    }
                }
            }
        }

        IEnumerable<string> UIReadSwitchesFromTXT()
        {
            var path = Application.dataPath + "/RPGSystem/Editor/switches.txt";
            var dataLines = File.ReadAllLines(path);

            foreach (var line in dataLines)
            {
                yield return line;
            }
        }

        IEnumerable<string> UIReadVariablesFromTXT()
        {
            var path = Application.dataPath + "/RPGSystem/Editor/variables.txt"; ;
            var dataLines = File.ReadAllLines(path);

            foreach (var line in dataLines)
            {
                yield return line;
            }
        }

        public void SubscribeToConditionTable(ref List<int> _subscribedSwitchList, ref List<int> _subscribedVariableList, ref List<int> _subscribedLocalVariableList, Action action)
        {
            foreach (var s in switchTable)
            {
                var ID = s.ID();
                if (_subscribedSwitchList.Contains(ID)) continue; // Avoiding resubscription
                RPGManager.GameData.SubscribeToSwitchChangedEvent(ID, action);
                _subscribedSwitchList.Add(ID);
            }
            foreach (var v in variableTable)
            {
                var ID = v.ID();
                if (_subscribedVariableList.Contains(ID)) continue;
                RPGManager.GameData.SubscribeToVariableChangedEvent(ID, action);
                _subscribedVariableList.Add(ID);
            }
            foreach (var lv in localVariableTable)
            {
                var ID = lv.ID();
                if (!_subscribedLocalVariableList.Contains(ID))
                {
                    _subscribedLocalVariableList.Add(ID);
                    RPGManager.GameData.SubscribeToLocalVariableChangedEvent(ID, action);
                }
            }
        }

        public void UnsubscribeConditionTable(ref List<int> _subscribedSwitchList, ref List<int> _subscribedVariableList, ref List<int> _subscribedLocalVariableList, Action action)
        {
            foreach (var id in _subscribedLocalVariableList) RPGManager.GameData.UnsubscribeToLocalVariableChangedEvent(id, action);
            foreach (var id in _subscribedSwitchList) RPGManager.GameData.UnsubscribeToSwitchChangedEvent(id, action);
            foreach (var id in _subscribedVariableList) RPGManager.GameData.UnsubscribeToVariableChangedEvent(id, action);
            _subscribedSwitchList.Clear();
            _subscribedVariableList.Clear();
        }

        public bool IsAllConditionOK()
        {
            foreach (var requiredLocalVariable in localVariableTable)
            {
                var variableValue = RPGManager.GameData.GetLocalVariable(requiredLocalVariable.ID());
                switch (requiredLocalVariable.conditionality)
                {
                    case Conditionality.Equals: if (requiredLocalVariable.value == variableValue) continue; break;
                    case Conditionality.GreaterThan: if (requiredLocalVariable.value > variableValue) continue; break;
                    case Conditionality.LessThan: if (requiredLocalVariable.value < variableValue) continue; break;
                }
                return false;
            }
            foreach (var requiredSwitch in switchTable)
            {
                var switchValue = RPGManager.GameData.GetSwitch(requiredSwitch.ID());
                if (requiredSwitch.value != switchValue) return false;
            }
            foreach (var requiredVariable in variableTable)
            {
                var variableValue = RPGManager.GameData.GetVariable(requiredVariable.ID());
                switch (requiredVariable.conditionality)
                {
                    case Conditionality.Equals: if (requiredVariable.value == variableValue) continue; break;
                    case Conditionality.GreaterThan: if (requiredVariable.value < variableValue) continue; break;
                    case Conditionality.LessThan: if (requiredVariable.value > variableValue) continue; break;
                }
                return false;
            }
            return true;
        }
    }

    [Serializable]
    public class VariableTableSet
    {
        [TableList]
        public List<UISwitch> switchTable = new();
        [Space]
        [TableList]
        public List<UIVariableSet> setVariableTable = new();
        [Space]
        [TableList]
        public List<UILocalVariableSet> setLocalVariableTable = new();

        /// <summary>Refresh variable names if they have changed in .txt</summary>
        public void Refresh()
        {
            if (switchTable.Count > 0)
            {
                var switchLineList = new List<string>();
                foreach (var line in UIReadSwitchesFromTXT())
                {
                    switchLineList.Add(line);
                }
                foreach (var sw in switchTable)
                {
                    if (sw.switchID == null || sw.switchID == "") return;
                    var ID = sw.switchID[..4];
                    var txtID = switchLineList[int.Parse(ID)];
                    if (txtID != sw.switchID)
                    {
                        sw.switchID = txtID;
                    }
                }
            }

            if (setVariableTable.Count > 0)
            {
                var variableLineList = new List<string>();
                foreach (var line in UIReadVariablesFromTXT())
                {
                    variableLineList.Add(line);
                }
                foreach (var vr in setVariableTable)
                {
                    if (vr.variableID == null) return;
                    var ID = vr.variableID[..4];
                    var txtID = variableLineList[int.Parse(ID)];
                    if (txtID != vr.variableID)
                    {
                        vr.variableID = txtID;
                    }
                }
            }
        }

        IEnumerable<string> UIReadSwitchesFromTXT()
        {
            var path = Application.dataPath + "/RPGSystem/Editor/switches.txt";
            var dataLines = File.ReadAllLines(path);

            foreach (var line in dataLines)
            {
                yield return line;
            }
        }

        IEnumerable<string> UIReadVariablesFromTXT()
        {
            var path = Application.dataPath + "/RPGSystem/Editor/variables.txt"; ;
            var dataLines = File.ReadAllLines(path);

            foreach (var line in dataLines)
            {
                yield return line;
            }
        }
    }

    [Serializable]
    public class SwitchCondition
    {
        public string name;
        public bool value;

        public int ID()
        {
            return int.Parse(name.Substring(0, 4));
        }
    }

    [Serializable]
    public class VariableCondition
    {
        public string name;
        public int value;
        public Conditionality conditionality;

        public int ID()
        {
            return int.Parse(name.Substring(0, 4));
        }
    }

    [Serializable] public class SwitchDictionary : UnitySerializedDictionary<int, Observable<bool>> { }
    [Serializable] public class VariableDictionary : UnitySerializedDictionary<int, Observable<int>> { }
    [Serializable] public class LocalVariableDictionary : UnitySerializedDictionary<int, Observable<int>> { }
    [Serializable] public class Inventory : UnitySerializedDictionary<ScriptableItem, int> { }
    public abstract class UnitySerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        // This class is required for Odin to serialize dictionaries
        [SerializeField, HideInInspector]
        private List<TKey> keyData = new List<TKey>();

        [SerializeField, HideInInspector]
        private List<TValue> valueData = new List<TValue>();

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++)
            {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (var item in this)
            {
                this.keyData.Add(item.Key);
                this.valueData.Add(item.Value);
            }
        }

    }

}