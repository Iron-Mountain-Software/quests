namespace IronMountain.Quests.UI
{
    public class QuestDisplayDescription : QuestDisplayText
    { 
        protected override void Refresh()
        {
            if (!text) return;
            text.text = questDisplay && questDisplay.Quest ? questDisplay.Quest.Description : string.Empty;
        }
    }
}