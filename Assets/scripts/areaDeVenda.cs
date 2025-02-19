using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class areaDeVenda : NetworkBehaviour
{
    public List<GameObject> itensNaArea = new List<GameObject>(); // Lista de itens na área de venda
    public int precoPorItem = 10; // Preço fixo para cada item vendido

    // Função chamada para vender os itens
    [ServerRpc(RequireOwnership = false)]
    public void VenderItensServerRpc()
    {
        if (!IsServer) return;

        int dinheiroRecebido = 0;

        // Calcula o dinheiro total e destrói os itens
        foreach (GameObject item in itensNaArea)
        {
            if (item != null && item.TryGetComponent(out NetworkObject networkObject))
            {
                dinheiroRecebido += precoPorItem;
                networkObject.Despawn(true); // Remove o item do servidor
            }
        }

        itensNaArea.Clear(); // Limpa a lista após vender

        // Adiciona o dinheiro ao sistema global
        GameManager.Instance.AddMoneyServerRpc(dinheiroRecebido);
        Debug.Log($"Itens vendidos! Dinheiro recebido: {dinheiroRecebido}");
    }

    // Adiciona itens à lista quando eles entram na área de venda
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            itensNaArea.Add(other.gameObject);
            Debug.Log("Item adicionado à área de venda.");
        }
    }

    // Remove itens da lista se eles saem da área de venda
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            itensNaArea.Remove(other.gameObject);
            Debug.Log("Item removido da área de venda.");
        }
    }
}
