using UnityEngine;
using Unity.Netcode;

public class Monetario : NetworkBehaviour
{
   public static Monetario Instance; // Singleton para fácil acesso

    [Header("Dinheiro")]
    public NetworkVariable<int> sharedMoney = new NetworkVariable<int>(0); // Sincronizado para todos os jogadores

    private void Awake()
    {
        // Garante que apenas um SharedMoneyManager exista
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(int amount)
    {
        if (IsServer) // Apenas o servidor deve alterar o valor
        {
            sharedMoney.Value += amount;
            Debug.Log($"Dinheiro compartilhado agora é: {sharedMoney.Value}");
        }
    }

    public void SpendMoney(int amount)
    {
        if (IsServer && sharedMoney.Value >= amount) // Apenas gasta se houver saldo suficiente
        {
            sharedMoney.Value -= amount;
            Debug.Log($"Dinheiro compartilhado agora é: {sharedMoney.Value}");
        }
        else
        {
            Debug.Log("Dinheiro insuficiente!");
        }
    }
}
