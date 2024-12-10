namespace Btms.Business.Services.Linking;

public class LinkException(Exception inner) : Exception("Failed to link", inner);