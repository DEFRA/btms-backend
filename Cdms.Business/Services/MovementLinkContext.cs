using Cdms.Model;

namespace Cdms.Business.Services;

public record MovementLinkContext(Movement ReceivedMovement, Movement? ExistingMovement) : LinkContext;