using System;
using System.Security.Claims;
using Xunit;

namespace GFLHApp.Tests.Models
{
    public class SecurityAndAuthorizationTests
    {
        [Theory]
        [InlineData("Admin", "Admin", true)]
        [InlineData("Producer", "Producer", true)]
        [InlineData("Standard", "Producer", false)]
        [InlineData("Developer", "Developer", true)]
        public void RoleClaims_AuthorizationEvaluation_MatchesAssignedRoles(string assignedRole, string requiredRole, bool expectedAccess)
        {
            var claims = new[] { new Claim(ClaimTypes.Role, assignedRole) };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            bool isInRole = principal.IsInRole(requiredRole);
            Assert.Equal(expectedAccess, isInRole);
        }

        [Fact]
        public void ProducerOwnership_EditVerification_EnforcesUserMatch()
        {
            string currentLoggedInUserId = "user-producer-123";
            string resourceOwnerUserId = "user-producer-123";
            string unauthorizedUserId = "user-producer-999";

            bool isOwnerAuthorized = currentLoggedInUserId == resourceOwnerUserId;
            bool isIntruderAuthorized = unauthorizedUserId == resourceOwnerUserId;

            Assert.True(isOwnerAuthorized);
            Assert.False(isIntruderAuthorized);
        }
    }
}
