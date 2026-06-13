using UnityEngine;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI textPontuacaoFinal;
    [SerializeField] private TextMeshProUGUI textRecorde;

    [Header("Painel")]
    [SerializeField] private GameObject painelGameOver;

    void Start()
    {
        // Painel começa escondido
        painelGameOver.SetActive(false);

        // Se inscreve no evento de Game Over do GameManager
        GameManager.Instance.OnJogoEncerrado += MostrarGameOver;
    }

    void OnDestroy()
    {
        GameManager.Instance.OnJogoEncerrado -= MostrarGameOver;
    }

    private void MostrarGameOver()
    {
        painelGameOver.SetActive(true);

        textPontuacaoFinal.text =
            $"PontuaçãoFinal: {GameManager.Instance.PontuacaoAtual:N0}";

        textRecorde.text =
            $"Recorde: {GameManager.Instance.Recorde:N0}";
    }

    // Chamado pelo botão "Jogar Novamente"
    public void BotaoReiniciar()
    {
        GameManager.Instance.Reiniciar();
    }
}