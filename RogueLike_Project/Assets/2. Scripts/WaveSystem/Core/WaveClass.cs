using System;
using System.Collections.Generic;
using System.Numerics;



[Serializable]
public class WaveData
{
    public int waveIndex;
    public bool isMultiMap;
    public int wallHeight;
    public bool isRepeating=false;
    public List<MapInfo> maps;
    public bool isRandomEnemy=false;
    public bool isRandomPos=false;
    public List<EnemyInfo> enemies;
    public MissionInfo mission;
    public bool isRandomEvent;
    public List<EventInfo> events;
    public DefaultEvents defaultEvents;
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
    public float firstDelay=0;
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
    public string type; //Killing, Boss, Survive, Capture, Item

    //óġor������
    public int count;

    //����or�������ɽð�or������ȹ��ҿ�ð�
    public float time;

    //����
    public Vector2Data footholdPoint;
    public float footholdHeight;
    public Vector2Data footholdSize;

    //������
    public List<Vector2Data> itemPoints;
}

[Serializable]
public class EventInfo
{
    public string type;
    public int repeat;
    public float delay;
    public float startDelay = 0;
    public int count;
}

[Serializable]
public class DefaultEvents
{
    public bool enabled = true;
    public bool skipCapture = true;
    public bool skipItem = true;
    public float spikeChance = 0.5f;
    public float sinkHoleChance = 0.5f;
    public EventInfo spike = new EventInfo();
    public EventInfo sinkHole = new EventInfo();
}
