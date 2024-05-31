using TestProject1.WebServices;

namespace TestProject1.Cads;

public class CadContextRequest<TCadRequest>: ICadRequest
{
    public string OrganizationId { get; set; }
    public string PrcId { get; set; }
    public TCadRequest Request { get; set; }
}