using System;
using ATCG.Cards;
using ATCG.Utilities;
using UnityEngine;

namespace ATCG
{
    public abstract class AbilitiesUITab : MonoBehaviour
    {
        public event Action OnOpen;
        public event Action OnClose;

        [SerializeField]
        private CanvasGroup canvasGroup;
        [field: SerializeField]
        public string TabName { get; private set; }


        public abstract bool Build(IGameCard gameCard);
        public abstract void Clear();

        public void Activate()
        {
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }
        public void Open()
        {
            canvasGroup.Show(.2f);
            OnOpen?.Invoke();
        }

        public void Close()
        {
            canvasGroup.Hide(.2f);
            OnClose?.Invoke();
        }
    }
}