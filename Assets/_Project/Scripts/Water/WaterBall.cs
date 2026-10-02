// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;

namespace MeltTheDeep
{
    /// <summary>
    /// Bolita de agua con física. Sale del WaterBallPool y vuelve a él cuando deja de usarse
    /// (se aspira, se fusiona con otra o se cae fuera del mapa). Dos bolitas del mismo nivel
    /// que se tocan se fusionan en una del nivel siguiente, como en Suika.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class WaterBall : MonoBehaviour
    {
        public Rigidbody Body { get; private set; }
        public int Level { get; private set; }
        /// <summary>Litros de agua que lleva: lo que sumará al tanque.</summary>
        public float Volume { get; private set; }

        WaterBallPool pool;
        MeshRenderer meshRenderer;

        void Awake()
        {
            Body = GetComponent<Rigidbody>();
            meshRenderer = GetComponent<MeshRenderer>();
        }

        public void SetPool(WaterBallPool owner) => pool = owner;

        /// <summary>La coloca y la activa. La mueve mientras está desactivada para que la física no la vea "teletransportarse".</summary>
        public void Spawn(int level, Vector3 position, Vector3 velocity)
        {
            ApplyLevel(level);
            transform.SetPositionAndRotation(position, Random.rotation);
            gameObject.SetActive(true);
            Body.linearVelocity = velocity;
            Body.angularVelocity = Vector3.zero;
        }

        /// <summary>Devuelve la bolita al pool: se desactiva y queda lista para reutilizarse.</summary>
        public void Release() => pool.Release(this);

        // Tamaño, volumen, masa y color salen de la tabla de niveles.
        void ApplyLevel(int level)
        {
            WaterBallLevels.Level data = pool.Levels.Get(level);
            Level = level;
            Volume = data.volume;
            Body.mass = data.volume;                                 // 1 litro de agua pesa 1 kg
            transform.localScale = Vector3.one * (data.radius * 2f); // la esfera de Unity mide 1 de diámetro
            meshRenderer.sharedMaterial = data.material;
        }

        void OnCollisionEnter(Collision collision) => TryMerge(collision);

        // Stay cubre a dos bolitas que ya se tocaban cuando una de ellas subió de nivel:
        // ese contacto no vuelve a disparar Enter. Las bolitas dormidas no generan Stay.
        void OnCollisionStay(Collision collision) => TryMerge(collision);

        void TryMerge(Collision collision)
        {
            if (!isActiveAndEnabled) return;
            if (!collision.collider.TryGetComponent(out WaterBall other) || !other.isActiveAndEnabled) return;
            if (other.Level != Level || Level >= pool.Levels.MaxLevel) return;

            // Las dos bolitas reciben este mismo choque. Solo lo resuelve la de menor InstanceID,
            // así cada pareja se fusiona una sola vez.
            if (GetInstanceID() > other.GetInstanceID()) return;

            // La superviviente queda entre las dos y conserva el momento lineal:
            // con masas iguales, m·v1 + m·v2 = 2m·v, así que v es la media.
            Vector3 middle = (Body.position + other.Body.position) * 0.5f;
            Vector3 velocity = (Body.linearVelocity + other.Body.linearVelocity) * 0.5f;

            other.Release();
            ApplyLevel(Level + 1);
            Body.position = middle;
            Body.linearVelocity = velocity;
        }

        void FixedUpdate()
        {
            // Red de seguridad: si se sale del mapa, vuelve al pool en vez de caer para siempre.
            if (pool != null && transform.position.y < pool.KillHeight) Release();
        }
    }
}
