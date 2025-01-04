using UnityEngine;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    public GameObject itemPrefab; // Prefab do item a ser spawnado
    public int maxItems = 10; // Número máximo de itens na área
    public float spawnInterval = 5f; // Intervalo entre spawns
    private BoxCollider spawnArea; // Área de spawn

    private void Start()
    {
   
        spawnArea = GetComponent<BoxCollider>();
        if (!IsServer)
    {
        StartCoroutine(SpawnItemsRoutine());
    }
    }
    private void Update()
{
    if (Input.GetKeyDown(KeyCode.P)) // Pressione P para spawnar manualmente
    {
        Debug.Log("Spawn manual acionado.");
        SpawnItem();
    }
}

    private System.Collections.IEnumerator SpawnItemsRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (transform.childCount < maxItems)
            {
                SpawnItem();
            }
        }
    }

    private void SpawnItem()
    {
        Debug.Log("spawn");
        // Gera uma posição aleatória dentro do Box Collider
        Vector3 spawnPosition = GetRandomPositionInArea();

        // Instancia o objeto na posição gerada
        GameObject itemInstance = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        
        // Associa o objeto à rede
        NetworkObject networkObject = itemInstance.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn(); // Sincroniza com os clientes
        }

        // Organiza como filho do ItemSpawner (opcional, para organização na hierarquia)
        itemInstance.transform.parent = transform;
    }

    private Vector3 GetRandomPositionInArea()
    {
        Vector3 areaCenter = spawnArea.center + transform.position;
        Vector3 areaSize = spawnArea.size / 2;

        float randomX = Random.Range(-areaSize.x, areaSize.x);
        float randomY = Random.Range(-areaSize.y, areaSize.y);
        float randomZ = Random.Range(-areaSize.z, areaSize.z);

        return new Vector3(areaCenter.x + randomX, areaCenter.y + randomY, areaCenter.z + randomZ);
    }
}