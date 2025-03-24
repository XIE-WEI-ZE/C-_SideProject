using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordNoteConsoleApp
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public OperationResult(bool success, string message, T data)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static OperationResult<T> Ok(T data) => new(true, null, data);
        public static OperationResult<T> Fail(string message) => new(false, message, default);
    }
}