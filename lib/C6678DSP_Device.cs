using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Data;

using Jungo.wdapi_dotnet;
using wdc_err = Jungo.wdapi_dotnet.WD_ERROR_CODES;
using item_types = Jungo.wdapi_dotnet.ITEM_TYPE;
using UINT64 = System.UInt64;
using UINT32 = System.UInt32;
using DWORD = System.UInt32;
using WORD = System.UInt16;
using BYTE = System.Byte;
using BOOL = System.Boolean;
using WDC_DEVICE_HANDLE = System.IntPtr;   
using HANDLE = System.IntPtr;
namespace Jungo.c6678dsp_lib
{
    /* PCI diagnostics plug-and-play and power management events handler *
     * function type */
    public delegate void USER_EVENT_CALLBACK(ref WD_EVENT pEvent, C6678DSP_Device dev);
    /* PCI diagnostics interrupt handler function type */
    public delegate void USER_INTERRUPT_CALLBACK(C6678DSP_Device device);


    public class C6678DSP_Device
    {
        private WDC_DEVICE m_wdcDevice = new WDC_DEVICE();
        protected MarshalWdcDevice m_wdcDeviceMarshaler;
        private USER_EVENT_CALLBACK m_userEventHandler;
        private USER_INTERRUPT_CALLBACK m_userIntHandler;
        private EVENT_HANDLER_DOTNET m_eventHandler;
        private INT_HANDLER m_intHandler;
        protected string m_sDeviceLongDesc;
        protected string m_sDeviceShortDesc;
        private C6678DSP_Regs m_regs;
        private WD_DMA mDma;
        private WD_PCI_CARD_INFO deviceInfo;

#region " constructors " 
        /* constructors & destructors */
        internal protected C6678DSP_Device(WD_PCI_SLOT slot): this(0, 0, slot){}

        internal protected C6678DSP_Device(DWORD dwVendorId, DWORD dwDeviceId,
            WD_PCI_SLOT slot)
        {
            m_wdcDevice = new WDC_DEVICE();
            m_wdcDevice.id.pciId.dwVendorId = dwVendorId;
            m_wdcDevice.id.pciId.dwDeviceId = dwDeviceId;
            m_wdcDevice.slot.pciSlot = slot;
            m_wdcDeviceMarshaler = new MarshalWdcDevice();
            m_eventHandler = new EVENT_HANDLER_DOTNET(C6678DSP_EventHandler);
            m_regs = new C6678DSP_Regs();
            SetDescription();
        } 

        public void Dispose()
        {
            Close();
        }
#endregion



#region " properties " 
        /*********************
         *  properties       *
         *********************/

        public IntPtr Handle
        {
            get
            {
                return m_wdcDevice.hDev;
            }
            set
            {
                m_wdcDevice.hDev = value;
            }
        }

        protected WDC_DEVICE wdcDevice
        {
            get
            {
                return m_wdcDevice;
            }
            set
            {
                m_wdcDevice = value;
            }
        }

        public WD_PCI_ID id
        {
            get
            {
                return m_wdcDevice.id.pciId;
            }
            set
            {
                m_wdcDevice.id.pciId = value;
            }
        }

        public WD_PCI_SLOT slot
        {
            get
            {
                return m_wdcDevice.slot.pciSlot;
            }
            set
            {
                m_wdcDevice.slot.pciSlot = value;
            }
        }

        public WDC_ADDR_DESC[] AddrDesc
        {
            get
            {
                return m_wdcDevice.pAddrDesc;
            }
            set
            {
                m_wdcDevice.pAddrDesc = value;
            }
        }

        public C6678DSP_Regs Regs
        {
            get
            {
                return m_regs;
            }
        }

#endregion

#region " utilities " 
        /********************
         *     utilities    *
         *********************/

        /* public methods */

