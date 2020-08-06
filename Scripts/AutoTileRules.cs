using Archi.Core.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ZeroVector.Common.Reorderable;
namespace Archi.Core.Objects
{
    [System.Serializable]
    public class TileDictionary : ReorderableDictionary<TileBitMask, AutoTileData, TileDictionary.KeyValuePair> {
        public override TileBitMask DeduplicateKey(TileBitMask duplicateKey)
        {
            return TileBitMask.None;
        }

        [System.Serializable]
        public new class KeyValuePair : ReorderableDictionary<TileBitMask, AutoTileData, KeyValuePair>.KeyValuePair { }
    }

    [System.Serializable]
    public class AutoTileDataList : ReorderableList<AutoTileData> { }

    [CreateAssetMenu(fileName = "AutoTileRules", menuName = "Archi/Auto tile rules", order = 1)]
    public class AutoTileRules : ScriptableObject
    {

        [Reorderable(paginate = true, pageSize = 0, elementNameProperty = "Value")]
        public TileDictionary autoTileRulesDictionary=new TileDictionary();

        [Tooltip("The default tile that will be placed if no rule can be found. THIS SHOULD NOT BE THE SAME AS YOUR 'MAIN' TILE (the one that contains the rule info)")]
        public AutoTileData defaultTile;
    }
}