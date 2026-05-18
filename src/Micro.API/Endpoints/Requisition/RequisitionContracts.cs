namespace Micro.API.Endpoints.Requisition;

public record CreateRequisitionRequest(string Title, string Department, int Openings);
public record UpdateRequisitionRequest(string Title, string Department, int Openings);
