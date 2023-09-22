using BepInEx.Logging;
using System;

namespace Myriad; 

public class TryCatchUtils {
    #nullable enable
    public static T? wrapTryCatch<T>(Func<T> supplier, Func<String> message, ManualLogSource logger) where T : class {
        return wrapTryCatch(supplier, message(), logger);
    }

    public static T? wrapTryCatch<T>(Func<T> supplier, String message, ManualLogSource logger) where T : class {
        return wrapTryCatch(supplier, exception => {
            logger.LogError(message);
            logger.LogError(exception.Message);
        });
    }

    public static T? wrapTryCatch<T>(Func<T> supplier, Action<Exception> logException) where T : class {
        try {
            return supplier();
        } catch (Exception e) {
            logException(e);
        }
        
        return null;
    }
    #nullable disable
}