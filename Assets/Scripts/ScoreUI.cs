using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("Textos da HUD")]
    [SerializeField] private TextMeshProUGUI textPontuacao;
    [SerializeField] private TextMeshProUGUI textCombo;
    [SerializeField] private TextMeshProUGUI textTimer;

    void Start()
    {
        GameManager.Instance.OnPontuacaoAlterada += AtualizarPontuacao;
        GameManager.Instance.OnComboAlterado     += AtualizarCombo;
    }

    void OnDisable()
    {
        GameManager.Instance.OnPontuacaoAlterada -= AtualizarPontuacao;
        GameManager.Instance.OnComboAlterado     -= AtualizarCombo;
    }

    void Update()
    {
        // Timer atualiza todo frame pois muda continuamente
        float t = GameManager.Instance.TempoSobrevivencia;
        int minutos = (int)(t / 60f);
        int segundos = (int)(t % 60f);
        textTimer.text = $"{minutos}:{segundos:D2}";
    }

    private void AtualizarPontuacao()
    {
        textPontuacao.text = GameManager.Instance.PontuacaoAtual.ToString("N0");
    }

    private void AtualizarCombo()
    {
        int combo = GameManager.Instance.Multiplicador;
        textCombo.text = $"x{combo}";

        // Esconde o indicador quando combo está em 1 (sem combo ativo)
        textCombo.gameObject.SetActive(combo > 1);
    }
}