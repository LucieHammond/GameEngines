using GameEngine.PMR.Modules.Specialization;
using GameEngine.PMR.Rules;

namespace GameEnginesTest.Tools.Mocks.Stubs
{
    public class StubSpecializedTask : SpecializedTask
    {
        public override float GetProgress()
        {
            return State == SpecializedTaskState.InitCompleted ? 1f : 0f;
        }

        protected override void Initialize(RulesDictionary rules)
        {
            FinishInitialization();
        }

        protected override void Unload(RulesDictionary rules)
        {
            FinishUnload();
        }

        protected override void Update(int maxDuration)
        {

        }
    }
}
