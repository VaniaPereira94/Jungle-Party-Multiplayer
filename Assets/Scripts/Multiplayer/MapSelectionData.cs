using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Data/MapListScriptable", fileName = "MapListScriptable")]
public class MapListScriptable : ScriptableObject
{
    public List<MapInfo> Maps;
}

[Serializable]
public struct MapInfo
{
    public string MapName;
    public string SceneName;
    public Sprite coverName;
}