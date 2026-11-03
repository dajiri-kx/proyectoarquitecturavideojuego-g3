using UnityEngine;

// La cámara sigue al objeto con Tag "Player" (sirve tanto para el jugador de prueba como para Theo).
public class CamaraSeguir : MonoBehaviour
{
    public Transform objetivo;
    public Vector2 desplazamiento = new Vector2(2f, 1.5f);
    public float suavizado = 6f;

    [Header("Límites del nivel")]
    public bool usarLimites = true;
    public float minX = 0f;
    public float maxX = 80f;
    public float minY = 0f;
    public float maxY = 14f;

    void LateUpdate()
    {
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador == null) return;
            objetivo = jugador.transform;
        }

        Vector3 destino = new Vector3(objetivo.position.x + desplazamiento.x,
                                      objetivo.position.y + desplazamiento.y,
                                      transform.position.z);

        if (usarLimites)
        {
            Camera cam = GetComponent<Camera>();
            float altoMedio = cam != null ? cam.orthographicSize : 5f;
            float anchoMedio = altoMedio * (cam != null ? cam.aspect : 16f / 9f);
            destino.x = Mathf.Clamp(destino.x, minX + anchoMedio, Mathf.Max(minX + anchoMedio, maxX - anchoMedio));
            destino.y = Mathf.Clamp(destino.y, minY + altoMedio, Mathf.Max(minY + altoMedio, maxY - altoMedio));
        }

        transform.position = Vector3.Lerp(transform.position, destino, suavizado * Time.deltaTime);
    }
}
