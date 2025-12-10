using System;
using Keymaker.Service.Dns;
using Moq;
using Shouldly;
using Xunit;

namespace Keymaker.Tests.Service.Dns;

public sealed class DnsServiceTests
{
    [Theory]
    [InlineData("example.com", "_acme-challenge")]
    [InlineData("test.example.com", "_acme-challenge.test")]
    [InlineData("test.lan.example.com", "_acme-challenge.test.lan")]
    [InlineData("*.example.com", "_acme-challenge")]
    [InlineData("*.test.example.com", "_acme-challenge.test")]
    [InlineData("*.test.lan.example.com", "_acme-challenge.test.lan")]
    [InlineData("***.test.lan.example.com", "_acme-challenge.test.lan")]
    [InlineData(".test.lan.example.com", "_acme-challenge.test.lan")]
    public void SetDomain_InputIsValid_ReturnsExpectedOutput(string input, string output)
    {
        var dnsLookup = new Mock<IDnsLookupService>();
        var azureDnsService = new DnsService(input, dnsLookup.Object);
        
        azureDnsService.SetDomain.ShouldBe(output);
    }
    
    [Theory]
    [InlineData("example.com", "_acme-challenge.example.com")]
    [InlineData("test.example.com", "_acme-challenge.test.example.com")]
    [InlineData("test.lan.example.com", "_acme-challenge.test.lan.example.com")]
    [InlineData("*.example.com", "_acme-challenge.example.com")]
    [InlineData("*.test.example.com", "_acme-challenge.test.example.com")]
    [InlineData("*.test.lan.example.com", "_acme-challenge.test.lan.example.com")]
    public void CheckDomain_InputIsValid_ReturnsExpectedOutput(string input, string output)
    {
        var dnsLookup = new Mock<IDnsLookupService>();
        var azureDnsService = new DnsService(input, dnsLookup.Object);
        
        azureDnsService.CheckDomain.ShouldBe(output);
    }
    
    [Theory]
    [InlineData(".com")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Create_ShowdTrhow(string input)
    {
        var dnsLookup = new Mock<IDnsLookupService>();
        
        Should.Throw<ArgumentException>(() => new DnsService(input, dnsLookup.Object));
    }
}