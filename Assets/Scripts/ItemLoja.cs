using UnityEngine;
using UnityEngine.UI;

public class ItemLoja : MonoBehaviour
{
    [Header("Configurações do Item")]
    public string idItem; // Um nome único, ex: "Carro1", "FaseLua"
    public int preco;

    [Header("Imagens de Fundo (Sprites)")]
    [Tooltip("exemploSc")]
    public Sprite spriteSemCompra;
    [Tooltip("exemploComprado")]
    public Sprite spriteComprado;
    [Tooltip("exemploSelecionado")]
    public Sprite spriteSelecionado;

    [Header("Referências da UI")]
    public Image imagemFundo; // O componente Image que vai mudar de fundo
    public Button botao;      // O componente Button deste item

    [Tooltip("Opcional: Objeto que vai aparecer só quando selecionado (ex: Botão Jogar)")]
    public GameObject objetoParaAtivarQuandoSelecionado;

    [Header("Eventos")]
    [Tooltip("O que fazer se o item for clicado e JÁ estiver comprado? (Apenas para categorias 'FasesOuUpgrades'. Ex: Carregar Cena)")]
    public UnityEngine.Events.UnityEvent eventoAoClicarJaComprado;

    private void Awake()
    {
        GarantirComponentes();
    }

    private void GarantirComponentes()
    {
        // Tenta achar automaticamente a Imagem e o Botão se você tiver esquecido de arrastar no Inspector!
        if (imagemFundo == null) imagemFundo = GetComponent<Image>();
        if (botao == null) botao = GetComponent<Button>();
    }

    private void OnEnable()
    {
        // Toda vez que a aba da loja for aberta (ativada), ele reforça a atualização do visual.
        // Isso evita o bug do botão ficar cinza se a loja começar fechada!
        AtualizarVisual();
    }

    private GerenciadorDeItens gerenciador;
    private bool comprado = false;
    private bool selecionado = false;

    // Chamado pelo Gerenciador quando a tela carrega
    public void Inicializar(GerenciadorDeItens gen, bool jaComprado, bool jaSelecionado)
    {
        GarantirComponentes(); // Garante que temos a imagem mesmo se a aba da loja estiver fechada e o Awake não rodou

        gerenciador = gen;
        comprado = jaComprado;
        selecionado = jaSelecionado;

        // Limpa eventos antigos para evitar duplicação e adiciona o evento de clique
        if (botao != null)
        {
            botao.onClick.RemoveAllListeners();
            botao.onClick.AddListener(AoClicar);
        }
        
        AtualizarVisual();
    }

    public void AoClicar()
    {
        gerenciador.ClicouNoItem(this);
    }

    public void Comprar()
    {
        comprado = true;
        AtualizarVisual();
    }

    public void Selecionar()
    {
        selecionado = true;
        AtualizarVisual();
    }

    public void Deselecionar()
    {
        selecionado = false;
        AtualizarVisual();
    }

    public void AtualizarVisual()
    {
        if (imagemFundo != null)
        {
            if (selecionado)
            {
                if (spriteSelecionado != null) imagemFundo.sprite = spriteSelecionado;
                else Debug.LogWarning($"[{idItem}] Ops! Você esqueceu de arrastar o 'Sprite Selecionado' no botão no Unity!");
            }
            else if (comprado)
            {
                if (spriteComprado != null) imagemFundo.sprite = spriteComprado;
                else Debug.LogWarning($"[{idItem}] Ops! Você esqueceu de arrastar o 'Sprite Comprado' no botão no Unity!");
            }
            else
            {
                if (spriteSemCompra != null) imagemFundo.sprite = spriteSemCompra;
                else Debug.LogWarning($"[{idItem}] Ops! Você esqueceu de arrastar o 'Sprite Sem Compra' no botão no Unity!");
            }
        }

        // Se você arrastou um botão 'Jogar', ele só vai ficar ativo se este item estiver selecionado!
        if (objetoParaAtivarQuandoSelecionado != null)
        {
            objetoParaAtivarQuandoSelecionado.SetActive(selecionado);
        }
    }

    public bool IsComprado()
    {
        return comprado;
    }

    public void DispararEventoClicouComprado()
    {
        if (eventoAoClicarJaComprado != null)
        {
            eventoAoClicarJaComprado.Invoke();
        }
    }
}