        public string[] AddrDescToString(bool bMemOnly)
        {
            string[] sAddr = new string[AddrDesc.Length];
            for (int i = 0; i<sAddr.Length; ++i)
            {
                sAddr[i] = "BAR " + AddrDesc[i].dwAddrSpace.ToString() + 
                     ((AddrDesc[i].fIsMemory)? " Memory " : " I/O ");

                if (wdc_lib_decl.WDC_AddrSpaceIsActive(Handle, 
                    AddrDesc[i].dwAddrSpace))
                {
                    WD_ITEMS item =
                        m_wdcDevice.cardReg.Card.Item[AddrDesc[i].dwItemIndex];
                    UINT64 dwAddr = (UINT64)(AddrDesc[i].fIsMemory?
                        item.I.Mem.dwPhysicalAddr : item.I.IO.dwAddr);

                    sAddr[i] += dwAddr.ToString("X") + " - " + 
                        (dwAddr + AddrDesc[i].dwBytes - 1).ToString("X") + 
                        " (" + AddrDesc[i].dwBytes.ToString("X") + " bytes)";
                }
                else
                    sAddr[i] += "Inactive address space";
            }
            return sAddr;
        }

        public string ToString(BOOL bLong)
        {
            return (bLong)? m_sDeviceLongDesc: m_sDeviceShortDesc;
        }

        public bool IsMySlot(ref WD_PCI_SLOT slot)
        {
            if(m_wdcDevice.slot.pciSlot.dwBus == slot.dwBus &&
                m_wdcDevice.slot.pciSlot.dwSlot == slot.dwSlot &&
                m_wdcDevice.slot.pciSlot.dwFunction == slot.dwFunction)
                return true;

            return false;
        }

        /* protected methods */

        protected void SetDescription()
        {
            m_sDeviceLongDesc = string.Format("C6678DSP Device: Vendor ID 0x{0:X}, " 
                + "Device ID 0x{1:X}, Physical Location {2:X}:{3:X}:{4:X}", 
                id.dwVendorId, id.dwDeviceId, slot.dwBus, slot.dwSlot, 
                slot.dwFunction);

            m_sDeviceShortDesc = string.Format("Device " + 
                "{0:X},{1:X} {2:X}:{3:X}:{4:X}", id.dwVendorId, 
                id.dwDeviceId, slot.dwBus, slot.dwSlot, slot.dwFunction); 
        }

        /* private methods */

        private bool DeviceValidate()
        {
            DWORD i, dwNumAddrSpaces = m_wdcDevice.dwNumAddrSpaces;

            /* NOTE: You can modify the implementation of this function in     *
             * order to verify that the device has the resources you expect to *
             * find */

            /* Verify that the device has at least one active address space */
            for (i = 0; i < dwNumAddrSpaces; i++)
            {
                if (wdc_lib_decl.WDC_AddrSpaceIsActive(Handle, i))
                    return true;
            }

            Log.TraceLog("C6678DSP_Device.DeviceValidate: Device does not have "
                + "any active memory or I/O address spaces " + "(" +
                this.ToString(false) + ")" );
            return true;
        } 

#endregion

#region " Device Open/Close " 
        /****************************
         *  Device Open & Close      *
         *****************************/

        /* public methods */

        public virtual DWORD Open()
        {
            DWORD dwStatus;
            deviceInfo = new WD_PCI_CARD_INFO();

            /* Retrieve the device's resources information */
            deviceInfo.pciSlot = slot;
            dwStatus = wdc_lib_decl.WDC_PciGetDeviceInfo(deviceInfo);
            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("C6678DSP_Device.Open: Failed retrieving the " 
                    + "device's resources information. Error 0x" + 
                    dwStatus.ToString("X") + ": " + utils.Stat2Str(dwStatus) +
                    "(" + this.ToString(false) +")" );
                return dwStatus;
             }

            /* NOTE: You can modify the device's resources information here, 
             * if necessary (mainly the deviceInfo.Card.Items array or the
             * items number - deviceInfo.Card.dwItems) in order to register
             * only some of the resources or register only a portion of a
             * specific address space, for example. */

            dwStatus = wdc_lib_decl.WDC_PciDeviceOpen(ref m_wdcDevice,
                deviceInfo, IntPtr.Zero, IntPtr.Zero, "", IntPtr.Zero);

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("C6678DSP_Device.Open: Failed opening a " +
                    "WDC device handle. Error 0x" + dwStatus.ToString("X") +
                    ": " + utils.Stat2Str(dwStatus) + "(" + 
                    this.ToString(false) + ")");
                goto Error;
            }

            Log.TraceLog("C6678DSP_Device.Open: Opened a PCI device " + 
                this.ToString(false));

            /* Validate device information */
            if (DeviceValidate() != true)
            {
                dwStatus = (DWORD)wdc_err.WD_NO_RESOURCES_ON_DEVICE;
                goto Error;
            }

