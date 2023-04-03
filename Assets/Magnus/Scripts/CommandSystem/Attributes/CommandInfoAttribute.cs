using System;

namespace Rhinox.Magnus.CommandSystem
{
    public class CommandInfoAttribute : Attribute
    {
        public string Description { get; }
        public string GroupName { get; }
        
        public CommandInfoAttribute(string description, string groupName = "")
        {
            this.Description = description;
            this.GroupName = groupName;
        }
    }
}