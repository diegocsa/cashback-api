using Cashback.Infra.CrossCutting.Auth;
using System.Collections.Generic;
using Xunit;

namespace Cashback.Infra.CrossCutting.Test
{
    public class HasherTest
    {
        [Theory]
        [InlineData("test@test.com")]
        [InlineData("test1@test.com")]
        public void CreatePasswordHash_ValidData_HashesPassword(string email)
        {
            var asserts = new Dictionary<string, string>()
            {
                {"test@test.com", "+yKVL0EtcmHxNhJkBRThI9DR5Ao2H4axXFeZjzIKt6g=" },
                {"test1@test.com", "QNtfVbiSq5nHiF0UCvJmGp4pxNYjpsFl1qvnmi8wC8Y=" },
            };

            var hash = Hasher.CreatePasswordHash(email, "123456");

            Assert.Equal(asserts[email], hash);

        }
    }
}
