namespace MusicLibraryNet.Services;

public record OperationResult<T>(T Value, string Message, bool Success);

public record Success<T>(T Value) : OperationResult<T>(Value, string.Empty, true);
public record Fail<T>(string Message) : OperationResult<T>(default!, Message, false);