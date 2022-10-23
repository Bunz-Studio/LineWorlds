using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ExternMaker
{
    public class ExtModTriggerDialog : MonoBehaviour
    {
        public Dropdown componentList;
        public GameObject triggerInstance;

        private void Awake()
        {
            ReadDropdownList();
        }

        public void Create()
        {
            var selectedType = componentList.value > -1 ? componentList.options[componentList.value].text : null;
            if (string.IsNullOrWhiteSpace(selectedType)) return;
            CreateTrigger(ExtCompiler.TryGetType(selectedType));
        }

        public object CreateTrigger(Type type)
        {
            var obj = ExtCore.instance.CreateObject(triggerInstance);
            var comp = obj.gameObject.AddComponent(type);
            var trig = (LineWorldsMod.ModTrigger)comp;
            trig.enabled = false;
            return comp;
        }

        public void ReadDropdownList()
        {
            componentList.ClearOptions();
            var list = new List<Dropdown.OptionData>();
            foreach(var type in ExtCompiler.modTriggerTypes)
            {
                var option = new Dropdown.OptionData(type.FullName);
                list.Add(option);
            }
            componentList.AddOptions(list);
        }
    }
}