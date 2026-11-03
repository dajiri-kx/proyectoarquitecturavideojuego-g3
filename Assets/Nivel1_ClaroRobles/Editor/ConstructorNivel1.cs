using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

// Herramienta de editor: construye automáticamente la escena del Nivel 1 "El Claro de los Robles".
// Se abre desde el menú superior de Unity: Lost in the Forest > Construir Nivel 1
public class ConstructorNivel1 : EditorWindow
{
    const string Carpeta = "Assets/Nivel1_ClaroRobles";
    const string CarpetaSprites = Carpeta + "/Sprites";
    const string CarpetaTiles = Carpeta + "/Tiles";
    const string RutaEscena = Carpeta + "/Nivel1_ClaroRobles.unity";
    const string NombreCapaSuelo = "Ground";

    // Mapa del nivel (cada carácter es un cuadro de la cuadrícula):
    // '#' tierra   '=' plataforma   '^' trampa   'K' llave
    // 'B' caja de recompensa   'D' puerta de salida   'P' inicio del jugador   '.' vacío
    static readonly string[] Mapa =
    {
            "#..............................................................................#",
            "#..............................................................................#",
            "#..............................................................................#",
            "#..............................................................................#",
            "#..............................................................................#",
            "#..............................................................................#",
            "#...........................................K..................................#",
            "#....................B....................====.........................B.......#",
            "#...................====..............................................===......#",
            "#...................................#####.................^....................#",
            "#..P.....B.............^^...........#####^^^^^^..B....#########.....B.......D..#",
            "###############...#############...#################...#########..###############",
            "###############...#############...#################...#########..###############",
            "###############...#############...#################...#########..###############"
    };

    static readonly string[] SpritesIncluidos =
    {
        "suelo_tierra", "suelo_cesped", "plataforma_tronco", "fondo_bosque", "trampa_espinas", "llave",
        "caja_cerrada", "caja_abierta", "puerta_cerrada", "puerta_abierta", "jugador_prueba"
    };

    public Sprite spriteTierra, spriteCesped, spritePlataforma, spriteFondo, spriteTrampa, spriteLlave;
    public Sprite spriteCajaCerrada, spriteCajaAbierta, spritePuertaCerrada, spritePuertaAbierta, spriteJugador;
    public GameObject prefabTheo;
    public string siguienteEscena = "";
    Vector2 scroll;

    [MenuItem("Lost in the Forest/Construir Nivel 1")]
    static void Abrir()
    {
        var ventana = GetWindow<ConstructorNivel1>("Constructor Nivel 1");
        ventana.minSize = new Vector2(400, 560);
    }

    void OnEnable()
    {
        PrepararSpritesIncluidos();
        CargarSpritesIncluidos(false);
    }

