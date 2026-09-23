namespace Clase07.DI.ServiceLocatorPattern
{
    public static class ServiceLocatorCompositionRoot
    {
        public static void Bootstrap()
        {
            ServiceLocator.Register<IScoreService>(new ScoreService());
            ServiceLocator.Register<IAudioService>(new AudioService());
        }
    }
}
