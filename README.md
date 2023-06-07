# Simple-Downloader

[![Build and Install .NET Framework](https://github.com/niceEli/Simple-Downloader/actions/workflows/main.yml/badge.svg)](https://github.com/niceEli/Simple-Downloader/actions/workflows/main.yml)

## The Dependancy To Destroy Them All

This Downloads Things


Example

``` batch
@echo off
simple-downloader "https://example.com/download" "C://Program Files/Example"
```

``` CSharp
using System;
using System.Diagnostics;

public class Program() 
{
    public void main(string args[])
    {
        Process.Start("Simple-Downloader", "https://example.com/download", "C://Program Files/Example");
    }
}
```

both of these will download https://example.com/download (not real and for demonstration) to C://Program Files/Example
