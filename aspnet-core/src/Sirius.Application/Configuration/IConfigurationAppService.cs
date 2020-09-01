using System.Threading.Tasks;
using Sirius.Configuration.Dto;

namespace Sirius.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
