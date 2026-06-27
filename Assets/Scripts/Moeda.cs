using UnityEngine;

public class Moeda : MonoBehaviour
{
    [Header("Configurações da Moeda")]
    [Tooltip("Quantas moedas o jogador ganha ao coletar esta?")]
    public int valorDaMoeda = 10;
    
    [Tooltip("Qual é a 'Tag' do seu carro? (Geralmente é 'Player')")]
    public string tagDoJogador = "Player";

    [Header("Efeitos (Opcional)")]
    public AudioClip somDeColeta;

    // Se o seu jogo for 2D (Física 2D)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(tagDoJogador))
        {
            Coletar();
        }
    }

    // Se o seu jogo for 3D (Física 3D)
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.CompareTag(tagDoJogador))
        {
            Coletar();
        }
    }

    private void Coletar()
    {
        // Adiciona o dinheiro globalmente
        if (GerenciadorDeDinheiro.Instancia != null)
        {
            GerenciadorDeDinheiro.Instancia.AdicionarDinheiro(valorDaMoeda);
        }
        else
        {
            Debug.LogWarning("O jogador coletou uma moeda, mas não encontrei o GerenciadorDeDinheiro nesta cena!");
        }

        // Toca o som, se houver
        if (somDeColeta != null)
        {
            // Usa o PlayClipAtPoint para o som tocar mesmo depois que a moeda for destruída
            AudioSource.PlayClipAtPoint(somDeColeta, transform.position);
        }

        // Destrói a moeda (faz ela sumir da tela)
        Destroy(gameObject);
    }
}
