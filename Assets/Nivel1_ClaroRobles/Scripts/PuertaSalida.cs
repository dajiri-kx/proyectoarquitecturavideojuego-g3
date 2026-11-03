using UnityEngine;
using UnityEngine.SceneManagement;

// Solo deja salir al jugador si ya tiene la llave.
public class PuertaSalida : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena del siguiente nivel. Vacío = solo muestra el mensaje de nivel completado.")]
    public string siguienteEscena = "";
    public Sprite spriteAbierta;
    public float esperaAntesDeCambiar = 2f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || EstadoNivel.nivelCompletado) return;

        if (!EstadoNivel.tieneLlave)
        {
            EstadoNivel.MostrarMensaje("La puerta está cerrada. Necesitás la llave.");
            return;
        }

        EstadoNivel.nivelCompletado = true;
        if (spriteAbierta != null)
            GetComponent<SpriteRenderer>().sprite = spriteAbierta;
        EstadoNivel.MostrarMensaje("¡Nivel 1 completado!", 10f);

        if (!string.IsNullOrEmpty(siguienteEscena))
            Invoke(nameof(CargarSiguiente), esperaAntesDeCambiar);
    }

    void CargarSiguiente()
    {
        EstadoNivel.Reiniciar();
        SceneManager.LoadScene(siguienteEscena);
    }
}
