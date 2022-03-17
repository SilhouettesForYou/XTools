using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace XTools
{
    public class JsonDataModuleList : SerializedScriptableObject
    {
        [Title("All Json")]
        [ListDrawerSettings(ListElementLabelName = "mainTableName", DraggableItems = true, ShowIndexLabels = true)]
        public List<JsonDataModule> jsonList = new List<JsonDataModule>();
    }
}
