using UnityEngine;

// ============================================================
//  PlayerController.cs — Hit Zero
//  ONDE COLOCAR: No GameObject do personagem principal.
//
//  COMPONENTES NECESSÁRIOS no mesmo GameObject:
//    - Rigidbody2D   (Body Type: Dynamic, Gravity Scale: 0)
//    - Collider2D    (CircleCollider2D ou CapsuleCollider2D)
//    - Sprite Renderer (com o sprite do personagem)
//
//  CONFIGURAR NO INSPECTOR:
//    - Projectile Prefab: arraste o prefab do projétil
//    - Shoot Point:       crie um objeto filho no personagem,
//                         posicionado na "ponta da arma"
//
//  O QUE FAZ:
//    - Movimento WASD (diagonal suportado)
//    - Rotação do sprite apontando para o cursor
//    - Tiro com clique esquerdo, projétil vai em direção ao cursor
//    - Dash com Shift (deslocamento rápido + cooldown de 5s)
// ============================================================

public class PlayerController : MonoBehaviour
{
    // ----------------------------------------------------------
    // CONFIGURAÇÕES DE MOVIMENTO
    // ----------------------------------------------------------
    [Header("Movimento")]
    [SerializeField] private float velocidade = 5f;

    // ----------------------------------------------------------
    // CONFIGURAÇÕES DO DASH
    // ----------------------------------------------------------
    [Header("Dash")]
    [SerializeField] private float velocidadeDash = 18f;  // Quão rápido é o dash
    [SerializeField] private float duracaoDash    = 0.12f; // Quantos segundos dura
    [SerializeField] private float cooldownDash   = 5f;   // Espera entre usos

    // ----------------------------------------------------------
    // CONFIGURAÇÕES DE TIRO
    // ----------------------------------------------------------
    [Header("Tiro")]
    [SerializeField] private GameObject prefabProjetil;   // Arraste o Prefab do projétil aqui
    [SerializeField] private Transform  pontoDisparo;     // Objeto filho na ponta da arma
    [SerializeField] private float      velocidadeProjetil = 15f;
    [SerializeField] private float      cooldownTiro = 0.5f;
    private float timerCooldownTiro = 0f;         

    // ----------------------------------------------------------
    // REFERÊNCIAS PRIVADAS — preenchidas no Start
    // ----------------------------------------------------------
    private Rigidbody2D rb;
    private Camera      cam;

    // ----------------------------------------------------------
    // ESTADO DO MOVIMENTO
    // ----------------------------------------------------------
    private Vector2 entradaMovimento;   // Direção do WASD atual

    // ----------------------------------------------------------
    // ESTADO DO DASH
    // ----------------------------------------------------------
    private bool    estaDashando        = false;
    private float   timerDashDuracao    = 0f;
    private float   timerCooldownDash   = 0f;
    private Vector2 direcaoDash;

    // Propriedade pública: o ScoreUI usa isso para desenhar o indicador
    // Retorna 0 quando pronto, 1 quando recém usado (cheio = em cooldown)
    public float CooldownDashNormalizado => timerCooldownDash / cooldownDash;
    public bool  DashPronto              => timerCooldownDash <= 0f;

    // ----------------------------------------------------------
    // START — inicializar referências
    // ----------------------------------------------------------
    void Start()
    {
        rb  = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        // Segurança: se não definiu ponto de disparo no Inspector,
        // usar a posição do próprio personagem
        if (pontoDisparo == null)
            pontoDisparo = transform;
    }

    // ----------------------------------------------------------
    // UPDATE — lógica de input (roda todo frame)
    // ----------------------------------------------------------
    void Update()
    {
        // Tudo para se o jogo acabou
        if (GameManager.Instance.JogoEncerrado) return;

        LerEntrada();
        RotacionarParaMouse();
        GerenciarCooldownDash();
        VerificarDisparo();
    }

    // ----------------------------------------------------------
    // FIXED UPDATE — movimentação física (frequência fixa)
    // ----------------------------------------------------------
    void FixedUpdate()
    {
        if (GameManager.Instance.JogoEncerrado) return;

        if (estaDashando)
            ExecutarDash();
        else
            Mover();
    }

