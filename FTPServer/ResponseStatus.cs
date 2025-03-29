using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FTPServer
{
    public static class ResponseStatus
    {
        // ✅ Success
        public const int SUCCESS = 200;
        public const string SUCCESS_MESSAGE = "Success";

        // 🚀 Created
        public const int CREATED = 201;
        public const string CREATED_MESSAGE = "File created successfully";
        
        public const int READY = 202;
        public const string READY_MESSAGE = "Server ready to do action";

        // ❌ General error
        public const int ERROR = 500;
        public const string ERROR_MESSAGE = "System error, please try again";

        // 🚫 Unauthorized access
        public const int UNAUTHORIZED = 401;
        public const string UNAUTHORIZED_MESSAGE = "Access denied";

        // 🔐 Authentication Required
        public const int AUTHENTICATION_REQUIRED = 403;
        public const string AUTHENTICATION_REQUIRED_MESSAGE = "Authentication is required to access this resource";

        // 🔑 Invalid Credentials
        public const int INVALID_CREDENTIALS = 401;
        public const string INVALID_CREDENTIALS_MESSAGE = "Invalid username or password";

        // ⛔ Account Not Found
        public const int ACCOUNT_NOT_FOUND = 404;
        public const string ACCOUNT_NOT_FOUND_MESSAGE = "User account not found";

        // 🔒 Account Locked
        public const int ACCOUNT_LOCKED = 423;
        public const string ACCOUNT_LOCKED_MESSAGE = "Account has been locked due to multiple failed login attempts";

        // 🔄 Token Expired
        public const int TOKEN_EXPIRED = 440;
        public const string TOKEN_EXPIRED_MESSAGE = "Session has expired, please log in again";

        // ❌ Invalid Token
        public const int INVALID_TOKEN = 498;
        public const string INVALID_TOKEN_MESSAGE = "Invalid or malformed authentication token";

        // ⚠️ Account Already Exists
        public const int ACCOUNT_ALREADY_EXISTS = 409;
        public const string ACCOUNT_ALREADY_EXISTS_MESSAGE = "User account already exists";

        // ⛔ Not found
        public const int NOT_FOUND = 404;
        public const string NOT_FOUND_MESSAGE = "File not found";

        // 🔄 Invalid request
        public const int BAD_REQUEST = 400;
        public const string BAD_REQUEST_MESSAGE = "Invalid request";

        // 🛑 File too large
        public const int FILE_TOO_LARGE = 413;
        public const string FILE_TOO_LARGE_MESSAGE = "File size exceeds the allowed limit";

        // 🖼️ Unsupported file format
        public const int UNSUPPORTED_MEDIA_TYPE = 415;
        public const string UNSUPPORTED_MEDIA_TYPE_MESSAGE = "File format is not supported";

        // 📂 Insufficient storage
        public const int INSUFFICIENT_STORAGE = 507;
        public const string INSUFFICIENT_STORAGE_MESSAGE = "Not enough storage space available";

        // 🔄 File deletion failed
        public const int DELETE_FAILED = 409;
        public const string DELETE_FAILED_MESSAGE = "Unable to delete the file";

        // ⏳ Timeout
        public const int TIMEOUT = 408;
        public const string TIMEOUT_MESSAGE = "Upload or download request timed out";
    }
}
