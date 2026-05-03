using Archipelago.Core.Util;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DC2AP.Helpers
{
    internal class OptionsHelper
    {
        private const ulong HookPtr = 0x0035099C;
        private const ulong CaveAddr = 0x01F70E00;

        private const ulong LabelMemBase = 0x01F80000;
        private const int LabelRowStride = 0x80;

        private const ulong GlobalFormY = 0x01F80054; // int — OptionButtonForm.y
        private const ulong GlobalClipTop = 0x01F8004C; // int — visible clip top
        private const ulong GlobalClipBottom = 0x01F80050; // int — visible clip bottom
        private const ulong GlobalNumRows = 0x01F80058; // int — how many custom rows

        private const int OffLabelStr = 0x00; // ASCII label text (up to 36 chars)
        private const int OffLabelX = 0x40; // int — screen X for label
        private const int OffLabelRelY = 0x44; // int — Y relative to form origin

        private const int MnuCursorRow = 0x374; // int — which row cursor is on
        private const int MnuSubSelect = 0x37C; // int — which button within row
        private const int MnuMaxToggle = 0x114; // int[row] — max buttons per row
        private const int MnuConfigPtr = 0x254; // ptr[row] — config value ptr per row
        private const int MnuBtnPartPtr = 0x164; // ptr[row][btn] — part ptr, stride 0xC

        private const short OBFPartCount = 0x68; // short — total part count
        private const int OBFPartsPtr = 0x6C; // ptr — part array base
        private const int OBFFormX = 0x0C; // float — form origin X
        private const int OBFFormY = 0x10; // float — form origin Y

        private const int PartEnabled = 0x04;
        private const int PartVisible = 0x05;
        private const int PartType = 0x06;
        private const int PartBrightR = 0x07;
        private const int PartBrightG = 0x08;
        private const int PartBrightB = 0x09;
        private const int PartAlpha = 0x0A;
        private const int PartNamePtr = 0x00;
        private const int PartTexPtr = 0x14;
        private const int PartTexIdx = 0x18;
        private const int PartFlags = 0x19;
        private const int PartABlend = 0x1A;
        private const int PartX = 0x1C; // float
        private const int PartY = 0x20; // float
        private const int PartW = 0x24; // float
        private const int PartH = 0x28; // float

        private const ulong CavePartBase = 0x01F72000;
        private const ulong ConfigBase = 0x01F81000;
        private const int LabelX = 86;
        private const int NativeRowCount = 16;

        private const ulong CursorCaveAddr = 0x01F70B80;
        private const ulong CursorHookAddr = 0x002C1F48;
        private const uint CursorHookJal = 0x0C7DC2E0;
        private const ulong CursorNopAddr = 0x002C1F7C;
        private const ulong FlagBase = 0x01F70B60;

        private static readonly byte[] LabelCaveMips =
        {
            0xC0, 0xFF, 0xBD, 0x27, 0x00, 0x00, 0xBF, 0xAF, 0x04, 0x00, 0xB0, 0xAF, 0x08, 0x00, 0xB1, 0xAF, // 0x01F70E00
            0x0C, 0x00, 0xB2, 0xAF, 0x10, 0x00, 0xB3, 0xAF, 0x50, 0x94, 0x84, 0x8F, 0x0C, 0xAD, 0x08, 0x0C, // 0x01F70E10
            0x00, 0x00, 0x00, 0x00, 0xF8, 0x01, 0x10, 0x3C, 0x58, 0x00, 0x11, 0x8E, 0x40, 0x00, 0x20, 0x12, // 0x01F70E20
            0x00, 0x00, 0x00, 0x00, 0x4C, 0x00, 0x12, 0x8E, 0x50, 0x00, 0x13, 0x8E, 0x54, 0x00, 0x02, 0x8E, // 0x01F70E30
            0x1C, 0x00, 0xA2, 0xAF, 0x00, 0x00, 0x02, 0x92, 0x35, 0x00, 0x40, 0x10, 0x00, 0x00, 0x00, 0x00, // 0x01F70E40
            0x44, 0x00, 0x07, 0x8E, 0x1C, 0x00, 0xA2, 0x8F, 0x21, 0x38, 0xE2, 0x00, 0x2A, 0x18, 0xF2, 0x00, // 0x01F70E50
            0x2F, 0x00, 0x60, 0x14, 0x2A, 0x18, 0xF3, 0x00, 0x2D, 0x00, 0x60, 0x10, 0x00, 0x00, 0x00, 0x00, // 0x01F70E60
            0x18, 0x00, 0xA7, 0xAF, 0x3E, 0x00, 0x04, 0x3C, 0x90, 0x80, 0x84, 0x24, 0x5C, 0x51, 0x0B, 0x0C, // 0x01F70E70
            0x05, 0x00, 0x05, 0x34, 0x3E, 0x00, 0x04, 0x3C, 0x90, 0x80, 0x84, 0x24, 0x10, 0x00, 0x05, 0x34, // 0x01F70E80
            0x28, 0x51, 0x0B, 0x0C, 0x14, 0x00, 0x06, 0x34, 0x3E, 0x00, 0x04, 0x3C, 0x90, 0x80, 0x84, 0x24, // 0x01F70E90
            0x10, 0x00, 0x05, 0x34, 0x2C, 0x51, 0x0B, 0x0C, 0x14, 0x00, 0x06, 0x34, 0x3E, 0x00, 0x04, 0x3C, // 0x01F70EA0
            0x90, 0x80, 0x84, 0x24, 0x68, 0x80, 0x05, 0x3C, 0x48, 0x51, 0x0B, 0x0C, 0x6B, 0x6A, 0xA5, 0x34, // 0x01F70EB0
            0x3E, 0x00, 0x04, 0x3C, 0x90, 0x80, 0x84, 0x24, 0x00, 0x00, 0x05, 0x26, 0x40, 0x00, 0x06, 0x8E, // 0x01F70EC0
            0x88, 0x56, 0x0B, 0x0C, 0x18, 0x00, 0xA7, 0x8F, 0x24, 0x00, 0x02, 0x92, 0x07, 0x00, 0x40, 0x10, // 0x01F70ED0
            0x00, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x04, 0x3C, 0x90, 0x80, 0x84, 0x24, 0x24, 0x00, 0x05, 0x26, // 0x01F70EE0
            0x48, 0x00, 0x06, 0x8E, 0x88, 0x56, 0x0B, 0x0C, 0x4C, 0x00, 0x07, 0x8E, 0x2C, 0x00, 0x02, 0x92, // 0x01F70EF0
            0x07, 0x00, 0x40, 0x10, 0x00, 0x00, 0x00, 0x00, 0x3E, 0x00, 0x04, 0x3C, 0x90, 0x80, 0x84, 0x24, // 0x01F70F00
            0x2C, 0x00, 0x05, 0x26, 0x50, 0x00, 0x06, 0x8E, 0x88, 0x56, 0x0B, 0x0C, 0x54, 0x00, 0x07, 0x8E, // 0x01F70F10
            0x80, 0x00, 0x10, 0x26, 0xFF, 0xFF, 0x31, 0x26, 0xC6, 0xFF, 0x20, 0x16, 0x00, 0x00, 0x00, 0x00, // 0x01F70F20
            0x10, 0x00, 0xB3, 0x8F, 0x0C, 0x00, 0xB2, 0x8F, 0x08, 0x00, 0xB1, 0x8F, 0x04, 0x00, 0xB0, 0x8F, // 0x01F70F30
            0x00, 0x00, 0xBF, 0x8F, 0x08, 0x00, 0xE0, 0x03, 0x40, 0x00, 0xBD, 0x27,                         // 0x01F70F40
        };
        private static readonly byte[] CursorCaveMips =
        {
            0xE0, 0xFF, 0xBD, 0x27, 0x00, 0x00, 0xBF, 0xAF, 0xEC, 0xAA, 0x08, 0x0C, 0x00, 0x00, 0x00, 0x00, // 0x01F70B80
            0xF7, 0x01, 0x02, 0x3C, 0x60, 0x0B, 0x43, 0x8C, 0x15, 0x00, 0x60, 0x10, 0x00, 0x00, 0x00, 0x00, // 0x01F70B90
            0x64, 0x0B, 0x44, 0x8C, 0x68, 0x0B, 0x45, 0x8C, 0x6C, 0x0B, 0x46, 0x8C, 0x70, 0x0B, 0x47, 0x8C, // 0x01F70BA0
            0x74, 0x0B, 0x48, 0x8C, 0x78, 0x0B, 0x49, 0x8C, 0xF8, 0x94, 0x82, 0x8F, 0x38, 0x01, 0x4A, 0x8C, // 0x01F70BB0
            0x3C, 0x01, 0x4B, 0x8C, 0x04, 0x00, 0x02, 0x34, 0x20, 0x00, 0x42, 0xA1, 0x24, 0x00, 0x43, 0xAD, // 0x01F70BC0
            0x28, 0x00, 0x44, 0xAD, 0x20, 0x00, 0x62, 0xA1, 0x24, 0x00, 0x65, 0xAD, 0x28, 0x00, 0x66, 0xAD, // 0x01F70BD0
            0x03, 0x00, 0xE0, 0x10, 0x00, 0x00, 0x00, 0x00, 0x24, 0x00, 0xE8, 0xAC, 0x28, 0x00, 0xE9, 0xAC, // 0x01F70BE0
            0x00, 0x00, 0xBF, 0x8F, 0x08, 0x00, 0xE0, 0x03, 0x20, 0x00, 0xBD, 0x27,                         // 0x01F70BF0
        };
        public class OptionRow
        {
            /// <summary>Label shown in the options menu (max 36 chars)</summary>
            public string Label { get; init; }

            /// <summary>Number of toggle buttons (2 = On/Off, 3 = Low/Med/High etc.)</summary>
            public int ButtonCount { get; init; }

            /// <summary>
            /// Called once during injection to get the initial selected button index.
            /// Return 0 for first button, 1 for second, etc.
            /// </summary>
            public Func<int> GetInitialValue { get; init; }

            /// <summary>
            /// Called when the player changes this row's value in-game.
            /// Parameter is the new button index (0-based).
            /// </summary>
            public Action<int> OnChanged { get; init; }
        }

        private readonly List<OptionRow> _rows;
        private CancellationTokenSource _cts;
        private Task _pollTask;
        private bool _injected;
        private readonly Dictionary<int, int> _lastValues = new();

        private readonly Dictionary<(int row, int btn), ulong> _partAddrs = new();
        private ulong _menuOptionPine;

        public OptionsHelper(List<OptionRow> rows)
        {
            _rows = rows ?? throw new ArgumentNullException(nameof(rows));
        }
        public void Start()
        {
            _cts = new CancellationTokenSource();
            _pollTask = Task.Run(() => PollLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _pollTask?.Wait(TimeSpan.FromSeconds(2));
            Uninstall();
        }
        private bool IsOptionsPageOpen()
        {
            var mci = (uint)Memory.ReadInt(Addresses.MenuCommonInfo);
            if (mci == 0) return false;
            int page = Memory.ReadInt(mci + 0x54);
            return page == 7;
        }
        private void PollLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    bool optionsOpen = IsOptionsPageOpen();
                    if (optionsOpen)
                    {
                        if (!_injected)
                        {
                            Log.Information("[OptionsInjector] Options screen detected — injecting");
                            Inject();
                        }
                        else
                        {

                            // Re-validate hook each poll in case of save state reload
                            EnsureHookInstalled();
                            UpdateScrollData();
                            PollCursor();
                        }
                    }
                    else if (_injected)
                    {
                        Log.Information("[OptionsInjector] Options page closed — resetting");
                        _injected = false;
                        _lastValues.Clear();
                        _partAddrs.Clear();
                        Memory.Write(GlobalNumRows, 0);
                        Memory.Write(FlagBase + 0x00, 0);  // clear cursor override
                        Memory.Write(FlagBase + 0x7C, 0);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[OptionsInjector] Poll error: {Ex}", ex.Message);
                }

                Thread.Sleep(200);
            }
        }
        private void Inject()
        {
            try
            {
                WriteCave();
                EnsureHookInstalled();
                InjectButtonParts();
                WriteLabelData();
                _injected = true;
                Log.Information("[OptionsInjector] Injected {N} custom rows", _rows.Count);
            }
            catch (Exception ex)
            {
                Log.Error("[OptionsInjector] Injection failed: {Ex}", ex);
            }
        }
        private void WriteCave()
        {
            Memory.WriteByteArray(CaveAddr, LabelCaveMips);
            Memory.WriteByteArray(CursorCaveAddr, CursorCaveMips);
            Log.Debug("[OptionsInjector] MIPS cave written to 0x{Addr:X8}", CaveAddr);
        }
        private void EnsureHookInstalled()
        {
            if ((uint)Memory.ReadInt(HookPtr) != (uint)CaveAddr)
            {
                Memory.Write(HookPtr, (int)CaveAddr);
            }

            if ((uint)Memory.ReadInt(CursorHookAddr) != CursorHookJal)
            {
                Memory.Write(CursorHookAddr, (int)CursorHookJal);
                Memory.Write(CursorNopAddr, 0);
            }
        }
        private void Uninstall()
        {
            // Zero the num_rows so the cave skips drawing immediately
            Memory.Write(GlobalNumRows, 0);
        }

        private void InjectButtonParts()
        {
            var obfPs2 = (uint)Memory.ReadInt(Addresses.OptionButtonForm);
            if (obfPs2 == 0) throw new InvalidOperationException("OptionButtonForm ptr is null");

            ulong obf = obfPs2; // Memory class handles PCSX2 offset

            var nativeParts = (ushort)Memory.ReadShort(obf + (ulong)OBFPartCount);
            var nativePartsPtr = (uint)Memory.ReadInt(obf + OBFPartsPtr);

            // Read template from native parts 0 and 1 (ON / OFF buttons)
            var templates = new PartTemplate[2];
            for (int t = 0; t < 2; t++)
            {
                ulong p = nativePartsPtr + (ulong)(t * 0x48);
                templates[t] = new PartTemplate
                {
                    TexPtr = (uint)Memory.ReadInt(p + PartTexPtr),
                    TexIdx = Memory.ReadByte(p + PartTexIdx),
                    Flags = Memory.ReadByte(p + PartFlags),
                    ABlend = Memory.ReadByte(p + PartABlend),
                    Type = Memory.ReadByte(p + PartType),
                    W = Memory.ReadFloat(p + PartW),
                    H = Memory.ReadFloat(p + PartH),
                };
            }

            // Copy existing native parts into cave area
            ulong cave = CavePartBase;
            int copySize = nativeParts * 0x48;
            var nativePartsData = Memory.ReadByteArray(nativePartsPtr, copySize);
            Memory.WriteByteArray(CavePartBase, nativePartsData);

            // Read CMenuOption ptr for wiring later
            var menuObjPs2 = (uint)Memory.ReadInt(Addresses.CMenuOptionPtr); // reuse — placeholder
            // TODO: use the correct CMenuOption ptr address from Reforged's addr table
            _menuOptionPine = menuObjPs2; // see note below

            // Append custom button parts
            _partAddrs.Clear();
            int partIdx = nativeParts;
            ulong namesBase = cave + (ulong)((nativeParts + TotalButtonCount()) * 0x48);
            int nameIdx = 0;

            for (int rowI = 0; rowI < _rows.Count; rowI++)
            {
                var row = _rows[rowI];
                int gameRow = NativeRowCount + rowI;
                int initVal = row.GetInitialValue?.Invoke() ?? 0;

                // Write initial config value
                Memory.Write(ConfigBase + (ulong)(rowI * 4), initVal);
                _lastValues[rowI] = initVal;

                for (int b = 0; b < row.ButtonCount; b++)
                {
                    ulong partAddr = cave + (ulong)(partIdx * 0x48);

                    // Zero the part
                    for (int i = 0; i < 0x48; i += 4)
                        Memory.Write(partAddr + (ulong)i, 0);

                    // Pick template: btn 0 = ON style, btn 1 = OFF style (clamp to available)
                    var tmpl = templates[Math.Min(b, 1)];

                    // Brightness: selected button is full bright, others dimmed
                    byte bright = (b == initVal) ? (byte)0x80 : (byte)0x40;

                    Memory.WriteByte(partAddr + PartEnabled, 1);
                    Memory.WriteByte(partAddr + PartVisible, 1);
                    Memory.WriteByte(partAddr + PartType, tmpl.Type);
                    Memory.WriteByte(partAddr + PartBrightR, bright);
                    Memory.WriteByte(partAddr + PartBrightG, bright);
                    Memory.WriteByte(partAddr + PartBrightB, bright);
                    Memory.WriteByte(partAddr + PartAlpha, 0x80);
                    Memory.Write(partAddr + PartTexPtr, (int)tmpl.TexPtr);
                    Memory.WriteByte(partAddr + PartTexIdx, tmpl.TexIdx);
                    Memory.WriteByte(partAddr + PartFlags, tmpl.Flags);
                    Memory.WriteByte(partAddr + PartABlend, tmpl.ABlend);

                    float x = 230.0f + b * (tmpl.W + 4.0f);
                    float y = 384.0f + rowI * 24.0f;
                    Memory.Write(partAddr + PartX, x);
                    Memory.Write(partAddr + PartY, y);
                    Memory.Write(partAddr + PartW, tmpl.W);
                    Memory.Write(partAddr + PartH, tmpl.H);

                    // Write part name string "INDEX{gameRow}{b}\0" into names area
                    var name = Encoding.ASCII.GetBytes($"INDEX{gameRow}{b}\0\0\0\0");
                    ulong nameAddr = namesBase + (ulong)(nameIdx * 16);
                    for (int i = 0; i < Math.Min(name.Length, 12); i++)
                        Memory.WriteByte(nameAddr + (ulong)i, name[i]);
                    Memory.Write(partAddr + PartNamePtr, (int)(nameAddr));

                    _partAddrs[(rowI, b)] = partAddr;
                    nameIdx++;
                    partIdx++;
                }

                // Wire button ptrs and config ptr into CMenuOption
                // Only wire if offset is safe (won't overlap native config_ptrs)
                for (int b = 0; b < row.ButtonCount; b++)
                {
                    int btnPtrOff = MnuBtnPartPtr + gameRow * 0xC + b * 4;
                    if (btnPtrOff < 0x254)
                    {
                        var partPs2 = (uint)(_partAddrs[(rowI, b)]);
                        Memory.Write(_menuOptionPine + (ulong)btnPtrOff, (int)partPs2);
                    }
                }
                for (int b = row.ButtonCount; b < 3; b++)
                {
                    int btnPtrOff = MnuBtnPartPtr + gameRow * 0xC + b * 4;
                    if (btnPtrOff < 0x254)
                        Memory.Write(_menuOptionPine + (ulong)btnPtrOff, 0);
                }

                // Set max toggle count
                Memory.Write(_menuOptionPine + (ulong)(MnuMaxToggle + gameRow * 4), row.ButtonCount);

                // Set config pointer
                var configPs2 = (uint)(ConfigBase + (ulong)(rowI * 4));
                Memory.Write(_menuOptionPine + (ulong)(MnuConfigPtr + gameRow * 4), (int)configPs2);
            }

            // Update OptionButtonForm: new parts count and pointer
            int newCount = nativeParts + TotalButtonCount();
            var newPartsPs2 = (uint)(cave);
            Memory.Write(obf + OBFPartsPtr, (int)newPartsPs2);
            Memory.Write(obf + (ulong)OBFPartCount, (short)newCount);

            // Extend CONFIG_OPTION_NUM so the game scrolls to all rows
            int totalRows = NativeRowCount + _rows.Count;
            Memory.Write(Addresses.ConfigOptionNumI, BitConverter.ToInt32(BitConverter.GetBytes((float)totalRows), 0));
            Memory.Write(Addresses.ConfigOptionNumF, BitConverter.ToInt32(BitConverter.GetBytes((float)totalRows), 0));
        }
        private void WriteLabelData()
        {
            Memory.Write(GlobalNumRows, _rows.Count);

            for (int i = 0; i < _rows.Count; i++)
            {
                ulong rowBase = LabelMemBase + (ulong)(i * LabelRowStride);
                WriteAsciiString(rowBase + OffLabelStr, _rows[i].Label, 36);
                Memory.Write(rowBase + OffLabelX, LabelX);
            }

            // Pre-write cursor rect part ptr and fixed size — done once at inject time
            var mciPs2 = (uint)Memory.ReadInt(Addresses.MenuCommonInfo);
            if (mciPs2 != 0)
            {
                var rectFormPs2 = (uint)Memory.ReadInt(mciPs2 + 0x13C);
                if (rectFormPs2 != 0)
                {
                    var rectPart0Ps2 = Memory.ReadInt(rectFormPs2 + 0x6C);
                    Memory.Write(FlagBase + 0x10, rectPart0Ps2);   // rect fill part ptr
                    Memory.Write(FlagBase + 0x14, 50.0f);           // fixed width
                    Memory.Write(FlagBase + 0x18, 16.0f);           // fixed height
                }
            }
        }
        private void UpdateScrollData()
        {
            try
            {
                // Clip region from LocalMenuClipForm
                var clipFormPs2 = (uint)Memory.ReadInt(Addresses.LocalMenuClipForm);
                if (clipFormPs2 != 0)
                {
                    ulong clipForm = clipFormPs2;
                    int clipY = (int)Memory.ReadFloat(clipForm + 0x10);
                    int clipH = Memory.ReadShort(clipForm + 0x06);
                    Memory.Write(GlobalClipTop, clipY);
                    Memory.Write(GlobalClipBottom, clipY + clipH - 24);
                }

                // OptionButtonForm Y — the cave adds this to each row's rel_y
                var obfPs2 = (uint)Memory.ReadInt(Addresses.OptionButtonForm);
                if (obfPs2 != 0)
                {
                    ulong obf = obfPs2;
                    float formY = Memory.ReadFloat(obf + OBFFormY);
                    Memory.Write(GlobalFormY, (int)formY);

                    // Write each row's Y relative to the form
                    for (int i = 0; i < _rows.Count; i++)
                    {
                        if (_partAddrs.TryGetValue((i, 0), out ulong partAddr))
                        {
                            float btnRelY = Memory.ReadFloat(partAddr + PartY);
                            ulong rowBase = LabelMemBase + (ulong)(i * LabelRowStride);
                            Memory.Write(rowBase + OffLabelRelY, (int)btnRelY + 2);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[OptionsInjector] UpdateScrollData: {Ex}", ex.Message);
            }
        }
        private void PollCursor()
        {
            if (_menuOptionPine == 0) return;

            try
            {
                int cursorRow = Memory.ReadInt(_menuOptionPine + MnuCursorRow);

                if (cursorRow >= NativeRowCount)
                {
                    int rowI = cursorRow - NativeRowCount;
                    if (rowI >= _rows.Count) return;

                    int subSel = Memory.ReadInt(_menuOptionPine + MnuSubSelect);
                    int maxBtn = _rows[rowI].ButtonCount;
                    subSel = Math.Clamp(subSel, 0, maxBtn - 1);
                    Memory.Write(_menuOptionPine + MnuSubSelect, subSel);

                    // Get the selected button's absolute screen position
                    if (_partAddrs.TryGetValue((rowI, subSel), out ulong partAddr))
                    {
                        var obfPs2 = (uint)Memory.ReadInt(Addresses.OptionButtonForm);
                        if (obfPs2 != 0)
                        {
                            float formX = Memory.ReadFloat(obfPs2 + OBFFormX);
                            float formY = Memory.ReadFloat(obfPs2 + OBFFormY);
                            float btnX = Memory.ReadFloat(partAddr + PartX);
                            float btnY = Memory.ReadFloat(partAddr + PartY);
                            float bsx = formX + btnX;
                            float bsy = formY + btnY;
                            float btnW = Memory.ReadFloat(partAddr + PartW);

                            float highlightWidth = 210.0f;
                            float highlightHeight = 16.0f;

                            Memory.Write(FlagBase + 0x00, (int)(bsx - 50));
                            Memory.Write(FlagBase + 0x04, (int)(bsy - 3));
                            Memory.Write(FlagBase + 0x08, (int)(bsx - 2));
                            Memory.Write(FlagBase + 0x0C, (int)(bsy - 3));
                            Memory.Write(FlagBase + 0x7C, maxBtn);

                        }
                    }

                    // Fire OnChanged if config value changed
                    int newVal = Memory.ReadInt(ConfigBase + (ulong)(rowI * 4));
                    if (_lastValues.TryGetValue(rowI, out int oldVal) && newVal != oldVal)
                    {
                        Log.Information("[OptionsInjector] Row '{Label}' -> {Val}", _rows[rowI].Label, newVal);
                        _rows[rowI].OnChanged?.Invoke(newVal);
                        _lastValues[rowI] = newVal;
                        UpdateButtonBrightness(rowI, newVal);
                    }
                }
                else
                {
                    // Native row selected — clear FLAG_BASE so cursor cave does nothing
                    Memory.Write(FlagBase + 0x00, 0);
                    Memory.Write(FlagBase + 0x7C, 0);
                }
            }
            catch (Exception ex)
            {
                Log.Warning("[OptionsInjector] PollCursor: {Ex}", ex.Message);
            }
        }

        private void UpdateButtonBrightness(int rowI, int selectedBtn)
        {
            var row = _rows[rowI];
            for (int b = 0; b < row.ButtonCount; b++)
            {
                if (!_partAddrs.TryGetValue((rowI, b), out ulong partAddr)) continue;
                byte bright = (b == selectedBtn) ? (byte)0x80 : (byte)0x40;
                Memory.WriteByte(partAddr + PartBrightR, bright);
                Memory.WriteByte(partAddr + PartBrightG, bright);
                Memory.WriteByte(partAddr + PartBrightB, bright);
            }
        }
        private int TotalButtonCount()
        {
            int total = 0;
            foreach (var r in _rows) total += r.ButtonCount;
            return total;
        }

        private static void WriteAsciiString(ulong addr, string s, int maxLen)
        {
            var bytes = Encoding.ASCII.GetBytes(s);
            int len = Math.Min(bytes.Length, maxLen);
            for (int i = 0; i < len; i++)
                Memory.WriteByte(addr + (ulong)i, bytes[i]);
            Memory.WriteByte(addr + (ulong)len, 0); // null terminator
        }

        private struct PartTemplate
        {
            public uint TexPtr;
            public byte TexIdx, Flags, ABlend, Type;
            public float W, H;
        }
    }
}
