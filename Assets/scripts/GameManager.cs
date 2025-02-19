using UnityEngine;
using Unity.Netcode;
using System.Diagnostics;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance; // Singleton para acesso global

    public NetworkVariable<int> GlobalMoney = new NetworkVariable<int>(0);

    private void Awake()
    {
        // Garante que existe apenas um GameManager
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Garante que o objeto n�o ser� destru�do em mudan�as de cena
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddMoneyServerRpc(int amount)
    {
        GlobalMoney.Value += amount;
        UnityEngine.Debug.Log($"Dinheiro global atualizado: {GlobalMoney.Value}");
    }
}
