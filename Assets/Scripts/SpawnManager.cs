using UnityEngine;

// ============================================================
//  SpawnManager.cs — Hit Zero
//  ONDE COLOCAR: Em um GameObject vazio chamado "SpawnManager"
//                na raiz da cena.
//
//  O QUE FAZ:
//    - Spawna inimigos (Boffs) em posições aleatórias nas bordas
//    - Começa devagar e fica progressivamente mais rápido
//    - Para de spawnar quando o jogo encerra
//
//  COMO CALIBRAR A DIFICULDADE (tudo no Inspector):
//    - TaxaSpawnInicial:        tempo entre spawns no começo (ex: 2s)
//    - TaxaSpawnMinima:         menor intervalo possível (ex: 0.3s)
//    - ReducaoTaxaSpawn:        quanto reduz a cada ciclo (ex: 0.1s)
//    - IntervaloAumentoDificuldade: a cada quantos segundos fica mais difícil
// ============================================================

public class SpawnManager : MonoBehaviour
{
    // ----------------------------------------------------------
    // CONFIGURAÇÕES — editáveis no Inspector
    // ----------------------------------------------------------
    [Header("Inimigo")]
    [SerializeField] private GameObject prefabInimigo;   // Arraste o Prefab do Boff aqui

    [Header("Taxa de Spawn")]
    [Tooltip("Tempo (segundos) entre spawns no início da partida")]
    [SerializeField] private float taxaSpawnInicial = 2f;

    [Tooltip("Menor intervalo possível — limita o caos máximo")]
    [SerializeField] private float taxaSpawnMinima  = 0.3f;

    [Tooltip("Quanto o intervalo diminui a cada ciclo de dificuldade")]
    [SerializeField] private float reducaoTaxaSpawn = 0.15f;

    [Tooltip("A cada quantos segundos de sobrevivência a dificuldade aumenta")]
    [SerializeField] private float intervaloAumentoDificuldade = 8f;

    [Header("Dimensões da Arena")]
    [Tooltip("Metade da largura da arena — ajuste conforme o tamanho real")]
    [SerializeField] private float metadeLargura = 9f;

    [Tooltip("Metade da altura da arena — ajuste conforme o tamanho real")]
    [SerializeField] private float metadeAltura  = 5f;

    [Tooltip("Offset para spawnar levemente fora da borda visível")]
    [SerializeField] private float offsetBorda   = 0.5f;

    // ----------------------------------------------------------
    // ESTADO INTERNO
    // ----------------------------------------------------------
    private float taxaSpawnAtual;     // Taxa atual (diminui com o tempo)
    private float timerSpawn;         // Conta o tempo até próximo spawn
    private float timerDificuldade;   // Conta o tempo até próxima escalada

    // ----------------------------------------------------------
    // START — inicializar com os valores configurados
    // ----------------------------------------------------------
    void Start()
    {
        if (prefabInimigo == null)
        {
            Debug.LogError("SpawnManager: prefabInimigo não foi definido no Inspector!");
            return;
        }

        taxaSpawnAtual    = taxaSpawnInicial;
        timerSpawn        = taxaSpawnAtual;        // Primeiro spawn após 1 ciclo completo
        timerDificuldade  = intervaloAumentoDificuldade;
    }

    // ----------------------------------------------------------
    // UPDATE — controlar spawns e escalada de dificuldade
    // ----------------------------------------------------------
    void Update()
    {
        // Parar tudo se o jogo acabou
        if (GameManager.Instance.JogoEncerrado) return;

        GerenciarSpawn();
        GerenciarDificuldade();
    }

    // ----------------------------------------------------------
    // GERENCIAR O RITMO DE SPAWN
    // ----------------------------------------------------------
    private void GerenciarSpawn()
    {
        timerSpawn -= Time.deltaTime;

        if (timerSpawn <= 0f)
        {
            SpawnarInimigo();
            timerSpawn = taxaSpawnAtual; // Resetar o timer com a taxa atual
        }
    }

    // ----------------------------------------------------------
    // GERENCIAR A ESCALADA DE DIFICULDADE
    // ----------------------------------------------------------
    private void GerenciarDificuldade()
    {
        timerDificuldade -= Time.deltaTime;

        if (timerDificuldade <= 0f)
        {
            AumentarDificuldade();
            timerDificuldade = intervaloAumentoDificuldade;
        }
    }

    // ----------------------------------------------------------
    // SPAWNAR UM INIMIGO EM POSIÇÃO ALEATÓRIA NA BORDA
    // ----------------------------------------------------------
    private void SpawnarInimigo()
    {
        Vector2 posicaoSpawn = ObterPosicaoBorda();
        Instantiate(prefabInimigo, posicaoSpawn, Quaternion.identity);
    }

    // ----------------------------------------------------------
    // CALCULAR POSIÇÃO ALEATÓRIA NAS 4 BORDAS DA ARENA
    // ----------------------------------------------------------
    private Vector2 ObterPosicaoBorda()
    {
        // Sorteia uma das 4 bordas: 0=cima, 1=baixo, 2=esquerda, 3=direita
        int borda = Random.Range(0, 4);

        switch (borda)
        {
            case 0: // Borda de cima
                return new Vector2(
                    Random.Range(-metadeLargura, metadeLargura),
                    metadeAltura + offsetBorda
                );

            case 1: // Borda de baixo
                return new Vector2(
                    Random.Range(-metadeLargura, metadeLargura),
                    -metadeAltura - offsetBorda
                );

            case 2: // Borda esquerda
                return new Vector2(
                    -metadeLargura - offsetBorda,
                    Random.Range(-metadeAltura, metadeAltura)
                );

            default: // Borda direita
                return new Vector2(
                    metadeLargura + offsetBorda,
                    Random.Range(-metadeAltura, metadeAltura)
                );
        }
    }

    // AUMENTAR DIFICULDADE — reduzir o intervalo entre spawn
    private void AumentarDificuldade()
    {
        // Mathf.Max garante que não passe do limite mínimo configurado
        taxaSpawnAtual = Mathf.Max(taxaSpawnMinima, taxaSpawnAtual - reducaoTaxaSpawn);

        Debug.Log($"[SpawnManager] Dificuldade aumentou! " +
                  $"Novo intervalo: {taxaSpawnAtual:F2}s | " +
                  $"Tempo sobrevivido: {GameManager.Instance.TempoSobrevivencia:F0}s");
    }

    // ----------------------------------------------------------
    // GIZMOS — desenha a área da arena no Editor para referência visual
    // Visível apenas no Unity Editor, não no jogo final
    // ----------------------------------------------------------
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            Vector3.zero,
            new Vector3(metadeLargura * 2f, metadeAltura * 2f, 0f)
        );
    }
}
