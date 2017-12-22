using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketGeneralPurpose.Model;

namespace WebSocketGeneralPurpose
{
    public class Startup
    {
        public static Dictionary<string, WebSocket> UserSockets = new Dictionary<string, WebSocket>();

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvcWithDefaultRoute();

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var protocol = context.WebSockets.WebSocketRequestedProtocols.FirstOrDefault();
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync(protocol);

                        UserSockets[protocol] = webSocket;

                        await Echo(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });

            app.UseFileServer();
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            string jsonString;
            Message message;
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                jsonString = Encoding.UTF8.GetString(buffer);

                try
                {
                    message = JsonConvert.DeserializeObject<Message>(jsonString);

                    await SendAsync(buffer, result, message);
                }
                catch (Exception)
                {

                }

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task SendAsync(byte[] buffer, WebSocketReceiveResult result, Message message)
        {
            if (!UserSockets.ContainsKey(message.User))
            {
                return;
            }
            var client = UserSockets[message.User];

            await client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
        }
    }
}
