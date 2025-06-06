﻿namespace RimDialogueObjects
{
  public static class ExceptionHelper
  {
    public static Exception[] GetExceptionStack(Exception exception)
    {
      var exceptions = new List<Exception>([exception]);
      Exception? innerException = exception;
      while (innerException != null)
      {
        exceptions.Add(innerException);
        innerException = innerException?.InnerException;
      }
      return exceptions.ToArray();
    }
  }
}
