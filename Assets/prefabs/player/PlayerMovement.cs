using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 4f; // Velocidade padrão
    public float sprintSpeed = 8f; // Velocidade ao correr
    public int stamina = 1000; // Estamina inicial
    public float lookSpeed = 2f;
    public float jumpForce = 5f;
    public float gravity = -12f;
    private bool isSprinting = false; // Jogador está correndo
    private float staminaCooldown = 8f; // Tempo para começar a regenerar estamina
    private float lastSprintTime; // Última vez que o jogador correu
    public Item currentItem;

    private CharacterController characterController;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        playerCamera.transform.localPosition = new Vector3(0, 0.5f, 0);

        // Trava o cursor no centro da tela e o torna invisível
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Desativa a câmera para jogadores não locais
        if (!IsOwner)
        {
            playerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleCamera();

        // Permite desbloquear o cursor com a tecla Esc para teste ou menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Opcional: Rebloqueia o cursor com a tecla F
        if (Input.GetKeyDown(KeyCode.F))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void HandleMovement()
    {
        // Checa se está no chão
        isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Mantém o player no chão
        }

        // Controle de corrida
            if (Input.GetKey(KeyCode.LeftShift) && stamina > 0)
        {
            isSprinting = true;
            moveSpeed = sprintSpeed;
            stamina -= Mathf.Max(1, Mathf.FloorToInt(Time.deltaTime * 1)); // Diminui a estamina mais devagar
            if (stamina <= 0) stamina = 0;
            lastSprintTime = Time.time; // Atualiza o tempo da última corrida
            
        }
        else
        {
            isSprinting = false;
            moveSpeed = 5f;

            // Se o jogador parou de correr e passou o cooldown, começa a regenerar
            if (Time.time - lastSprintTime >= staminaCooldown && stamina < 3000)
            {
              
                
                stamina += Mathf.Max(1, Mathf.FloorToInt(Time.deltaTime - 2)); // Regenera a estamina mais devagar
                if (stamina >= 3000) stamina = 3000;
            }
        }

        // Input de movimento
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Pulo
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Aplica gravidade
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        // Input do mouse
        float mouseX = Input.GetAxis("Mouse X") * lookSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeed;

        // Rotação horizontal do player
        transform.Rotate(Vector3.up * mouseX);

        // Rotação vertical da câmera
        float currentXRotation = playerCamera.transform.localEulerAngles.x;
        currentXRotation = currentXRotation > 180 ? currentXRotation - 360 : currentXRotation;

        float newRotationX = Mathf.Clamp(currentXRotation - mouseY, -90f, 90f);
        playerCamera.transform.localRotation = Quaternion.Euler(newRotationX, 0f, 0f);
    }
    public bool CanPickupItem()
    {
        return currentItem == null; // Retorna true se o jogador não estiver segurando nenhum item
    }
     public void PickupItem(Item item)
    {
        currentItem = item; // Atualiza o item atualmente segurado
    }
    public void DropItem()
    {
        if (currentItem != null)
        {
            currentItem.DropItemServerRpc(); // Solta o item atual
            currentItem = null; // Reseta a referência
        }
    }
}
