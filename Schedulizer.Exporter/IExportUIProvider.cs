using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ShomreiTorah.Common;

namespace ShomreiTorah.Schedules.Export {
	public interface IExportUIProvider {
		void PerformOperation(Action<IProgressReporter> method, bool cancellable);
	}
}
