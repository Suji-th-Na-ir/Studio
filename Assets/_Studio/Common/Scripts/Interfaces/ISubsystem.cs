using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public interface ISubsystem
    {
        public void Initialize(Scene scene);
        public void Dispose();
        public Scene GetScene();
    }
}