    // ----------------------------------------------------------
    // LER ENTRADA DO TECLADO
    // ----------------------------------------------------------
    private void LerEntrada()
    {
        entradaMovimento.x = Input.GetAxisRaw("Horizontal"); // A/D ou ←/→
        entradaMovimento.y = Input.GetAxisRaw("Vertical");   // W/S ou ↑/↓

        // Normalizar: garante que diagonal não seja mais rápido que reto
        entradaMovimento = entradaMovimento.normalized;

        // Verificar se o jogador quer dar dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && DashPronto && !estaDashando)
            IniciarDash();
    }

    // ----------------------------------------------------------
    // MOVIMENTAÇÃO NORMAL
    // ----------------------------------------------------------
    private void Mover()
    {
        rb.MovePosition(rb.position + entradaMovimento * velocidade * Time.fixedDeltaTime);
    }

    // ----------------------------------------------------------
    // ROTACIONAR O SPRITE PARA APONTAR AO CURSOR
    // ----------------------------------------------------------
    private void RotacionarParaMouse()
    {
        // Converter posição do mouse (pixels) para posição no mundo 2D
        Vector3 posicaoMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direcao      = (posicaoMouse - transform.position).normalized;

        // Calcular o ângulo e aplicar como rotação Z
        // O -90 corrige se o sprite aponta para cima por padrão.
        // Se o seu sprite aponta para a direita, remova o -90.
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    // ----------------------------------------------------------
    // INICIAR O DASH
    // ----------------------------------------------------------
    private void IniciarDash()
    {
        // Só dasha se estiver se movendo; dash parado não faz sentido
        if (entradaMovimento == Vector2.zero) return;

        estaDashando     = true;
        direcaoDash      = entradaMovimento;
        timerDashDuracao = duracaoDash;
        timerCooldownDash = cooldownDash;
    }

    // ----------------------------------------------------------
    // EXECUTAR O DESLOCAMENTO DO DASH (chamado no FixedUpdate)
    // ----------------------------------------------------------
    private void ExecutarDash()
    {
        rb.MovePosition(rb.position + direcaoDash * velocidadeDash * Time.fixedDeltaTime);

        timerDashDuracao -= Time.fixedDeltaTime;
        if (timerDashDuracao <= 0f)
            estaDashando = false;
    }

    // ----------------------------------------------------------
    // GERENCIAR O COOLDOWN DO DASH (apenas contagem regressiva)
    // ----------------------------------------------------------
    private void GerenciarCooldownDash()
    {
        if (timerCooldownDash > 0f)
            timerCooldownDash -= Time.deltaTime;
    }

    // ----------------------------------------------------------
    // VERIFICAR SE O JOGADOR CLICOU PARA ATIRAR
    // ----------------------------------------------------------
    private void VerificarDisparo()
    {
        // Reduzir o cooldown a cada frame
        if (timerCooldownTiro > 0f)
            timerCooldownTiro -= Time.deltaTime;

        // Só atira se clicar E o cooldown estiver zerado
        if (Input.GetMouseButtonDown(0) && timerCooldownTiro <= 0f)
        {
            Atirar();
            timerCooldownTiro = cooldownTiro;
        }
    }

    // ----------------------------------------------------------
    // INSTANCIAR O PROJÉTIL NA DIREÇÃO DO CURSOR
    // ----------------------------------------------------------
    private void Atirar()
    {
        if (prefabProjetil == null)
        {
            Debug.LogWarning("PlayerController: prefabProjetil não foi definido no Inspector!");
            return;
        }

        // Calcular direção do ponto de disparo até o cursor
        Vector3 posicaoMouse = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direcao      = (posicaoMouse - pontoDisparo.position).normalized;

        // Criar o projétil no ponto de disparo com a rotação do jogador
        GameObject projetil = Instantiate(prefabProjetil, pontoDisparo.position, transform.rotation);

        // Dar velocidade ao projétil
        Rigidbody2D rbProjetil = projetil.GetComponent<Rigidbody2D>();
        if (rbProjetil != null)
            rbProjetil.linearVelocity = direcao * velocidadeProjetil;
    }
}
