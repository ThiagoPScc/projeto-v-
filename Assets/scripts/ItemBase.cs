using UnityEngine;
using Unity.Netcode;

public class Item : NetworkBehaviour
{
    private int idItem = 0;// Será usado para identificar o item.
    private string nome = "";//Nome do item.
    public int raridade = 1; // é a raridade to item, quanto maior a raridade, (essa raridade vem com as semanas sobrevividas)
    public int valor = 0; // valor do item
    public int tipoDeItem = 0;//ira definir o tipo do item, dependendo do tipo de item ele será utilizavael ou terá alteração no valor;
    private bool isPlayerNearby = false; // Se o jogador está próximo
    private GameObject currentPlayer; // referência ao jogador interagindo
    private Transform originalParent; // Guarda o pai original do item
    private NetworkVariable<ulong> holderId = new NetworkVariable<ulong>(ulong.MaxValue); // Valor inicial inválido

    private void Start()
    {   
        //raridade = radidade + mundo.semana;
        //aqui será decidido qual tipo do item. Tipos de items: ferramenta: 1 / consumivel:2  / sucata:3 /bugiganga:4 / jóias:5/itens miticos(itens que teram 1 spawn por noite)
        //aqui sera uma função que ira pegar o valor da semana e ira somar ao valor de raridade e tambem o tipo de item
        DefineTipoDeItem();
        calculaValorItem();

        originalParent = transform.parent; // Salva o pai original
        
    }

    private void DefineTipoDeItem()
    {
        // Gera um número aleatório de 1 a 100
        int randomValue = Random.Range(1, 101);

        // Atribui o tipo do item com base nas probabilidades
        if (randomValue <= 5) // 5% para ferramenta
        {
            tipoDeItem = 1;
        }
        else if (randomValue <= 15) // 10% para consumível (5% anterior + 10%)
        {
            tipoDeItem = 2;
        }
        else if (randomValue <= 60) // 45% para sucata (15% anterior + 45%)
        {
            tipoDeItem = 3;
        }
        else if (randomValue <= 85) // 25% para bugiganga (60% anterior + 25%)
        {
            tipoDeItem = 4;
        }
        else if (randomValue <= 95) // 10% para jóia (85% anterior + 10%)
        {
            tipoDeItem = 5;
        }
        else // 5% restante para item mítico
        {
            tipoDeItem = 6;
        }
    }
    private int calculaValorItem()
    {
        int minValor = 0;
        int maxValor = 0;
        int iValor = 0;

        switch (tipoDeItem)
            {
                case 1: // Ferramenta
                    maxValor = 10 + raridade;
                    minValor = 1;
                    break;
                case 2: // Consumível
                    maxValor = 20 + raridade;
                    minValor = 1;
                    break;
                case 3: // Sucata
                    iValor = (raridade * tipoDeItem) + 20;
                    maxValor = iValor + 30;
                    minValor = 30;
                    break;
                case 4: // Bugiganga
                    iValor = (raridade * tipoDeItem) + 30;
                    maxValor = iValor + 40;
                    minValor = 40;
                    break;
                case 5: // Joia
                    iValor = (raridade * 2 * tipoDeItem) + 50;
                    maxValor = iValor + 20;
                    minValor = 40;
                    break;
                case 6: // Item Mítico
                    iValor = (raridade * 4 * tipoDeItem) + 60;
                    maxValor = iValor + 20;
                    minValor = 80;
                    break;
                default:
                    Debug.LogWarning("Tipo de item desconhecido!");
                    valor = 1;
                    break;
            }
        valor = UnityEngine.Random.Range(minValor, maxValor);
        return UnityEngine.Random.Range(minValor, maxValor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            isPlayerNearby = true;
            currentPlayer = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            isPlayerNearby = false;
            currentPlayer = null;
        }
    }

  
     private void Update()  
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PlayerMovement PlayerMovement = currentPlayer.GetComponent<PlayerMovement>();
            if (PlayerMovement != null && PlayerMovement.CanPickupItem())
            {
                RequestPickupServerRpc();
                PlayerMovement.PickupItem(this); // Atualiza o item segurado no jogador
            }
            else
            {
                Debug.Log("Você já está segurando um item!");
            }
        }

        if (holderId.Value != ulong.MaxValue && NetworkManager.Singleton.ConnectedClients.ContainsKey(holderId.Value))
        {
            if (holderId.Value == NetworkManager.Singleton.LocalClientId)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    InteractWithItem();
                }

                if (Input.GetKeyDown(KeyCode.G))
                {
                    PlayerMovement PlayerMovement = currentPlayer.GetComponent<PlayerMovement>();
                    if (PlayerMovement != null)
                    {
                        PlayerMovement.DropItem(); // Solta o item
                    }
                }
            }
        
    
        }
          if (transform.position.y <= -2)
            {
                Vector3 newPosition = transform.position;
                newPosition.y = 2; 
                transform.position = newPosition;
            }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestPickupServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (holderId.Value == ulong.MaxValue) // Nenhum jogador está segurando
        {
            holderId.Value = clientId; // Atualiza o ID do jogador segurando o item
            ChangeOwnership(clientId); // Transfere a propriedade para o jogador
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropItemServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (holderId.Value == clientId)
        {
            holderId.Value = ulong.MaxValue; // Nenhum jogador está segurando
            transform.SetParent(originalParent); // Restaura o pai original
            ChangeOwnership(NetworkManager.ServerClientId); // Retorna a propriedade para o servidor
            
        }
    }

    private void ChangeOwnership(ulong newOwnerId)
    {
        NetworkObject.ChangeOwnership(newOwnerId);
    }

    private void InteractWithItem()
    {
        // Defina a lógica de interação com o item aqui
        Debug.Log("Interagiu com o item!");
    }

    private void LateUpdate()
    {
        if (NetworkManager.Singleton == null || 
            !NetworkManager.Singleton.ConnectedClients.ContainsKey(holderId.Value) ||
            NetworkManager.Singleton.ConnectedClients[holderId.Value].PlayerObject == null)
        {
            return; // Não faça nada se o NetworkManager ou o cliente não for válido
        }

        GameObject holder = NetworkManager.Singleton.ConnectedClients[holderId.Value].PlayerObject.gameObject;

        // Atualiza posição e rotação
        transform.position = holder.transform.position + holder.transform.forward * 1.5f; // Ajuste a posição
        transform.rotation = holder.transform.rotation; // Segue a rotação do jogador
  }
  }

