using System.Collections.Generic;
using C1.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menu_Journal.UI {
    public abstract class JournalDataTabBase : JournalTabBase {
        [Header("List")]
        [SerializeField] private Transform listContent;
        [SerializeField] private Button listButtonPrefab;
        [SerializeField] private ScrollRect listScrollRect;

        [Header("Details")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI summaryText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image iconImage;
        [SerializeField] private ScrollRect detailScrollRect;

        [Header("Empty State")]
        [SerializeField] private string emptyTitle = "No records";
        [SerializeField, TextArea(2, 5)] private string emptyBody = "There is no journal data for this section yet.";

        private readonly List<Button> spawnedButtons = new List<Button>();
        private List<PlayerJournalEntry> currentEntries = new List<PlayerJournalEntry>();

        public override void OnOpen()
        {
            base.OnOpen();
            Refresh();
        }

        protected abstract List<PlayerJournalEntry> BuildEntries(PlayerSystem player);

        public void Refresh()
        {
            PlayerSystem player = PlayerSystem.GetOrCreate();
            currentEntries = BuildEntries(player) ?? new List<PlayerJournalEntry>();
            DrawList();

            if (currentEntries.Count > 0) {
                SelectEntry(currentEntries[0]);
            }
            else {
                ShowEmpty();
            }
        }

        protected void SelectEntry(PlayerJournalEntry entry)
        {
            if (entry == null) {
                ShowEmpty();
                return;
            }

            if (titleText != null) titleText.text = entry.Title;
            if (summaryText != null) summaryText.text = entry.Summary;
            if (bodyText != null) bodyText.text = entry.Body;

            if (iconImage != null) {
                iconImage.sprite = entry.Icon;
                iconImage.enabled = entry.Icon != null;
            }

            ScrollToTop(detailScrollRect);
        }

        private void DrawList()
        {
            ClearList();
            if (listContent == null || listButtonPrefab == null) {
                return;
            }

            for (int i = 0; i < currentEntries.Count; i++) {
                PlayerJournalEntry entry = currentEntries[i];
                Button button = Instantiate(listButtonPrefab, listContent);
                TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null) {
                    label.text = entry.Title;
                }

                PlayerJournalEntry captured = entry;
                button.onClick.AddListener(() => SelectEntry(captured));
                spawnedButtons.Add(button);
            }

            ScrollToTop(listScrollRect);
        }

        private void ClearList()
        {
            for (int i = 0; i < spawnedButtons.Count; i++) {
                if (spawnedButtons[i] != null) {
                    Destroy(spawnedButtons[i].gameObject);
                }
            }

            spawnedButtons.Clear();
        }

        private void ShowEmpty()
        {
            if (titleText != null) titleText.text = emptyTitle;
            if (summaryText != null) summaryText.text = string.Empty;
            if (bodyText != null) bodyText.text = emptyBody;
            if (iconImage != null) iconImage.enabled = false;
            ScrollToTop(detailScrollRect);
        }

        private static void ScrollToTop(ScrollRect scrollRect)
        {
            if (scrollRect == null) {
                return;
            }

            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 1f;
        }
    }

    public sealed class JournalInfoTab : JournalDataTabBase {
        protected override List<PlayerJournalEntry> BuildEntries(PlayerSystem player)
        {
            return player.BuildInfoEntries();
        }
    }

    public sealed class JournalQuestsTab : JournalDataTabBase {
        [SerializeField] private bool showCompleted;

        protected override List<PlayerJournalEntry> BuildEntries(PlayerSystem player)
        {
            return player.BuildQuestEntries(showCompleted);
        }
    }

    public sealed class JournalKnowledgeTab : JournalDataTabBase {
        [SerializeField] private PlayerJournalEntryType entryType = PlayerJournalEntryType.Person;

        protected override List<PlayerJournalEntry> BuildEntries(PlayerSystem player)
        {
            return player.BuildKnowledgeEntries(entryType);
        }
    }

    public sealed class JournalMapTab : JournalDataTabBase {
        [SerializeField] private bool globalMap;

        protected override List<PlayerJournalEntry> BuildEntries(PlayerSystem player)
        {
            return player.BuildMapEntries(globalMap);
        }
    }
}
