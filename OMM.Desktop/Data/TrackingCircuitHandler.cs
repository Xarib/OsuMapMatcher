using Microsoft.AspNetCore.Components.Server.Circuits;
using OMM.Desktop.Data.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OMM.Desktop.Data
{
    public class TrackingCircuitHandler : CircuitHandler
    {
        private HashSet<Circuit> circuits = new HashSet<Circuit>();
        private readonly ISettings _settings;

        public TrackingCircuitHandler(ISettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public override Task OnConnectionUpAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            circuits.Add(circuit);

            Console.WriteLine($"Connection started {DateTime.Now} connections {ConnectedCircuits}");

            return Task.CompletedTask;
        }

        public override Task OnConnectionDownAsync(Circuit circuit,
            CancellationToken cancellationToken)
        {
            circuits.Remove(circuit);

            Console.WriteLine($"Connection lost {DateTime.Now} connections {ConnectedCircuits}");

            if (ConnectedCircuits == 0 && _settings.UserSettings.ExitOnAllTabsClosed)
                Environment.Exit(0);

            return Task.CompletedTask;
        }

        public int ConnectedCircuits => circuits.Count;
    }
}
