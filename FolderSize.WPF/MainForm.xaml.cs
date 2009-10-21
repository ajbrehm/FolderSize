﻿
/* ****************************************************************************
 *
 * Copyright (c) Mårten Rånge.
 *
 * This source code is subject to terms and conditions of the Microsoft Public License. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Microsoft Public License, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Microsoft Public License.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;


namespace FolderSize.WPF
{
   partial class MainForm
   {

      // ----------------------------------------------------------------------

      Storyboard m_storyboard;

      // ----------------------------------------------------------------------

      [StructLayout(LayoutKind.Sequential)]
      struct MARGINS
      {
         public int cxLeftWidth;      // width of left border that retains its size
         public int cxRightWidth;     // width of right border that retains its size
         public int cyTopHeight;      // height of top border that retains its size
         public int cyBottomHeight;   // height of bottom border that retains its size
      };

      // ----------------------------------------------------------------------

      [DllImport("DwmApi.dll")]
      static extern int DwmExtendFrameIntoClientArea(
          IntPtr hwnd,
          ref MARGINS pMarInset);

      // ----------------------------------------------------------------------

      public MainForm()
      {
         InitializeComponent();
         m_storyboard = (Storyboard) Resources["InfoBlockStoryboard"];

         var mouseMoveEvent = MouseMoveEvent;
         MouseEventHandler eventHandler = WindowMouseMove;
         AddHandler(
            mouseMoveEvent,
            eventHandler,
            true);
      }

      // ----------------------------------------------------------------------

      void WindowMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
      {
         if (InfoBlock.Visibility == Visibility.Visible)
         {
            var timeSpan = m_storyboard.GetCurrentTime ();
            if (timeSpan.TotalSeconds > 1)
            {
               m_storyboard.Seek (TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            }
        }
      }

      // ----------------------------------------------------------------------

      public void WindowClosed(object sender, EventArgs value)
      {
         var job = FolderTreeView.Job;
         if (job != null)
         {
            job.StopJob = true;
         }
      }

      // ----------------------------------------------------------------------

      void WindowLoaded(object sender, RoutedEventArgs e)
      {
         try
         {
            // Obtain the window handle for WPF application
            var mainWindowPtr = new WindowInteropHelper(this).Handle;
            var mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);
            mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(
               0,
               0,
               0,
               0);

            var margins =
               new MARGINS
               {
                  cxLeftWidth = -1,
                  cxRightWidth = -1,
                  cyTopHeight = -1,
                  cyBottomHeight = -1
               };

            var hresult =
               DwmExtendFrameIntoClientArea(
                  mainWindowSrc.Handle,
                  ref margins);
            //
            if (hresult < 0)
            {
               throw new Exception(
                  string.Format(
                     "DwmExtendFrameIntoClientArea failed: {0}", hresult)
                     );
            }
         }
         catch
         {
            // Fallback brush
            Application.Current.MainWindow.Background =
               (Brush)Application.Current.Resources["WindowGradient"];
         }

      }

      // ----------------------------------------------------------------------

      void GoButtonClick(object sender, RoutedEventArgs e)
      {
         StartInfoBlock.Visibility = Visibility.Collapsed;
         InfoBlock.Visibility = Visibility.Visible;
         m_storyboard.Begin();
      }

      // ----------------------------------------------------------------------

   }
}