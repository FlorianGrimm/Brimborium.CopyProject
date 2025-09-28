set name=Brimborium.CopyProject
mkdir project
dotnet new sln --name %name% --output project
copy %~dp0\Directory.Build.props project\Directory.Build.props
copy %~dp0\Directory.Packages.props project\Directory.Packages.props

mkdir project\description

mkdir project\src
dotnet new console --output project\src\%name%
dotnet sln ".\project\%name%.sln" add ".\project\src\%name%\%name%.csproj"

mkdir project\test
dotnet new console --output project\test\%name%.Tests
dotnet sln ".\project\%name%.sln" add ".\project\test\%name%.Tests\%name%.Tests.csproj"

dotnet nuget add project\test\%name%.Tests --package TUnit
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" package "TUnit"
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" package "Verify"
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" package "Verify.TUnit"
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" reference ".\project\src\%name%\%name%.csproj"
del project\test\Brimborium.CopyProject.Tests\Program.cs

mkdir project\test\%name%.Tests\Services\
mkdir project\test\%name%.Tests\TestUtility\
mkdir project\test\%name%.Tests\TUnit.Playwright\

copy %~dp0\CreateProject\test\appsettings.json project\test\%name%.Tests\appsettings.json
copy %~dp0\CreateProject\test\GlobalUsings.cs project\test\%name%.Tests\GlobalUsings.cs
copy %~dp0\CreateProject\test\Services project\test\%name%.Tests\Services
copy %~dp0\CreateProject\test\TestUtility project\test\%name%.Tests\TestUtility
copy %~dp0\CreateProject\test\TUnit.Playwright project\test\%name%.Tests\TUnit.Playwright
copy %~dp0\CreateProject\test\Services\ReplacementsExtensions.cs project\test\%name%.Tests\Services\ReplacementsExtensions.cs
copy %~dp0\CreateProject\test\TestUtility\AppPageTest.cs project\test\%name%.Tests\TestUtility\AppPageTest.cs
copy %~dp0\CreateProject\test\TestUtility\TestAuthHandler.cs project\test\%name%.Tests\TestUtility\TestAuthHandler.cs
copy %~dp0\CreateProject\test\TestUtility\TestPrepares.cs project\test\%name%.Tests\TestUtility\TestPrepares.cs
copy %~dp0\CreateProject\test\TestUtility\TestsIntegration.cs project\test\%name%.Tests\TestUtility\TestsIntegration.cs
copy %~dp0\CreateProject\test\TestUtility\WebApplicationFactoryIntegration.cs project\test\%name%.Tests\TestUtility\WebApplicationFactoryIntegration.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\BrowserService.cs project\test\%name%.Tests\TUnit.Playwright\BrowserService.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\BrowserTest.cs project\test\%name%.Tests\TUnit.Playwright\BrowserTest.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\ContextTest.cs project\test\%name%.Tests\TUnit.Playwright\ContextTest.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\DefaultPlaywrightParallelLimiter.cs project\test\%name%.Tests\TUnit.Playwright\DefaultPlaywrightParallelLimiter.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\IWorkerService.cs project\test\%name%.Tests\TUnit.Playwright\IWorkerService.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\PageTest.cs project\test\%name%.Tests\TUnit.Playwright\PageTest.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\PlaywrightSkipAttribute.cs project\test\%name%.Tests\TUnit.Playwright\PlaywrightSkipAttribute.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\PlaywrightTest.cs project\test\%name%.Tests\TUnit.Playwright\PlaywrightTest.cs
copy %~dp0\CreateProject\test\TUnit.Playwright\TUnit.Playwright.csproj project\test\%name%.Tests\TUnit.Playwright\TUnit.Playwright.csproj
copy %~dp0\CreateProject\test\TUnit.Playwright\WorkerAwareTest.cs project\test\%name%.Tests\TUnit.Playwright\WorkerAwareTest.cs


