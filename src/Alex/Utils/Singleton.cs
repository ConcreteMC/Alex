namespace Alex.Utils
{
    public class Singleton<T> where T : class, new()
    {
        private static readonly object LockObject = new object();
        private static T _instance;

        public static T Instance
        {
            get
            {
                lock (LockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                }
                return _instance;
            }
        }
    }
}