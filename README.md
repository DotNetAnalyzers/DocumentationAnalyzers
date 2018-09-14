# Documentation Analyzers for the .NET Compiler Platform

[![NuGet](https://img.shields.io/nuget/v/DocumentationAnalyzers.svg)](https://www.nuget.org/packages/DocumentationAnalyzers)[![NuGet Beta](https://img.shields.io/nuget/vpre/DocumentationAnalyzers.svg)](https://www.nuget.org/packages/DocumentationAnalyzers)

[![Join the chat at https://gitter.im/DotNetAnalyzers/DocumentationAnalyzers](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/DotNetAnalyzers/DocumentationAnalyzers?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

[![Build status](https://ci.appveyor.com/api/projects/status/8jw2lq431kgg44jl/branch/master?svg=true)](https://ci.appveyor.com/project/sharwell/Documentationanalyzers/branch/master)

[![codecov.io](http://codecov.io/github/DotNetAnalyzers/DocumentationAnalyzers/coverage.svg?branch=master)](http://codecov.io/github/DotNetAnalyzers/DocumentationAnalyzers?branch=master)

This repository contains an implementation of .NET documentation rules using the .NET Compiler Platform. Where possible, code fixes are also provided to simplify the process of correcting violations.

## Using DocumentationAnalyzers

The preferable way to use the analyzers is to add the nuget package [DocumentationAnalyzers](http://www.nuget.org/packages/DocumentationAnalyzers/)
to the project where you want to enforce documentation rules.

The severity of individual rules may be configured using [rule set files](https://docs.microsoft.com/en-us/visualstudio/code-quality/using-rule-sets-to-group-code-analysis-rules)
in Visual Studio 2015 or newer. See [Configuration.md](docs/Configuration.md) for more information.

For documentation and reasoning on the rules themselves, see the [DOCUMENTATION.md](DOCUMENTATION.md).

## Installation

DocumentationAnalyzers can be installed using the NuGet command line or the NuGet Package Manager in Visual Studio 2015.

**Install using the command line:**
```bash
Install-Package DocumentationAnalyzers
```

## Team Considerations

If you use older versions of Visual Studio in addition to Visual Studio 2015 or Visual Studio 2017, you may still install these analyzers. They will be automatically disabled when you open the project back up in Visual Studio 2013 or earlier.

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md)

## Current status

An up-to-date list of which documentation rules are implemented and which have code fixes can be found [here](https://dotnetanalyzers.github.io/DocumentationAnalyzers/).
