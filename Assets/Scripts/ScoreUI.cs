using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Header("Textos da HUD")]
    [SerializeField] private TextMeshProUGUI textPontuacao;
    [SerializeField] private TextMeshProUGUI textCombo;
    [SerializeField] private TextMeshProUGUI textTimer;

    [Header("Dash")]
    [SerializeField] private UnityEngine.UI.Image imagemCooldownDash;

    [Header("Referências")]
    [SerializeField] private PlayerController playerController;

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
        // Timer de sobrevivência
        float t = GameManager.Instance.TempoSobrevivencia;
        int minutos = (int)(t / 60f);
        int segundos = (int)(t % 60f);
        textTimer.text = $"{minutos}:{segundos:D2}";

        // Cooldown do dash — preenche a imagem de 0 a 1
        if (imagemCooldownDash != null)
            imagemCooldownDash.fillAmount = 1f - playerController.CooldownDashNormalizado;
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