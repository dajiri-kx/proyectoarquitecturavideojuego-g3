using UnityEngine;
using UnityEngine.SceneManagement;

// Si el jugador toca la trampa (o cae al vacío), el nivel se reinicia.
public class Trampa : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        EstadoNivel.Reiniciar();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
