using System;
using System.Threading.Tasks;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Web.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orleans;

namespace Darchatty.Gateway.Controllers
{
    [ApiController]
    [Route("Auth")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IGrainFactory grainFactory;

        public AuthController(IGrainFactory grainFactory)
        {
            this.grainFactory = grainFactory;
        }

        [Route("Login")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> LoginAsync([FromBody] LoginDto request)
        {
            var users = grainFactory.GetGrain<IUsersGrain>(0);
            var userGrain = await users.GetUserWithUsernameAsync(request.Username);
            return Ok(userGrain.GetPrimaryKey());
        }

        [Route("Register")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> RegisterAsync([FromBody] RegisterDto request)
        {
            var users = grainFactory.GetGrain<IUsersGrain>(0);
            await users.CreateUserAsync(request.Username, request.Password);
            return Ok();
        }
    }
}
