using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedOnboarding.Auth.Features.Me.UserProfile;

public record GetUserProfileResponse
(
        long UserId, string FirstName, string LastName, string Email, DateTime CreatedAt
    );
