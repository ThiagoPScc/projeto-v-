using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class areaDeVenda : NetworkBehaviour
{
    private List<Item> itensNaArea = new List<Item>(); // Referenciar a classe Item

    private void OnTriggerEnter(Collider other)
    {
        Item item = other.GetComponent<Item>(); // Usar a classe Item
        if (item != null)
        {
            itensNaArea.Add(item);
            Debug.Log($"Item {item.name} adicionado. Valor: {item.valor}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Item item = other.GetComponent<Item>(); // Usar a classe Item
        if (item != null && itensNaArea.Contains(item))
        {
            itensNaArea.Remove(item);
            Debug.Log($"Item {item.name} removido da área.");
        }
    }

    [ClientRpc]
    public void VenderItensClientRpc()
    {
        if (!IsOwner) return;

        int valorTotal = 0;

        foreach (Item item in itensNaArea) // Usar a classe Item
        {
            valorTotal += item.valor;
            Destroy(item.gameObject);
        }

        itensNaArea.Clear();
        Debug.Log($"Itens vendidos! Valor total: {valorTotal}");
    }
}
