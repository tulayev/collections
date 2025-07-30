namespace Collections.Services.Admin.FieldGroup
{
    public interface IFieldGroupService
    {
        Task<List<Models.FieldGroup>> GetAllFieldGroupsAsync();
    }
}
