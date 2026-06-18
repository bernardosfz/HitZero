using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Fontes de Áudio")]
    [SerializeField] private AudioSource fonteMusica;
    [SerializeField] private AudioSource fonteEfeitos;

    [Header("Clips")]
    [SerializeField] private AudioClip musicaFundo;
    [SerializeField] private AudioClip somTiro;
    [SerializeField] private AudioClip somMorteInimigo;
    [SerializeField] private AudioClip somGameOver;

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

    void Start()
    {
        fonteMusica.clip = musicaFundo;
        fonteMusica.loop = true;
        fonteMusica.Play();

        GameManager.Instance.OnJogoEncerrado += TocarSomGameOver;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnJogoEncerrado -= TocarSomGameOver;
    }

    public void TocarTiro()
    {
        fonteEfeitos.PlayOneShot(somTiro);
    }

    public void TocarMorteInimigo()
    {
        fonteEfeitos.PlayOneShot(somMorteInimigo, 3f);
    }

    private void TocarSomGameOver()
    {
        fonteMusica.Stop();
        fonteEfeitos.PlayOneShot(somGameOver);
    }
}