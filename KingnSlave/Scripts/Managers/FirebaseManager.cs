namespace starinc.io.kingnslave
{
    public class FirebaseManager : Singleton<FirebaseManager>
    {
		public bool isFirebaseStarted;
		private Firebase.FirebaseApp app;
		private Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

        // Start is called before the first frame update
        void Start()
        {
			InitFirebase();
		}

		void InitFirebase()
		{
			Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
			{
				dependencyStatus = task.Result;
				if (dependencyStatus == Firebase.DependencyStatus.Available)
				{
					// Create and hold a reference to your FirebaseApp,
					// where app is a Firebase.FirebaseApp property of your application class.
					app = Firebase.FirebaseApp.DefaultInstance;

					// Set a flag here to indicate whether Firebase is ready to use by your app.
					isFirebaseStarted = true;
				}
				else
				{
					UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
					// Firebase Unity SDK is not safe to use here.
				}
			});
		}
	}
}