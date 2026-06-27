using UnityEngine;

public class ColetaCombustivel : MonoBehaviour
{
    [Header("Configurações")]
    public float quantidadeCombustivel = 40f; // Quantidade que vai encher o tanque
    
    [Tooltip("Qual é a 'Tag' do seu carro? (Geralmente é 'Player')")]
    public string tagDoJogador = "Player";

    private bool coletado = false; // Evita que o combustível seja coletado 2 vezes

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (coletado) return; // Se já pegou, não faz mais nada
        
        // Verifica se quem bateu foi o jogador (usando a Tag)
        if (collision.CompareTag(tagDoJogador))
        {
            // Procura o script do carro no objeto que colidiu
            ControleCarro carroScript = collision.GetComponentInParent<ControleCarro>();
            
            if (carroScript != null)
            {
                coletado = true; // Trava para não ser coletado por outra roda no mesmo frame
                carroScript.Abastecer(quantidadeCombustivel);
                Destroy(gameObject); // Some da tela
            }
        }
    }
}
