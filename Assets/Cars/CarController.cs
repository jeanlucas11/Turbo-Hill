using UnityEngine;
using UnityEngine.InputSystem; // Linha obrigatória para o novo sistema
using UnityEngine.UI; // Necessário para acessar elementos de HUD/UI

public class ControleCarro : MonoBehaviour
{
    [Header("Rodas e Motor")]
    public WheelJoint2D rodaTraseira; 
    public WheelJoint2D rodaDianteira; // Adicionado roda dianteira
    public bool tracao4x4 = true; // Se ativado, a roda da frente também acelera
    public float velocidadeMotor = 1500f;

    [Header("Rotação no Ar")]
    public Rigidbody2D chassiRb; 
    public float forcaRotacao = 200f;

    [Header("Câmera")]
    public bool seguirCarro = true;
    public Vector3 offsetCamera = new Vector3(0, 2f, -10f);
    public float suavizacaoCamera = 5f;
    private Camera camPrincipal;

    [Header("Combustível")]
    public float combustivelMaximo = 100f;
    public float combustivelAtual;
    public float taxaConsumo = 5f; // Quanto gasta por segundo
    public Image barraCombustivel; // A imagem que vai esvaziar na HUD

    [Header("Bônus de Upgrades (Tuning)")]
    public float bonusVelocidadeMotor = 500f; // Quanto a velocidade aumenta se tiver motor
    public float bonusDurezaSuspensao = 3f;   // Quanto a 'Frequency' aumenta se tiver suspensao
    public float bonusReducaoConsumo = 2f;    // Quanto a 'taxaConsumo' diminui se tiver combustivel
    
    [Header("Configurações do Nitro")]
    public float forcaNitro = 5000f;          // Força aplicada pra frente (Impulso)
    public float recargaNitro = 5f;           // Tempo de recarga do nitro (Cooldown)
    private float tempoUltimoNitro = -100f;   // Registra quando foi usado
    private bool possuiNitro = false;         // Controlado pelos upgrades

    [Header("Morte e Game Over")]
    public float tempoParaMorrer = 5f; // Segundos seguidos até morrer (capotado ou sem gasolina)
    private float tempoDeCapotamento = 0f;
    private float tempoSemCombustivel = 0f;
    private bool estaMorto = false;

    [Header("Manobras (Flips)")]
    public int recompensaPorFlip = 1500;
    public float distanciaParaOChao = 2.5f; // Ajuste no Inspector para o raio alcançar o chão quando as rodas tocam
    private float anguloZAnterior;
    private float rotacaoAcumulada;

    private float movimento;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        camPrincipal = Camera.main;

        // Como os dois WheelJoint2D estão no mesmo objeto (Car), vamos pegá-los automaticamente
        WheelJoint2D[] rodas = GetComponents<WheelJoint2D>();
        
        if (rodas.Length >= 2)
        {
            // A roda com o X do Anchor maior é a da frente (assumindo que o carro aponta para a direita)
            if (rodas[0].anchor.x > rodas[1].anchor.x)
            {
                rodaDianteira = rodas[0];
                rodaTraseira = rodas[1];
            }
            else
            {
                rodaDianteira = rodas[1];
                rodaTraseira = rodas[0];
            }
        }
        else if (rodas.Length == 1)
        {
            rodaTraseira = rodas[0];
        }

        // Inicializa o combustível
        combustivelAtual = combustivelMaximo;

