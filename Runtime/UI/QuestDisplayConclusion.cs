namespace IronMountain.Quests.UI
{
    public class QuestDisplayConclusion : QuestDisplayText
    {
        protected override void Refresh()
        {
            if (!text) return;
            text.text = questDisplay && questDisplay.Quest ? questDisplay.Quest.Conclusion : string.Empty;
        }
    }
}