# Quests
*Version: 1.5.3*
## Description: 
Scriptable Object Quests.
## Use Cases: 
* For creating scripted sequences of game events.
## Dependencies: 
* com.unity.localization (1.3.2)
* com.iron-mountain.conditions (1.5.0)
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
1. public class **Quest** : ScriptableObject
   * Actions: 
      * public event Action ***OnStateChanged*** 
      * public event Action ***OnViewsChanged*** 
   * Properties: 
      * public String ***ID***  { get; set; }
      * public String ***Name***  { get; }
      * public Int32 ***Priority***  { get; }
      * public StoryType ***Type***  { get; }
      * public Condition ***Prerequisites***  { get; set; }
      * public Boolean ***PrerequisitesSatisfied***  { get; }
      * public List<ScriptableAction> ***ActionsOnActivate***  { get; }
      * public List<ScriptableAction> ***ActionsOnComplete***  { get; }
      * public List<QuestRequirement> ***Requirements***  { get; }
      * public List<QuestRequirement> ***CompletedRequirements***  { get; }
      * public Boolean ***ReadyToComplete***  { get; }
      * public String ***LocalizedName***  { get; }
      * public String ***Description***  { get; }
      * public String ***Conclusion***  { get; }
      * public StateType ***State***  { get; }
      * public Int32 ***Views***  { get; set; }
      * public Boolean ***DescriptionHasErrors***  { get; }
      * public Boolean ***PrerequisitesHaveErrors***  { get; }
      * public Boolean ***RequirementsHaveErrors***  { get; }
   * Methods: 
      * public void ***Refresh***()
      * public virtual Boolean ***Activate***()
      * public virtual Boolean ***Complete***()
      * public virtual void ***Reset***()
      * public virtual void ***OnValidate***()
      * public virtual String ***WriteDocumentation***()
      * public virtual Boolean ***HasErrors***()
1. public class **QuestRequirement** : ScriptableObject
   * Actions: 
      * public event Action ***OnStateChanged*** 
   * Properties: 
      * public String ***ID***  { get; set; }
      * public String ***Name***  { get; }
      * public Quest ***Quest***  { get; set; }
      * public String ***Detail***  { get; }
      * public String ***Tip***  { get; }
      * public List<QuestRequirement> ***Dependencies***  { get; }
      * public List<ScriptableAction> ***ActionsOnTrack***  { get; }
      * public List<ScriptableAction> ***ActionsOnComplete***  { get; }
      * public Condition ***Condition***  { get; set; }
      * public Condition ***FailCondition***  { get; set; }
      * public Sprite ***Depiction***  { get; }
      * public StateType ***State***  { get; set; }
      * public Boolean ***DescriptionHasErrors***  { get; }
      * public Boolean ***DependenciesHaveErrors***  { get; }
      * public Boolean ***CompletionConditionHasErrors***  { get; }
   * Methods: 
      * public void ***StartTracking***()
      * public virtual void ***Reset***()
      * public virtual void ***OnValidate***()
      * public virtual Boolean ***HasErrors***()
1. public class **QuestsManager** : MonoBehaviour
### Conditions
1. public class **ConditionQuestRequirementState** : Condition
   * Properties: 
      * public Sprite ***Depiction***  { get; }
   * Methods: 
      * public override Boolean ***Evaluate***()
      * public override Boolean ***HasErrors***()
      * public override String ***ToString***()
1. public class **ConditionQuestState** : Condition
   * Properties: 
      * public Sprite ***Depiction***  { get; }
   * Methods: 
      * public override Boolean ***Evaluate***()
      * public override Boolean ***HasErrors***()
      * public override String ***ToString***()
