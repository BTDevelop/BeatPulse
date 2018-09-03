﻿using BeatPulse.Core;
using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace BeatPulse.Network
{
    public class PingLiveness : IBeatPulseLiveness
    {
        private readonly PingLivenessOptions _options;

        public PingLiveness(PingLivenessOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<(string, bool)> IsHealthy(LivenessExecutionContext context, CancellationToken cancellationToken = default)
        {
            var configuredHosts = _options.ConfiguredHosts.Values;

            foreach (var item in configuredHosts)
            {
                try
                {
                    using (var ping = new Ping())
                    {
                        var pingReply = await ping.SendPingAsync(item.Host, item.TimeOut);

                        if (pingReply.Status != IPStatus.Success)
                        {
                            return ($"Ping check for host {item.Host} is failed with status reply:{pingReply.Status}", false);
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = !context.IsDevelopment ? string.Format(BeatPulseKeys.BEATPULSE_HEALTHCHECK_DEFAULT_ERROR_MESSAGE, context.Name)
                       : $"Exception {ex.GetType().Name} with message ('{ex.Message}')";

                    return (message, false);
                }
            }

            return (BeatPulseKeys.BEATPULSE_HEALTHCHECK_DEFAULT_OK_MESSAGE, true);
        }
    }
}
