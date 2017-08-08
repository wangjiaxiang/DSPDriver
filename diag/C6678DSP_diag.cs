using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

using Jungo.wdapi_dotnet;
using Jungo.c6678dsp_lib;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using BYTE = System.Byte;
using BOOL = System.Boolean;
using UINT32 = System.UInt32;
using UINT64 = System.UInt64;
using WDC_DEVICE_HANDLE = System.IntPtr;
using WDC_ADDR_SIZE = System.UInt32;
using HANDLE = System.IntPtr;

namespace Jungo.c6678dsp_diag
{
    public enum RW
    {
        READ = 0,
        WRITE = 1,
        READ_ALL = 2
    }

    public enum TRANSFER_TYPE
    {
        BLOCK = 0,
        NONBLOCK = 1
    }

    public enum ACTION_TYPE
    {
        CFG = 0,
        RT = 1,        
    }

    public class C6678DSP_diag : System.Windows.Forms.Form
    {
        private IContainer components;

        private C6678DSP_DeviceList pciDevList;
        private Log log;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuAddrSpaces;
        private System.Windows.Forms.MenuItem menuEvents;

        private System.Windows.Forms.MenuItem menuRegisterEvent;

        private System.Windows.Forms.MenuItem menuCfgSpace;
        private System.Windows.Forms.MenuItem menuRTRegs;
        private System.Windows.Forms.Label lblLog;
        private System.Windows.Forms.Label lblC6678DSPDev;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btLog;
        private System.Windows.Forms.Button btExit;
        private System.Windows.Forms.ListBox lstBxDevices;
        private System.Windows.Forms.MenuItem menuCfgOffset;
        private System.Windows.Forms.MenuItem menuCfgReg;
        private System.Windows.Forms.MenuItem menuAddrRW;
        private System.Windows.Forms.MenuItem menuRTRegsRW;
        private System.Windows.Forms.MenuItem menuInterrupts;

        private System.Windows.Forms.MenuItem menuEnableInt;
        private MenuItem menuItem1;
        private MenuItem menuItem2;
        private MenuItem menuItem3;

        private System.Windows.Forms.Button btDevice;
        
        public C6678DSP_diag()
        {
            InitializeComponent();

            log = new Log(new Log.TRACE_LOG(TraceLog),
                new Log.ERR_LOG(ErrLog));
            pciDevList = C6678DSP_DeviceList.TheDeviceList();
        }

