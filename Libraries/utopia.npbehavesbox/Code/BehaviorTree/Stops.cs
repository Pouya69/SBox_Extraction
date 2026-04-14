namespace NPBehave
{
    public enum Stops
    {
	    /// <summary>
	    /// The decorator will only check it's condition once it is started and will never stop any running nodes.
	    /// </summary>
        None,
	    /// <summary>
	    /// The decorator will check it's condition once it is started and if it is met,
	    /// it will observe the blackboard for changes.
	    /// Once the condition is no longer met, it will stop itself allowing the parent composite to proceed with it's next node.
	    /// </summary>
        Self,
	    
	    /// <summary>
	    /// The decorator will check it's condition once it is started and if it's not met, it will observe the blackboard for changes.
	    /// Once the condition is met, it will stop the lower priority node allowing the parent composite to proceed with it's next node
	    /// </summary>
        LowerPriority,
	    
	    /// <summary>
	    /// The decorator will stop both: self and lower priority nodes.
	    /// </summary>
        Both,
	    
	    /// <summary>
	    /// The decorator will check it's condition once it is started and if it's not met, it will observe the blackboard for changes.
	    /// Once the condition is met, it will stop the lower priority node and order the parent composite to restart the Decorator immediately.
	    /// </summary>
        ImmediateRestart,
	    
	    /// <summary>
	    /// The decorator will check it's condition once it is started and if it's not met, it will observe the blackboard for changes.
	    /// Once the condition is met, it will stop the lower priority node and order the parent composite to restart the Decorator immediately.
	    /// As in BOTH it will also stop itself as soon as the condition is no longer met.
	    /// </summary>
        LowerPriorityImmediateRestart
    }
}
