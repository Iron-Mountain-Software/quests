# Quests
*Version: 2.0.4*
## Description: 
Scriptable Object Quests.
## Use Cases: 
* For creating scripted sequences of game events.
## Dependencies: 
* com.unity.localization (1.3.2)
* com.iron-mountain.conditions (1.5.6)
* com.iron-mountain.scriptable-actions (1.0.5)
* com.iron-mountain.save-system (1.0.4)
## Package Mirrors: 
[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODk4LnBuZw==/original/Rv4m96.png'>](https://iron-mountain.itch.io/quests)[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODg3LnBuZw==/original/npRUfq.png'>](https://github.com/Iron-Mountain-Software/quests.git)[<img src='https://img.itch.zone/aW1nLzEzNzQ2ODkyLnBuZw==/original/Fq0ORM.png'>](https://www.npmjs.com/package/com.iron-mountain.quests)
---
## Key Scripts & Components: 
1. public class **Database** : ScriptableObject
   * Properties: 
      * public List<Quest> ***Quests***  { get; }
   * Methods: 
      * public Quest ***GetQuestByName***(String sceneName)
      * public Quest ***GetQuestByID***(String id)
      * public Quest ***GetRandomQuest***()
      * public void ***SortList***()
      * public void ***RebuildDictionary***()
      * public override String ***ToString***()
1. public class **Quest** : StoryEvent
   * Actions: 
      * public event Action ***OnViewsChanged*** 
   * Properties: 
      * public Int32 ***Priority***  { get; set; }
      * public StoryType ***Type***  { get; set; }
      * public List<QuestRequirement> ***Requirements***  { get; }
      * public String ***Name***  { get; set; }
      * public String ***Description***  { get; set; }
      * public String ***Conclusion***  { get; set; }
      * public Int32 ***Views***  { get; set; }
      * public Boolean ***DescriptionHasErrors***  { get; }
      * public Boolean ***RequirementsHaveErrors***  { get; }
   * Methods: 
      * public override void ***Refresh***()
      * public override void ***Restart***()
      * public virtual void ***Reset***()
      * public override void ***OnValidate***()
      * public override String ***GetDocumentation***()
      * public override Boolean ***HasErrors***()
1. public class **QuestRequirement** : StoryEvent
   * Properties: 
      * public String ***Name***  { get; }
      * public Quest ***Quest***  { get; set; }
      * public String ***Detail***  { get; }
      * public String ***Tip***  { get; }
      * public Sprite ***Depiction***  { get; }
      * public Boolean ***DescriptionHasErrors***  { get; }
   * Methods: 
      * public override void ***Refresh***()
      * public virtual void ***Reset***()
      * public override void ***OnValidate***()
      * public override Boolean ***HasErrors***()
      * public override String ***GetDocumentation***()
1. public class **QuestStateCondition** : Condition
   * Properties: 
      * public Sprite ***Depiction***  { get; }
   * Methods: 
      * public override Boolean ***Evaluate***()
      * public override Boolean ***HasErrors***()
      * public override String ***ToString***()
1. public class **QuestsManager** : MonoBehaviour
1. public abstract class **StoryEvent** : ScriptableObject
   * Actions: 
      * public event Action ***OnStateChanged*** 
   * Properties: 
      * public StateType ***State***  { get; }
      * public String ***ID***  { get; }
      * public Boolean ***CanActivate***  { get; }
      * public Boolean ***CanComplete***  { get; }
      * public Boolean ***CanFail***  { get; }
      * public Boolean ***PrerequisitesHaveErrors***  { get; }
      * public Boolean ***CompletionConditionHasErrors***  { get; }
      * public Boolean ***FailConditionHasErrors***  { get; }
   * Methods: 
      * public abstract void ***Refresh***()
      * public void ***TryActivate***()
      * public void ***TryComplete***()
      * public void ***TryFail***()
      * public virtual void ***Restart***()
      * public virtual void ***Activate***()
      * public virtual void ***Complete***()
      * public virtual void ***Fail***()
      * public virtual void ***OnValidate***()
      * public virtual Boolean ***HasErrors***()
      * public abstract String ***GetDocumentation***()
1. public class **StoryEventController** : MonoBehaviour
1. public class **StoryEventStateCondition** : Condition
   * Properties: 
      * public Sprite ***Depiction***  { get; }
   * Methods: 
      * public override Boolean ***Evaluate***()
      * public override Boolean ***HasErrors***()
      * public override String ***ToString***()
