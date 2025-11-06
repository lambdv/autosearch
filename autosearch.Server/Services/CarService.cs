using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Io;
using autosearch.Controllers;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace autosearch.Services;

/// <summary>
/// service to get car listings from websites
/// </summary>
public interface ICarService
{
    Task<JsonArray> GetAsync();
    Task RevalidateAsync();
}