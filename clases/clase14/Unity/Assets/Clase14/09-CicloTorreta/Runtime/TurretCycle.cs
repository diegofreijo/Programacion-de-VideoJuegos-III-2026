using UnityEngine;

namespace Clase14
{
    /// <summary>
    /// Maneja a una Turret con un ciclo de estados (el asset que genera el grafo .turretcycle): la deja
    /// disparar o no, ajusta su cadencia y la pinta del color del Estado.
    /// </summary>
    [RequireComponent(typeof(Turret))]
    public class TurretCycle : MonoBehaviour
    {
        public TurretCycleAsset cycle;

        public TurretCycleRunner Runner { get; private set; }

        Turret m_Turret;
        float m_BaseShotsPerSecond;
        SpriteRenderer m_Renderer;
        TextMesh m_Label;

        void Awake() => Begin();

        void OnDestroy()
        {
            if (m_Turret != null) m_Turret.Fired -= OnFired;
        }

        /// <summary>Arranca el ciclo en su Estado inicial. Lo llama Awake; en Edit Mode, quien lo necesite (la captura).</summary>
        public void Begin()
        {
            m_Turret = GetComponent<Turret>();
            m_Turret.Fired -= OnFired;
            m_Turret.Fired += OnFired;
            m_BaseShotsPerSecond = m_Turret.shotsPerSecond;

            m_Renderer = GetComponent<SpriteRenderer>();
            if (m_Renderer == null)
                m_Renderer = SpriteFactory.AddRenderer(gameObject, SpriteFactory.Square, Color.white, 3);
            if (m_Label == null)
                m_Label = WorldText.Create(transform, "Estado", new Vector3(0f, 0.9f, 0f), 0.5f, Color.white, TextAnchor.LowerCenter);

            Runner = new TurretCycleRunner(cycle);
            Apply();
        }

        void OnFired() => Runner?.RegisterShot();

        void Update()
        {
            if (Runner == null) return;
            if (Runner.Tick(Time.deltaTime, m_Turret.HasEnemyInRange))
                Apply();
        }

        /// <summary>Pasa el Estado actual a la torreta: si dispara, su cadencia y su color.</summary>
        public void Apply()
        {
            var state = Runner?.Current;
            if (state == null) return;
            m_Turret.canFire = state.canFire;
            m_Turret.shotsPerSecond = m_BaseShotsPerSecond * state.fireRateMultiplier;
            m_Renderer.color = state.color;
            m_Label.text = state.name;
            m_Label.color = state.color;
        }
    }
}