        // Inicializa a rotação do flip
        anguloZAnterior = chassiRb.transform.eulerAngles.z;
    }

    void Update()
    {
        if (estaMorto) return; // Se morreu, trava todos os controles e o jogo

        movimento = 0;

        // Reduz o combustível constantemente (estilo Hill Climb Racing)
        if (combustivelAtual > 0)
        {
            combustivelAtual -= taxaConsumo * Time.deltaTime;

            // Só lê os controles se tiver combustível
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                    movimento = 1;
                else if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                    movimento = -1;

                // Lógica do Nitro (Shift)
                if (possuiNitro && Keyboard.current.shiftKey.wasPressedThisFrame)
                {
                    AtivarNitro();
                }
            }
        }
        else
        {
            combustivelAtual = 0; // Garante que não fique negativo
        }

        // Atualiza a UI (HUD)
        if (barraCombustivel != null)
        {
            barraCombustivel.fillAmount = combustivelAtual / combustivelMaximo;
        }

        // Checa Manobras (Flips)
        ChecarFlips();

        // Checa as condições de derrota
        ChecarMorte();
    }

    private void ChecarFlips()
    {
        float anguloAtual = chassiRb.transform.eulerAngles.z;
        float deltaAngulo = Mathf.DeltaAngle(anguloZAnterior, anguloAtual);
        anguloZAnterior = anguloAtual;

        // Lança um raio para baixo do carro para ver se ele está perto do chão
        RaycastHit2D hit = Physics2D.Raycast(chassiRb.transform.position, -chassiRb.transform.up, distanciaParaOChao);
        
        bool tocandoChao = false;
        // Ignora colisões com as próprias rodas do carro (se o hit bater em algo que não é filho do carro)
        if (hit.collider != null && !hit.collider.transform.IsChildOf(transform))
        {
            tocandoChao = true;
        }

        if (tocandoChao)
        {
            // Se tocou no chão, zera a rotação acumulada (não pode fazer flip no chão)
            rotacaoAcumulada = 0f;
        }
        else
        {
            // Se está no ar, acumula a rotação
            rotacaoAcumulada += deltaAngulo;
            
            // Se a rotação acumulada passar de 300 graus (quase um círculo completo), conta como Flip!
            if (Mathf.Abs(rotacaoAcumulada) >= 300f)
            {
                // Tira 300 ou -300 da acumulação para permitir um segundo flip no mesmo salto (Double Flip!)
                rotacaoAcumulada = 0f; 
                
                DarRecompensaFlip();
            }
        }
    }

    private void DarRecompensaFlip()
    {
        if (GerenciadorDeDinheiro.Instancia != null)
        {
            GerenciadorDeDinheiro.Instancia.AdicionarDinheiro(recompensaPorFlip);
        }
        Debug.Log("MANOBRA! Flip Completo! +1500 Moedas!");
    }

    private void ChecarMorte()
    {
        // 1. Checa se o combustível zerou
        if (combustivelAtual <= 0)
        {
            tempoSemCombustivel += Time.deltaTime;
            if (tempoSemCombustivel >= tempoParaMorrer)
            {
                Morrer("Falta de Combustível");
                return;
            }
        }
        else
        {
            tempoSemCombustivel = 0f; // Abasteceu, zera o timer
        }

        // 2. Checa se está capotado (eixo Y do carro apontando para baixo)
        if (Vector3.Dot(chassiRb.transform.up, Vector3.up) < 0f)
        {
            tempoDeCapotamento += Time.deltaTime;
            if (tempoDeCapotamento >= tempoParaMorrer)
            {
                Morrer("Capotamento");
                return;
            }
        }
        else
        {
            tempoDeCapotamento = 0f; // Desvirou, zera o timer
        }
    }

    private void Morrer(string motivo)
    {
        estaMorto = true;
        movimento = 0;
        
        // Desliga o motor das rodas
        rodaTraseira.useMotor = false;
        if (rodaDianteira != null) rodaDianteira.useMotor = false;

        // Avisa o Gerenciador de Partida para mostrar a tela!
        if (GerenciadorDePartida.Instancia != null)
        {
            GerenciadorDePartida.Instancia.GameOver(motivo);
        }
    }

    void FixedUpdate()
    {
        if (estaMorto) return; // Não faz física no motor se estiver morto

        if (movimento == 0)
        {
            // Se não está acelerando, desliga o motor para a roda girar livremente (fluido)
            rodaTraseira.useMotor = false;
            if (rodaDianteira != null) rodaDianteira.useMotor = false;
        }
        else
        {
            // Liga o motor e aplica a velocidade
            rodaTraseira.useMotor = true;
            JointMotor2D motorTraseiro = rodaTraseira.motor;
            motorTraseiro.motorSpeed = movimento * -velocidadeMotor; 
            rodaTraseira.motor = motorTraseiro;

            // Lógica para a roda dianteira (evitar que fique travada)
            if (rodaDianteira != null)
            {
                if (tracao4x4)
                {
                    rodaDianteira.useMotor = true;
                    JointMotor2D motorDianteiro = rodaDianteira.motor;
                    motorDianteiro.motorSpeed = movimento * -velocidadeMotor;
                    rodaDianteira.motor = motorDianteiro;
                }
                else
                {
                    rodaDianteira.useMotor = false; // Apenas roda livre
                }
            }
        }

        // Aplica o torque de rotação no ar
        // Reduzimos um pouco a força e usamos Rigidbody para adicionar o torque
        chassiRb.AddTorque(movimento * -forcaRotacao * Time.fixedDeltaTime);
    }

    void LateUpdate()
    {
        if (seguirCarro && camPrincipal != null)
        {
            Vector3 posicaoAlvo = transform.position + offsetCamera;
            // Interpolação suave para a câmera seguir o carro
            Vector3 posicaoSuave = Vector3.Lerp(camPrincipal.transform.position, posicaoAlvo, suavizacaoCamera * Time.deltaTime);
            posicaoSuave.z = offsetCamera.z; // Mantém a profundidade da câmera fixa no 2D
            
            camPrincipal.transform.position = posicaoSuave;
        }
    }

    // Método chamado pelo galão de combustível quando o carro encosta nele
    public void Abastecer(float quantidade)
    {
        combustivelAtual += quantidade;
        // Garante que não passe do máximo
        if (combustivelAtual > combustivelMaximo)
        {
            combustivelAtual = combustivelMaximo;
        }
    }

    // Função Pública para Ativar o Nitro (pode ser chamada pelo teclado ou por um botão na tela de celular depois)
    public void AtivarNitro()
    {
        if (possuiNitro && combustivelAtual > 0)
        {
            // Checa se já passou o tempo de recarga
            if (Time.time >= tempoUltimoNitro + recargaNitro)
            {
                tempoUltimoNitro = Time.time;
                
                // Aplica a força de nitro pra frente na direção em que o chassi está apontando
                if (chassiRb != null)
                {
                    chassiRb.AddForce(chassiRb.transform.right * forcaNitro, ForceMode2D.Impulse);
                    Debug.Log("NITRO ATIVADO! Vruuuum!");
                }
            }
            else
            {
                Debug.Log($"Nitro recarregando... Faltam {(tempoUltimoNitro + recargaNitro) - Time.time:F1} segundos.");
            }
        }
    }

    // Função chamada pelo GerenciadorDePartida ao spawnar o carro
    public void AplicarUpgrades(bool temMotor, bool temSuspensao, bool temNitro, bool temCombustivel)
    {
        if (temMotor)
        {
            velocidadeMotor += bonusVelocidadeMotor;
        }

        if (temSuspensao)
        {
            if (rodaTraseira != null)
            {
                JointSuspension2D suspT = rodaTraseira.suspension;
                suspT.frequency += bonusDurezaSuspensao;
                rodaTraseira.suspension = suspT;
            }
            if (rodaDianteira != null)
            {
                JointSuspension2D suspD = rodaDianteira.suspension;
                suspD.frequency += bonusDurezaSuspensao;
                rodaDianteira.suspension = suspD;
            }
        }

        if (temCombustivel)
        {
            taxaConsumo -= bonusReducaoConsumo;
            if (taxaConsumo < 0.5f) taxaConsumo = 0.5f; // Limite mínimo para nunca gastar zero
        }

        possuiNitro = temNitro;
    }
}