using UnityEngine;
using UnityEngine.SceneManagement;

public class CarregarCena : MonoBehaviour
{
    [Tooltip("Nome da cena que este script vai carregar caso a função MudarDeCena seja chamada sem parâmetros")]
    public string nomeDaCenaPadrao;

    public void MudarDeCena(string nomeDaCena)
    {
        SceneManager.LoadScene(nomeDaCena);
    }

    public void MudarParaCenaPadrao()
    {
        if (!string.IsNullOrEmpty(nomeDaCenaPadrao))
        {
            SceneManager.LoadScene(nomeDaCenaPadrao);
        }
        else
        {
            Debug.LogWarning("Nome da cena padrão não foi preenchido!");
        }
    }
}
