using System;

namespace KE.VSIX.Commands
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class CommandIDAttribute : Attribute
    {
        #region Var
        public int CommandID { get; }
        public Guid CommandSet { get; }
        #endregion

        #region Init
        public CommandIDAttribute(Guid CommandSet, int CommandID)
        {
            this.CommandSet = CommandSet;
            this.CommandID = CommandID;
        }

        public CommandIDAttribute(string CommandSet, int CommandID) : this(Guid.Parse(CommandSet), CommandID) { }
        #endregion
    }
}
