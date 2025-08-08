namespace Server.Attributes.Authorization
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HubAuthorizeIPAttribute : Attribute
    {
        // этот атрибут создан для работы hub фильтра "HubAuthorizeIPFilter", он проверяет на наличие установленного атрибута перед методом, и выполняет свою логику
    }
}
