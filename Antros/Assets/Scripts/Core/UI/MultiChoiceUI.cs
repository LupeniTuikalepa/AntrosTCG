using System;
using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ATCG.UI
{
    public class MultiChoiceUI : MonoBehaviour
    {
        public event Action<int, Choice> OnChoiceChangesMade;

        [System.Serializable]
        public struct Choice
        {
            [field: SerializeField]
            public string Title { get; private set; }
            [field: SerializeField]
            public Button Button { get; private set; }

            [field: SerializeField]
            public UnityEvent OnChose { get; private set; }
        }

        [SerializeField]
        private List<Choice> choices;

        [SerializeField]
        private RectTransform selectedChoice;


        [field: SerializeField, DisableInEditorMode]
        public int CurrentChoiceIndex { get; private set; }

        [field: SerializeField, PropertyRange(0, nameof(ChoicesCount))]
        public int StartChoiceIndex { get; private set; }

        [field: SerializeField]
        public UnityEvent<int, Choice> OnChoiceChangesMadeEvent { get; private set; }

        public int ChoicesCount => choices.Count;


        private void Awake()
        {
            for (int i = 0; i < choices.Count; i++)
            {
                Choice choice = choices[i];
                if (choice.Button != null)
                {
                    int index = i;
                    choice.Button.onClick.AddListener(() => SetChoice(index));
                }
            }

        }

        private void OnEnable()
        {
            CurrentChoiceIndex = StartChoiceIndex;
            TriggerChoiceChange();
        }

        public void Reset()
        {
            CurrentChoiceIndex = 0;
            TriggerChoiceChange();
        }

        public void SetChoice(int index)
        {
            CurrentChoiceIndex = index;
            TriggerChoiceChange();
        }

        private void TriggerChoiceChange()
        {
            if (ChoicesCount > 0)
            {
                Choice choice = choices[CurrentChoiceIndex];
                choice.OnChose.Invoke();

                Debug.Log($"New choice => {choice.Title}");
                OnChoiceChangesMadeEvent.Invoke(CurrentChoiceIndex, choice);
                OnChoiceChangesMade?.Invoke(CurrentChoiceIndex, choice);
                RectTransform rectTransform = choice.Button.transform as RectTransform;
                Tween.StopAll(selectedChoice);
                if (rectTransform != null)
                {
                    float duration = .3f;

                    Tween.UIAnchoredPosition(selectedChoice, rectTransform.anchoredPosition, duration, Ease.OutExpo);
                    Tween.UISizeDelta(selectedChoice, rectTransform.sizeDelta, duration, Ease.OutExpo);
                    Tween.UIAnchorMax(selectedChoice, rectTransform.anchorMax, duration, Ease.OutExpo);
                    Tween.UIAnchorMin(selectedChoice, rectTransform.anchorMin, duration, Ease.OutExpo);
                    Tween.UIPivot(selectedChoice, rectTransform.pivot, duration, Ease.OutExpo);
                }
            }
        }
    }
}