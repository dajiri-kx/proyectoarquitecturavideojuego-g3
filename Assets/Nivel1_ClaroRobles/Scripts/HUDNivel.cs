using UnityEngine;

// Muestra en pantalla la llave, las recompensas y los mensajes del nivel.
public class HUDNivel : MonoBehaviour
{
    public string tituloNivel = "Nivel 1: El Claro de los Robles";
    private int totalRecompensas = 0;
    private GUIStyle estiloInfo;
    private GUIStyle estiloMensaje;

    void Awake()
    {
        EstadoNivel.Reiniciar();
    }

    void Start()
    {
        foreach (var caja in FindObjectsByType<CajaRecompensa>(FindObjectsSortMode.None))
            totalRecompensas += caja.valor;
        EstadoNivel.MostrarMensaje(tituloNivel, 3f);
    }

    void OnGUI()
    {
        if (estiloInfo == null)
        {
            estiloInfo = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleLeft };
            estiloInfo.normal.textColor = Color.white;
            estiloMensaje = new GUIStyle(GUI.skin.box) { fontSize = 28, alignment = TextAnchor.MiddleCenter };
            estiloMensaje.normal.textColor = new Color(1f, 0.92f, 0.5f);
        }

        string llave = EstadoNivel.tieneLlave ? "Sí" : "No";
        GUI.Box(new Rect(15, 15, 300, 70),
            "  Llave: " + llave + "\n  Recompensas: " + EstadoNivel.recompensas + " / " + totalRecompensas,
            estiloInfo);

        if (!string.IsNullOrEmpty(EstadoNivel.mensaje) && Time.time < EstadoNivel.mensajeHasta)
        {
            float ancho = 620f;
            GUI.Box(new Rect((Screen.width - ancho) / 2f, 40, ancho, 60), EstadoNivel.mensaje, estiloMensaje);
        }
    }
}
