using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
public class botaoVender : NetworkBehaviour
{
    public NetworkObjectReference areaDeVenda;
    private bool isPlayerNearby = false;
    private GameObject currentPlayer = null;  
    

    // Update is called once per frame
   void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && IsOwner)
        {
            // Chama a interação no servidor
            InteractServerRpc();
        }
    }

    // Função chamada no servidor para executar a lógica
    [ServerRpc(RequireOwnership = false)]
    private void InteractServerRpc(ServerRpcParams rpcParams = default)
    {
        Debug.Log("apertado");
        if (areaDeVenda.TryGet(out NetworkObject networkObject))
        {
            // Executa a ação no objeto alvo
            var targetScript = networkObject.GetComponent<areaDeVenda>();
            if (targetScript != null)
            {
                
                targetScript.VenderItensClientRpc(); // Notifica os clientes
            }
        }
    }

    // Detecta a entrada do jogador na área do botão
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            isPlayerNearby = true;
            currentPlayer = other.gameObject;
            Debug.Log("Jogador próximo ao botão.");
        }
    }

    // Detecta a saída do jogador da área do botão
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            isPlayerNearby = false;
            currentPlayer = null;
            Debug.Log("Jogador saiu do alcance do botão.");
        }
    }
}