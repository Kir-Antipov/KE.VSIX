using System;

namespace KE.VSIX.Commands
{
    public abstract class BaseCommand
    {
        #region Functions
        public static bool IsCommand(Type CmdType)
        {
            Type baseType = CmdType;
            while (baseType != null)
            {
                Type check = baseType.IsGenericType ? baseType.GetGenericTypeDefinition() : baseType;
                if (check == typeof(BaseCommand<>))
                    return true;
                baseType = baseType.BaseType;
            }
            return false;
        }
        #endregion
    }
}
