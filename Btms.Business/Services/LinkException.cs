namespace Btms.Business.Services;

public class LinkException(Exception inner) : Exception("Failed to link", inner);