        protected override void Dispose( bool disposing )
        {
            if( disposing )
            {
                if (components != null) 
                {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

#region Windows Form Designer generated code
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lstBxDevices = new System.Windows.Forms.ListBox();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuAddrSpaces = new System.Windows.Forms.MenuItem();
            this.menuAddrRW = new System.Windows.Forms.MenuItem();
            this.menuInterrupts = new System.Windows.Forms.MenuItem();
            this.menuEnableInt = new System.Windows.Forms.MenuItem();
            this.menuEvents = new System.Windows.Forms.MenuItem();
            this.menuRegisterEvent = new System.Windows.Forms.MenuItem();
            this.menuCfgSpace = new System.Windows.Forms.MenuItem();
            this.menuCfgOffset = new System.Windows.Forms.MenuItem();
            this.menuCfgReg = new System.Windows.Forms.MenuItem();
            this.menuRTRegs = new System.Windows.Forms.MenuItem();
            this.menuRTRegsRW = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.lblLog = new System.Windows.Forms.Label();
            this.lblC6678DSPDev = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btLog = new System.Windows.Forms.Button();
            this.btExit = new System.Windows.Forms.Button();
            this.btDevice = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(29, 190);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(749, 224);
            this.txtLog.TabIndex = 24;
            // 
            // lstBxDevices
            // 
            this.lstBxDevices.ItemHeight = 12;
            this.lstBxDevices.Location = new System.Drawing.Point(29, 86);
            this.lstBxDevices.Name = "lstBxDevices";
            this.lstBxDevices.Size = new System.Drawing.Size(499, 52);
            this.lstBxDevices.TabIndex = 27;
            this.lstBxDevices.SelectedIndexChanged += new System.EventHandler(this.lstBxDevices_SelectedIndexChanged);
            this.lstBxDevices.DoubleClick += new System.EventHandler(this.lstBxDevices_DoubleClicked);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAddrSpaces,
            this.menuInterrupts,
            this.menuEvents,
            this.menuCfgSpace,
            this.menuRTRegs,
            this.menuItem1});
            // 
            // menuAddrSpaces
            // 
            this.menuAddrSpaces.Index = 0;
            this.menuAddrSpaces.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuAddrRW});
            this.menuAddrSpaces.Text = "Address Spaces";
            // 
            // menuAddrRW
            // 
            this.menuAddrRW.Index = 0;
            this.menuAddrRW.Text = "Read/Write Address Space";
            this.menuAddrRW.Click += new System.EventHandler(this.menuAddrRW_Click);
            // 
            // menuInterrupts
            // 
            this.menuInterrupts.Index = 1;
            this.menuInterrupts.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuEnableInt});
            this.menuInterrupts.Text = "Interrupts";
            this.menuInterrupts.Select += new System.EventHandler(this.menuInterrupts_Select);
            // 
            // menuEnableInt
            // 
            this.menuEnableInt.Index = 0;
            this.menuEnableInt.Text = "Enable Interrupts";
            this.menuEnableInt.Click += new System.EventHandler(this.menuEnableInt_Click);
            // 
            // menuEvents
            // 
            this.menuEvents.Index = 2;
            this.menuEvents.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuRegisterEvent});
            this.menuEvents.Text = "Events";
            this.menuEvents.Select += new System.EventHandler(this.menuEvents_Select);
            // 
            // menuRegisterEvent
            // 
            this.menuRegisterEvent.Index = 0;
            this.menuRegisterEvent.Text = "Regsiter Events";
            this.menuRegisterEvent.Click += new System.EventHandler(this.menuRegisterEvent_Click);
            // 
            // menuCfgSpace
            // 
            this.menuCfgSpace.Index = 3;
            this.menuCfgSpace.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuCfgOffset,
            this.menuCfgReg});
            this.menuCfgSpace.Text = "Configuration Space";
            // 
            // menuCfgOffset
            // 
            this.menuCfgOffset.Index = 0;
            this.menuCfgOffset.Text = "By Offset ";
            this.menuCfgOffset.Click += new System.EventHandler(this.menuCfgOffset_Click);
            // 
            // menuCfgReg
            // 
            this.menuCfgReg.Index = 1;
            this.menuCfgReg.Text = "By Register";
            this.menuCfgReg.Click += new System.EventHandler(this.menuCfgReg_Click);
            // 
            // menuRTRegs
            // 
            this.menuRTRegs.Index = 4;
            this.menuRTRegs.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuRTRegsRW});
            this.menuRTRegs.Text = "RunTime Registers";
            // 
            // menuRTRegsRW
            // 
            this.menuRTRegsRW.Index = 0;
            this.menuRTRegsRW.Text = "Read/Write RT Registers";
            this.menuRTRegsRW.Click += new System.EventHandler(this.menuRTRegsRW_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 5;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.menuItem3});
            this.menuItem1.Text = "initDSP";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "initinBound";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.Text = "ReadDMA";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // lblLog
            // 
            this.lblLog.Location = new System.Drawing.Point(29, 164);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new System.Drawing.Size(120, 24);
            this.lblLog.TabIndex = 28;
            this.lblLog.Text = "Log";
            // 
            // lblC6678DSPDev
            // 
            this.lblC6678DSPDev.Location = new System.Drawing.Point(29, 60);
            this.lblC6678DSPDev.Name = "lblC6678DSPDev";
            this.lblC6678DSPDev.Size = new System.Drawing.Size(144, 25);
            this.lblC6678DSPDev.TabIndex = 29;
            this.lblC6678DSPDev.Text = "C6678DSP Devices Found:";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(48, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(278, 25);
            this.label1.TabIndex = 30;
            this.label1.Text = "Select a device to activate its menu";
            // 
            // btLog
            // 
            this.btLog.Location = new System.Drawing.Point(806, 224);
            this.btLog.Name = "btLog";
            this.btLog.Size = new System.Drawing.Size(96, 43);
            this.btLog.TabIndex = 31;
            this.btLog.Text = "Clear Log";
            this.btLog.Click += new System.EventHandler(this.btLog_Click);
            // 
            // btExit
            // 
            this.btExit.Location = new System.Drawing.Point(806, 327);
            this.btExit.Name = "btExit";
            this.btExit.Size = new System.Drawing.Size(96, 43);
            this.btExit.TabIndex = 32;
            this.btExit.Text = "Exit";
            this.btExit.Click += new System.EventHandler(this.btExit_Click);
            // 
            // btDevice
            // 
            this.btDevice.Location = new System.Drawing.Point(557, 103);
            this.btDevice.Name = "btDevice";
            this.btDevice.Size = new System.Drawing.Size(134, 25);
            this.btDevice.TabIndex = 33;
            this.btDevice.Text = "Open Device";
            this.btDevice.Click += new System.EventHandler(this.btDevice_Click);
            // 
            // C6678DSP_diag
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(768, 409);
            this.Controls.Add(this.btDevice);
            this.Controls.Add(this.btExit);
            this.Controls.Add(this.btLog);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblC6678DSPDev);
            this.Controls.Add(this.lblLog);
            this.Controls.Add(this.lstBxDevices);
            this.Controls.Add(this.txtLog);
            this.Menu = this.mainMenu1;
            this.Name = "C6678DSP_diag";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "C6678DSP .NET Sample";
            this.Closed += new System.EventHandler(this.C6678DSP_diag_Closing);
            this.Load += new System.EventHandler(this.C6678DSP_diag_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
#endregion

        /// The main entry point for the application.
        [STAThread]
            static void Main() 
            {
                Application.Run(new C6678DSP_diag());
            }


        /* Open a handle to a device */
        private bool DeviceOpen(int iSelectedIndex)
        {
            DWORD dwStatus;
            C6678DSP_Device device = pciDevList.Get(iSelectedIndex);

            /* Open a handle to the device */
            dwStatus = device.Open();
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            {
                Log.ErrLog("C6678DSP_diag.DeviceOpen: Failed opening a " +
                    "handle to the device (" + device.ToString(false) + ")" );
                return false;
            }
            Log.TraceLog("C6678DSP_diag.DeviceOpen: The device was successfully open." +
                "You can now activate the device through the enabled menu above");    
            return true;
        }

        /* Close handle to a C6678DSP device */
        private BOOL DeviceClose(int iSelectedIndex)
        {
            C6678DSP_Device device = pciDevList.Get(iSelectedIndex);
            BOOL bStatus = false;
                        
            if (device.Handle != IntPtr.Zero && !(bStatus = device.Close()))
            {
                Log.ErrLog("C6678DSP_diag.DeviceClose: Failed closing C6678DSP "
                    + "device (" + device.ToString(false) + ")");
            }
            else
                device.Handle = IntPtr.Zero;
            return bStatus;
        }

        private void C6678DSP_diag_Load(object sender, System.EventArgs e)
        {
            DWORD dwStatus = pciDevList.Init();           
            if(dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                goto Error;
            
            foreach(C6678DSP_Device dev in pciDevList)
                lstBxDevices.Items.Add(dev.ToString(true));
            lstBxDevices.SelectedIndex = 0;

            return;            
Error:
            DisableMenu();
            btDevice.Enabled = false;            
        }


        private void C6678DSP_IntHandler(C6678DSP_Device dev)
        {
            Log.TraceLog("interrupt for device {" + dev.ToString(false) +
                "} received!");
        }



        private void C6678DSP_EventHandler(ref WD_EVENT wdEvent, C6678DSP_Device dev)
        {
            string sAction;
            switch((WD_EVENT_ACTION)wdEvent.dwAction)
            {
                case WD_EVENT_ACTION.WD_INSERT:
                    sAction = "WD_INSERT";
                    break;
                case WD_EVENT_ACTION.WD_REMOVE:
                    sAction = "WD_REMOVE";
                    break;
                case WD_EVENT_ACTION.WD_POWER_CHANGED_D0:
                    sAction = "WD_POWER_CHANGED_D0";
                    break;
                case WD_EVENT_ACTION.WD_POWER_CHANGED_D1:
                    sAction = "WD_POWER_CHANGED_D1";
                    break;
                case WD_EVENT_ACTION.WD_POWER_CHANGED_D2:
                    sAction = "WD_POWER_CHANGED_D2";
                    break;
                case WD_EVENT_ACTION.WD_POWER_CHANGED_D3:
                    sAction = "WD_POWER_CHANGED_D3";
                    break;
                case WD_EVENT_ACTION.WD_POWER_SYSTEM_WORKING:
                    sAction = "WD_POWER_SYSTEM_WORKING";
                    break;
                case WD_EVENT_ACTION.WD_POWER_SYSTEM_SLEEPING1:
                    sAction = "WD_POWER_SYSTEM_SLEEPING1";
                    break;
                case WD_EVENT_ACTION.WD_POWER_SYSTEM_SLEEPING2:
                    sAction = "WD_POWER_SYSTEM_SLEEPING2";
                    break;
                case WD_EVENT_ACTION.WD_POWER_SYSTEM_SLEEPING3:
                    sAction = "WD_POWER_SYSTEM_SLEEPING3";
                    break;
                case WD_EVENT_ACTION.WD_POWER_SYSTEM_HIBERNATE:
                    sAction = "WD_POWER_SYSTEM_HIBERNATE";
                    break;
                case WD_EVENT_ACTION.WD_POWER_SYSTEM_SHUTDOWN:
                    sAction = "WD_POWER_SYSTEM_SHUTDOWN";
                    break;
                default:
                    sAction = wdEvent.dwAction.ToString("X");
                    break;
            }
            Log.TraceLog("Received event notification of type " + sAction +
                " on " + dev.ToString(false));    
        }


        private void C6678DSP_diag_Closing(object sender, System.EventArgs e)
        {
            pciDevList.Dispose();            
        }        

        /* list box lstBxDevices */
        private void lstBxDevices_SelectedIndexChanged(object sender, 
            System.EventArgs e)
        {
            if(lstBxDevices.SelectedIndex < 0)
            {
                DisableMenu();
                btDevice.Enabled = false;
            }
            else
            {
                C6678DSP_Device dev = 
                    pciDevList.Get(lstBxDevices.SelectedIndex);
                UpdateMenu(lstBxDevices.SelectedIndex);
                btDevice.Enabled = true;
                if(dev.Handle == IntPtr.Zero)
                    btDevice.Text = "Open Device";
                else 
                    btDevice.Text = "Close Device";

                menuRTRegs.Visible = (dev.Regs.gC6678DSP_RT_Regs.Length > 0)? 
                    true : false;
            }
        }

        private void lstBxDevices_DoubleClicked(object sender, 
            System.EventArgs e)
        {
            btDevice_Click(sender, e);
        }

        /* device button */
        private void btDevice_Click(object sender, System.EventArgs e)
        {
            if(btDevice.Text == "Open Device")
            {
                if(DeviceOpen(lstBxDevices.SelectedIndex) == true)
                {
                    btDevice.Text = "Close Device";
                    EnableMenu();                
                }
            }
            else
            {
                C6678DSP_Device dev =
                    pciDevList.Get(lstBxDevices.SelectedIndex);
                DeviceClose(lstBxDevices.SelectedIndex);
                btDevice.Text = "Open Device";
                DisableMenu(); 
            }        
        }

        /* Menu*/
        private void UpdateMenu(int index)
        {
            C6678DSP_Device dev =
                pciDevList.Get(lstBxDevices.SelectedIndex);
            if(dev.Handle == IntPtr.Zero)
                DisableMenu();
            else 
                EnableMenu();            
        }

        private void EnableMenu()
        {
            ToggleMenu(true);                   
        }

        private void DisableMenu()
        {
            ToggleMenu(false);
        }

        private void ToggleMenu(bool flag)
        {
            for(int index=0; index < mainMenu1.MenuItems.Count; ++index)
                mainMenu1.MenuItems[index].Enabled = flag;
        }


        /* Address Space Item */
        private void menuAddrRW_Click(object sender, System.EventArgs e)
        {
            C6678DSP_Device dev =
                pciDevList.Get(lstBxDevices.SelectedIndex);
            string[] sBars = dev.AddrDescToString(false);
            AddrSpaceTransferForm addrSpcFrm = new 
                AddrSpaceTransferForm(dev, sBars);
            addrSpcFrm.GetInput();
        }
        

        /* Interrupts items*/

        private void menuInterrupts_Select(object sender, 
            System.EventArgs e)
        {
            if(menuInterrupts.Enabled == false)
                return;
            C6678DSP_Device dev = pciDevList.Get(lstBxDevices.SelectedIndex);
            bool bIsEnb = dev.IsEnabledInt();

            menuEnableInt.Text = bIsEnb? "Disable Interrupts":
                "Enable Interrupts";     
        }

        private void menuEnableInt_Click(object sender,
            System.EventArgs e)
        {
            C6678DSP_Device dev = pciDevList.Get(lstBxDevices.SelectedIndex);
            if(menuEnableInt.Text == "Enable Interrupts")
            {
                DWORD dwStatus = dev.EnableInterrupts(new
                    USER_INTERRUPT_CALLBACK(C6678DSP_IntHandler), dev.Handle);
                if(dwStatus == (DWORD)wdc_err.WD_STATUS_SUCCESS)
                    menuEnableInt.Text = "Disable Interrupts";
            }
            else
            {
                DWORD dwStatus = dev.DisableInterrupts();
                if(dwStatus == (DWORD)wdc_err.WD_STATUS_SUCCESS)
                    menuEnableInt.Text = "Enable Interrupts";
            }                        
        } 



        /* Event Items*/
        private void menuEvents_Select(object sender, System.EventArgs e)
        {
            if(menuEvents.Enabled == false)
                return;
            C6678DSP_Device dev = pciDevList.Get(lstBxDevices.SelectedIndex);
            menuRegisterEvent.Text = dev.IsEventRegistered() ?
                "Unregister Events" : "Register Events";
        }

        private void menuRegisterEvent_Click(object sender, System.EventArgs e)
        {
            if(menuRegisterEvent.Text == "Register Events")
            {
                pciDevList.Get(lstBxDevices.SelectedIndex).
                    EventRegister(new USER_EVENT_CALLBACK(C6678DSP_EventHandler));
                menuRegisterEvent.Text = "Unregister Events";
            }
            else
            {
                pciDevList.Get(lstBxDevices.SelectedIndex).
                    EventUnregister();
                menuRegisterEvent.Text = "Register Events";
            }                
        }


        /* Configuration Space Items*/
        private void menuCfgOffset_Click(object sender, System.EventArgs e)
        {
            C6678DSP_Device dev =
                pciDevList.Get(lstBxDevices.SelectedIndex);
            CfgTransfersForm cfgOffsetFrom = new CfgTransfersForm(dev);
            cfgOffsetFrom.GetInput();        
        }

        private void menuCfgReg_Click(object sender, System.EventArgs e)
        {
            C6678DSP_Device dev =
                pciDevList.Get(lstBxDevices.SelectedIndex);
            RegistersForm regForm = new RegistersForm(dev, ACTION_TYPE.CFG);
            regForm.GetInput();
        }

        /*RunTime Registers Items*/
        private void menuRTRegsRW_Click(object sender, System.EventArgs e)
        {
            C6678DSP_Device dev =
                pciDevList.Get(lstBxDevices.SelectedIndex);
            RegistersForm regForm = new RegistersForm(dev, ACTION_TYPE.RT);
            regForm.GetInput();        
        }                

        private void btExit_Click(object sender, System.EventArgs e)
        {
            Close();
            Dispose();
        }

        private void btLog_Click(object sender, System.EventArgs e)
        {
            txtLog.Clear();
        }                                                

        public void LogFunc(string str)
        {
            if(txtLog != null)
                txtLog.Text += str + Environment.NewLine;
        }

        public void TraceLog(string str)
        {
            if(this.InvokeRequired)
                Invoke(new Log.TRACE_LOG(LogFunc), new object[]{str});
            else
                LogFunc(str);
        }

        public void ErrLog(string str)
        {
            if(this.InvokeRequired)
                Invoke(new Log.ERR_LOG(LogFunc), new object[]{str});
            else
                LogFunc(str);
        }
        //initInbound
        private void menuItem3_Click(object sender, EventArgs e)
        {
            C6678DSP_Device dev = pciDevList.Get(lstBxDevices.SelectedIndex);
            dev.InitInBound();
        }

        private void menuItem2_Click(object sender, EventArgs e)
        {
            C6678DSP_Device dev = pciDevList.Get(lstBxDevices.SelectedIndex);
            IntPtr data = IntPtr.Zero;
            dev.ReadDMA(0x0c000000, 0x1000, ref data);
            FileStream fs;

            fs = new FileStream("F:\\test", FileMode.OpenOrCreate, FileAccess.Write);
            byte[] array = new byte[1024];
            Marshal.Copy(data,array,0,1024);
            fs.Write(array,0,1024);
            fs.Close();
        }      
    }  

    public class diag_lib
    {
        public static string PadBuffer(string str, uint fromIndex, uint toIndex)
        {
            for(uint i=fromIndex; i < toIndex; ++i)
                str += "0";

            return str;
        }

        public static string DisplayHexBuffer(object[] obj, DWORD dwBuffSize, 
             WDC_ADDR_MODE mode)
        {
            string display = "";

            switch(mode)
            {
            case WDC_ADDR_MODE.WDC_MODE_8:
                {
                    BYTE[] buff = (BYTE[])obj[0];
                    for(uint i=0; i<dwBuffSize; ++i)
                        display = string.Concat(display, 
                            buff[i].ToString("X2"), " ");
                    break;
                }
            case WDC_ADDR_MODE.WDC_MODE_16:
                {
                    WORD[] buff = (WORD[])obj[0];
                    for(int i=0; i<dwBuffSize/2; ++i)
                        display = string.Concat(display, 
                            buff[i].ToString("X4"), " ");
                    break;
                }
            case WDC_ADDR_MODE.WDC_MODE_32:
                {
                    UINT32[] buff = (UINT32[])obj[0];
                    for(int i=0; i<dwBuffSize/4; ++i)
                        display = string.Concat(display,
                            buff[i].ToString("X8"), " ");
                    break;
                }
            case WDC_ADDR_MODE.WDC_MODE_64:
                {
                    UINT64[] buff = (UINT64[])obj[0];
                    for(int i=0; i<dwBuffSize/8; ++i)
                        display = string.Concat(display, 
                            buff[i].ToString("X16"), " ");
                    break;
                }
            }                                
            return display;
        }

        public static string DisplayHexBuffer(byte[] buff, uint dwBuffSize)
        {
            return DisplayHexBuffer(new object[]{buff}, dwBuffSize,
                WDC_ADDR_MODE.WDC_MODE_8);
        }
    };    
}

