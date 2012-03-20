using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using ShomreiTorah.Common;
using ShomreiTorah.WinForms.Forms;

namespace ShomreiTorah.Schedules.WinClient {
	static class Waiter {
		static readonly Queue<Action> pendingActions = new Queue<Action>();
		static bool isRunning;

		public static void ExecAsync(Action userMethod, string caption) { ExecAsync(_ => userMethod(), caption, false); }
		public static void ExecAsync(Action<IProgressReporter> userMethod, string caption, bool cancellable) {
			var syncContext = SynchronizationContext.Current;
			Debug.Assert(syncContext is WindowsFormsSynchronizationContext);


			Action wrapper = delegate {
				Debug.Assert(syncContext is WindowsFormsSynchronizationContext);
				isRunning = true;
				try {
					using (var dialog = new ProgressForm {
						Caption = caption,
						Maximum = -1,
						CanCancel = cancellable
					})
					using (var waiter = new ManualResetEvent(false)) {
						ThreadPool.QueueUserWorkItem(delegate {
							try {
								userMethod(dialog);
							} catch (Exception ex) {
								syncContext.Post(delegate {
									var task = Char.ToLower(caption[0]) + caption.Substring(1).TrimEnd('.');
									XtraMessageBox.Show("An error occurred while " + task + ".\r\n\r\n" + ex, "Shomrei Torah Schedulizer", MessageBoxButtons.OK, MessageBoxIcon.Error);
									dialog.Close();
								}, null);
								return;
							} finally { waiter.Set(); }
							syncContext.Post(_ => dialog.Close(), null);
						});

						if (waiter.WaitOne(TimeSpan.FromSeconds(.5), false))
							return;	//If it finishes very quickly, don't show progress.  Otherwise, we get an annoying focus bounce whenever we switch cells

						dialog.ShowDialog();
					}
				} finally {
					isRunning = false;
					if (pendingActions.Count > 0)
						BeginInvoke(pendingActions.Dequeue());
				}
			};

			if (isRunning)
				pendingActions.Enqueue(wrapper);
			else {
				isRunning = true;	//In case we get called again in this message loop before the wrapper starts
				BeginInvoke(wrapper);
			}
		}

		private static void BeginInvoke(Action method) {
			Debug.Assert(SynchronizationContext.Current is WindowsFormsSynchronizationContext);
			SynchronizationContext.Current.Post(_ => method(), null);
		}

		class ProgressForm : ProgressDialog, IProgressReporter {
			public new string Caption {
				get { return base.Caption; }
				set { MyInvoke(() => base.Caption = value); }
			}

			//TODO: Scale longs to int ranges; maintain private actual values
			public new long Maximum {
				get { return base.Maximum; }
				set { MyInvoke(() => base.Maximum = (int)value); }
			}
			public long Progress {
				get { return base.Value; }
				set { MyInvoke(() => base.Value = (int)value); }
			}
			void MyInvoke(Action method) {
				if (IsHandleCreated && InvokeRequired)
					BeginInvoke(method);
				else
					method();
			}

			protected override void OnCancelClicked(EventArgs e) {
				base.OnCancelClicked(e);
				WasCanceled = true;
				CancelState = ButtonMode.Disabled;
			}
			public bool WasCanceled { get; private set; }


			public bool CanCancel {
				get { return CancelState != ButtonMode.Hidden; }
				set {
					MyInvoke(() => {
						if (value)
							CancelState = WasCanceled ? ButtonMode.Disabled : ButtonMode.Normal;
						else
							CancelState = ButtonMode.Hidden;
					});
				}
			}
		}
	}
}