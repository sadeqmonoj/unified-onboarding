using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using Platform.Contracts.Configurations;
using UnifiedOnboarding.Registration.Features.OTP.Abstractions;

namespace UnifiedOnboarding.Registration.Features.OTP.Services;

public class OtpApi : IOtpApi
{
    private readonly HttpClient _http;
    private readonly IOtpTokenApi _otpTokenApi;
    private readonly OtpAuthOptions _authOptions;
    public OtpApi(HttpClient http, IOtpTokenApi otpTokenApi, IOptions<OtpAuthOptions> authOptions)
    {
        _http = http;
        _otpTokenApi = otpTokenApi;
        _authOptions = authOptions.Value;
    }

    public async Task<SendOtpResultDto> SendOtpAsync(string MobileNumber, CancellationToken cancellationToken)
    {
        HttpResponseMessage? response = await _http.GetAsync($"api/sendOtp?MobileNumber", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content!.ReadFromJsonAsync<SendOtpResultDto>(cancellationToken: cancellationToken)
               ?? new SendOtpResultDto(false, "OTP Send Failed");
    }

    public async Task<VerifyOtpResultDto> VerifyOtpAsync(
    VerifyOtpRequestDto requestDto,
    CancellationToken cancellationToken)
    {
        // 1. Send OTP verification request
        HttpResponseMessage response =
            await _http.PostAsJsonAsync("api/VerifyOtp", requestDto, cancellationToken);

        // 2. If HTTP fails → we let middleware handle
        response.EnsureSuccessStatusCode();

        // 3. Parse response
        VerifyOtpResultDto otpVerificationResponse =
            await response.Content.ReadFromJsonAsync<VerifyOtpResultDto>(cancellationToken: cancellationToken)
            ?? new VerifyOtpResultDto(false, "OTP verification failed", string.Empty, string.Empty);

        // 4. If backend says NOT verified → return directly
        if (!otpVerificationResponse.IsVerified)
        {
            return otpVerificationResponse;
        }

        // 5. Build token request
        var tokenRequestDto = new GetTokenRequestDto(
            GrantType: _authOptions.GrantType!,
            Username: requestDto.MobileNumber,
            Password: requestDto.MobileNumber,
            ClientId: _authOptions.ClientId!,
            ClientSecret: _authOptions.ClientSecret!
        );

        // 6. Get token from token service
        GetTokenResponseDto? tokenResponseDto =
            await _otpTokenApi.GetOtpTokenAsync(tokenRequestDto, cancellationToken);

        // 7. If token retrieval failed → return failed result
        if (tokenResponseDto is null || string.IsNullOrWhiteSpace(tokenResponseDto.access_token))
        {
            return new VerifyOtpResultDto(
                false,
                "OTP verified but token retrieval failed",
                string.Empty,
                string.Empty
            );
        }

        // 8. Success → return merged response
        return new VerifyOtpResultDto(
            otpVerificationResponse.IsVerified,
            otpVerificationResponse.Message,
            tokenResponseDto.access_token,
            tokenResponseDto.token_type // Correct mapping
        );
    }

}
