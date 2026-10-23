using UnityEngine;
using InkRun.Core;

namespace InkRun.World
{
    public enum PickupKind { Coin, Magnet, Dash }

    /// <summary>Coins and power-ups. Coins drift toward the hero while a magnet is active.</summary>
    public class Pickup : MonoBehaviour
    {
        public PickupKind Kind;
        public float SpinDegreesPerSecond = 240f;
        public float MagnetPullSpeed = 14f;

        bool _collected;

        void Update()
        {
            transform.Rotate(0f, SpinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
            var gm = GameManager.Instance;
            if (Kind == PickupKind.Coin && gm != null && gm.MagnetTimeLeft > 0f)
            {
                float ahead = transform.position.z;
                if (ahead > -1f && ahead < gm.Tuning.MagnetRange)
                {
                    Vector3 target = gm.Player.transform.position + Vector3.up * 0.6f;
                    target.z = transform.position.z;
                    transform.position = Vector3.MoveTowards(transform.position, target, MagnetPullSpeed * Time.deltaTime);
                }
            }
        }

        public void Collect(GameManager gm)
        {
            if (_collected) return;
            _collected = true;
            switch (Kind)
            {
                case PickupKind.Coin: gm.CollectCoin(transform.position); break;
                case PickupKind.Magnet: gm.ActivateMagnet(); break;
                case PickupKind.Dash: gm.ActivateDash(); break;
            }
            gameObject.SetActive(false);
        }
    }
}
