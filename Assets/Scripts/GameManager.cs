using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Pontuação")]
    [SerializeField] private int pontosPorKill = 100;

    [Header("Combo")]
    [SerializeField] private float tempoResetCombo = 3f;

    // Thresholds: kills acumuladas necessárias para subir cada nível
    private readonly int[] thresholdsCombo = { 0, 5, 10, 20, 50, 100, 200, 500, 1000, 2000 };

    // Estado do jogo
    public bool  JogoEncerrado       { get; private set; } = false;
    public int   PontuacaoAtual      { get; private set; } = 0;
    public int   ComboAtual          { get; private set; } = 0;
    public int   Multiplicador       { get; private set; } = 1;
    public float TimerCombo          { get; private set; } = 0f;
    public float TempoSobrevivencia  { get; private set; } = 0f;

    private const string CHAVE_RECORDE = "HitZeroRecorde";
    public int Recorde => PlayerPrefs.GetInt(CHAVE_RECORDE, 0);

    public System.Action OnPontuacaoAlterada;
    public System.Action OnComboAlterado;
    public System.Action OnJogoEncerrado;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (JogoEncerrado) return;

        TempoSobrevivencia += Time.deltaTime;

        if (ComboAtual > 0)
        {
            TimerCombo -= Time.deltaTime;
            if (TimerCombo <= 0f)
                ZerarCombo();
        }
    }

    public void AdicionarPonto()
    {
        ComboAtual++;
        TimerCombo = tempoResetCombo;

        // Subir multiplicador se atingiu o próximo threshold
        if (Multiplicador < 10)
        {
            int proximoThreshold = thresholdsCombo[Multiplicador];
            if (ComboAtual >= proximoThreshold)
                Multiplicador++;
        }

        int pontos = pontosPorKill * Multiplicador;
        PontuacaoAtual += pontos;

        OnPontuacaoAlterada?.Invoke();
        OnComboAlterado?.Invoke();
    }

    public void TriggerGameOver()
    {
        if (JogoEncerrado) return;

        JogoEncerrado = true;

        if (PontuacaoAtual > Recorde)
        {
            PlayerPrefs.SetInt(CHAVE_RECORDE, PontuacaoAtual);
            PlayerPrefs.Save();
        }

        OnJogoEncerrado?.Invoke();
        Time.timeScale = 0f;
    }

    public void Reiniciar()
    {
        Time.timeScale    = 1f;
        JogoEncerrado     = false;
        PontuacaoAtual    = 0;
        ComboAtual        = 0;
        Multiplicador     = 1;
        TimerCombo        = 0f;
        TempoSobrevivencia = 0f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void ZerarCombo()
    {
        ComboAtual    = 0;
        Multiplicador = 1;
        TimerCombo    = 0f;
        OnComboAlterado?.Invoke();
    }

    [ContextMenu("Zerar Recorde")]
    public void ZerarRecorde()
    {
        PlayerPrefs.DeleteKey(CHAVE_RECORDE);
        PlayerPrefs.Save();
        Debug.Log("Recorde zerado!");
    }
}