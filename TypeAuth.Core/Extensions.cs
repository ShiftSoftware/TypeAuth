namespace ShiftSoftware.TypeAuth.Core
{
    public static class Extensions
    {
        public static List<DynamicAction> AddSelfReference(this List<DynamicAction> list, ActionType permissionTypes, string Text)
        {
            list.Insert(0, new DynamicAction(TypeAuthContext.SelfRererenceKey, Text, permissionTypes));
            return list;
        }
    }
}
