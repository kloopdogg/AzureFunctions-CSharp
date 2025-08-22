# AzureFunctions-CSharp

Sample Azure Functions v4 project with C# (Isolated Worker Model)

## Project Structure
The recommended folder structure for a C# Azure Functions v4 application using the isolated worker model:
```text
<project_root>/
├── src/
│   ├── FunctionApp/
│   │   ├── Functions/
│   │   │   ├── MyFirstFunction.cs
│   │   │   └── MySecondFunction.cs
│   │   ├── FunctionApp.csproj
│   │   ├── host.json
│   │   ├── local.settings.json
│   │   └── Program.cs
│   └── Shared/
│       └── Models/
│           └── MyModel.cs
├── test/
│   ├── FunctionApp.Tests/
│   │   ├── Fakes/
│   │   │   ├── FakeHttpCookies.cs
│   │   │   ├── FakeHttpRequestData.cs
│   │   │   └── FakeHttpResponseData.cs
│   │   ├── Mocks/
│   │   │   └── MockDataAccess.cs
│   │   ├── FunctionApp.Tests.csproj
│   │   ├── MyFirstFunctionTests.cs
│   │   └── MySecondFunctionTests.cs
│   └── RestClient.Tests/
│       ├── MyFirstFunction.http
│       └── MySecondFunction.http
└── SampleFunctionApp.sln
```

## Running the Project

To run the project locally:

1. Ensure you have the Azure Functions Core Tools and .NET 6+ SDK installed.
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build the project:
   ```bash
   dotnet build
   ```
4. Start the Functions host:
   ```bash
   cd src/SampleFunctionApp
   func start
   ```

This will launch the Azure Functions host and run your C# functions in the isolated worker process.

**Note:** The entry point for the isolated worker is typically `Program.cs`, which configures the host and registers functions.

## Testing

### Running Unit Tests

Unit tests for C# Azure Functions are typically written using xUnit, NUnit, or MSTest. To run tests:

1. Navigate to the test project directory (if present):
   ```bash
   cd tests/AzureFunctions-CSharp.Tests
   ```
2. Run all tests:
   ```bash
   dotnet test
   ```

### REST Client Tests

You can write REST client integration tests using `.http` files, which can be executed with tools like [REST Client extension for VS Code](https://marketplace.visualstudio.com/items?itemName=humao.rest-client).

Example `.http` file (`FunctionApp.http`):

```http
### Get sample data
GET http://localhost:7071/api/MyFunction
Accept: application/json

### Post sample data
POST http://localhost:7071/api/MyFunction
Content-Type: application/json

{
   "property1": "value1",
   "property2": 123
}
```

To run these tests:

1. Start the Azure Functions host locally.
2. Open the `.http` file in VS Code and click "Send Request" above each request, or use a compatible CLI tool.

This approach helps automate and document API testing alongside your codebase.

> ⚠️NOTE: Testing with `.http` files requires the VS Code extension `humao.rest-client` to be installed