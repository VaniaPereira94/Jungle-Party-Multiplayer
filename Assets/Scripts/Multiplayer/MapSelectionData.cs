using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace Multiplayer
{
    [CreateAssetMenu(menuName = "Data/MapSelectionData", fileName = "MapSelectionData")]
    public class MapSelectionData : ScriptableObject
    {
        public List<MapInfo> Maps;
    }
}

[Serializable]
public class MapInfo
{
    public string MapName;
    public string SceneName;
    public MonoScript LevelController;
}