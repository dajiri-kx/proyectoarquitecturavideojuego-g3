using UnityEngine;

// La llave flota un poco y el jugador la recoge al tocarla.
public class Llave : MonoBehaviour
{
    public float amplitud = 0.15f;
    public float velocidad = 2.5f;
    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        transform.position = posicionInicial + Vector3.up * Mathf.Sin(Time.time * velocidad) * amplitud;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        EstadoNivel.tieneLlave = true;
        EstadoNivel.MostrarMensaje("¡Llave recogida! Buscá la puerta de salida.");
        Destroy(gameObject);
    }
}
