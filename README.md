# QAToolKit Engine HttpTester library
[![Build .NET Library](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/workflows/Build%20.NET%20Library/badge.svg)](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/actions)
[![CodeQL](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/workflows/CodeQL%20Analyze/badge.svg)](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/security/code-scanning)
[![Sonarcloud Quality gate](https://github.com/qatoolkit/qatoolkit-engine-httptester-net/workflows/Sonarqube%20Analyze/badge.svg)](https://sonarcloud.io/dashboard?id=qatoolkit_qatoolkit-engine-httptester-net)
[![NuGet package](https://img.shields.io/nuget/v/QAToolKit.Engine.HttpTester?label=QAToolKit.Engine.HttpTester)](https://www.nuget.org/packages/QAToolKit.Engine.HttpTester/)
[![Discord](https://img.shields.io/discord/787220825127780354?color=%23267CB9&label=Discord%20chat)](https://discord.gg/hYs6ayYQC5)

## Description
`QAToolKit.Engine.HttpTester` is a .NET Standard 2.1 library, that that contains an implementation of `IHttpTesterClient` that is a thin wrapper around .NET `HttpClient` to allow to write easy Http Request calls.

Supported .NET frameworks and standards: `netstandard2.0`, `netstandard2.1`, `netcoreapp3.1`, `net5.0`

Get in touch with me on:

[![Discord](https://img.shields.io/discord/787220825127780354?color=%23267CB9&label=Discord%20chat)](https://discord.gg/hYs6ayYQC5)

### HttpTesterClient

A sample on how to easily call the HTTP request with .NET `HttpClient`:

##### GET request
```csharp
using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
        .WithMethod(HttpMethod.Get)
        .WithPath("/api/bicycles")
        .WithBearerAuthentication("eXy....")
        .Start();

    var msg = await response.GetResponseJsonBody<List<Bicycle>>();

    var expectedResponse = BicycleFixture.GetBicycles().ToExpectedObject();
    expectedResponse.ShouldEqual(msg);

    Assert.True(client.Duration < 2000); //Start() method execution duration
    Assert.True(client.HttpDuration < 2000); //HTTP request duration
    Assert.True(response.IsSuccessStatusCode);
}
```

##### POST request
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
        .WithBasicAuthentication("user", "pass")
        .Start();

    var msg = await response.GetResponseJsonBody<Bicycle>();

    var expectedResponse = bicycle.ToExpectedObject();
    expectedResponse.ShouldEqual(msg);

    Assert.True(client.Duration < 2000);
    Assert.True(response.IsSuccessStatusCode);
    Assert.Equal("Giant", msg.Brand);
}
```

##### POST File Upload request

You can upload files with `multipart/form-data` content type like shown below. 
There are 2 overloads of `WithMultipart`, one for uploading binary data and the other for string data.

```csharp
using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
        .WithMethod(HttpMethod.Post)
        .WithMultipart(image, "FileContent", "logo.png")
        .WithMultipart("MetaData", "My metadata.")
        .WithPath("/api/bicycles/1/images")
        .Start();

    var msg = await response.GetResponseBodyString();

    Assert.Equal("File name: miha.txt, length: 119305", msg);
    Assert.True(response.IsSuccessStatusCode);
}
```

There is content-type safety built-in and you can not do `WithMultipart` and `WithJsonBody` in the same request.

##### Deserialize HttpResponse message body

`QAToolKit.Engine.HttpTester` supports 4 `HttpResponse` helper methods to give you maximum flexibility when reading reponse body.

- `GetResponseBodyString`: return response content as a string.
- `GetResponseJsonBody`: deserialize response content from JSON to object
- `GetResponseXmlBody`: deserialize response content from XML to object.
- `GetResponseBodyBytes`: return response content as byte array.
- [Obsolete] `GetResponseBody`: this is obsolete, it deserializes response content from JSON to object. It's replaced by `GetResponseJsonBody`.

##### HttpClient execution time is measured

The library extends `HttpClient` object with `Duration` and `HttpDuration` properties. The first returns the measured duration of `Start();` method and the latter return the duration of HTTP request execution.

```csharp
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
        .Start();
        
    client.Duration; //Start() method execution duration
    client.HttpDuration; //HTTP request duration
```

##### Create Tester client from QAToolKit Swagger request

If you are using QAToolKit Swagger library to generate `HttpRequest` object you can use a `CreateHttpRequest` override.

In this sample below, `CreateHttpRequest` accepts a first request from the list of requests generated by Swagger library.

```csharp
//GET Requests from Swagger file
var urlSource = new SwaggerUrlSource();

var requests = await urlSource.Load(new Uri[] {
    new Uri("https://qatoolkitapi.azurewebsites.net/swagger/v1/swagger.json")
});

using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(requests.FirstOrDefault())
        .Start();
    ....
}
```

The `CreateHttpRequest` override will assign only `BaseUrl`, `Path`, `HttpMethod`, `Query parameters` and `Header parameters`. You need to assign `HttpBody` manually with `WithJsonBody`.
For example:

```csharp
using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(requests.FirstOrDefault())
        .WithJsonBody(object)
        .Start();
    ....
}
```

#### HttpTesterClient Authentication

Currently `HttpTesterClient` supports:

##### Basic authentication

```csharp
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        ....
        .WithBasicAuthentication("user", "pass")
        .Start();
```

##### Bearer token authentication

```csharp
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        ....
        .WithBearerAuthentication("eXy....")
        .Start();
```

##### NTLM authentication

```csharp
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        ....
        .WithNTLMAuthentication("user", "pass") // or default security context .WithNTLMAuthentication()
        .Start();
```

### HttpTestAsserter

This is an implementation of the HTTP response message asserter, which can be used to assert different parameters.

Here is a list of Asserters:
- `ResponseContentContains`: HTTP body contains a string (ignores case)
- `RequestDurationEquals`: Verify request duration
- `ResponseStatusCodeEquals`: Verify if response code equals
- `ResponseHasHttpHeader`: HTTP response contains a header
- `ResponseStatusCodeIsSuccess`: HTTP response status code is one of 2xx
- `ResponseBodyIsEmpty`: HTTP response body is empty

Asserter produces a list of `AssertResult`:

```csharp
    /// <summary>
    /// Assert result object
    /// </summary>
    public class AssertResult
    {
        /// <summary>
        /// Assert name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Is assert true
        /// </summary>
        public bool IsTrue { get; set; }
        /// <summary>
        /// Assert message
        /// </summary>
        public string Message { get; set; }
    }
```

If we extend the previous example:

```csharp
using (var client = new HttpTesterClient())
{
    var response = await client
        .CreateHttpRequest(new Uri("https://qatoolkitapi.azurewebsites.net"))
        .WithQueryParams(new Dictionary<string, string>() { { "api-version", "1" } })
        .WithMethod(HttpMethod.Get)
        .WithPath("/api/bicycles")
        .Start();

    var duration = client.Duration;

    //pass response to the asserter
    var asserter = new HttpTestAsserter(response);

    //Produce the list of assert results
    var assertResults = asserter
        .ResponseContentContains("scott")
        .RequestDurationEquals(duration, (x) => x < 1000)
        .ResponseStatusCodeEquals(HttpStatusCode.OK)
        .ResponseStatusCodeIsSuccess()
        .AssertAll();

    //if you use xUnit, you can assert the results like this
    foreach (var result in assertResults)
    {
        Assert.True(result.IsTrue, result.Message);
    }
}
```

## To-do

- **This library is a beta version**
- Add more Http Asserters
- Cover more test cases with HTTP Tester client

## License

MIT License

Copyright (c) 2020-2021 Miha Jakovac

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