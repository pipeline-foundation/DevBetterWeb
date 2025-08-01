﻿using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using DevBetterWeb.Core.Interfaces;
using Microsoft.Extensions.Options;

namespace DevBetterWeb.Infrastructure.Services;

public class Smtp2GoEmailService : IEmailService
{
  private readonly HttpClient _httpClient;
  public Smtp2GoEmailService(IOptions<ApiMailSenderOptions> optionsAccessor)
  {
    Guard.Against.Null(optionsAccessor, nameof(optionsAccessor));
    Guard.Against.Null(optionsAccessor.Value, nameof(optionsAccessor.Value));
    Options = optionsAccessor.Value;
    _httpClient = new HttpClient();
  }

	public ApiMailSenderOptions Options { get; }
	public async Task SendEmailAsync(string email, string subject, string message)
  {
    if (string.IsNullOrEmpty(Options.ApiKey)) throw new Exception("SMTP API Key not set.");
    if (string.IsNullOrEmpty(Options.ApiBaseUrl)) throw new Exception("SMTP API Base URL not set.");
    if (string.IsNullOrEmpty(Options.Sender)) throw new Exception("SMTP Sender not set.");

		var request = new HttpRequestMessage(HttpMethod.Post, Options.ApiBaseUrl.TrimEnd('/') + "/email/send");
		request.Headers.Add("X-Smtp2go-Api-Key", Options.ApiKey);

		var payload = new
    {
			sender = Options.Sender,
			to = new[] { email },
      subject = subject,
      text_body = message,
      html_body = message
    };
    string json = JsonSerializer.Serialize(payload);
    request.Content = new StringContent(json, Encoding.UTF8, "application/json");

    var response = await _httpClient.SendAsync(request);
    if (!response.IsSuccessStatusCode)
    {
      var error = await response.Content.ReadAsStringAsync();
      throw new Exception($"SMTP2GO API error: {response.StatusCode} - {error}");
    }
  }
}
