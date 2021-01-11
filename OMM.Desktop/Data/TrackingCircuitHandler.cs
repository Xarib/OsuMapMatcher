using Microsoft.AspNetCore.Components.Server.Circuits;
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

            //if (ConnectedCircuits == 0)
                //Environment.Exit(0);

            return Task.CompletedTask;
        }

        public int ConnectedCircuits => circuits.Count;
    }
}
