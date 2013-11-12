using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Microsoft.NodejsTools.NpmUI{
    public partial class BusyPopup : Form{
        public BusyPopup(){
            InitializeComponent();
            CreateHandle();
        }

        public string Message{
            get { return _busyControl.Message; }
            set { _busyControl.Message = value; }
        }

        private void Completed(){
            BeginInvoke(
                new Action(
                    () =>{
                        DialogResult = DialogResult.OK;
                        Close();
                    }));
        }

        public void ShowPopup(IWin32Window parent, Action action){
            Task t = new Task(action);
            t.ContinueWith(t2 => Completed());
            t.Start();

            ShowDialog(parent);
        }
    }
}
