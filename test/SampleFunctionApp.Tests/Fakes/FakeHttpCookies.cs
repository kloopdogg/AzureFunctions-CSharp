using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace SampleFunctionApp.Tests.Fakes;

public class FakeHttpCookies : HttpCookies
{
    private Dictionary<string, string> _cookies = [];

    public override void Append(string name, string value)
    {
        _cookies[name] = value;
    }

    public override void Append(IHttpCookie cookie)
    {
        if (cookie != null)
        {
            _cookies[cookie.Name] = cookie.Value;
        }
    }

    public IReadOnlyDictionary<string, string> GetAll()
    {
        return _cookies;
    }

    public override IHttpCookie CreateNew()
    {
        // For testing, you can return a simple fake or throw NotImplementedException if not needed
        throw new NotImplementedException("CreateNew is not implemented for FakeHttpCookies");
    }
}