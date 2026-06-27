using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("O painel que deve aparecer primeiro (ex: Menu Principal).")]
    public GameObject painelInicial;

    // Pilha para armazenar o histórico de painéis abertos
    private Stack<GameObject> historicoPaineis = new Stack<GameObject>();
    
    // O painel que está ativo no momento
    private GameObject painelAtual;

    private void Start()
    {
        // Se houver um painel inicial definido, abre ele
        if (painelInicial != null)
        {
            // Ativa apenas o painel inicial e limpa o histórico
            AbrirPainel(painelInicial);
            historicoPaineis.Clear(); // Garante que não há como "voltar" antes do inicial
        }
    }

    /// <summary>
    /// Abre um novo painel e adiciona o atual ao histórico para podermos voltar a ele.
    /// Para usar: coloque no OnClick do botão e arraste o painel destino.
    /// </summary>
    public void AbrirPainel(GameObject novoPainel)
    {
        if (novoPainel == null) return;

        // Se já tiver um painel aberto, desativa ele e salva no histórico
        if (painelAtual != null)
        {
            painelAtual.SetActive(false);
            historicoPaineis.Push(painelAtual);
        }

        // Define o novo painel como o atual e ativa ele
        painelAtual = novoPainel;
        painelAtual.SetActive(true);
    }

    /// <summary>
    /// Fecha o painel atual e volta para o anterior no histórico.
    /// Para usar: coloque no OnClick do botão "Voltar".
    /// </summary>
    public void VoltarPainel()
    {
        if (historicoPaineis.Count > 0)
        {
            // Desativa o atual
            if (painelAtual != null)
            {
                painelAtual.SetActive(false);
            }

            // Puxa o último painel do histórico e ativa
            painelAtual = historicoPaineis.Pop();
            painelAtual.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Não há telas anteriores no histórico para voltar!");
        }
    }
}
