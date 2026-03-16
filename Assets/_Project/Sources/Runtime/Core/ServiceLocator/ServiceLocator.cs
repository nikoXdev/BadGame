namespace Sources.Runtime.Core.ServiceLocator
{
    public static class ServiceLocator
    {
        public static readonly ProjectServiceLocator Project = new();

        public static ServiceLocatorBase Scene { get; private set; }

        public static void SetSceneLocator(ServiceLocatorBase locator)
        {
            Scene = locator;
        }

        public static void ClearSceneLocator()
        {
            Scene = null;
        }

        public static T Get<T>() where T : IService
        {
            var sceneService = Scene.GetService<T>();

            if (sceneService != null)
                return sceneService;

            return Project.GetService<T>();
        }
    }
}