using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NewsletterUtil
{

  public class LinkListCommand : Command
  {

    private Option InOption;
    private Option OutOption;
    private HttpClient theClient;

    public LinkListCommand()
      : base("linklist", "Create a markdown file of links from a file of links, including title and site name")
    {
      theClient = new HttpClient();
      theClient.DefaultRequestHeaders.Accept.Clear();
      theClient.DefaultRequestHeaders.Accept.ParseAdd("*/*");
      theClient.DefaultRequestHeaders.UserAgent.Clear();
      theClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; NewsletterUtil/1.0)");

      CreateInOption();
      CreateOutOption();

      Handler = CommandHandler.Create<FileInfo, FileInfo>(OnExecuteAsync);

    }

    async Task OnExecuteAsync(FileInfo input, FileInfo output)
    {
      try
      {
        Console.WriteLine($"Creating List of Links to {output.Name}");
        var bldr = new StringBuilder();

        var list = await File.ReadAllLinesAsync(input.FullName);
        if (list.Length > 0)
        {
          Console.WriteLine($"Loading {list.Length} items...");
          foreach (var link in list)
          {
            await GetLinkInfo(link, bldr);
            Console.Write(".");
          }
          var markdown = bldr.ToString();
          File.WriteAllText(output.FullName, markdown);
          Console.WriteLine();
          Console.WriteLine($"Complete: {output.Name}");
          return;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine();
        Console.WriteLine($"Exception Thrown: {ex}");
      }

      Console.WriteLine();
      Console.WriteLine("Failed to read the input file");
    }

    async Task GetLinkInfo(string link, StringBuilder bldr)
    {
      try
      {
        var html = await theClient.GetStringAsync(link);
        var doc = new HtmlDocument();
        doc.LoadHtml(html);
        bldr.AppendLine(FormatFromDoc(doc, link));
      }
      catch (Exception ex)
      {
        var msg = ex.Message;
        Console.WriteLine();
        Console.WriteLine($"Failed to get info on: {link}");
      }
    }

    private string FormatFromDoc(HtmlDocument doc, string link)
    {
      var title = FindNodes(doc.DocumentNode, "//meta[@property='twitter:title']", "//meta[@property='og:title']", "//meta[@name='title']", "//title");
      var site = FindNodes(doc.DocumentNode, "//meta[@name='author']", "//meta[@property='twitter:site']", "//meta[@property='og:site_name']", "//meta[@name='og:site_name']", "//title");
      return @$"- {title}{Environment.NewLine}  - [{site}]({link})";
    }

    public string FindNodes(HtmlNode node, params string[] xpaths)
    {
      foreach (var xpath in xpaths)
      {
        var item = node.SelectSingleNode(xpath);
        var result = "";
        if (item != null)
        {
          if (item.Attributes.Contains("content")) result = item.Attributes["content"].Value;
          else result = item.InnerText;
        }
        if (!string.IsNullOrWhiteSpace(result)) return result;
      }
      return "(Unknown)";
    }

    public void CreateInOption()
    {
      InOption = new Option("--input")
      {
        Required = true,
        Description = "Filename of simple list of links.",
        Argument = new Argument<FileInfo>().ExistingOnly()
      };

      InOption.AddAlias("-i");
      AddOption(InOption);
    }

    public void CreateOutOption()
    {
      OutOption = new Option("--output")
      {
        Required = true,
        Description = "Filename of output markdown file. If exists, will be deleted.",
        Argument = new Argument<FileInfo>()
      };

      OutOption.AddAlias("-o");

      AddOption(OutOption);
    }

  }
}
