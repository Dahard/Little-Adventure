using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : MonoBehaviour
{
    private List<SpawnPoint> spawnPointList;
    private List<Character> spawnedCharactersList;
    private bool hasSpawned;
    public Collider collider;
    public UnityEvent OnAllSpawnsEliminated;

    private void Awake()
    {
        var spawnPointArray = transform.parent.GetComponentsInChildren<SpawnPoint>();
        spawnPointList = new List<SpawnPoint>(spawnPointArray);
        spawnedCharactersList = new List<Character>();
    }

    private void Update()
    {

        if (!hasSpawned || spawnedCharactersList.Count == 0)
            return;

        bool allSpawnedAreDead = true;

        foreach (Character c in spawnedCharactersList)
        {
            if (c.CurrentState != Character.CharacterState.Dead)
            {
                allSpawnedAreDead = false;
                break;
            }
        }

        if (allSpawnedAreDead)
        {
            if (OnAllSpawnsEliminated != null)
            {
                OnAllSpawnsEliminated.Invoke();
            }

            spawnedCharactersList.Clear();
        }
        
    }

    public void SpawnCharacters()
    {
        if (hasSpawned)
        {
            return;
        }

        foreach (SpawnPoint spawnPoint in spawnPointList)
        {
            if(spawnPoint.EnemyToSpawn != null)
            {
                GameObject spawnedGameObject = Instantiate(spawnPoint.EnemyToSpawn, spawnPoint.transform.position, spawnPoint.transform.rotation); ;
                spawnedCharactersList.Add(spawnedGameObject.GetComponent<Character>());
            }
        }

        hasSpawned = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if( other.tag == "Player")
        {
            SpawnCharacters();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, collider.bounds.size);
    }
}
