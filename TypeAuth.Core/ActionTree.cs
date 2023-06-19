namespace ShiftSoftware.TypeAuth.Core
{
    /// <summary>
    /// Used for organizing Actions in a hierarchical (Tree) structure.
    /// </summary>
    public class ActionTree: Attribute
    {
        /// <summary>
        /// Optional ID for identifying the Action Tree.
        /// </summary>
        //public string? ID { get; set; }

        /// <summary>
        /// Friendly name for identifying the Action Tree.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Additional description about the Action Tree.
        /// </summary>
        public string? Description { get; set; }

        public ActionTree()
        {

        }

        public ActionTree(string? name, string? description/*, string? Id = null*/)
        {
            //this.ID = Id;
            this.Name = name;
            this.Description = description;
        }
    }

    public class DynamicActionTree : ActionTree
    {

    }
}
