using GameBoy.Core.Hardware;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GameBoy.Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Core.GameBoySystem gameBoySystem;
        private readonly WriteableBitmap imageBuffer;
        private readonly WriteableBitmap bgBuffer;
        private readonly WriteableBitmap spriteBuffer;
        private readonly WriteableBitmap windowBuffer;
        private readonly WriteableBitmap tileBuffer1;
        private readonly WriteableBitmap tileBuffer2;
        private readonly WriteableBitmap tileBuffer3;

        private int frameRenderCount = 0;
        private readonly System.Threading.Timer fpsTimer;

        private const int cpuStateLength = 100;
        private readonly ConcurrentQueue<CpuState> cpuStates = new();

        private readonly JoypadState joypadState = new();

        private readonly string[] hexStrings = new string[ushort.MaxValue + 1];
        private CancellationTokenSource CancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();

            InitHexStrings();

            imageBuffer = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Rgb24, null);
            spriteBuffer = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Rgb24, null);
            windowBuffer = new WriteableBitmap(160, 144, 96, 96, PixelFormats.Rgb24, null);
            bgBuffer = new WriteableBitmap(256, 256, 96, 96, PixelFormats.Rgb24, null);
            tileBuffer1 = new WriteableBitmap(128, 64, 96, 96, PixelFormats.Rgb24, null);
            tileBuffer2 = new WriteableBitmap(128, 64, 96, 96, PixelFormats.Rgb24, null);
            tileBuffer3 = new WriteableBitmap(128, 64, 96, 96, PixelFormats.Rgb24, null);

            imgGameImage.Source = imageBuffer;
            imgBgImage.Source = bgBuffer;
            imgWindowImage.Source = windowBuffer;
            imgSpriteImage.Source = spriteBuffer;
            imgTileImage.Source = tileBuffer1;
            imgTileImage2.Source = tileBuffer2;
            imgTileImage3.Source = tileBuffer3;

            fpsTimer = new System.Threading.Timer(FpsTimerCallBack, null, 0, 1000);
        }

        private void SelectCartridge(string fileName)
        {
            if (gameBoySystem != null)
            {
                CancellationTokenSource.Cancel();
            }

            gameBoySystem = new Core.GameBoySystem(fileName, false, joypadState, FrameRenderComplete, CpuCycleCompleted, ExceptionRaised);

#if DEBUG
            gameBoySystem.Gpu.RenderDebugBuffers = true;
#else
            GameBoySystem.Gpu.RenderDebugBuffers = false;
#endif

            Title = $"GameBoy Player - {gameBoySystem.Cartridge.Title.Replace("\0", "")} ({gameBoySystem.Cartridge.MemoryBankController.MbcType})";

            PlayGame();
        }

        private void PlayGame()
        {
            CancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => gameBoySystem.Run(CancellationTokenSource.Token));
        }

        private void FpsTimerCallBack(object state)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                txtFPS.Text = frameRenderCount.ToString();
                frameRenderCount = 0;
            }));
        }

        private async Task FrameRenderComplete(byte[] imageBuffer)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                frameRenderCount++;
                FlushBuffer(imageBuffer);
            }));
        }

        private void FlushBuffer(byte[] imageBuffer)
        {
            //if (imageBuffer.Any(b => b != 255))
            //{
            //    var nonWhite = imageBuffer.Where(b => b != 255).ToList();
            //    //Debugger.Break();
            //}

            // A frame has been completely rendered
            this.imageBuffer.WritePixels(new Int32Rect(0, 0, 160, 144), imageBuffer, Gpu.Stride, 0);

            bgBuffer.WritePixels(new Int32Rect(0, 0, 256, 256), gameBoySystem.Gpu.BgBuffer, 256 * 3, 0);
            spriteBuffer.WritePixels(new Int32Rect(0, 0, 160, 144), gameBoySystem.Gpu.SpriteBuffer, Gpu.Stride, 0);
            windowBuffer.WritePixels(new Int32Rect(0, 0, 160, 144), gameBoySystem.Gpu.WindowBuffer, Gpu.Stride, 0);

            tileBuffer1.WritePixels(new Int32Rect(0, 0, 128, 64), gameBoySystem.Gpu.TileBuffer1, 3 * 16 * 8, 0);
            tileBuffer2.WritePixels(new Int32Rect(0, 0, 128, 64), gameBoySystem.Gpu.TileBuffer2, 3 * 16 * 8, 0);
            tileBuffer3.WritePixels(new Int32Rect(0, 0, 128, 64), gameBoySystem.Gpu.TileBuffer3, 3 * 16 * 8, 0);

            DisplayLastCpuState();
            //PopulateCpuStateList();
        }

        private CpuState? GetLastCpuState()
        {
            CpuState? lastCpuState = null;

            if (cpuStates.Any())
            {
                lastCpuState = cpuStates.Last();
            }

            return lastCpuState;
        }

        private void CpuCycleCompleted()
        {
            while (cpuStates.Count >= cpuStateLength)
            {
                cpuStates.TryDequeue(out _);
            }

            cpuStates.Enqueue(gameBoySystem.Cpu.GetState());
        }

        private void DisplayLastCpuState()
        {
            var lastCpuState = GetLastCpuState();

            DisplayCpuState(ref lastCpuState);
        }

        private void DisplayCpuState(ref CpuState? cpuStateParam)
        {
            if (!cpuStateParam.HasValue)
            {
                return;
            }

            var cpuState = cpuStateParam.Value;

            txtRegisterA.Text = hexStrings[cpuState.A];
            txtRegisterF.Text = hexStrings[cpuState.F];

            txtRegisterB.Text = hexStrings[cpuState.B];
            txtRegisterC.Text = hexStrings[cpuState.C];

            txtRegisterD.Text = hexStrings[cpuState.D];
            txtRegisterE.Text = hexStrings[cpuState.E];

            txtRegisterH.Text = hexStrings[cpuState.H];
            txtRegisterL.Text = hexStrings[cpuState.L];

            txtPC.Text = hexStrings[cpuState.ProgramCounter];
            txtSP.Text = hexStrings[cpuState.StackPointer];
        }

        private void ExceptionRaised()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                DisplayLastCpuState();
                PopulateCpuStateList();
            }));
        }

        private void PopulateCpuStateList()
        {
            lstCpuStates.Items.Clear();

            foreach (var cpuState in cpuStates.Reverse())
            {
                lstCpuStates.Items.Add(cpuState);
            }
        }

        private void InitHexStrings()
        {
            for (var i = 0; i <= ushort.MaxValue; i++)
            {
                hexStrings[i] = i.ToString("X2");
            }
        }

        private void lstCpuStates_Selected(object sender, SelectionChangedEventArgs e)
        {
            var cpuState = (CpuState?)lstCpuStates.SelectedItem;
            DisplayCpuState(ref cpuState);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            HandleKeyChange(e.Key, true);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            HandleKeyChange(e.Key, false);
        }

        private void HandleKeyChange(Key key, bool keyDown)
        {
            switch (key)
            {
                case Key.Enter:
                    joypadState.StartPressed = keyDown;
                    break;
                case Key.RightShift:
                case Key.LeftShift:
                    joypadState.SelectPressed = keyDown;
                    break;
                case Key.A:
                    joypadState.APressed = keyDown;
                    break;
                case Key.B:
                    joypadState.BPressed = keyDown;
                    break;
                case Key.Down:
                    joypadState.InputDownPressed = keyDown;
                    break;
                case Key.Up:
                    joypadState.InputUpPressed = keyDown;
                    break;
                case Key.Left:
                    joypadState.InputLeftPressed = keyDown;
                    break;
                case Key.Right:
                    joypadState.InputRightPressed = keyDown;
                    break;
            }
        }

        private void imgGameImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Filter = "Gameboy ROM|*.gb"
            };

            if (fileDialog.ShowDialog().Value)
            {
                SelectCartridge(fileDialog.FileName);
            }
        }
    }
}
