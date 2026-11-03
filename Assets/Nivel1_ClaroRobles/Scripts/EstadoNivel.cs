using UnityEngine;

// Guarda el progreso del Nivel 1 mientras se juega.
public static class EstadoNivel
{
    public static bool tieneLlave = false;
    public static int recompensas = 0;
    public static bool nivelCompletado = false;

    public static string mensaje = "";
    public static float mensajeHasta = 0f;

    public static void Reiniciar()
    {
        tieneLlave = false;
        recompensas = 0;
        nivelCompletado = false;
        mensaje = "";
        mensajeHasta = 0f;
    }

    public static void MostrarMensaje(string texto, float segundos = 2.5f)
    {
        mensaje = texto;
        mensajeHasta = Time.time + segundos;
    }
}
