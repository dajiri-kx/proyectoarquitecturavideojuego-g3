using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// Personaje TEMPORAL para probar el nivel mientras el Integrante 1 termina el prefab de Theo.
// Controles: A/D o flechas para moverse, Espacio / W / flecha arriba para saltar.
[RequireComponent(typeof(Rigidbody2D))]
public class JugadorPrueba : MonoBehaviour
{
    public float velocidad = 6f;
    public float fuerzaSalto = 14.5f;
    public LayerMask capaSuelo;

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
        // Sin fricción para que no se quede pegado a las paredes
        if (col != null) col.sharedMaterial = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
        if (capaSuelo.value == 0) capaSuelo = LayerMask.GetMask("Ground");
    }

    void Update()
    {
        float x = LeerHorizontal();
        Vector2 v = Velocidad;
        v.x = x * velocidad;
        if (SaltoPresionado() && EnSuelo()) v.y = fuerzaSalto;
        Velocidad = v;

        if (sr != null && x != 0f) sr.flipX = x < 0f;
    }

    bool EnSuelo()
    {
        Bounds b = col.bounds;
        Vector2 centro = new Vector2(b.center.x, b.min.y - 0.05f);
        Vector2 tamano = new Vector2(b.size.x * 0.9f, 0.1f);
        return Physics2D.OverlapBox(centro, tamano, 0f, capaSuelo) != null;
    }

    Vector2 Velocidad
    {
#if UNITY_6000_0_OR_NEWER
        get { return rb.linearVelocity; }
        set { rb.linearVelocity = value; }
#else
        get { return rb.velocity; }
        set { rb.velocity = value; }
#endif
    }

    float LeerHorizontal()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard k = Keyboard.current;
        if (k == null) return 0f;
        float x = 0f;
        if (k.aKey.isPressed || k.leftArrowKey.isPressed) x -= 1f;
        if (k.dKey.isPressed || k.rightArrowKey.isPressed) x += 1f;
        return x;
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    bool SaltoPresionado()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard k = Keyboard.current;
        return k != null && (k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#endif
    }
}
