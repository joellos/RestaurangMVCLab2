
namespace RestaurangMVCLab2.Models
{
    public class ServiceResponse
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        // Constructors
        public ServiceResponse() { }

        public ServiceResponse(bool succeeded, string message, object? data = null)
        {
            Succeeded = succeeded;
            Message = message;
            Data = data;
        }

        public ServiceResponse(bool succeeded, string message, List<string> errors)
        {
            Succeeded = succeeded;
            Message = message;
            Errors = errors;
        }

        // Static factory methods
        public static ServiceResponse Success(string message, object? data = null)
        {
            return new ServiceResponse(true, message, data);
        }

        public static ServiceResponse Success(object data, string message = "Operation completed successfully")
        {
            return new ServiceResponse(true, message, data);
        }

        public static ServiceResponse Failure(string message)
        {
            return new ServiceResponse(false, message);
        }

        public static ServiceResponse Failure(string message, List<string> errors)
        {
            return new ServiceResponse(false, message, errors);
        }

        public static ServiceResponse Failure(List<string> errors)
        {
            return new ServiceResponse(false, "Operation failed", errors);
        }

        // Helper methods
        public bool HasErrors => Errors.Any();

        public void AddError(string error)
        {
            Errors.Add(error);
            if (Succeeded)
            {
                Succeeded = false;
                Message = "Operation failed with errors";
            }
        }

        // Type-safe data retrieval
        public T? GetData<T>() where T : class
        {
            return Data as T;
        }
    }
}