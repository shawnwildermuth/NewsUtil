using System;
using System.CommandLine;

namespace NewsletterUtil
{
  class Program
  {
    static int Main(string[] args)
    {
      return new AppCommand().InvokeAsync(args).Result;
    }
  }
}
