using System;
using System.Collections.Generic;
using System.Numerics;

[Serializable]
public class WaveData
{
    public int waveIndex;
    public bool isMultiMap;
    public List<MapInfo> maps;
    public bool isRandomEnemy;
    public List<EnemyInfo> enemies;
    public MissionInfo mission;
    public bool isRandomEvent;
    public List<EventInfo> events;
}

[Serializable]
public class MapInfo
{
    public string file;
    public float duration;
}

[Serializable]
public class EnemyInfo
{
    public EnemyType type;
    public int count;
    public List<Vector2Data> spawnPoints;
    public float spawnDelay;
}
[Serializable]
public class Vector2Data
{
    public int x;
    public int y;
}

[Serializable]
public class MissionInfo
{
    public string type;
    public float time;
    public int count;
}

[Serializable]
public class EventInfo
{
    public string type;
    public float time;
}
