using UnityEngine;
using TMPro; // Usado para TextMeshPro
using UnityEngine.UI; // Usado para textos normais

public class GerenciadorDeDinheiro : MonoBehaviour
{
    // Padrão Singleton para acessar de qualquer outro script facilmente
    public static GerenciadorDeDinheiro Instancia;

    [Header("UI - Interface")]
    [Tooltip("Arraste seu texto de dinheiro (TextMeshPro) aqui.")]
    public TextMeshProUGUI textoDinheiroTMP;
    
    [Tooltip("Arraste seu texto de dinheiro (Texto normal) aqui, caso não use o TMP.")]
    public Text textoDinheiroNormal;

    [Header("Testes")]
    [Tooltip("Dinheiro inicial que o jogador ganha se for a primeira vez jogando.")]
    public int dinheiroInicialParaTestes = 0;

    private int dinheiroAtual = 0;
    private const string CHAVE_DINHEIRO = "Dinheiro_Jogador";

    private void Awake()
    {
        // Configura o Singleton
        if (Instancia == null)
        {
            Instancia = this;
        }
        else if (Instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        CarregarDinheiro();
    }

    private void Start()
    {
        AtualizarUI();
    }

    private void CarregarDinheiro()
    {
        // Se for a primeira vez jogando, usa o dinheiroInicialParaTestes
        if (!PlayerPrefs.HasKey(CHAVE_DINHEIRO))
        {
            dinheiroAtual = dinheiroInicialParaTestes;
            SalvarDinheiro(); // Já salva
        }
        else
        {
            // Lê o dinheiro salvo no dispositivo
            dinheiroAtual = PlayerPrefs.GetInt(CHAVE_DINHEIRO, 0);
        }
    }

    public void AdicionarDinheiro(int quantidade)
    {
        dinheiroAtual += quantidade;
        SalvarDinheiro();
        AtualizarUI();
    }

    public bool GastarDinheiro(int quantidade)
    {
        if (dinheiroAtual >= quantidade)
        {
            dinheiroAtual -= quantidade;
            SalvarDinheiro();
            AtualizarUI();
            return true; // Compra efetuada com sucesso
        }
        
        return false; // Não tem dinheiro suficiente
    }

    public int ObterDinheiro()
    {
        return dinheiroAtual;
    }

    private void SalvarDinheiro()
    {
        PlayerPrefs.SetInt(CHAVE_DINHEIRO, dinheiroAtual);
        PlayerPrefs.Save();
    }

    private void AtualizarUI()
    {
        if (textoDinheiroTMP != null)
        {
            textoDinheiroTMP.text = dinheiroAtual.ToString();
        }
        
        if (textoDinheiroNormal != null)
        {
            textoDinheiroNormal.text = dinheiroAtual.ToString();
        }
    }

    [ContextMenu("Adicionar 100.000 Moedas (Teste)")]
    public void AdicionarMoedasTeste()
    {
        AdicionarDinheiro(100000);
        Debug.Log("Adicionado 100.000 moedas para teste!");
    }

    [ContextMenu("Zerar Dinheiro (Teste)")]
    public void ZerarDinheiroTeste()
    {
        dinheiroAtual = 0;
        SalvarDinheiro();
        AtualizarUI();
        Debug.Log("Dinheiro zerado!");
    }
}
