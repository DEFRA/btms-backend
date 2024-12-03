using Microsoft.AspNetCore.Builder;

namespace BtmsBackend.Test.Config;

public class EnvironmentTest
{

   [Fact]
   public void IsNotDevModeByDefault()
   {
      var _builder = WebApplication.CreateBuilder();

      var isDev = BtmsBackend.Config.Environment.IsDevMode(_builder);

      Assert.False(isDev);
   }
}
