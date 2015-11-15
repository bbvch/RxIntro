namespace Bbv.Rx.Destruction
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Windows.Forms;

    using FSharp.Charting;

    using Microsoft.FSharp.Core;

    public partial class CanonControl : Form
    {
        private const double BulletTime = 10;

        private ChartTypes.ChartControl chartControl;

        public CanonControl()
        {
            this.InitializeComponent();
            this.StartSimulation();
        }

        private static ChartTypes.GenericChart CreateChart(IObservable<Tuple<double, double>> trajectory)
        {
            var noString = FSharpOption<string>.None;
            return LiveChart.FastLineIncremental(trajectory, noString, noString, FSharpOption<Color>.None, noString, noString);
        }

        private void StartSimulation()
        {
            this.AcquireTarget()
                .Select(_ => BallisticSimulation.Run(_, BulletTime))
                .ObserveOn(new WindowsFormsSynchronizationContext())
                .Select(CreateChart)
                .Scan(
                    Enumerable.Empty<ChartTypes.GenericChart>(),
                    (charts, chart) => charts.Reverse().Take(9).Reverse().Concat(new[] { chart }))

                .Select(Chart.Combine)
                .Select(_ => new ChartTypes.ChartControl(_) { Dock = DockStyle.Fill })
                .Subscribe(this.ExchangeChart);
        }

        private IObservable<BallisticParameters> AcquireTarget()
        {
            var power =
                Observable.FromEventPattern(ev => this.power.ValueChanged += ev, ev => this.power.ValueChanged -= ev)
                    .Select(_ => this.power.Value);

            var angle =
                Observable.FromEventPattern(ev => this.angle.ValueChanged += ev, ev => this.angle.ValueChanged -= ev)
                    .Select(_ => this.angle.Value);

            return Observable.CombineLatest(power, angle, (p, a) => new BallisticParameters(p, a))
                .Throttle(TimeSpan.FromMilliseconds(250));
        }

        private void ExchangeChart(ChartTypes.ChartControl next)
        {
            if (this.chartControl != null)
            {
                this.Controls.Remove(this.chartControl);
                this.chartControl.Dispose();
            }

            this.Controls.Add(next);
            this.chartControl = next;
        }

        private void AngleScroll(object sender, ScrollEventArgs e)
        {
            this.toolTipAngle.SetToolTip(this.angle, e.NewValue.ToString());
        }
    }
}
