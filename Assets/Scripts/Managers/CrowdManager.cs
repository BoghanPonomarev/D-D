using System.Collections.Generic;
using UnityEngine;

public class CrowdManager : MonoBehaviour
{
    [SerializeField] NPCData[] npcProfiles;
    [SerializeField] LanePath[] lanePaths;
    [SerializeField] int maxActiveNPCs = 20;
    [SerializeField] float spawnInterval = 3f;
    [SerializeField] float logicTickRate = 8f;

    private GameObject npcsObjectContainer;
    
    readonly List<NPCEntity> activeNPCs = new List<NPCEntity>();
    float spawnTimer;
    float tickTimer;
    float tickInterval;

    public void Initialize()
    {
        npcsObjectContainer = GameObject.Find("Characters Container");
        
        tickInterval = 1f / logicTickRate;
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        HandleSpawning();
        HandleLogicTick();
    }

    void HandleSpawning()
    {
        if (activeNPCs.Count >= maxActiveNPCs) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        spawnTimer = spawnInterval;
        TrySpawnNpc();
    }

    void HandleLogicTick()
    {
        tickTimer -= Time.deltaTime;
        if (tickTimer > 0f) return;

        tickTimer = tickInterval;

        for (int i = 0; i < activeNPCs.Count; i++)
            activeNPCs[i].Tick();
    }

    private void TrySpawnNpc()
    {
        if (npcProfiles == null || npcProfiles.Length == 0) return;
        if (lanePaths == null || lanePaths.Length == 0) return;

        NPCData profile = npcProfiles[Random.Range(0, npcProfiles.Length)];
        LanePath path = lanePaths[Random.Range(0, lanePaths.Length)];

        if (!profile || !profile.prefab || !path) return;
        if (path.WaypointCount < 2) return;

        GameObject obj = GameManager.Instance.Pool.Get(profile.archetypeId, profile.prefab, transform);
        NPCEntity entity = obj.GetComponent<NPCEntity>();

        if (!entity)
        {
            GameManager.Instance.Pool.Return(profile.archetypeId, obj);
            return;
        }

        entity.Initialize(profile, path, this);
        activeNPCs.Add(entity);
    }

    public void DespawnNPC(NPCEntity npc)
    {
        activeNPCs.Remove(npc);
        GameManager.Instance.Pool.Return(npc.Data.archetypeId, npc.gameObject);
    }
}
