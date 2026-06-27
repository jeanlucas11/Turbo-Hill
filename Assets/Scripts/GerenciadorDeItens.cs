using System.Collections.Generic;
using UnityEngine;

public class GerenciadorDeItens : MonoBehaviour
{
    public enum TipoDeCategoria
    {
        Carros, // Unowned -> Owned -> Selected. Só pode selecionar um.
        FasesOuUpgrades // Unowned -> Owned. Pode ter vários comprados. Ao clicar não seleciona, apenas ativa o evento.
    }

    [Header("Configurações da Loja")]
    [Tooltip("Ex: 'Carros', 'Fases'. Importante para salvar os dados separadamente.")]
    public string categoria; 

    [Tooltip("Se for Carros, ele muda o fundo para Selecionado. Se for FasesOuUpgrades, ele apenas fica Comprado e permite jogar/usar.")]
    public TipoDeCategoria tipoDaCategoria = TipoDeCategoria.Carros;

    [Header("Upgrades por Carro (Opcional)")]
    [Tooltip("Se for Upgrades, arraste o Gerenciador de Carros aqui para que os upgrades sejam salvos apenas para o carro selecionado!")]
    public GerenciadorDeItens dependeDestaCategoria;

    // Evento chamado quando a seleção muda (usado para atualizar os upgrades)
    public System.Action AoMudarSelecao;

    [Tooltip("Lista de todos os itens (botões) desta categoria")]
    public List<ItemLoja> itensDaLoja;

    [Tooltip("Item que já vem desbloqueado por padrão (ex: Sedan ou Terra)")]
    public ItemLoja itemPadraoInicial;

    private ItemLoja itemSelecionadoAtual;

    private void Start()
    {
        if (dependeDestaCategoria != null)
        {
            dependeDestaCategoria.AoMudarSelecao += RecarregarItens;
        }

        RecarregarItens();
    }

    public string ObterIdSelecionado()
    {
        if (itemSelecionadoAtual != null) return itemSelecionadoAtual.idItem;
        return PlayerPrefs.GetString(categoria + "_selecionado", "");
    }

    public string ObterPrefixo()
    {
        if (dependeDestaCategoria != null)
        {
            return categoria + "_" + dependeDestaCategoria.ObterIdSelecionado();
        }
        return categoria;
    }

    public void RecarregarItens()
    {
        string prefixo = ObterPrefixo();

        // Desbloqueia o item padrão na primeira vez que abre o jogo
        if (!PlayerPrefs.HasKey(prefixo + "_iniciado") && itemPadraoInicial != null)
        {
            Debug.Log($"[{prefixo}] Primeira vez iniciando! Desbloqueando item padrão: {itemPadraoInicial.idItem}");
            PlayerPrefs.SetInt(prefixo + "_" + itemPadraoInicial.idItem + "_comprado", 1);
            if (tipoDaCategoria == TipoDeCategoria.Carros)
            {
                PlayerPrefs.SetString(prefixo + "_selecionado", itemPadraoInicial.idItem);
            }
            PlayerPrefs.SetInt(prefixo + "_iniciado", 1);
            PlayerPrefs.Save();
        }

        itemSelecionadoAtual = null;

        // Carrega o estado de cada item ao iniciar
        foreach (ItemLoja item in itensDaLoja)
        {
            bool comprado = PlayerPrefs.GetInt(prefixo + "_" + item.idItem + "_comprado", 0) == 1;
            bool selecionado = false;

            if (tipoDaCategoria == TipoDeCategoria.Carros)
            {
                string selecionadoSalvo = PlayerPrefs.GetString(prefixo + "_selecionado", "");
                selecionado = (selecionadoSalvo == item.idItem);
                
                if (selecionado)
                {
                    itemSelecionadoAtual = item;
                }
            }

            Debug.Log($"[{prefixo}] Carregando item: {item.idItem} | Comprado: {comprado} | Selecionado: {selecionado}");
            item.Inicializar(this, comprado, selecionado);
        }

        // --- Proteção contra ID vazio ou erro de digitação ---
        if (tipoDaCategoria == TipoDeCategoria.Carros && itemSelecionadoAtual == null)
        {
            Debug.LogWarning($"Opa! O script no objeto '{gameObject.name}' tentou carregar a categoria {prefixo}, mas não achou o carro selecionado (ou a lista de botões dele estava vazia). Por segurança, ele forçou a seleção do primeiro! Isso pode estar sobrescrevendo sua escolha se este objeto estiver na cena errada.");
            if (itemPadraoInicial != null)
            {
                MudarSelecao(itemPadraoInicial);
            }
            else if (itensDaLoja.Count > 0)
            {
                MudarSelecao(itensDaLoja[0]);
            }
        }
    }

