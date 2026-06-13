using UnityEngine;
using UnityEngine.SceneManagement;

// ============================================================
//  GameManager.cs — Hit Zero
//  ONDE COLOCAR: Em um GameObject vazio chamado "GameManager"
//                na raiz da cena. Não coloque dentro de outro.
//
//  O QUE FAZ: Controla tudo que é global no jogo:
//    - Pontuação e multiplicador de combo
//    - Timer de sobrevivência
//    - Estado do jogo (rodando / game over)
//    - Salvar e carregar o recorde (PlayerPrefs)
//    - Singleton: qualquer script acessa via GameManager.Instance
// ============================================================

public class GameManager : MonoBehaviour
{
    // ----------------------------------------------------------
    // SINGLETON
    // Garante que existe apenas UM GameManager em toda a cena.
    // Qualquer script pode acessar com: GameManager.Instance
    // ----------------------------------------------------------
    public static GameManager Instance { get; private set; }

    // ----------------------------------------------------------
    // CONFIGURAÇÕES — editáveis no Inspector da Unity
    // ----------------------------------------------------------
    [Header("Pontuação")]
    [SerializeField] private int pontosPorKill = 100;

    [Header("Combo")]
    [SerializeField] private float tempoResetCombo = 3f; // Segundos sem kill para zerar combo

    // ----------------------------------------------------------
    // PROPRIEDADES PÚBLICAS (leitura) — usadas pela UI e scripts
    // ----------------------------------------------------------
    public bool JogoEncerrado        { get; private set; } = false;
    public int  PontuacaoAtual       { get; private set; } = 0;
    public int  ComboAtual           { get; private set; } = 0;
    public float TimerCombo          { get; private set; } = 0f;
    public float TempoSobrevivencia  { get; private set; } = 0f;

    // Multiplicador: combo 1 = x1, combo 5 = x5, etc.
    // Mathf.Max garante mínimo de 1x mesmo com combo zerado
    public int Multiplicador => Mathf.Max(1, ComboAtual);

    // Recorde salvo no dispositivo do jogador
    private const string CHAVE_RECORDE = "HitZeroRecorde";
    public int Recorde => PlayerPrefs.GetInt(CHAVE_RECORDE, 0);

    // ----------------------------------------------------------
    // EVENTOS — a UI se inscreve aqui para atualizar a tela
    // Exemplo de uso no ScoreUI.cs:
    //   GameManager.Instance.OnPontuacaoAlterada += AtualizarTexto;
    // ----------------------------------------------------------
    public System.Action OnPontuacaoAlterada;
    public System.Action OnComboAlterado;
    public System.Action OnJogoEncerrado;

    // ----------------------------------------------------------
    // AWAKE — executado antes do Start, ideal para o Singleton
    // ----------------------------------------------------------
    void Awake()
    {
        // Se ainda não existe nenhum GameManager, este será o oficial
        if (Instance == null)
        {
            Instance = this;
            // Mantém o GameManager ao trocar de cena (útil para menus)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Se já existe um, este duplicado é destruído
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------------
    // UPDATE — roda todo frame enquanto o jogo está ativo
    // ----------------------------------------------------------
    void Update()
    {
        if (JogoEncerrado) return;

        // Incrementar o cronômetro de sobrevivência
        TempoSobrevivencia += Time.deltaTime;

        // Fazer a contagem regressiva do combo
        if (ComboAtual > 0)
        {
            TimerCombo -= Time.deltaTime;
            if (TimerCombo <= 0f)
            {
                ZerarCombo();
            }
        }
    }

    // ----------------------------------------------------------
    // MÉTODO PÚBLICO — chamado pelo EnemyAI quando inimigo morre
    // ----------------------------------------------------------
    public void AdicionarPonto()
    {
        // Incrementar combo e reiniciar o timer de reset
        ComboAtual++;
        TimerCombo = tempoResetCombo;

        // Calcular pontos com multiplicador
        int pontos = pontosPorKill * Multiplicador;
        PontuacaoAtual += pontos;

        // Avisar a UI que os valores mudaram
        OnPontuacaoAlterada?.Invoke();
        OnComboAlterado?.Invoke();
    }

    // ----------------------------------------------------------
    // MÉTODO PÚBLICO — chamado pelo EnemyAI quando toca o jogador
    // ----------------------------------------------------------
    public void TriggerGameOver()
    {
        // Evitar chamar duas vezes caso dois inimigos toquem ao mesmo tempo
        if (JogoEncerrado) return;

        JogoEncerrado = true;

        // Salvar recorde se a pontuação atual for maior
        if (PontuacaoAtual > Recorde)
        {
            PlayerPrefs.SetInt(CHAVE_RECORDE, PontuacaoAtual);
            PlayerPrefs.Save();
        }

        // Avisar a UI para mostrar a tela de Game Over
        OnJogoEncerrado?.Invoke();

        // Pausa o jogo (inimigos e spawner param via JogoEncerrado,
        // mas Time.timeScale = 0 paralisa físicas e animações também)
        Time.timeScale = 0f;
    }

    // ----------------------------------------------------------
    // MÉTODO PÚBLICO — chamado pelo botão "Reiniciar" na UI
    // ----------------------------------------------------------
    public void Reiniciar()
    {
        // Retomar o tempo antes de recarregar a cena
        Time.timeScale = 1f;

        // Resetar todos os valores
        JogoEncerrado       = false;
        PontuacaoAtual      = 0;
        ComboAtual          = 0;
        TimerCombo          = 0f;
        TempoSobrevivencia  = 0f;

        // Recarregar a cena atual (reinicia todos os inimigos, spawn, player)
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // ----------------------------------------------------------
    // MÉTODO PRIVADO — zera o combo internamente
    // ----------------------------------------------------------
    private void ZerarCombo()
    {
        ComboAtual = 0;
        TimerCombo = 0f;
        OnComboAlterado?.Invoke();
    }

    // DELETE DEPOIS — apenas para testes
    [ContextMenu("Zerar Recorde")]
    public void ZerarRecorde()
    {
        PlayerPrefs.DeleteKey("HitZeroRecorde");
        PlayerPrefs.Save();
        Debug.Log("Recorde zerado!");
    }
}
