namespace GameEngine.Core.System
{
    public class UnityTime : ITime
    {
        public float DeltaTime => UnityEngine.Time.deltaTime;

        public float Time => UnityEngine.Time.time;

        public int FrameCount => UnityEngine.Time.frameCount;

        public float RealtimeSinceStartup => UnityEngine.Time.realtimeSinceStartup;
    }
}