    // ---------- Interfaz de la ventana ----------

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Nivel 1: El Claro de los Robles", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Ya vienen sprites provisionales cargados. Si querés usar los del pack del grupo, " +
            "arrastrá cada sprite desde la ventana Project al campo correspondiente. " +
            "Luego presioná 'Construir Nivel 1'.", MessageType.Info);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Escenario", EditorStyles.boldLabel);
        spriteTierra = CampoSprite("Tierra (interior)", spriteTierra);
        spriteCesped = CampoSprite("Tierra con césped (borde)", spriteCesped);
        spritePlataforma = CampoSprite("Plataforma flotante", spritePlataforma);
        spriteFondo = CampoSprite("Fondo del bosque (opcional)", spriteFondo);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Objetos", EditorStyles.boldLabel);
        spriteTrampa = CampoSprite("Trampa", spriteTrampa);
        spriteLlave = CampoSprite("Llave", spriteLlave);
        spriteCajaCerrada = CampoSprite("Caja cerrada", spriteCajaCerrada);
        spriteCajaAbierta = CampoSprite("Caja abierta (opcional)", spriteCajaAbierta);
        spritePuertaCerrada = CampoSprite("Puerta cerrada", spritePuertaCerrada);
        spritePuertaAbierta = CampoSprite("Puerta abierta (opcional)", spritePuertaAbierta);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Jugador", EditorStyles.boldLabel);
        prefabTheo = (GameObject)EditorGUILayout.ObjectField("Prefab de Theo (opcional)", prefabTheo, typeof(GameObject), false);
        if (prefabTheo == null)
        {
            EditorGUILayout.HelpBox("Sin prefab de Theo se usa un jugador de prueba temporal.", MessageType.None);
            spriteJugador = CampoSprite("Sprite jugador de prueba", spriteJugador);
        }

        EditorGUILayout.Space();
        siguienteEscena = EditorGUILayout.TextField("Siguiente escena (opcional)", siguienteEscena);

        EditorGUILayout.Space();
        if (GUILayout.Button("Restaurar sprites provisionales")) CargarSpritesIncluidos(true);
        GUILayout.Space(6);
        if (GUILayout.Button("Construir Nivel 1", GUILayout.Height(40))) EditorApplication.delayCall += Construir;

        EditorGUILayout.EndScrollView();
    }

    static Sprite CampoSprite(string etiqueta, Sprite valor)
    {
        return (Sprite)EditorGUILayout.ObjectField(etiqueta, valor, typeof(Sprite), false,
            GUILayout.Height(EditorGUIUtility.singleLineHeight));
    }

    // ---------- Sprites provisionales incluidos ----------

    static void PrepararSpritesIncluidos()
    {
        foreach (string nombre in SpritesIncluidos)
        {
            string ruta = CarpetaSprites + "/" + nombre + ".png";
            var imp = AssetImporter.GetAtPath(ruta) as TextureImporter;
            if (imp == null) continue;
            bool correcto = imp.textureType == TextureImporterType.Sprite
                            && imp.spriteImportMode == SpriteImportMode.Single
                            && Mathf.Approximately(imp.spritePixelsPerUnit, 16f)
                            && imp.filterMode == FilterMode.Point
                            && imp.textureCompression == TextureImporterCompression.Uncompressed;
            if (correcto) continue;
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 16f;
            imp.filterMode = FilterMode.Point;
            imp.textureCompression = TextureImporterCompression.Uncompressed;
            imp.mipmapEnabled = false;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }
    }

    void CargarSpritesIncluidos(bool reemplazar)
    {
        spriteTierra = Cargar("suelo_tierra", spriteTierra, reemplazar);
        spriteCesped = Cargar("suelo_cesped", spriteCesped, reemplazar);
        spritePlataforma = Cargar("plataforma_tronco", spritePlataforma, reemplazar);
        spriteFondo = Cargar("fondo_bosque", spriteFondo, reemplazar);
        spriteTrampa = Cargar("trampa_espinas", spriteTrampa, reemplazar);
        spriteLlave = Cargar("llave", spriteLlave, reemplazar);
        spriteCajaCerrada = Cargar("caja_cerrada", spriteCajaCerrada, reemplazar);
        spriteCajaAbierta = Cargar("caja_abierta", spriteCajaAbierta, reemplazar);
        spritePuertaCerrada = Cargar("puerta_cerrada", spritePuertaCerrada, reemplazar);
        spritePuertaAbierta = Cargar("puerta_abierta", spritePuertaAbierta, reemplazar);
        spriteJugador = Cargar("jugador_prueba", spriteJugador, reemplazar);
    }

    static Sprite Cargar(string nombre, Sprite actual, bool reemplazar)
    {
        if (actual != null && !reemplazar) return actual;
        return AssetDatabase.LoadAssetAtPath<Sprite>(CarpetaSprites + "/" + nombre + ".png");
    }

    // ---------- Construcción del nivel ----------

    void Construir()
    {
        if (spriteTierra == null || spriteCesped == null || spritePlataforma == null || spriteTrampa == null ||
            spriteLlave == null || spriteCajaCerrada == null || spritePuertaCerrada == null ||
            (prefabTheo == null && spriteJugador == null))
        {
            EditorUtility.DisplayDialog("Faltan sprites",
                "Hay campos obligatorios vacíos. Presioná 'Restaurar sprites provisionales' o asigná los sprites.", "OK");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (File.Exists(RutaEscena) &&
            !EditorUtility.DisplayDialog("Reemplazar nivel",
                "Ya existe la escena Nivel1_ClaroRobles. ¿Querés reemplazarla por el nivel nuevo?",
                "Sí, reemplazar", "Cancelar"))
            return;

        int capaSuelo = AsegurarCapa(NombreCapaSuelo);
        if (!AssetDatabase.IsValidFolder(CarpetaTiles)) AssetDatabase.CreateFolder(Carpeta, "Tiles");

        Tile tileTierra = CrearTile("TileTierra", spriteTierra);
        Tile tileCesped = CrearTile("TileCesped", spriteCesped);
        Tile tilePlataforma = CrearTile("TilePlataforma", spritePlataforma);
        AssetDatabase.SaveAssets();

        var escena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        int alto = Mapa.Length;
        int ancho = Mapa[0].Length;

        // Cámara
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.62f, 0.82f, 0.90f);
        camGO.transform.position = new Vector3(10f, 7f, -10f);
        camGO.AddComponent<AudioListener>();
        var seguir = camGO.AddComponent<CamaraSeguir>();
        seguir.minX = 0f;
        seguir.maxX = ancho;
        seguir.minY = 0f;
        seguir.maxY = alto + 2f;

        CrearLuzGlobal2D();

        // Fondo
        if (spriteFondo != null) CrearFondo(spriteFondo, ancho, alto);

        // Cuadrícula y suelo sólido
        var gridGO = new GameObject("Grid");
        gridGO.AddComponent<Grid>();
        var sueloGO = new GameObject("Suelo");
        sueloGO.transform.SetParent(gridGO.transform, false);
        var suelo = sueloGO.AddComponent<Tilemap>();
        var rendererSuelo = sueloGO.AddComponent<TilemapRenderer>();
        rendererSuelo.sortingOrder = 0;
        if (capaSuelo >= 0) sueloGO.layer = capaSuelo;
        var colTilemap = sueloGO.AddComponent<TilemapCollider2D>();
        var rbSuelo = sueloGO.AddComponent<Rigidbody2D>();
        rbSuelo.bodyType = RigidbodyType2D.Static;
        var compuesto = sueloGO.AddComponent<CompositeCollider2D>();
        compuesto.geometryType = CompositeCollider2D.GeometryType.Polygons;
#if UNITY_2023_1_OR_NEWER
        colTilemap.compositeOperation = Collider2D.CompositeOperation.Merge;
#else
        colTilemap.usedByComposite = true;
#endif

        var objetos = new GameObject("Objetos").transform;
        var trampas = new GameObject("Trampas").transform;
        trampas.SetParent(objetos);
        var cajas = new GameObject("Cajas").transform;
        cajas.SetParent(objetos);

        Vector3 inicio = new Vector3(3.5f, 4f, 0f);

        for (int fila = 0; fila < alto; fila++)
        {
            for (int c = 0; c < ancho; c++)
            {
                char ch = c < Mapa[fila].Length ? Mapa[fila][c] : '.';
                int y = alto - 1 - fila;
                var celda = new Vector3Int(c, y, 0);
                Vector3 baseCelda = suelo.CellToWorld(celda) + new Vector3(0.5f, 0f, 0f); // centro inferior

                switch (ch)
                {
                    case '#':
                        bool tierraArriba = fila > 0 && Mapa[fila - 1][c] == '#';
                        suelo.SetTile(celda, tierraArriba ? tileTierra : tileCesped);
                        break;
                    case '=':
                        suelo.SetTile(celda, tilePlataforma);
                        break;
                    case '^':
                    {
                        var t = CrearObjeto("Trampa", spriteTrampa, baseCelda, 1f, 1f, 2, trampas);
                        var box = Caja(t);
                        box.isTrigger = true;
                        box.size = new Vector2(box.size.x * 0.8f, box.size.y * 0.5f);
                        box.offset = new Vector2(box.offset.x, box.offset.y - box.size.y * 0.5f);
                        t.AddComponent<Trampa>();
                        break;
                    }
                    case 'K':
                    {
                        var k = CrearObjeto("Llave", spriteLlave, baseCelda + Vector3.up * 0.1f, 0.8f, 0.8f, 5, objetos);
                        var circulo = k.AddComponent<CircleCollider2D>();
                        circulo.radius = Mathf.Max(spriteLlave.bounds.extents.x, spriteLlave.bounds.extents.y);
                        circulo.offset = spriteLlave.bounds.center;
                        circulo.isTrigger = true;
                        k.AddComponent<Llave>();
                        break;
                    }
                    case 'B':
                    {
                        var b = CrearObjeto("CajaRecompensa", spriteCajaCerrada, baseCelda, 0.9f, 1f, 3, cajas);
                        Caja(b).isTrigger = true;
                        var caja = b.AddComponent<CajaRecompensa>();
                        caja.spriteAbierta = spriteCajaAbierta;
                        break;
                    }
                    case 'D':
                    {
                        var d = CrearObjeto("PuertaSalida", spritePuertaCerrada, baseCelda, 2f, 1.5f, 3, objetos);
                        Caja(d).isTrigger = true;
                        var puerta = d.AddComponent<PuertaSalida>();
                        puerta.spriteAbierta = spritePuertaAbierta;
                        puerta.siguienteEscena = siguienteEscena;
                        break;
                    }
                    case 'P':
                        inicio = baseCelda + Vector3.up * 0.05f;
                        break;
                }
            }
        }

        // Zona de caída (debajo de todo el nivel)
        var zona = new GameObject("ZonaCaida");
        zona.transform.position = new Vector3(ancho / 2f, -3f, 0f);
        var colZona = zona.AddComponent<BoxCollider2D>();
        colZona.isTrigger = true;
        colZona.size = new Vector2(ancho + 40f, 2f);
        zona.AddComponent<Trampa>();

        // Jugador
        GameObject jugador;
        if (prefabTheo != null)
        {
            jugador = (GameObject)PrefabUtility.InstantiatePrefab(prefabTheo);
            jugador.transform.position = inicio + Vector3.up * 0.5f;
            if (!jugador.CompareTag("Player")) jugador.tag = "Player";
        }
        else
        {
            jugador = CrearObjeto("JugadorPrueba", spriteJugador, inicio, 0.9f, 0.9f, 10, null);
            jugador.tag = "Player";
            var box = Caja(jugador);
            box.size = new Vector2(box.size.x * 0.8f, box.size.y);
            var rb = jugador.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            var control = jugador.AddComponent<JugadorPrueba>();
            if (capaSuelo >= 0) control.capaSuelo = 1 << capaSuelo;
        }
        seguir.objetivo = jugador.transform;
        camGO.transform.position = new Vector3(jugador.transform.position.x + 6f, 7f, -10f);

        // Interfaz en pantalla
        new GameObject("HUD").AddComponent<HUDNivel>();

        EditorSceneManager.SaveScene(escena, RutaEscena);
        AgregarABuildSettings(RutaEscena);
        Selection.activeGameObject = jugador;

        EditorUtility.DisplayDialog("Nivel 1 listo",
            "Se creó y guardó la escena Nivel1_ClaroRobles.\n\nPresioná Play para probarla.\n" +
            "Controles: A/D o flechas para moverse, Espacio para saltar.", "OK");
    }

    // ---------- Ayudantes ----------

    // Agrega un BoxCollider2D del tamaño exacto del sprite del objeto.
    static BoxCollider2D Caja(GameObject go)
    {
        var box = go.AddComponent<BoxCollider2D>();
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            box.size = sr.sprite.bounds.size;
            box.offset = sr.sprite.bounds.center;
        }
        return box;
    }

    static Tile CrearTile(string nombre, Sprite sprite)
    {
        string ruta = CarpetaTiles + "/" + nombre + ".asset";
        var tile = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
        if (tile == null)
        {
            tile = CreateInstance<Tile>();
            AssetDatabase.CreateAsset(tile, ruta);
        }
        tile.sprite = sprite;
        tile.colliderType = Tile.ColliderType.Grid;
        float lado = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float escala = lado > 0f ? 1f / lado : 1f; // ajusta cualquier sprite a 1 cuadro
        tile.transform = Matrix4x4.Scale(new Vector3(escala, escala, 1f));
        tile.flags = TileFlags.LockTransform | TileFlags.LockColor;
        EditorUtility.SetDirty(tile);
        return tile;
    }

    // Crea un objeto con sprite, escalado para caber en (anchoMax x altoMax) y apoyado en "baseCentro".
    static GameObject CrearObjeto(string nombre, Sprite sprite, Vector3 baseCentro, float altoMax, float anchoMax,
        int orden, Transform padre)
    {
        var go = new GameObject(nombre);
        if (padre != null) go.transform.SetParent(padre, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = orden;
        Vector2 tam = sprite.bounds.size;
        float escala = Mathf.Min(altoMax / Mathf.Max(tam.y, 0.0001f), anchoMax / Mathf.Max(tam.x, 0.0001f));
        go.transform.localScale = new Vector3(escala, escala, 1f);
        go.transform.position = baseCentro;
        Bounds b = sr.bounds;
        go.transform.position += new Vector3(baseCentro.x - b.center.x, baseCentro.y - b.min.y, 0f);
        return go;
    }

    static void CrearFondo(Sprite sprite, int ancho, int alto)
    {
        var contenedor = new GameObject("Fondo").transform;
        float altoFondo = alto + 6f;
        float escala = altoFondo / sprite.bounds.size.y;
        float anchoCopia = sprite.bounds.size.x * escala;
        int i = 0;
        for (float x = -10f; x < ancho + 10f; x += anchoCopia, i++)
        {
            var go = new GameObject("Fondo_" + i);
            go.transform.SetParent(contenedor, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = -20;
            go.transform.localScale = new Vector3(escala, escala, 1f);
            go.transform.position = Vector3.zero;
            Bounds b = sr.bounds;
            go.transform.position = new Vector3(x - b.min.x, -3f - b.min.y, 0f);
        }
    }

    // En proyectos URP 2D los sprites se ven negros sin una luz global 2D.
    static void CrearLuzGlobal2D()
    {
        Type tipo = Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        if (tipo == null) return;
        var go = new GameObject("Global Light 2D");
        Component luz = go.AddComponent(tipo);
        bool listo = false;
        try
        {
            var prop = tipo.GetProperty("lightType");
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(luz, Enum.Parse(prop.PropertyType, "Global"));
                listo = true;
            }
        }
        catch (Exception) { }
        if (!listo)
        {
            var so = new SerializedObject(luz);
            var p = so.FindProperty("m_LightType");
            if (p != null)
            {
                p.intValue = 4; // Global
                so.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }

    static int AsegurarCapa(string nombre)
    {
        int existente = LayerMask.NameToLayer(nombre);
        if (existente >= 0) return existente;

        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var capas = tagManager.FindProperty("layers");
        for (int i = 8; i < capas.arraySize; i++)
        {
            var capa = capas.GetArrayElementAtIndex(i);
            if (string.IsNullOrEmpty(capa.stringValue))
            {
                capa.stringValue = nombre;
                tagManager.ApplyModifiedProperties();
                return i;
            }
        }
        Debug.LogWarning("No hay espacio para crear la capa '" + nombre + "'.");
        return -1;
    }

    static void AgregarABuildSettings(string ruta)
    {
        var escenas = EditorBuildSettings.scenes.ToList();
        if (escenas.Any(e => e.path == ruta)) return;
        escenas.Add(new EditorBuildSettingsScene(ruta, true));
        EditorBuildSettings.scenes = escenas.ToArray();
    }
}
