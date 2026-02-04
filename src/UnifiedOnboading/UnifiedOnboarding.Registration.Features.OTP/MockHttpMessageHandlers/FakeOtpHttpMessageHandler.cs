using System.Net;
using System.Net.Http.Json;
using UnifiedOnboarding.Registration.Features.OTP.Abstractions;

namespace UnifiedOnboarding.Registration.Features.OTP.MockHttpMessageHandlers;

public class FakeOtpHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string path = request.RequestUri!.AbsolutePath.ToLower();
        if (path.Contains("sendOtp", StringComparison.OrdinalIgnoreCase))
        {
            var fakeResponse = new SendOtpResultDto(
                true,
                "FAKE: OTP sent successfully"
            );
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(fakeResponse)
            };
            return Task.FromResult(httpResponse);
        }
        else if (path.Contains("VerifyOtp", StringComparison.OrdinalIgnoreCase))
        {


            var fakeResponse = new VerifyOtpResultDto(true, "FAKE: OTP verification Succussed", string.Empty, string.Empty);
            var httpResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(fakeResponse)
            };
            return Task.FromResult(httpResponse);
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
