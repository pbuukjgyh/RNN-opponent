using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaManager : MonoBehaviour
{
    public static AreaManager Instance { get; private set; }
    public List<BoxCollider> areas;
    private Dictionary<BoxCollider, int> areaVisitCounts = new Dictionary<BoxCollider, int>();
    private Dictionary<GameObject, BoxCollider> entityCurrentAreas = new Dictionary<GameObject, BoxCollider>(); //tracks current areas for entities
    private BoxCollider _lastPlayerArea { get; set; }

    void Start()
    {
        foreach (var area in areas)
        {
            areaVisitCounts[area] = 0;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
    }

    public BoxCollider GetLastPlayerArea()
    {
        return _lastPlayerArea;
    }

    public int GetAreaIndex(GameObject entity)
    {
        BoxCollider currentArea = GetEntityCurrentArea(entity);
        if (currentArea != null)
        {
            return areas.IndexOf(currentArea); // Returns the index of the area
        }
        return -1; // Not in any area
    }

    public void UpdateEntityArea(GameObject entity)
    {
        BoxCollider currentArea = GetEntityCurrentArea(entity);

        if (currentArea != null)
        {
            if (!entityCurrentAreas.ContainsKey(entity) || entityCurrentAreas[entity] != currentArea)
            {
                entityCurrentAreas[entity] = currentArea;

                // Increment visit count only for the player
                if (entity.CompareTag("Player"))
                {
                    _lastPlayerArea = currentArea;
                    areaVisitCounts[currentArea]++;
                }
            }
        }
        else
        {
            entityCurrentAreas.Remove(entity); // No longer in any area
        }
    }

    private BoxCollider GetEntityCurrentArea(GameObject entity)
    {
        foreach (var area in areas)
        {
            if (area.bounds.Contains(entity.transform.position))
            {
                return area;
            }
        }
        return null;
    }

    public int GetVisitCount(BoxCollider area)
    {
        return areaVisitCounts.ContainsKey(area) ? areaVisitCounts[area] : 0;
    }

    public BoxCollider GetMostVisitedArea()
    {
        BoxCollider mostVisited = null;
        int maxVisits = 0;

        foreach (var kvp in areaVisitCounts)
        {
            if (kvp.Value > maxVisits)
            {
                maxVisits = kvp.Value;
                mostVisited = kvp.Key;
            }
        }

        return mostVisited;
    }
}
