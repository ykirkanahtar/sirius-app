using System.Threading.Tasks;
using Sirius.Models.TokenAuth;
using Sirius.Web.Controllers;
using Shouldly;
using Xunit;

namespace Sirius.Web.Tests.Controllers
{
    public class HomeController_Tests: SiriusWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}