    public void ClicouNoItem(ItemLoja itemClicado)
    {
        if (itemClicado.IsComprado())
        {
            if (tipoDaCategoria == TipoDeCategoria.Carros)
            {
                // Se for carro e já tá comprado, seleciona ele
                MudarSelecao(itemClicado);
            }
            else
            {
                // Se for fase ou upgrade, apenas dispara o evento (ex: Carregar cena da fase)
                itemClicado.DispararEventoClicouComprado();
            }
        }
        else
        {
            // Se não tem, tenta comprar
            TentarComprar(itemClicado);
        }
    }

    private void TentarComprar(ItemLoja item)
    {
        if (GerenciadorDeDinheiro.Instancia != null)
        {
            if (GerenciadorDeDinheiro.Instancia.GastarDinheiro(item.preco))
            {
                Debug.Log("Comprou " + item.idItem + "!");
                
                item.Comprar(); // Muda estado para comprado
                SalvarCompra(item); // Salva no sistema
                
                if (tipoDaCategoria == TipoDeCategoria.Carros)
                {
                    MudarSelecao(item); // Já seleciona automaticamente após a compra
                }
                else
                {
                    item.AtualizarVisual(); // Apenas atualiza pra mostrar comprado
                }
            }
            else
            {
                Debug.LogWarning("Dinheiro insuficiente para comprar " + item.idItem);
            }
        }
        else
        {
            Debug.LogError("Falta o GerenciadorDeDinheiro na cena!");
        }
    }

    private void MudarSelecao(ItemLoja novoSelecionado)
    {
        // Se já havia um item selecionado antes, muda ele para apenas "comprado"
        if (itemSelecionadoAtual != null && itemSelecionadoAtual != novoSelecionado)
        {
            itemSelecionadoAtual.Deselecionar();
        }

        // Seleciona o novo item e muda o visual para "selecionado"
        itemSelecionadoAtual = novoSelecionado;
        itemSelecionadoAtual.Selecionar();

        string prefixo = ObterPrefixo();

        // Salva qual foi o último selecionado para lembrar quando fechar o jogo
        PlayerPrefs.SetString(prefixo + "_selecionado", novoSelecionado.idItem);
        PlayerPrefs.Save();
        
        Debug.Log($"[SISTEMA DE SAVE] A categoria '{prefixo}' acabou de salvar que o item selecionado agora é: {novoSelecionado.idItem}");

        // Avisa quem depende desta categoria (ex: Upgrades) para recarregar
        AoMudarSelecao?.Invoke();
    }

    private void SalvarCompra(ItemLoja item)
    {
        string prefixo = ObterPrefixo();
        // Salva que este item foi comprado
        PlayerPrefs.SetInt(prefixo + "_" + item.idItem + "_comprado", 1);
        PlayerPrefs.Save();
    }
    
    // Método utilitário caso queira dar itens de graça (como o primeiro carro)
    [ContextMenu("Desbloquear Primeiro Item (Para Testes)")]
    public void DesbloquearPrimeiro()
    {
        if (itensDaLoja.Count > 0)
        {
            SalvarCompra(itensDaLoja[0]);
            MudarSelecao(itensDaLoja[0]);
            Debug.Log("Primeiro item desbloqueado!");
        }
    }
    
    [ContextMenu("Resetar Progresso (Para Testes)")]
    public void ResetarProgresso()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("Todo o progresso foi apagado!");
    }
}
