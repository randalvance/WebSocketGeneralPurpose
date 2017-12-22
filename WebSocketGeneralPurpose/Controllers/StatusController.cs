using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using WebSocketGeneralPurpose.Model;

namespace WebSocketGeneralPurpose.Controllers
{
    [Route("/api/status")]
    public class StatusController : Controller
    {
        public IEnumerable<UserStatus> Get()
        {
            if (Startup.UserSockets != null)
            {
                return Startup.UserSockets.Select(item => new UserStatus()
                {
                    User = item.Key,
                    IsConnected = item.Value != null && item.Value.State == WebSocketState.Open
                }).ToList();
            }

            return new List<UserStatus>();
        }

        [Route("{user}")]
        [HttpGet]
        public bool GetUserStatus(string user)
        {
            if (Startup.UserSockets == null || Startup.UserSockets.ContainsKey(user))
            {
                return Startup.UserSockets[user].State == WebSocketState.Open;
            }

            return false;
        }
    }
}