            return dwStatus;
Error:    
            if (Handle != IntPtr.Zero)
                Close();

            return dwStatus;
        }

        public virtual bool Close()
        {
            DWORD dwStatus;

            if (Handle == IntPtr.Zero)
            {
                Log.ErrLog("C6678DSP_Device.Close: Error - NULL " 
                    + "device handle");
                return false;
            }

            /* unregister events*/
            dwStatus = EventUnregister();

            /* Disable interrupts */
            dwStatus = DisableInterrupts();

            /* Close the device */
            dwStatus = wdc_lib_decl.WDC_PciDeviceClose(Handle);
            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("C6678DSP_Device.Close: Failed closing a "
                    + "WDC device handle (0x" + Handle.ToInt64().ToString("X") 
                    + ". Error 0x" + dwStatus.ToString("X") + ": " +
                    utils.Stat2Str(dwStatus) + this.ToString(false));
            }
            else
            {
                Log.TraceLog("C6678DSP_Device.Close: " +
                    this.ToString(false) + " was closed successfully");
            }

            return ((DWORD)wdc_err.WD_STATUS_SUCCESS == dwStatus);
        }

#endregion

#region " Interrupts "
            /* public methods */
        public bool IsEnabledInt()
        {
            return wdc_lib_decl.WDC_IntIsEnabled(this.Handle);
        }

        protected virtual DWORD CreateIntTransCmds(out WD_TRANSFER[] 
            pIntTransCmds, out DWORD dwNumCmds)
        {
            /* Define the number of interrupt transfer commands to use */
            DWORD NUM_TRANS_CMDS = 0;
            pIntTransCmds = new WD_TRANSFER[NUM_TRANS_CMDS];
            /*
            TODO: Your hardware has level sensitive interrupts, which must be
          acknowledged in the kernel immediately when they are received.
                  Since the information for acknowledging the interrupts is
            hardware-specific, YOU MUST ADD CODE to read/write the relevant
            register(s) in order to correctly acknowledge the interrupts
            on your device, as dictated by your hardware's specifications.
            When adding transfer commands, be sure to also modify the
            definition of NUM_TRANS_CMDS (above) accordingly.
             
            *************************************************************************   
            * NOTE: If you attempt to use this code without first modifying it in   *
            *       order to correctly acknowledge your device's interrupts, as     *
            *       explained above, the OS will HANG when an interrupt occurs!     *
            *************************************************************************
            */
            dwNumCmds = NUM_TRANS_CMDS;
            return (DWORD)wdc_err.WD_STATUS_SUCCESS;
        }

        protected virtual DWORD DisableCardInts()
        {
            /* TODO: You can add code here to write to the device in order
             * to physically disable the hardware interrupts */ 
            return (DWORD)wdc_err.WD_STATUS_SUCCESS;
        }

        protected BOOL IsItemExists(WDC_DEVICE Dev, DWORD item)
        {
            int i;
            DWORD dwNumItems = Dev.cardReg.Card.dwItems;

            for (i=0; i<dwNumItems; i++)
            {
                if (Dev.cardReg.Card.Item[i].item == item)
                    return true;
            }

            return false;
        }


        public DWORD EnableInterrupts(USER_INTERRUPT_CALLBACK userIntCb, IntPtr pData)
        {
            DWORD dwStatus;
            WD_TRANSFER[] pIntTransCmds = null;
            DWORD dwNumCmds;
            if(userIntCb == null)
            {
                Log.TraceLog("C6678DSP_Device.EnableInterrupts: "
                    + "user callback is invalid");
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            if(!IsItemExists(m_wdcDevice, (DWORD)item_types.ITEM_INTERRUPT))
            {
                Log.TraceLog("C6678DSP_Device.EnableInterrupts: "
                    + "Device doesn't have any interrupts");
                return (DWORD)wdc_err.WD_OPERATION_FAILED;
            }

            m_userIntHandler = userIntCb;

            m_intHandler = new INT_HANDLER(C6678DSP_IntHandler);
            if(m_intHandler == null)
            {
                Log.ErrLog("C6678DSP_Device.EnableInterrupts: interrupt handler is " +
                    "null (" + this.ToString(false) + ")" ); 
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            if(wdc_lib_decl.WDC_IntIsEnabled(Handle))
            {
                Log.ErrLog("C6678DSP_Device.EnableInterrupts: "
                    + "interrupts are already enabled (" +
                    this.ToString(false) + ")" );
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            dwStatus = CreateIntTransCmds(out pIntTransCmds, out dwNumCmds);
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
                return dwStatus;
            dwStatus = wdc_lib_decl.WDC_IntEnable(wdcDevice, pIntTransCmds,
                dwNumCmds, 0, m_intHandler, pData, wdc_defs_macros.WDC_IS_KP(wdcDevice));

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("C6678DSP_Device.EnableInterrupts: Failed "
                    + "enabling interrupts. Error " + dwStatus.ToString("X") + ": " 
                    + utils.Stat2Str(dwStatus) + "(" + this.ToString(false) + ")");
                m_intHandler = null;
                return dwStatus;
            }
            /* TODO: You can add code here to write to the device in order
                 to physically enable the hardware interrupts */

            Log.TraceLog("C6678DSP_Device: enabled interrupts (" + this.ToString(false) + ")");
            return dwStatus;
        }

        public DWORD DisableInterrupts()
        {
            DWORD dwStatus;

            if (!wdc_lib_decl.WDC_IntIsEnabled(this.Handle))
            {
                Log.ErrLog("C6678DSP_Device.DisableInterrupts: interrupts are already disabled... " +
                    "(" + this.ToString(false) + ")" );
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            /* Physically disabling the hardware interrupts */
            dwStatus = DisableCardInts();

            dwStatus = wdc_lib_decl.WDC_IntDisable(m_wdcDevice);
            if (dwStatus != (DWORD)wdc_err.WD_STATUS_SUCCESS)
            {
                Log.ErrLog("C6678DSP_Device.DisableInterrupts: Failed to" +
                    "disable interrupts. Error " + dwStatus.ToString("X") 
                    + ": " + utils.Stat2Str(dwStatus) + " (" +
                    this.ToString(false) + ")" );
            }
            else
            {
                Log.TraceLog("C6678DSP_Device.DisableInterrupts: Interrupts are disabled" +
                    "(" + this.ToString(false) + ")");
            }

            return dwStatus;
        }

            /* private methods */
        private void C6678DSP_IntHandler(IntPtr pDev)
        {
            wdcDevice.Int =
                (WD_INTERRUPT)m_wdcDeviceMarshaler.MarshalDevWdInterrupt(pDev);

            /* to obtain the data that was read at interrupt use:
             * WD_TRANSFER[] transCommands;
             * transCommands = (WD_TRANSFER[])m_wdcDeviceMarshaler.MarshalDevpWdTrans(
             *     wdcDevice.Int.Cmd, wdcDevice.Int.dwCmds); */

            if(m_userIntHandler != null)
                m_userIntHandler(this);
        }

#endregion

#region " Events"
        /****************************
         *          Events          *
         * **************************/

        /* public methods */

        public bool IsEventRegistered()
        {
            if (Handle == IntPtr.Zero)
                return false;

            return wdc_lib_decl.WDC_EventIsRegistered(Handle);
        }

        public DWORD EventRegister(USER_EVENT_CALLBACK userEventHandler)
        {
            DWORD dwStatus;
            DWORD dwActions = (DWORD)windrvr_consts.WD_ACTIONS_ALL;
            /* TODO: Modify the above to set up the plug-and-play/power 
             * management events for which you wish to receive notifications.
             * dwActions can be set to any combination of the WD_EVENT_ACTION
             * flags defined in windrvr.h */

            if(userEventHandler == null)
            {
                Log.ErrLog("C6678DSP_Device.EventRegister: user callback is "
                    + "null");
                return (DWORD)wdc_err.WD_INVALID_PARAMETER;
            }

            /* Check if event is already registered */
            if(wdc_lib_decl.WDC_EventIsRegistered(Handle))
            {
                Log.ErrLog("C6678DSP_Device.EventRegister: Events are already "
                    + "registered ...");
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            m_userEventHandler = userEventHandler;

            /* Register event */
            dwStatus = wdc_lib_decl.WDC_EventRegister(m_wdcDevice, dwActions,
                m_eventHandler, Handle, wdc_defs_macros.WDC_IS_KP(wdcDevice));

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("C6678DSP_Device.EventRegister: Failed to register "
                    + "events. Error 0x" + dwStatus.ToString("X") 
                    + utils.Stat2Str(dwStatus));
                m_userEventHandler = null;
            }
            else
            {
                Log.TraceLog("C6678DSP_Device.EventRegister: events are " +
                    " registered (" + this.ToString(false) +")" );
            }

            return dwStatus;
        }

        public DWORD EventUnregister()
        {
            DWORD dwStatus;

            if (!wdc_lib_decl.WDC_EventIsRegistered(Handle))
            {
                Log.ErrLog("C6678DSP_Device.EventUnregister: No events " +
                    "currently registered ...(" + this.ToString(false) + ")" );
                return (DWORD)wdc_err.WD_OPERATION_ALREADY_DONE;
            }

            dwStatus = wdc_lib_decl.WDC_EventUnregister(m_wdcDevice);

            if ((DWORD)wdc_err.WD_STATUS_SUCCESS != dwStatus)
            {
                Log.ErrLog("C6678DSP_Device.EventUnregister: Failed to " +
                    "unregister events. Error 0x" + dwStatus.ToString("X") +
                    ": " + utils.Stat2Str(dwStatus) + "(" +
                    this.ToString(false) + ")");
            }
            else
            {
                Log.TraceLog("C6678DSP_Device.EventUnregister: Unregistered " +
                    " events (" + this.ToString(false) + ")" );
            }

            return dwStatus;
        }

        /** private methods **/

        /* event callback method */
        private void C6678DSP_EventHandler(IntPtr pWdEvent, IntPtr pDev)
        {
            MarshalWdEvent wdEventMarshaler = new MarshalWdEvent();
            WD_EVENT wdEvent = (WD_EVENT)wdEventMarshaler.MarshalNativeToManaged(pWdEvent);
            m_wdcDevice.Event =
                (WD_EVENT)m_wdcDeviceMarshaler.MarshalDevWdEvent(pDev);
            if(m_userEventHandler != null)
                m_userEventHandler(ref wdEvent, this); 
        }
#endregion

#region "Registers Read-Write " 
        // Function: C6678DSP_ReadINTCSR()
        //   Read from INTCSR register.
        // Parameters:
        //   None.
        // Return Value:
        //   The value read from the register.
        UINT32 C6678DSP_ReadINTCSR ()
        {
            UINT32 data = 0;

            wdc_lib_decl.WDC_ReadAddr32(Handle,
                m_regs.gC6678DSP_RT_Regs[0].dwAddrSpace,
                m_regs.gC6678DSP_RT_Regs[0].dwOffset,
                ref data);
            return data;
        }

        // Function: C6678DSP_WriteINTCSR()
        //   Write to INTCSR register.
        // Parameters:
        //   data [in] the data to write to the register.
        // Return Value:
        //   None.
        void C6678DSP_WriteINTCSR (UINT32 data)
        {
            wdc_lib_decl.WDC_WriteAddr32(Handle,
                m_regs.gC6678DSP_RT_Regs[0].dwAddrSpace,
                m_regs.gC6678DSP_RT_Regs[0].dwOffset,
                data);
        }

#endregion

        //#define OB_OFFSET_INDEX(n)           (0x200 + (8 * (n)))
        //#define OB_OFFSET_HI(n)              (0x204 + (8 * (n)))
        //#define IB_BAR(n)                    (0x300 + (0x10 * (n)))
        //#define IB_START_LO(n)               (0x304 + (0x10 * (n)))
        //#define IB_START_HI(n)               (0x308 + (0x10 * (n)))
        //#define IB_OFFSET(n)                 (0x30C + (0x10 * (n)))

        //#define PCIE_BASE_ADDRESS            0x21800000
        //#define LL2_START                    0x00800000
        //#define MSMC_START                   0x0C000000  /* Shared L2 */
        //#define DDR_START                    0x80000000
        //#define PCIE_DATA                    0x60000000
 
        public void InitInBound()
        {

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x300, 0);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x304, deviceInfo.Card.Item[1].I.Mem.dwPhysicalAddr);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x308, 0);

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x310, 1);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x314, deviceInfo.Card.Item[2].I.Mem.dwPhysicalAddr);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x318, 0);

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x320, 2);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x324, deviceInfo.Card.Item[3].I.Mem.dwPhysicalAddr);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x328, 0);

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x330, 3);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x334, deviceInfo.Card.Item[4].I.Mem.dwPhysicalAddr);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x338, 0);

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x30C, 0x21800000);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x31C, 0x00800000 + (1 << 28));
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x32C, 0x0C000000);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x33C, 0x80000000);

        }

        public void ReadDMA(uint uLocalAddr, uint dwBytes, ref IntPtr data)
        {
            //开DMA Buffer

            IntPtr pDMA = IntPtr.Zero;
            wdc_lib_decl.WDC_DMAContigBufLock(this.Handle, ref data, (uint)WD_DMA_OPTIONS.DMA_FROM_DEVICE, dwBytes, ref pDMA);

            MarshalWdDma m_wdDmaMarshaler = new MarshalWdDma();
            WD_DMA dma = (WD_DMA)m_wdDmaMarshaler.MarshalNativeToManaged(pDMA);

            //配置buffer的物理地址到DSP的outbound

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x30, 0x0); // 1MB outbound translation size 
            uint pageBase = (uint)(dma.Page[0].pPhysicalAddr & 0x100000);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x200, pageBase | 0x1);
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x204, 0x0);

            //将EDMA配置空间(0x02700000)映射到DSP的IB_OFFSET(3)，以便于PC控制DSP的EDMA

            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x33C, 0x02700000);

            /* EDMA registers */
            //#define EDMA_TPCC0_BASE_ADDRESS      0x02700000
            //#define DMAQNUM0                     0x0240  
            //#define ESR                          0x1010 
            //#define EESR                         0x1030                 
            //#define IESR                         0x1060
            //#define IPR                          0x1068 
            //#define ICR                          0x1070 
            //#define PARAM_0_OPT                  0x4000
            //#define PARAM_0_SRC                  0x4004
            //#define PARAM_0_A_B_CNT              0x4008
            //#define PARAM_0_DST                  0x400C
            //#define PARAM_0_SRC_DST_BIDX         0x4010
            //#define PARAM_0_LINK_BCNTRLD         0x4014
            //#define PARAM_0_SRC_DST_CIDX         0x4018
            //#define PARAM_0_CCNT                 0x401C
            //#define PCIE_DATA                    0x60000000 
            //#define DMA_TRANSFER_SIZE            0x400000   /* 4MB */

            /* Payload size in bytes over PCIE link. PCIe module supports 
               outbound payload size of 128 bytes and inbound payload size of 256 bytes */
            //#define PCIE_TRANSFER_SIZE           0x80               

            ///* For 1MB outbound translation window size */
            //#define PCIE_ADLEN_1MB               0x00100000
            //#define PCIE_1MB_BITMASK             0xFFF00000

            //启动DMA传输
            while (true)
            {
                /* Use TC0 for DBS = 128 bytes */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x0240, 0x0);

                //* Set the interrupt enable for 1st Channel (IER). */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x1060, 0x1);

                //* Clear any pending interrupt (IPR). */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x1070, 0x1);

                //* Populate the Param entry. */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x4000, 0x00100004);    /* Enable SYNCDIM and TCINTEN, TCC = 0 */

                //* Calculate the DSP PCI address for the PC address */
                uint tmp = (uint)(0x60000000 + (dma.Page[0].pPhysicalAddr & ~0xFFF00000));
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x400C, tmp);//dst address目标地址

                //PARAM_0_A_B_CNT
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x4008, 0x10000 | dwBytes);

                //PARAM_0_SRC
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x4004, (DWORD)uLocalAddr);//src address 源地址

                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x4010, ((0x80 << 16) | 0x80));
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x4014, 0xFFFF);
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x4018, 0x0);

                //* C Count is set to 1 since mostly size will not be more than 1.75GB */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x401C, 0x1);

                //* Set the Event Enable Set Register. */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x1030, 0x1);

                //* Set the event set register. */
                wdc_lib_decl.WDC_WriteAddr32(this.Handle, 3, 0x1010, 0x1);

                //等待dma结束
                while (true)
                {
                    wdc_lib_decl.WDC_ReadAddr32(this.Handle, 3, 0x1068, ref tmp);
                    if ((tmp & 0x1) == 1)
                    {
                        break;
                    }
                }
                break;
            }
            //恢复inbound配置
            wdc_lib_decl.WDC_WriteAddr32(this.Handle, 0, 0x33C, 0x80000000);
        }
    }
}
