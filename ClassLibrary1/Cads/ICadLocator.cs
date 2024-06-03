namespace ClassLibrary1.Cads;

public interface ICadLocator
{
    Task<string?> GetCadNode(string organizationId, string prcId);
    Task SetCadNode(string organizationId, string prcId, string nodeUrl);
    Task DeleteCadInformation(string organizationId, string prcId);
}