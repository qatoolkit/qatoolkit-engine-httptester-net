# QAToolKit Engine HttpTester library
![https://github.com/qatoolkit/qatoolkit-engine-httptester-net/actions](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/workflows/Build%20.NET%20Library/badge.svg)
![https://github.com/qatoolkit/qatoolkit-engine-httptester-net/security/code-scanning](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/workflows/CodeQL%20Analyze/badge.svg)
![https://sonarcloud.io/dashboard?id=qatoolkit_qatoolkit-engine-httptester-net](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/workflows/Sonarqube%20Analyze/badge.svg)
![https://www.nuget.org/packages/QAToolKit.Engine.HttpTester/](https://img.shields.io/nuget/v/QAToolKit.Engine.HttpTester?label=QAToolKit.Engine.HttpTester)

## Description
`QAToolKit.Engine.HttpTester` is a .NET Standard 2.1 library, that that contains an implementation of `IHttpTesterClient` that is a thin wrapper around .NET `HttpClient` to allow to write easy Http Request calls.

Supported .NET frameworks and standards: `netstandard2.0`, `netstandard2.1`, `netcoreapp3.1`, `net5.0`

### HttpTesterClient

A sample on how to easily call the HTTP request with .NET `HttpClient`:

**GET request**
```csharp
using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
        .WithMethod(HttpMethod.Get)
        .WithPath("/api/bicycles")
        .Start();

    var msg = await response.GetResponseBody<List<Bicycle>>();

    var expecterResponse = BicycleFixture.GetBicycles().ToExpectedObject();
    expecterResponse.ShouldEqual(msg);

    Assert.True(client.Duration < 2000);
    Assert.True(response.IsSuccessStatusCode);
}
```

**POST request**
```csharp

//Payload object
var bicycle = new Bicycle
{
    Id = 5,
    Name = "EXCEED CFR",
    Brand = "Giant",
    Type = BicycleType.Mountain
};

using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
        .WithMethod(HttpMethod.Post)
        .WithJsonBody(bicycle)
        .WithPath("/api/bicycles")
        .Start();

    var msg = await response.GetResponseBody<Bicycle>();

    var expecterResponse = bicycle.ToExpectedObject();
    expecterResponse.ShouldEqual(msg);

    Assert.True(client.Duration < 2000);
    Assert.True(response.IsSuccessStatusCode);
    Assert.Equal("Giant", msg.Brand);
}
```

## To-do

- **This library is an early alpha version**

## License

MIT License

Copyright (c) 2020 Miha Jakovac

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.