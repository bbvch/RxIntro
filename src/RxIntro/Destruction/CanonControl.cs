namespace Bbv.Rx.Destruction
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Windows.Forms;

    using FSharp.Charting;

    using Microsoft.FSharp.Core;

    public partial class CanonControl : Form
    {
        private const double BulletTime = 10;

        private ChartTypes.ChartControl chartControl;

        private IEnumerable<ChartTypes.GenericChart> charts = Enumerable.Empty<ChartTypes.GenericChart>();

        public CanonControl()
        {
            this.InitializeComponent();
            this.StartSimulation();
        }

        private void StartSimulation()
        {
            this.AcquireTarget()
                .Select(_ => BallisticSimulation.Run(_, BulletTime))

                // TODO Hint: UIs don't like foreign threads...
                .Subscribe(this.Show);
        }

        private IObservable<BallisticParameters> AcquireTarget()
        {
            var power =
                Observable.FromEventPattern(ev => this.power.ValueChanged += ev, ev => this.power.ValueChanged -= ev)
                    .Select(_ => this.power.Value);

            // TODO Ctrl+C / Ctrl+V
            IObservable<int> angle = null;

            // TODO Whenever an input parameter changes, combine it with the latest other.
            // TODO Do we want to trigger that many simulations while dragging a slider?
            return Observable.Return(new BallisticParameters(0, 0));
        }

        private void Show(IObservable<Tuple<double, double>> trajectory)
        {
            // No need to change anything here!
            Debug.Assert(SynchronizationContext.Current != null, "Cannot write to the UI using a background thread.");
            var noString = FSharpOption<string>.None;
            var chart = LiveChart.FastLineIncremental(trajectory, noString, noString, FSharpOption<Color>.None, noString, noString);
            this.charts = this.charts.Reverse().Take(9).Reverse().Concat(new[] { chart });

            this.SuspendLayout();
            if (this.chartControl != null)
            {
                this.Controls.Remove(this.chartControl);
                this.chartControl.Dispose();
            }
            
            this.chartControl = new ChartTypes.ChartControl(Chart.Combine(this.charts)) { Dock = DockStyle.Fill };
            this.Controls.Add(this.chartControl);
            this.ResumeLayout();
        }

        private void AngleScroll(object sender, ScrollEventArgs e)
        {
            this.toolTipAngle.SetToolTip(this.angle, e.NewValue.ToString());
        }
    }
}
