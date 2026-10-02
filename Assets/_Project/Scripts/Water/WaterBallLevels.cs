// <<hecho por IA>> 2026-10-02 · prototipo
using UnityEngine;

namespace MeltTheDeep
{
    /// <summary>
    /// Niveles de fusión de las bolitas. Cada elemento de la lista es un nivel: para añadir uno
    /// basta con añadir un elemento en el Inspector, sin tocar código.
    /// </summary>
    [CreateAssetMenu(fileName = "WaterBallLevels", menuName = "Melt the Deep/Water Ball Levels")]
    public class WaterBallLevels : ScriptableObject
    {
        [System.Serializable]
        public struct Level
        {
            [Tooltip("Radio en metros. Según el TDD, crece con la raíz cúbica del volumen.")]
            public float radius;
            [Tooltip("Litros de agua. Para que la fusión conserve el agua, cada nivel debe tener el doble que el anterior.")]
            public float volume;
            [Tooltip("Un material por nivel, en vez de cambiar el color de cada objeto, para no romper el SRP Batcher.")]
            public Material material;
        }

        [Tooltip("Del nivel 0 (el que suelta el hielo) al último, que ya no se fusiona más.")]
        [SerializeField] Level[] levels;

        public int MaxLevel => levels.Length - 1;

        public Level Get(int level) => levels[level];
    }
}
