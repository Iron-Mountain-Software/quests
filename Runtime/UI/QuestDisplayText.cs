using UnityEngine;
using UnityEngine.UI;

namespace IronMountain.Quests.UI
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Text))]
    public abstract class QuestDisplayText : MonoBehaviour
    {
        [SerializeField] protected QuestDisplay questDisplay;
        [SerializeField] protected Text text;

        protected abstract void Refresh();
        
        protected virtual void OnValidate()
        {
            if (!questDisplay) questDisplay = GetComponentInParent<QuestDisplay>();
            if (!text) text = GetComponent<Text>();
        }

        protected virtual void Awake() => OnValidate();

        protected virtual void OnEnable()
        {
            if (questDisplay) questDisplay.OnQuestChanged += Refresh;
            Refresh();
        }

        protected virtual void OnDisable()
        {
            if (questDisplay) questDisplay.OnQuestChanged -= Refresh;
        }
    }
}