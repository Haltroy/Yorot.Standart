using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Yorot.AppForms
{
    // TODO: Add XML comments to all of these

    #region Control Class

    /// <summary>
    /// The main class of all Yorot AppForms classes.
    /// </summary>
    public class Control
    {
        /// <summary>
        /// Creates a new <see cref="Control"/>.
        /// </summary>
        /// <param name="app"><see cref="YorotApp"/></param>
        public Control(YorotApp app)
        {
            App = app;
        }

        private string _Name = string.Empty;

        /// <summary>
        /// Name of this control.
        /// </summary>
        public string Name
        { get => _Name; set { if (OnNameChange != null) { OnNameChange(this, new DifferenceEventArgs<string>(_Name, value)); } _Name = value; } }

        /// <summary>
        /// Event raised when this control's name is changed.
        /// </summary>
        public event OnNameChangeDelegate OnNameChange;

        /// <summary>
        /// Delegate of <see cref="OnNameChange"/>.
        /// </summary>
        /// <param name="sender">The control that did its property changed.</param>
        /// <param name="e"><see cref="DifferenceEventArgs{T}"/> that contain both the new and old value.</param>
        public delegate void OnNameChangeDelegate(object sender, DifferenceEventArgs<string> e);

        private Control _Parent = null;

        /// <summary>
        /// The control that contains this control.
        /// </summary>
        public Control Parent
        { get => _Parent; set { if (OnParentChange != null) { OnParentChange(this, new DifferenceEventArgs<Control>(_Parent, value)); } _Parent = value; } }

        /// <summary>
        /// Event raised whe this control's parent is changed.
        /// </summary>
        public event OnParentChangeDelegate OnParentChange;

        /// <summary>
        /// Delegate of <see cref="OnParentChange"/>.
        /// </summary>
        /// <param name="sender">The control that did its property changed.</param>
        /// <param name="e"><see cref="DifferenceEventArgs{T}"/> that contain both the new and old value.</param>
        public delegate void OnParentChangeDelegate(object sender, DifferenceEventArgs<Control> e);

        /// <summary>
        /// The <see cref="YorotApp"/> associated with this control.
        /// </summary>
        public YorotApp App { get; }

        private YorotRectangle _Position = new YorotRectangle(0,0,100,50);
        private YorotRectangle _Maximums= new YorotRectangle(0, 0, 0,0);
        private YorotRectangle _Minimums= new YorotRectangle(0, 0, 0,0);
        /// <summary>
        /// Contains the size and position information of this control.
        /// </summary>
        public YorotRectangle Position { get => _Position; set {  _Position = value; } }

        public event OnPositionChangeDelegate OnPositionChange;
        public delegate void OnPositionChangeDelegate(object sender, DifferenceEventArgs<YorotRectangle> e);

        /// <summary>
        /// Contains the maximum size and positions of this control. 0 (zero) values means no restrictions.
        /// </summary>
        public YorotRectangle Maximums { get; set; }

        /// <summary>
        /// Contains the minimum size and positions of this control. 0 (zero) values means no restrictions.
        /// </summary>
        public YorotRectangle Minimums { get; set; }

        /// <summary>
        /// Determines where should this control dock on <see cref="Parent"/>.
        /// </summary>
        public DockStyle Dock { get; set; }

        /// <summary>
        /// THe background color to draw.
        /// </summary>
        public Color BackColor { get; set; }

        /// <summary>
        /// The <see cref="System.IO.Stream"/> that contains the image for background.
        /// </summary>
        public System.IO.Stream BackImageStream { get; set; }

        /// <summary>
        /// The foreground (text) color to draw.
        /// </summary>
        public Color ForeColor { get; set; }

        /// <summary>
        /// Determines if this control is visible or not.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Determines if this control is usable by the user or not.
        /// </summary>
        public bool Enabled { get; set; }
    }

    public class KeyEventArgs : EventArgs
    {
        public KeyEventArgs(ConsoleKey key)
        {
            Key = key;
        }

        public ConsoleKey Key { get; }
    }

    public class MouseEventArgs : EventArgs
    {
        public MouseEventArgs(MouseButton button) : this(button, new YorotRectangle(0, 0, 0, 0))
        {
        }

        public MouseEventArgs(MouseButton button, YorotRectangle position) : this(button, 1, position)
        {
        }

        public MouseEventArgs(MouseButton button, int count, YorotRectangle position)
        {
            Button = button;
            Count = count;
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public MouseEventArgs(float scroll) : this(scroll, new YorotRectangle(0, 0, 0, 0))
        {
        }

        public MouseEventArgs(float scroll, YorotRectangle position) : this(MouseButton.None, 0, position)
        {
            Scroll = scroll;
        }

        public MouseButton Button { get; }
        public int Count { get; }
        public float Scroll { get; }
        public YorotRectangle Position { get; }
    }

    public class DifferenceEventArgs<T> : EventArgs
    {
        public DifferenceEventArgs(T oldValue, T newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; }
        public T NewValue { get; }
    }

    public class HoverEventArgs : EventArgs
    {
        public HoverEventArgs(YorotRectangle position, HoverMode mode)
        {
            Position = position ?? throw new ArgumentNullException(nameof(position));
            Mode = mode;
        }

        public YorotRectangle Position { get; }
        public HoverMode Mode { get; }
    }

    public enum HoverMode
    {
        Enter,
        Continue,
        Leave
    }

    public enum MouseButton
    {
        None,
        Left,
        Middle,
        Right,
    }

    // TODO: Turn this into indivitual events
    // TODO: Combine Clicks, Hovers and Drags into one
    // TODO: Add property changed events
    public enum ActivateReason
    {
        Focused,
        Dragged,
        Dropped,
        DragDrop,
        TrackChange,
        ProgressChange,
        SelectionChange,
        CheckStateChanged,
    }

    public class Color
    {
        public Color(string hex)
        {
            hex = hex.Replace("#", "").ToLower();
            Alpha = int.Parse(hex.Length == 8 ? hex.Substring(0, 2) : "ff", System.Globalization.NumberStyles.HexNumber);
            Red = int.Parse(hex.Length == 8 ? hex.Substring(2, 2) : hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            Green = int.Parse(hex.Length == 8 ? hex.Substring(4, 2) : hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            Blue = int.Parse(hex.Length == 8 ? hex.Substring(6, 2) : hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        }

        public string Hex => "#" + Alpha.ToString("X") + Red.ToString("X") + Green.ToString("X") + Blue.ToString("X");

        public Color(int red, int green, int blue)
        {
            Alpha = 255;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public Color(int alpha, int red, int green, int blue)
        {
            Alpha = alpha;
            Red = red;
            Green = green;
            Blue = blue;
        }

        public int Alpha { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
    }

    public enum DockStyle
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        Center,
    }

    public class YorotRectangle
    {
        public YorotRectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    #endregion Control Class

    #region Enums

    public enum ImagePosition
    {
        Left,
        Center,
        Right,
        Top,
        Bottom,
    }

    public enum ImageTextVisibility
    {
        None,
        TextOnly,
        ImageOnly,
        TextBeforeImage,
        TextAfterImage,
        ImageOnLeft,
        ImageOnRight,
        ImageOnTop,
        ImageOnBottom,
    }

    public enum ProgressType
    {
        Normal,
        Loading,
        Circle,
    }

    public enum DialogType
    {
        OpenFile,
        OpenMultipleFile,
        SaveFile,
        OpenFolder
    }

    public enum TabOrientation
    {
        Top,
        Bottom,
        Left,
        Right,
    }

    public enum FontStyle
    {
        Regular,
        Italic,
        Bold,
        Understrike,
        Strikethrough
    }

    public enum TextOrientation
    {
        Left,
        Center,
        Right
    }

    public enum ImageSizeMode
    {
        Normal,
        Stretch,
        Zoom,
        Tile,
        Resize
    }

    #endregion Enums

    #region Controls

    public class Button : Text
    {
        public Button(YorotApp app) : base(app)
        {
        }

        public Color HoverColor { get; set; }
        public Color ClickColor { get; set; }
    }

    public class Text : Control
    {
        public Text(YorotApp app) : base(app)
        {
        }

        public System.IO.Stream ImageStream { get; set; }
        public ImageTextVisibility ImageTextVisibility { get; set; }
        public ImagePosition ImagePosition { get; set; }
        public ImageSizeMode ImageSizeMode { get; set; }
        public string Caption { get; set; }
        public string Header { get; set; }
        public string FontFamily { get; set; }
        public float FontSize { get; set; }
        public bool ReadOnly { get; set; }
        public bool AllowSelection { get; set; }
        public FontStyle FontStyle { get; set; }
        public TextOrientation Orientation { get; set; }
        public List<LinkItem> Links { get; set; }

        public class LinkItem
        {
            public LinkItem(string link, int linkStart, int linkLength)
            {
                LinkStart = linkStart;
                LinkLength = linkLength;
                Link = link ?? throw new ArgumentNullException(nameof(link));
            }

            public int LinkStart { get; set; }
            public int LinkLength { get; set; }
            public string Link { get; set; }
        }
    }

    public class Image : Control
    {
        public Image(YorotApp app) : base(app)
        {
        }

        public System.IO.Stream ImageStream { get; set; }
        public ImageSizeMode ImageSizeMode { get; set; }
    }

    public class ProgressIndicator : Control
    {
        public ProgressIndicator(YorotApp app) : base(app)
        {
        }

        public int Progress { get; set; }
        public int MaxProgress { get; set; }
        public int MinProgress { get; set; }
        public ProgressType Type { get; set; }
    }

    public class CheckBox : Text
    {
        public CheckBox(YorotApp app) : base(app)
        {
        }

        public bool Checked { get; set; }
    }

    public class SelectionBox : Text
    {
        public SelectionBox(YorotApp app) : base(app)
        {
        }

        public string[] Selections { get; set; }
    }

    public class TrackBar : Control
    {
        public TrackBar(YorotApp app) : base(app)
        {
        }

        public int TrackPosition { get; set; }
        public int MaxPosition { get; set; }
        public int MinPosition { get; set; }
        public bool Vertical { get; set; }
        public bool Reverse { get; set; }
    }

    #endregion Controls

    #region Timer

    public class Timer : Control
    {
        public Timer(YorotApp app) : base(app)
        {
        }

        private bool isRunning = false;

        private async void RunTask()
        {
            await Task.Run(() =>
            {
                if (isEnabled) { return; }
                System.Threading.Thread.Sleep(Interval);
                OnTick(this, new EventArgs());
                RunTask();
            });
        }

        public int Interval { get; set; }

        private bool isEnabled = false;

        public new bool Enabled
        {
            get
            {
                return isEnabled;
            }
            set
            {
                if (value && !isRunning)
                {
                    isRunning = true;
                    isEnabled = true;
                    RunTask();
                }
                else
                {
                    isRunning = false;
                    isEnabled = false;
                }
            }
        }

        public virtual void OnTick(object sender, EventArgs e)
        { }
    }

    #endregion Timer

    #region Containers

    public class Container : Control
    {
        public Container(YorotApp app) : base(app)
        {
        }

        public List<Control> Controls { get; set; }
    }

    public class StackContainer : Container
    {
        public StackContainer(YorotApp app) : base(app)
        {
        }
    }

    public class Tabs : Control
    {
        public Tabs(YorotApp app) : base(app)
        {
        }

        public List<TabPage> Pages { get; set; }
        public TabOrientation TabOrientation { get; set; }
    }

    public class TabPage : Container
    {
        public TabPage(YorotApp app) : base(app)
        {
        }

        public string Title { get; set; }
    }

    public abstract class Form : TabPage
    {
        protected Form(YorotApp app) : base(app)
        {
        }

        public abstract void Initialize(string[] args);
    }

    public class Menu : Control
    {
        public Menu(YorotApp app) : base(app)
        {
        }

        public List<MenuItem> Items { get; set; }
    }

    public class MenuItem : Control
    {
        public MenuItem(YorotApp app) : base(app)
        {
        }

        public List<MenuItem> Items { get; set; }
    }

    #endregion Containers

    #region Dialogs

    public class FileDialog : Control
    {
        public FileDialog(YorotApp app) : base(app)
        {
        }

        public DialogType Type { get; set; }
        public string Path { get; set; }

        public delegate void OnDialogShown(object sender, DialogResult<string> e);
    }

    public class DialogResult<T> : EventArgs
    {
        public T Selection { get; }
        public bool Success { get; set; }
    }

    public class CalendarDialog : Control
    {
        public CalendarDialog(YorotApp app) : base(app)
        {
        }

        public delegate void OnDialogShown(object sender, DialogResult<DateTime> e);
    }

    public class MessageDialog : Control
    {
        public MessageDialog(YorotApp app) : base(app)
        {
        }

        public string[] Buttons { get; set; }

        public delegate void OnDialogShown(object sender, DialogResult<int> e);
    }

    public class InputDialog : Control
    {
        public InputDialog(YorotApp app) : base(app)
        {
        }

        public string DefaultText { get; set; }
        public string[] Buttons { get; set; }

        public class InputDialogResult
        {
            public int Button { get; set; }
            public string Input { get; set; }
        }

        public delegate void OnDialogShown(object sender, DialogResult<InputDialogResult> e);
    }

    #endregion Dialogs
}