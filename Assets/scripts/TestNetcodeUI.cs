using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetcodeUI : MonoBehaviour
{
   [SerializeField] private Button BntHost;
   [SerializeField] private Button BntClient;

   private void Awake(){
      BntHost.onClick.AddListener(()=>{
         NetworkManager.Singleton.StartHost();
         Hide();
      });
       BntClient.onClick.AddListener(()=>{
         NetworkManager.Singleton.StartClient();
         Hide();
      });
   }
   private void Hide(){
      gameObject.SetActive(false);
   }
}