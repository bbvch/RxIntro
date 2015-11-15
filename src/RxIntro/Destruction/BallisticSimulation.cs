namespace Bbv.Rx.Destruction
{
    using System;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    using MathNet.Numerics.LinearAlgebra;

    public static class BallisticSimulation
    {
        private const double Δt = 0.001;

        private const double G = 9.81;

        private const double AirResistance = 10;

        private const int X = 0, Y = 1;

        public static IObservable<Tuple<double, double>> Run(BallisticParameters ballisticParameters, double bulletTimeFactor = 1)
        {
            var angle = ballisticParameters.Elevation * Math.PI / 180;
            var powerComponents = new[]
            {
                ballisticParameters.Power * Math.Sin(angle),
                ballisticParameters.Power * Math.Cos(angle)
            };
            var initial = new State(0, Vector<double>.Build.Dense(2, 0.0), Vector<double>.Build.Dense(powerComponents));

            var states = Observable.Generate(initial, Flying, NextState, _ => _, _ => TimeSpan.FromSeconds(Δt * bulletTimeFactor), Scheduler.Default);
            return states.Select(_ => new Tuple<double, double>(_.Position[X], _.Position[Y]));
        }

        private static Vector<double> NextPosition(Vector<double> position, Vector<double> speed)
        {
            return position + (speed * Δt);
        }

        private static Vector<double> NextSpeed(Vector<double> speed)
        {
            var drag = AirResistance * speed.L2Norm() * Δt;
            var vy = speed[Y] - (G * Δt) - (speed[Y] * drag);
            var vx = speed[X] - (speed[X] * drag);
            return Vector<double>.Build.Dense(new[] { vx, vy });
        }

        private static State NextState(State state)
        {
            return new State(state.Time + Δt, NextPosition(state.Position, state.Speed), NextSpeed(state.Speed));
        }

        private static bool Flying(State state)
        {
            return state.Time <= 0 || state.Position[Y] > 0;
        }

        private class State
        {
            public State(double time, Vector<double> position, Vector<double> speed)
            {
                this.Time = time;
                this.Position = position;
                this.Speed = speed;
            }

            public double Time { get; }

            public Vector<double> Position { get; }

            public Vector<double> Speed { get; }
        }
    }
}