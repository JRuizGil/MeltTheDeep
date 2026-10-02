// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;
using UnityEngine.Pool;

namespace MeltTheDeep
{
    /// <summary>
    /// Pool de bolitas de agua: las crea una vez y las reutiliza activándolas y desactivándolas,
    /// en vez de instanciar y destruir. Convierte los litros de hielo fundido en bolitas.
    /// </summary>
    public class WaterBallPool : MonoBehaviour
    {
        [Header("Bolitas")]
        [SerializeField] WaterBall prefab;
        [Tooltip("Tabla de niveles: radio, litros y material de cada nivel de fusión.")]
        [SerializeField] WaterBallLevels levels;
        [Tooltip("Litros de hielo que hay que fundir para soltar una bolita. Regula cuántas salen por segundo.")]
        [SerializeField, Min(0.1f)] float iceLitersPerBall = 10f;

        [Header("Salida")]
        [Tooltip("Hueco entre la bolita y la superficie del hielo al aparecer, en metros (se suma a su radio).")]
        [SerializeField, Min(0f)] float spawnOffset = 0.1f;
        [Tooltip("Velocidad con la que sale despedida desde la superficie.")]
        [SerializeField] float popSpeed = 0.4f;
        [Tooltip("Desorden aleatorio hacia los lados de la posición de salida, para que no salgan una encima de otra.")]
        [SerializeField, Min(0f)] float spawnJitter = 0.1f;

        [Header("Límites")]
        [Tooltip("Máximo de bolitas activas a la vez (tope de rendimiento del TDD).")]
        [SerializeField, Min(1)] int maxActive = 400;
        [Tooltip("Bolitas que se crean al empezar, para no instanciar en mitad del juego.")]
        [SerializeField, Min(0)] int prewarm = 100;
        [Tooltip("Si una bolita cae por debajo de esta altura, vuelve al pool.")]
        [SerializeField] float killHeight = -10f;

        ObjectPool<WaterBall> pool;
        float pendingLiters; // hielo fundido que aún no llega para una bolita

        public WaterBallLevels Levels => levels;
        public float KillHeight => killHeight;
        public int ActiveCount => pool.CountActive;

        void Awake()
        {
            pool = new ObjectPool<WaterBall>(
                createFunc: CreateBall,
                actionOnGet: null,                                        // la activa WaterBall.Spawn, ya colocada
                actionOnRelease: ball => ball.gameObject.SetActive(false),
                actionOnDestroy: ball => Destroy(ball.gameObject),
                collectionCheck: true,                                    // avisa si se devuelve dos veces la misma
                defaultCapacity: prewarm,
                maxSize: maxActive);

            // Precalentar: crearlas ya y devolverlas, para que luego solo haya que activarlas.
            var created = new WaterBall[prewarm];
            for (int i = 0; i < prewarm; i++) created[i] = pool.Get();
            foreach (WaterBall ball in created) pool.Release(ball);
        }

        WaterBall CreateBall()
        {
            WaterBall ball = Instantiate(prefab, transform);
            ball.gameObject.SetActive(false);
            ball.SetPool(this);
            return ball;
        }

        /// <summary>
        /// Recibe el hielo fundido en un punto de la superficie y suelta una bolita por cada iceLitersPerBall litros.
        /// </summary>
        public void AddMeltedIce(float iceLiters, Vector3 surfacePoint, Vector3 surfaceNormal)
        {
            pendingLiters += iceLiters;
            while (pendingLiters >= iceLitersPerBall)
            {
                pendingLiters -= iceLitersPerBall;

                // Con el tope alcanzado no se crean más y esa agua se pierde (simplificación del prototipo).
                if (pool.CountActive >= maxActive) continue;

                // Sale por el lado del aire (la normal apunta fuera del hielo), a su radio más el hueco, para no
                // nacer tocando el hielo: el collider de malla es hueco y una bolita que nace dentro cae a través.
                // El desorden va solo hacia los lados (en el plano de la superficie), nunca hacia dentro.
                float distance = levels.Get(0).radius + spawnOffset;
                Vector3 sideways = Vector3.ProjectOnPlane(Random.insideUnitSphere, surfaceNormal) * spawnJitter;
                Vector3 position = surfacePoint + surfaceNormal * distance + sideways;
                Vector3 velocity = (surfaceNormal + Random.insideUnitSphere * 0.5f) * popSpeed;
                pool.Get().Spawn(0, position, velocity); // el hielo siempre suelta bolitas de nivel 0
            }
        }

        public void Release(WaterBall ball) => pool.Release(ball);
    }
}
