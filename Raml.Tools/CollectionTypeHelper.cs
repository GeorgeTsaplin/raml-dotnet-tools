namespace Raml.Tools
{
    public class CollectionTypeHelper
    {
        /*// IList implementation
        public const string CollectionType = "IList";

        public static string GetCollectionType(string netType)
        {
            return CollectionType + "<" + netType + ">";
        }

        public static string GetBaseType(string type)
        {
            if (!type.StartsWith(CollectionType)) return type;

            type = type.Replace(CollectionType, string.Empty);
            type = type.Substring(1, type.Length - 2);
            type = type.Replace("<", string.Empty);
            type = type.Replace(">", string.Empty);
            return type;
        }

        public static bool IsCollection(string type)
        {
            return type.StartsWith(CollectionType);
        }
        */

        // Array impl
        private const string CollectionPostfix = "[]";

        public static string GetCollectionType(string netType) => netType + CollectionPostfix;

        public static string GetBaseType(string type) => !IsCollection(type) ? type : type.Substring(0, type.Length - CollectionPostfix.Length);

        public static bool IsCollection(string type) => type.EndsWith(CollectionPostfix);
        
    }
}