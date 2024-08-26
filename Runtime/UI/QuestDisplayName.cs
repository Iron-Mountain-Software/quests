namespace IronMountain.Quests.UI
{
    public class QuestDisplayName : QuestDisplayText
    {
        protected override void Refresh()
        {
            if (!text) return;
            text.text = questDisplay && questDisplay.Quest ? questDisplay.Quest.Name : string.Empty;
        }
    }
}