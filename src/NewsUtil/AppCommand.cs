using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsletterUtil
{
  public class AppCommand : RootCommand
  {
    public AppCommand(string description = "newsutil") : base(description)
    {
      AddCommand(new LinkListCommand());
    }
  }
}
