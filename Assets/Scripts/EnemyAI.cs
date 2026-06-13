using UnityEngine;

// ============================================================
//  EnemyAI.cs — Hit Zero
//  ONDE COLOCAR: No Prefab do inimigo (Boff).
//
//  COMPONENTES NECESSÁRIOS no prefab do inimigo:
//    - Rigidbody2D   (Body Type: Kinematic — não usa física, só collider)
//    - Collider2D    (CircleCollider2D, marcar "Is Trigger" = TRUE)
//    - Sprite Renderer
//
//  TAGS NECESSÁRIAS — configure em Edit > Tags and Layers:
//    - O personagem principal deve ter a Tag: "Player"
//    - O prefab do projétil deve ter a Tag:   "Projectile"
//
//  O QUE FAZ:
//    - Move em linha reta na direção do jogador a cada frame
//    - Ao tocar o jogador  → dispara Game Over
//    - Ao tocar o projétil → destroi ambos e avisa o GameManager
// ============================================================

public class EnemyAI : MonoBehaviour
{
    // ----------------------------------------------------------
    // CONFIGURAÇÕES — editáveis no Inspector
    // ----------------------------------------------------------
    [Header("Movimento")]
    [SerializeField] private float velocidade = 5f;

    // ----------------------------------------------------------
    // REFERÊNCIA AO JOGADOR — preenchida no Start
    // ----------------------------------------------------------
    private Transform transformJogador;

    // ----------------------------------------------------------
    // START — buscar o jogador na cena pelo Tag
    // ----------------------------------------------------------
    void Start()
    {
        // FindGameObjectWithTag é aceitável no Start (não no Update)
        GameObject jogador = GameObject.FindGameObjectWithTag("Player");

        if (jogador != null)
            transformJogador = jogador.transform;
        else
            Debug.LogError("EnemyAI: Nenhum objeto com a Tag 'Player' foi encontrado na cena!");
    }

    // ----------------------------------------------------------
    // UPDATE — perseguir o jogador todo frame
    // ----------------------------------------------------------
    void Update()
    {
        // Parar se o jogo acabou ou jogador não foi encontrado
        if (GameManager.Instance.JogoEncerrado) return;
        if (transformJogador == null) return;

        PerseguirJogador();
    }

    // ----------------------------------------------------------
    // MOVER EM DIREÇÃO AO JOGADOR
    // ----------------------------------------------------------
    private void PerseguirJogador()
    {
        // MoveTowards: desloca a posição atual em direção ao alvo
        // O terceiro parâmetro é a distância máxima por frame
        transform.position = Vector2.MoveTowards(
            transform.position,
            transformJogador.position,
            velocidade * Time.deltaTime
        );

        // Rotacionar o sprite para encarar o jogador enquanto persegue
        Vector2 direcao = (transformJogador.position - transform.position).normalized;
        float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }

    // ----------------------------------------------------------
    // DETECTAR COLISÕES (Is Trigger deve estar ATIVADO no Collider)
    // ----------------------------------------------------------
    private void OnTriggerEnter2D(Collider2D outro)
    {
        // ── Tocou no jogador → Game Over ──────────────────────
        if (outro.CompareTag("Player"))
        {
            GameManager.Instance.TriggerGameOver();

            // Opcional: destruir o inimigo que acertou para
            // evitar que ele continue visível na tela de Game Over
            // Destroy(gameObject);
            return;
        }

        // ── Tocou em um projétil → morreu ─────────────────────
        if (outro.CompareTag("Projectile"))
        {
            // Destruir o projétil
            Destroy(outro.gameObject);

            // Registrar a kill no GameManager (pontuação + combo)
            GameManager.Instance.AdicionarPonto();

            // Destruir este inimigo
            Morrer();
        }
    }

    // ----------------------------------------------------------
    // MORTE DO INIMIGO
    // ----------------------------------------------------------
    [Header("Efeitos")]
    [SerializeField] private GameObject efeitoMorte;
    private void Morrer()
    {
        if (efeitoMorte != null)
            Instantiate(efeitoMorte, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
