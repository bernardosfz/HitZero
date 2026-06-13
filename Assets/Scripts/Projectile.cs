using UnityEngine;

// ============================================================
//  Projectile.cs — Hit Zero
//  ONDE COLOCAR: No Prefab do projétil.
//
//  COMPONENTES NECESSÁRIOS no prefab do projétil:
//    - Rigidbody2D   (Body Type: Dynamic, Gravity Scale: 0)
//    - Collider2D    (CircleCollider2D, "Is Trigger" = TRUE)
//    - Sprite Renderer
//
//  TAG NECESSÁRIA:
//    - Este prefab deve ter a Tag: "Projectile"
//      (usada pelo EnemyAI para detectar o hit)
//
//  O QUE FAZ:
//    - Recebe velocidade do PlayerController ao ser criado
//    - Se destruído ao sair da arena (auto-destruição por tempo)
//    - A colisão com o inimigo é tratada no EnemyAI.cs
// ============================================================

public class Projectile : MonoBehaviour
{
    // ----------------------------------------------------------
    // CONFIGURAÇÕES
    // ----------------------------------------------------------
    [Header("Vida útil")]
    [Tooltip("Segundos antes de se auto-destruir caso não acerte nada")]
    [SerializeField] private float tempoDeVida = 1f;

    // ----------------------------------------------------------
    // START — agendar auto-destruição
    // ----------------------------------------------------------
    void Start()
    {
        // Destroy com delay: destrói o objeto após N segundos automaticamente.
        // Isso evita projéteis acumulando na memória caso errem os inimigos.
        Destroy(gameObject, tempoDeVida);
    }

    // ----------------------------------------------------------
    // NOTA SOBRE COLISÕES:
    // A detecção de "projétil + inimigo" está no EnemyAI.cs
    // (OnTriggerEnter2D verificando a Tag "Projectile").
    //
    // Isso é intencional: o inimigo é quem "registra" que morreu,
    // pois tem acesso direto ao GameManager.AdicionarPonto().
    // ----------------------------------------------------------
}
