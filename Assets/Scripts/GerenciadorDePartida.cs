using System.Collections.Generic;
using UnityEngine;

public class GerenciadorDePartida : MonoBehaviour
{
    public static GerenciadorDePartida Instancia; // Singleton para o carro achar fácil
    
    [System.Serializable]
    public class ConfiguracaoDeCarro
    {
        [Tooltip("Exatamente o mesmo ID que está na Loja (ex: Sedan)")]
        public string idDoCarro; 
        
        [Tooltip("O Prefab do carro que será jogado na pista")]
        public GameObject prefabDoCarro; 
    }

    [Header("Categorias (Igual na Loja)")]
    public string categoriaCarros = "Carros";
    public string categoriaUpgrades = "Upgrades";
    
    [Header("UI e HUD do Mapa")]
    [Tooltip("Arraste a barra (imagem) de combustível que está no Canvas da fase aqui! O script vai dar ela para o carro quando ele nascer.")]
    public UnityEngine.UI.Image barraDeCombustivelNaTela;
    
    [Tooltip("Arraste o Painel de Game Over (que começa desligado) do seu Canvas para cá.")]
    public GameObject telaDeMorte;

    [Header("Física do Mapa")]
    [Tooltip("Gravidade deste mapa. Padrão da Terra = -9.81. Para a Lua, use algo como -1.62")]
    public float gravidadeY = -9.81f;
    
    [Header("Nascimento")]
    [Tooltip("Um objeto vazio na cena que marca onde o carro vai começar")]
    public Transform pontoDeNascimento; 

    [Header("Lista de Veículos")]
    public List<ConfiguracaoDeCarro> carrosDisponiveis;

    private void Awake()
    {
        // Configuração do Singleton
        if (Instancia == null)
            Instancia = this;
        else if (Instancia != this)
            Destroy(gameObject);

        // Define a física gravitacional DESTA cena especificamente
        Physics2D.gravity = new Vector2(0, gravidadeY);
    }

    private void Start()
    {
        // Garante que a tela de morte comece desligada
        if (telaDeMorte != null)
        {
            telaDeMorte.SetActive(false);
        }

        NascerCarro();
    }

    private void NascerCarro()
    {
        // 1. Descobrir qual carro foi selecionado na Loja
        string chaveDeBusca = categoriaCarros + "_selecionado";
        string idCarroSelecionado = PlayerPrefs.GetString(chaveDeBusca, "");
        
        Debug.Log($"[SISTEMA DE LOAD] O Mapa tentou ler a chave '{chaveDeBusca}' e o resultado foi: '{idCarroSelecionado}'");

        if (string.IsNullOrEmpty(idCarroSelecionado))
        {
            Debug.LogError("Nenhum carro foi selecionado no menu! Talvez a loja não tenha sido aberta ainda.");
            return;
        }

        // 2. Procurar o prefab do carro na lista pelo ID
        GameObject prefabParaNascer = null;
        foreach (var config in carrosDisponiveis)
        {
            if (config.idDoCarro == idCarroSelecionado)
            {
                prefabParaNascer = config.prefabDoCarro;
                break;
            }
        }

        if (prefabParaNascer == null)
        {
            Debug.LogError($"Prefab do carro com ID '{idCarroSelecionado}' não foi encontrado na lista de Carros Disponiveis do Gerenciador De Partida!");
            return;
        }

        // 3. Instanciar (Nascer) o carro no ponto definido
        // Se pontoDeNascimento for nulo, nasce no meio (0,0,0)
        Vector3 posicao = pontoDeNascimento != null ? pontoDeNascimento.position : Vector3.zero;
        Quaternion rotacao = pontoDeNascimento != null ? pontoDeNascimento.rotation : Quaternion.identity;

        GameObject carroNaPista = Instantiate(prefabParaNascer, posicao, rotacao);
        
        // 3.5. Conectar a interface (HUD) da Fase no carro que acabou de nascer!
        ControleCarro scriptDoCarro = carroNaPista.GetComponent<ControleCarro>();
        if (scriptDoCarro != null && barraDeCombustivelNaTela != null)
        {
            scriptDoCarro.barraCombustivel = barraDeCombustivelNaTela;
        }

        // 4. Ler quais upgrades esse carro específico tem!
        LerUpgradesDoCarro(idCarroSelecionado, carroNaPista);
    }

    private void LerUpgradesDoCarro(string idCarro, GameObject carroInstanciado)
    {
        // O prefixo do save será "Upgrades_Sedan" (Igual ao que fizemos na loja)
        string prefixoDeSave = categoriaUpgrades + "_" + idCarro;

        // Vamos armazenar as chaves exatas que estamos buscando para mostrar no log e ajudar a debugar!
        string chaveMotor = prefixoDeSave + "_Motor_comprado";
        string chaveSuspensao = prefixoDeSave + "_Suspensao_comprado";
        string chaveNitro = prefixoDeSave + "_Nitro_comprado";
        string chaveCombustivel = prefixoDeSave + "_Combustivel_comprado";
        
        bool temMotor = PlayerPrefs.GetInt(chaveMotor, 0) == 1;
        bool temSuspensao = PlayerPrefs.GetInt(chaveSuspensao, 0) == 1;
        bool temNitro = PlayerPrefs.GetInt(chaveNitro, 0) == 1;
        bool temCombustivel = PlayerPrefs.GetInt(chaveCombustivel, 0) == 1;

        Debug.Log($"Procurando chaves exatas na memória: \nMotor: '{chaveMotor}'\nSuspensao: '{chaveSuspensao}'");
        Debug.Log($"O carro {idCarro} nasceu na pista! Status dos Upgrades: Motor({temMotor}), Suspensão({temSuspensao}), Nitro({temNitro}), Combustível({temCombustivel})");

        // AQUI você deve aplicar a força/velocidade. Como eu não conheço o seu script de carro, fiz um exemplo de como seria:
        // 
        // MeuScriptDeMovimento script = carroInstanciado.GetComponent<MeuScriptDeMovimento>();
        // if (script != null)
        // {
        //     if (temMotor) script.velocidadeMaxima += 50;
        //     if (temSuspensao) script.forcaDaMola += 20;
        // }

        ControleCarro scriptDoCarro = carroInstanciado.GetComponent<ControleCarro>();
        if (scriptDoCarro != null)
        {
            scriptDoCarro.AplicarUpgrades(temMotor, temSuspensao, temNitro, temCombustivel);
            Debug.Log($"Upgrades aplicados fisicamente no carro {idCarro} com sucesso!");
        }
    }

    // Função chamada pelo ControleCarro quando as condições de morte são atingidas
    public void GameOver(string motivo)
    {
        Debug.Log($"GAME OVER! Motivo: {motivo}");
        
        if (telaDeMorte != null)
        {
            telaDeMorte.SetActive(true); // Mostra a tela escura
        }
        else
        {
            Debug.LogWarning("O jogador morreu, mas a 'Tela De Morte' não está preenchida no GerenciadorDePartida!");
        }
    }
}
