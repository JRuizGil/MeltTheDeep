// <<hecho por IA>> 2026-10-02 · prototipo
using TMPro;
using UnityEngine;

namespace MeltTheDeep
{
    /// <summary>
    /// Texto del HUD con el porcentaje del tanque. Solo se actualiza cuando el tanque avisa de un cambio,
    /// en vez de preguntarle en cada frame.
    /// </summary>
    public class TankHUD : MonoBehaviour
    {
        [SerializeField] CollectionTank tank;
        [SerializeField] TMP_Text label;

        void OnEnable()
        {
            tank.Changed += Refresh;
            Refresh(tank);
        }

        void OnDisable() => tank.Changed -= Refresh;

        void Refresh(CollectionTank t)
        {
            // Floor y no Round: así solo marca 100 % cuando está lleno de verdad.
            int percent = Mathf.FloorToInt(t.Fill * 100f);
            label.text = $"Tanque {percent} %  ({t.Liters:0}/{t.Capacity:0} L)";
            if (t.IsFull) label.text += "\nLLENO · pulsa E para vaciar";
        }
    }
}
