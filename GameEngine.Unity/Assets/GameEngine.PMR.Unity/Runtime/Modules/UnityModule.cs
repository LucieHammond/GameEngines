using GameEngine.PMR.Modules.Specialization;
using GameEngine.PMR.Unity.Modules.Specialization;
using System.Collections.Generic;

namespace GameEngine.PMR.Unity.Modules
{
    /// <summary>
    /// A static class defining the setup operations specific to Unity modules
    /// </summary>
    public static class UnityModule
    {
        /// <summary>
        /// Define and add Unity-specific configuration operations to the list of specialized tasks
        /// </summary>
        /// <param name="tasks">The list of special tasks to be filled in the module setup</param>
        public static void SetSpecialTasks(ref List<SpecialTask> tasks)
        {
            tasks.Add(new ScenesCompositionTask());
            tasks.Add(new GameObjectsInsertionTask());
        }
    }
}
