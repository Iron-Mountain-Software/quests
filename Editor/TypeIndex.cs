using System;
using System.Collections.Generic;
using System.Linq;

namespace IronMountain.Quests.Editor
{
    public static class TypeIndex
    {
        public static readonly List<Type> QuestTypes;
        public static readonly List<Type> QuestRequirementTypes;

        public static string[] QuestTypeNames => QuestTypes.Select(t => t.Name).ToArray();
        public static string[] QuestRequirementTypeNames => QuestRequirementTypes.Select(t => t.Name).ToArray();
        
        static TypeIndex()
        {
            QuestTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && (type == typeof(Quest) || type.IsSubclassOf(typeof(Quest))))
                .ToList();
            QuestRequirementTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && (type == typeof(QuestRequirement) || type.IsSubclassOf(typeof(QuestRequirement))))
                .ToList();
        }
    }
}
