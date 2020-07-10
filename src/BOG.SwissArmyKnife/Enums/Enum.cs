namespace BOG.SwissArmyKnife.Enums
{
	/// <summary>
    /// Defines the processing state of the MegaAccordion
    /// </summary>
    public enum MegaAccordionState : int
    {
        /// <summary>
        /// There are items to be processed, and more items to process which are not queued.
        /// </summary>
        Active = 1,

        /// <summary>
        /// There are items to be processed, and all items are now in the queue.
        /// The number of items left to process is at or under the MaxItemsInProgress.
        /// </summary>
        Sunsetting = 2,

        /// <summary>
        /// All items have been processed.
        /// </summary>
        Completed = 3
    }
}
