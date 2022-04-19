using ShomreiTorah.Common.Calendar;
using ShomreiTorah.Schedules;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Schedulizer.Debugger
{
	public partial class CalendarForm : Form
	{
		public CalendarForm()
		{
			InitializeComponent();
		}

		private void monthCalendar_DateSelected(object sender, DateRangeEventArgs e)
		{
			ScheduleCalculator calc = new ScheduleCalculator(new HebrewDate(monthCalendar.SelectionStart));
			MessageBox.Show($"{calc.CalcTitle()}\n\n{string.Join("\n", calc.CalcTimes().ToList())}");
		}
	}
}
