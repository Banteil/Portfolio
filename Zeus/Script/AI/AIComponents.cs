namespace Zeus
{
    /// <summary>
    /// Is used to store in  <seealso cref="IControlAI"/> All components that inherit this interface
    /// </summary>    
    public partial interface IAIComponent
    {
        /// <summary>
        /// Type of Component. Used like a key to dicitionary        
        /// </summary>
        System.Type  ComponentType { get; }
    }

    /// <summary>
    /// Is a <seealso cref="IAIComponent"/> that receive the Start,Update and Pause events of the  <seealso cref="vIControlAI"/>
    /// </summary>   
    public partial interface IAIUpdateListener : IAIComponent
    {       
        /// <summary>
        /// Is called automatically by <seealso cref="IControlAI"/> when is started
        /// </summary>
        /// <param name="controller"></param>
        void OnStart(IControlAI controller);

        /// <summary>
        /// Is called automatically by <seealso cref="IControlAI"/> when is updated;
        /// </summary>
        /// <param name="controller"></param>
        void OnUpdate(IControlAI controller);

        /// <summary>
        /// Is called automatically by <seealso cref="IControlAI"/> when is paused;
        /// </summary>
        /// <param name="controller"></param>
        void OnPause(IControlAI controller);
    }  
}