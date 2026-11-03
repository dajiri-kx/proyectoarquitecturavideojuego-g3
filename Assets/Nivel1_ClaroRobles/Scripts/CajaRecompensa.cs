using UnityEngine;

// Al tocarla, la caja se abre una sola vez y suma recompensas.
public class CajaRecompensa : MonoBehaviour
{
    public int valor = 1;
    public Sprite spriteAbierta;
    private bool abierta = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (abierta || !other.CompareTag("Player")) return;
        abierta = true;
        EstadoNivel.recompensas += valor;
        EstadoNivel.MostrarMensaje("+" + valor + " recompensa", 1.5f);
        if (spriteAbierta != null)
            GetComponent<SpriteRenderer>().sprite = spriteAbierta;
    }
}
