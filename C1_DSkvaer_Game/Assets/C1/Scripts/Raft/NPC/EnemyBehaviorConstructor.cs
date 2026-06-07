using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Ship {
    [DisallowMultipleComponent]
    public class EnemyBehaviorConstructor : MonoBehaviour {
        [Header("Профиль поведения")]
        [InspectorLabel("Профиль тактик")]
        [Tooltip("ScriptableObject со списком тактик, приоритетами, весами и задержками. Он определяет, как NPC выбирает поведение.")]
        [SerializeField] private EnemyBehaviorProfileConfig profile;

        [InspectorLabel("NPC AI")]
        [Tooltip("Основной компонент искусственного интеллекта, который получает готовый список тактик.")]
        [SerializeField] private NPCAI npcAI;

        [InspectorLabel("Компоненты тактик")]
        [Tooltip("Компоненты тактик на этом враге или его дочерних объектах. Если список пуст, он заполнится автоматически.")]
        [SerializeField] private List<MonoBehaviour> tacticComponents = new();

        private void Awake()
        {
            CacheComponents();
        }

        private void Start()
        {
            ApplyProfile();
        }

        public void SetProfile(EnemyBehaviorProfileConfig newProfile)
        {
            profile = newProfile;
            ApplyProfile();
        }

        public void ApplyProfile()
        {
            CacheComponents();

            if (npcAI == null) {
                ;
                return;
            }

            if (profile == null) {
                npcAI.SetBehaviorProfile(null);
                npcAI.SetTactics(tacticComponents);
                return;
            }

            List<MonoBehaviour> orderedTactics = new();
            foreach (EnemyBehaviorTacticSlot slot in profile.Tactics.OrderBy(slot => slot.Priority)) {
                if (slot == null || !slot.Enabled) {
                    continue;
                }

                MonoBehaviour tactic = FindTactic(slot.Kind);
                if (tactic != null && !orderedTactics.Contains(tactic)) {
                    orderedTactics.Add(tactic);
                }
            }

            if (orderedTactics.Count == 0) {
                orderedTactics.AddRange(tacticComponents);
            }

            npcAI.SetDecisionInterval(profile.DecisionInterval);
            npcAI.SetBehaviorProfile(profile);
            npcAI.SetTactics(orderedTactics);
        }

        private void CacheComponents()
        {
            if (npcAI == null) {
                npcAI = GetComponent<NPCAI>();
            }

            if (tacticComponents == null || tacticComponents.Count == 0) {
                tacticComponents = GetComponentsInChildren<MonoBehaviour>(true)
                    .Where(component => component is IEnemyTactic)
                    .ToList();
            }
        }

        private MonoBehaviour FindTactic(EnemyTacticKind kind)
        {
            foreach (MonoBehaviour tactic in tacticComponents) {
                if (tactic == null) {
                    continue;
                }

                if (GetKind(tactic) == kind) {
                    return tactic;
                }
            }

            return null;
        }

        private static EnemyTacticKind GetKind(MonoBehaviour tactic)
        {
            return tactic switch {
                PatrolTactic => EnemyTacticKind.Patrol,
                RamAttackTactic => EnemyTacticKind.RamAttack,
                NPCShootingTactic => EnemyTacticKind.Shooting,
                FleeTactic => EnemyTacticKind.Flee,
                WolfHuntTactic => EnemyTacticKind.WolfHunt,
                DeadRockAmbushTactic => EnemyTacticKind.DeadRockAmbush,
                FireShipBlockadeTactic => EnemyTacticKind.FireShipBlockade,
                HookAndRamTactic => EnemyTacticKind.HookAndRam,
                HazardDroppingTactic => EnemyTacticKind.HazardDropping,
                SniperHarassTactic => EnemyTacticKind.SniperHarass,
                ShallowsLureTactic => EnemyTacticKind.ShallowsLure,
                FalseRetreatTactic => EnemyTacticKind.FalseRetreat,
                SmokeVeilTactic => EnemyTacticKind.SmokeVeil,
                SuicideExplosionTactic => EnemyTacticKind.SuicideExplosion,
                _ => EnemyTacticKind.Custom
            };
        }
    }
}
