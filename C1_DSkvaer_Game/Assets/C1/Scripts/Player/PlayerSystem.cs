using System;
using System.Collections.Generic;
using C1.Platformer.Characters.RPG;
using Menu_Journal;
using Menu_Journal.Systems;
using Ship;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace C1.Player {
    public enum PlayerWorldMode {
        TopDown,
        Platformer
    }

    public enum PlayerJournalEntryType {
        Character,
        Skill,
        Ship,
        ActiveQuest,
        CompletedQuest,
        Person,
        Location,
        Bestiary,
        Rumor,
        MapMarker,
        System
    }

    public enum PlayerDeathContext {
        Platformer,
        ShipTopDown
    }

    public enum PlayerShipOwnership {
        Starter,
        Purchased,
        Reward,
        Temporary
    }

    [Serializable]
    public sealed class PlayerJournalEntry {
        public string Id;
        public PlayerJournalEntryType Type;
        public string Title;
        [TextArea(2, 8)] public string Summary;
        [TextArea(4, 14)] public string Body;
        public Sprite Icon;
        public bool IsKnown = true;
        public bool IsCompleted;
        public int SortOrder;

        public PlayerJournalEntry Clone()
        {
            return (PlayerJournalEntry)MemberwiseClone();
        }
    }

    [Serializable]
    public sealed class PlayerQuestState {
        public string Id;
        public string Title;
        [TextArea(3, 12)] public string Description;
        public string Reward;
        public string TimeLimit;
        public bool IsActive = true;
        public bool IsCompleted;
        public int SortOrder;

        public PlayerJournalEntry ToJournalEntry()
        {
            return new PlayerJournalEntry {
                Id = Id,
                Type = IsCompleted ? PlayerJournalEntryType.CompletedQuest : PlayerJournalEntryType.ActiveQuest,
                Title = Title,
                Summary = Reward,
                Body = BuildBody(),
                IsCompleted = IsCompleted,
                SortOrder = SortOrder
            };
        }

        private string BuildBody()
        {
            string body = Description ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(Reward)) {
                body += $"\n\nReward: {Reward}";
            }

            if (!string.IsNullOrWhiteSpace(TimeLimit)) {
                body += $"\nTime: {TimeLimit}";
            }

            return body;
        }
    }

    [Serializable]
    public sealed class PlayerMapMarkerState {
        public string Id;
        public string Title;
        [TextArea(2, 8)] public string Description;
        public Vector2 WorldPosition;
        public bool IsDiscovered;
        public bool IsGlobalMapMarker;
    }

    [Serializable]
    public sealed class PlayerShipState {
        public string ShipId = "starter_ship";
        public string DisplayName = "Starter ship";
        public PlayerShipOwnership Ownership = PlayerShipOwnership.Starter;
        public bool IsStarterShip = true;
        public int Deaths;
        public int MaxDeathsBeforePermanentLoss = 3;
        public int LuckProtectionUsed;
        public bool IsPermanentlyDestroyed;

        public bool CanRestoreForFree => IsStarterShip || Ownership == PlayerShipOwnership.Starter;
        public int RemainingDeaths => Mathf.Max(0, MaxDeathsBeforePermanentLoss - Deaths);
    }

    [Serializable]
    public sealed class PlayerRuntimeState {
        public PlayerWorldMode WorldMode;
        public string CurrentScene;
        public string LastSafePortId;
        public string LastSafePortScene;
        public Vector3 LastSafePortPosition;
        public int PlatformerDeaths;
        public int ShipDeaths;
        public PlayerShipState CurrentShip = new PlayerShipState();
        public List<PlayerQuestState> Quests = new List<PlayerQuestState>();
        public List<PlayerJournalEntry> Knowledge = new List<PlayerJournalEntry>();
        public List<PlayerMapMarkerState> MapMarkers = new List<PlayerMapMarkerState>();
    }

    [Serializable]
    public sealed class PlayerHudMessage {
        public string Title;
        public string Body;
        public float Duration = 3f;
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("C1/Player/Player System")]
    public sealed class PlayerSystem : MonoBehaviour {
        private const string SaveKey = "C1.PlayerSystem.Save";

        public static PlayerSystem Instance { get; private set; }

        [Header("Development")]
        [InspectorLabel("Save Progress")]
        [Tooltip("Development toggle. Disable to test without writing progress to PlayerPrefs.")]
        [SerializeField] private bool saveProgress = true;

        [InspectorLabel("Load On Start")]
        [Tooltip("If enabled, PlayerSystem tries to load saved runtime data on Awake.")]
        [SerializeField] private bool loadOnStart = true;

        [Header("Scene Recovery")]
        [SerializeField] private string defaultTopDownScene = "DEMO_SEA";
        [SerializeField] private string defaultPlatformerReloadScene = "";
        [SerializeField] private bool loadSceneOnPlatformerDeath = true;
        [SerializeField] private bool moveShipToPortOnShipDeath = true;

        [Header("References")]
        [SerializeField] private PlatformerCharacterProgression characterProgression;

        [Header("State")]
        [SerializeField] private PlayerRuntimeState state = new PlayerRuntimeState();

        public bool SaveProgress {
            get => saveProgress;
            set => saveProgress = value;
        }

        public PlayerRuntimeState State => state;
        public PlatformerCharacterProgression CharacterProgression => ResolveCharacterProgression();

        public event Action<PlayerRuntimeState> OnStateChanged;
        public event Action<PlayerHudMessage> OnHudMessage;
        public event Action<PlayerDeathContext> OnPlayerDeath;

        public UnityEvent OnUnityStateChanged = new UnityEvent();
        public UnityEvent OnUnityHudMessage = new UnityEvent();

        private void Awake()
        {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (loadOnStart) {
                LoadProgress();
            }

            RefreshSceneName();
            PullLegacyCharacterCreationStats();
        }

        public static PlayerSystem GetOrCreate()
        {
            if (Instance != null) {
                return Instance;
            }

            Instance = FindFirstObjectByType<PlayerSystem>();
            if (Instance != null) {
                return Instance;
            }

            GameObject playerSystemObject = new GameObject("PlayerSystem");
            Instance = playerSystemObject.AddComponent<PlayerSystem>();
            return Instance;
        }

        public void SetWorldMode(PlayerWorldMode mode)
        {
            if (state.WorldMode == mode) {
                return;
            }

            state.WorldMode = mode;
            MarkChanged(true);
        }

        public void RegisterSafePort(string portId, string sceneName, Vector3 worldPosition)
        {
            state.LastSafePortId = portId;
            state.LastSafePortScene = sceneName;
            state.LastSafePortPosition = worldPosition;
            MarkChanged(true);
        }

        public void RegisterActiveShip(Transform shipTransform, PlayerShipState shipState = null)
        {
            if (shipState != null) {
                state.CurrentShip = shipState;
            }

            PlayerInventorySystem.RegisterActiveShip(shipTransform);
            MarkChanged(false);
        }

        public void BindShipHealth(IHealth health)
        {
            if (health == null) {
                return;
            }

            health.OnHealthChanged.RemoveListener(ReportHealthChanged);
            health.OnHealthChanged.AddListener(ReportHealthChanged);
            health.OnDeath.RemoveListener(HandleShipDeath);
            health.OnDeath.AddListener(HandleShipDeath);
            ReportHealthChanged();
        }

        public void BindPlatformerHealth(IHealth health)
        {
            if (health == null) {
                return;
            }

            health.OnHealthChanged.RemoveListener(ReportHealthChanged);
            health.OnHealthChanged.AddListener(ReportHealthChanged);
            health.OnDeath.RemoveListener(HandlePlatformerDeath);
            health.OnDeath.AddListener(HandlePlatformerDeath);
            ReportHealthChanged();
        }

        public void ReportHudMessage(string title, string body, float duration = 3f)
        {
            OnHudMessage?.Invoke(new PlayerHudMessage { Title = title, Body = body, Duration = duration });
            OnUnityHudMessage.Invoke();
        }

        public void AcceptQuest(PlayerQuestState quest)
        {
            if (quest == null || string.IsNullOrWhiteSpace(quest.Id)) {
                return;
            }

            PlayerQuestState existing = state.Quests.Find(q => q.Id == quest.Id);
            if (existing != null) {
                existing.Title = quest.Title;
                existing.Description = quest.Description;
                existing.Reward = quest.Reward;
                existing.TimeLimit = quest.TimeLimit;
                existing.IsActive = true;
                existing.IsCompleted = false;
            }
            else {
                state.Quests.Add(quest);
            }

            ReportHudMessage("Quest accepted", quest.Title);
            MarkChanged(true);
        }

        public void CompleteQuest(string questId)
        {
            PlayerQuestState quest = state.Quests.Find(q => q.Id == questId);
            if (quest == null) {
                return;
            }

            quest.IsActive = false;
            quest.IsCompleted = true;
            ReportHudMessage("Quest completed", quest.Title);
            MarkChanged(true);
        }

        public void AddKnowledge(PlayerJournalEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Id)) {
                return;
            }

            PlayerJournalEntry existing = state.Knowledge.Find(item => item.Id == entry.Id);
            if (existing != null) {
                existing.Title = entry.Title;
                existing.Summary = entry.Summary;
                existing.Body = entry.Body;
                existing.Type = entry.Type;
                existing.Icon = entry.Icon;
                existing.IsKnown = true;
            }
            else {
                state.Knowledge.Add(entry);
            }

            ReportHudMessage("Journal updated", entry.Title);
            MarkChanged(true);
        }

        public void AddMapMarker(PlayerMapMarkerState marker)
        {
            if (marker == null || string.IsNullOrWhiteSpace(marker.Id)) {
                return;
            }

            PlayerMapMarkerState existing = state.MapMarkers.Find(item => item.Id == marker.Id);
            if (existing != null) {
                existing.Title = marker.Title;
                existing.Description = marker.Description;
                existing.WorldPosition = marker.WorldPosition;
                existing.IsDiscovered = marker.IsDiscovered;
                existing.IsGlobalMapMarker = marker.IsGlobalMapMarker;
            }
            else {
                state.MapMarkers.Add(marker);
            }

            MarkChanged(true);
        }

        public List<PlayerJournalEntry> BuildInfoEntries()
        {
            var entries = new List<PlayerJournalEntry> {
                BuildCharacterEntry(),
                BuildShipEntry()
            };

            AddSkillEntries(entries);
            entries.Sort(CompareEntries);
            return entries;
        }

        public List<PlayerJournalEntry> BuildQuestEntries(bool completed)
        {
            var entries = new List<PlayerJournalEntry>();
            for (int i = 0; i < state.Quests.Count; i++) {
                PlayerQuestState quest = state.Quests[i];
                if (quest == null || quest.IsCompleted != completed) {
                    continue;
                }

                entries.Add(quest.ToJournalEntry());
            }

            entries.Sort(CompareEntries);
            return entries;
        }

        public List<PlayerJournalEntry> BuildKnowledgeEntries(PlayerJournalEntryType type)
        {
            var entries = new List<PlayerJournalEntry>();
            for (int i = 0; i < state.Knowledge.Count; i++) {
                PlayerJournalEntry entry = state.Knowledge[i];
                if (entry != null && entry.IsKnown && entry.Type == type) {
                    entries.Add(entry.Clone());
                }
            }

            entries.Sort(CompareEntries);
            return entries;
        }

        public List<PlayerJournalEntry> BuildMapEntries(bool globalMap)
        {
            var entries = new List<PlayerJournalEntry>();
            for (int i = 0; i < state.MapMarkers.Count; i++) {
                PlayerMapMarkerState marker = state.MapMarkers[i];
                if (marker == null || !marker.IsDiscovered || marker.IsGlobalMapMarker != globalMap) {
                    continue;
                }

                entries.Add(new PlayerJournalEntry {
                    Id = marker.Id,
                    Type = PlayerJournalEntryType.MapMarker,
                    Title = marker.Title,
                    Summary = marker.WorldPosition.ToString(),
                    Body = $"{marker.Description}\n\nPosition: {marker.WorldPosition}",
                    SortOrder = i
                });
            }

            return entries;
        }

        public void HandlePlatformerDeath()
        {
            state.PlatformerDeaths++;
            OnPlayerDeath?.Invoke(PlayerDeathContext.Platformer);
            ReportHudMessage("You are defeated", "Loading last safe state.");
            MarkChanged(false);

            if (!loadSceneOnPlatformerDeath) {
                return;
            }

            string sceneToLoad = !string.IsNullOrWhiteSpace(defaultPlatformerReloadScene)
                ? defaultPlatformerReloadScene
                : SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(sceneToLoad);
        }

        public void HandleShipDeath()
        {
            state.ShipDeaths++;
            state.CurrentShip.Deaths++;
            OnPlayerDeath?.Invoke(PlayerDeathContext.ShipTopDown);

            if (state.CurrentShip.CanRestoreForFree) {
                ReportHudMessage("Ship restored", "Starter ship can always be recovered in port.");
                RespawnShipAtSafePort();
                MarkChanged(true);
                return;
            }

            int luck = GetLuck();
            if (state.CurrentShip.Deaths >= state.CurrentShip.MaxDeathsBeforePermanentLoss) {
                if (state.CurrentShip.LuckProtectionUsed < luck) {
                    state.CurrentShip.LuckProtectionUsed++;
                    state.CurrentShip.Deaths = Mathf.Max(0, state.CurrentShip.MaxDeathsBeforePermanentLoss - 1);
                    ReportHudMessage("Luck saved the ship", $"{state.CurrentShip.DisplayName} avoided permanent loss.");
                }
                else {
                    state.CurrentShip.IsPermanentlyDestroyed = true;
                    ReportHudMessage("Ship lost", $"{state.CurrentShip.DisplayName} was permanently destroyed.");
                }
            }
            else {
                ReportHudMessage("Ship destroyed", $"Deaths before loss: {state.CurrentShip.RemainingDeaths}");
            }

            RespawnShipAtSafePort();
            MarkChanged(true);
        }

        public void SaveProgressNow()
        {
            if (!saveProgress) {
                return;
            }

            RefreshSceneName();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
        }

        public bool LoadProgress()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) {
                return false;
            }

            string json = PlayerPrefs.GetString(SaveKey);
            if (string.IsNullOrWhiteSpace(json)) {
                return false;
            }

            state = JsonUtility.FromJson<PlayerRuntimeState>(json) ?? new PlayerRuntimeState();
            EnsureState();
            MarkChanged(false);
            return true;
        }

        public void ClearSavedProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
        }

        public void PullLegacyCharacterCreationStats()
        {
            PlatformerCharacterProgression progression = ResolveCharacterProgression();
            if (progression != null) {
                progression.ApplyLegacyCharacterCreationPrefs();
            }
        }

        public int GetLuck()
        {
            PlatformerCharacterProgression progression = ResolveCharacterProgression();
            if (progression != null) {
                return progression.GetTotalStat(PlatformerCharacterStatId.Luck);
            }

            return PlayerPrefs.GetInt("Luck", 0);
        }

        private void RespawnShipAtSafePort()
        {
            if (!moveShipToPortOnShipDeath) {
                return;
            }

            Transform shipTransform = PlayerInventorySystem.GetOrCreateActiveShip();
            if (shipTransform != null && !string.IsNullOrWhiteSpace(state.LastSafePortId)) {
                shipTransform.position = state.LastSafePortPosition;
            }

            if (!string.IsNullOrWhiteSpace(state.LastSafePortScene)) {
                SceneManager.LoadScene(state.LastSafePortScene);
            }
            else if (!string.IsNullOrWhiteSpace(defaultTopDownScene)) {
                SceneManager.LoadScene(defaultTopDownScene);
            }
        }

        private PlayerJournalEntry BuildCharacterEntry()
        {
            PlatformerCharacterProgression progression = ResolveCharacterProgression();
            string body = "Character data is not connected yet.";
            if (progression != null) {
                body =
                    $"Level: {progression.Level}\n" +
                    $"Experience: {progression.Experience}\n" +
                    $"Skill points: {progression.SkillPoints}\n\n" +
                    $"Strength: {progression.GetTotalStat(PlatformerCharacterStatId.Strength)}\n" +
                    $"Dexterity: {progression.GetTotalStat(PlatformerCharacterStatId.Dexterity)}\n" +
                    $"Vitality: {progression.GetTotalStat(PlatformerCharacterStatId.Vitality)}\n" +
                    $"Stamina: {progression.GetTotalStat(PlatformerCharacterStatId.Stamina)}\n" +
                    $"Charm: {progression.GetTotalStat(PlatformerCharacterStatId.Charm)}\n" +
                    $"Luck: {progression.GetTotalStat(PlatformerCharacterStatId.Luck)}\n" +
                    $"Perception: {progression.GetTotalStat(PlatformerCharacterStatId.Perception)}";
            }

            return new PlayerJournalEntry {
                Id = "player.character",
                Type = PlayerJournalEntryType.Character,
                Title = "Character",
                Summary = "Stats and progression",
                Body = body,
                SortOrder = 0
            };
        }

        private PlayerJournalEntry BuildShipEntry()
        {
            PlayerShipState ship = state.CurrentShip;
            string body =
                $"Ship: {ship.DisplayName}\n" +
                $"Ownership: {ship.Ownership}\n" +
                $"Deaths: {ship.Deaths}/{ship.MaxDeathsBeforePermanentLoss}\n" +
                $"Luck protections used: {ship.LuckProtectionUsed}/{GetLuck()}\n" +
                $"Free restore: {(ship.CanRestoreForFree ? "Yes" : "No")}\n" +
                $"Destroyed forever: {(ship.IsPermanentlyDestroyed ? "Yes" : "No")}";

            return new PlayerJournalEntry {
                Id = "player.ship",
                Type = PlayerJournalEntryType.Ship,
                Title = "Ship",
                Summary = ship.DisplayName,
                Body = body,
                SortOrder = 10
            };
        }

        private void AddSkillEntries(List<PlayerJournalEntry> entries)
        {
            PlatformerCharacterProgression progression = ResolveCharacterProgression();
            IReadOnlyList<string> skills = progression != null ? progression.UnlockedSkillIds : null;
            if (skills == null || skills.Count == 0) {
                entries.Add(new PlayerJournalEntry {
                    Id = "player.skills.empty",
                    Type = PlayerJournalEntryType.Skill,
                    Title = "Skills",
                    Summary = "No unlocked skills",
                    Body = "Skill tree data is ready, but no skills are unlocked yet.",
                    SortOrder = 20
                });
                return;
            }

            for (int i = 0; i < skills.Count; i++) {
                PlatformerSkillDefinition skill = progression.FindSkillDefinition(skills[i]);
                string title = skill != null ? skill.DisplayName : skills[i];
                string summary = skill != null ? $"{skill.Category} / level {progression.GetSkillLevel(skills[i])}" : "Unlocked skill";
                string body = skill != null
                    ? $"{skill.DisplayName}\n" +
                      $"Category: {skill.Category}\n" +
                      $"Kind: {skill.Kind}\n" +
                      $"Level: {progression.GetSkillLevel(skills[i])}/{skill.MaxSkillLevel}\n" +
                      $"Practice: {progression.GetSkillPractice(skills[i]):0.#}\n\n" +
                      $"{skill.Description}"
                    : $"Unlocked skill ID: {skills[i]}";

                entries.Add(new PlayerJournalEntry {
                    Id = $"player.skill.{skills[i]}",
                    Type = PlayerJournalEntryType.Skill,
                    Title = title,
                    Summary = summary,
                    Body = body,
                    SortOrder = 20 + i
                });
            }
        }

        private PlatformerCharacterProgression ResolveCharacterProgression()
        {
            if (characterProgression != null) {
                return characterProgression;
            }

            characterProgression = FindFirstObjectByType<PlatformerCharacterProgression>();
            return characterProgression;
        }

        private void ReportHealthChanged()
        {
            ReportHudMessage("Status changed", "Health data updated.", 1.5f);
        }

        private void MarkChanged(bool save)
        {
            EnsureState();
            OnStateChanged?.Invoke(state);
            OnUnityStateChanged.Invoke();
            if (save) {
                SaveProgressNow();
            }
        }

        private void EnsureState()
        {
            state ??= new PlayerRuntimeState();
            state.CurrentShip ??= new PlayerShipState();
            state.Quests ??= new List<PlayerQuestState>();
            state.Knowledge ??= new List<PlayerJournalEntry>();
            state.MapMarkers ??= new List<PlayerMapMarkerState>();
        }

        private void RefreshSceneName()
        {
            state.CurrentScene = SceneManager.GetActiveScene().name;
        }

        private static int CompareEntries(PlayerJournalEntry left, PlayerJournalEntry right)
        {
            int order = left.SortOrder.CompareTo(right.SortOrder);
            return order != 0 ? order : string.Compare(left.Title, right.Title, StringComparison.Ordinal);
        }
    }
}
