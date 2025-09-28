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

dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" package "TUnit"
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" package "Verify"
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" package "Verify.TUnit"
dotnet add ".\project\test\%name%.Tests\%name%.Tests.csproj" reference ".\project\src\%name%\%name%.csproj"
del project\test\Brimborium.CopyProject.Tests\Program.cs

xcopy /S %~dp0\CreateProject\test project\test\%name%.Tests\
