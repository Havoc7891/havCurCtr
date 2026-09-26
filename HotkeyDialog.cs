// SPDX-License-Identifier: MIT

internal sealed class HotkeyDialog : Form
{
  private readonly TextBox _keyBox = new() { ReadOnly = true, ShortcutsEnabled = false, Width = 220 };
  private readonly List<(CheckBox Box, uint Modifier)> _modifiers = [];
  private readonly Label _hint = new() { AutoSize = true, MaximumSize = new Size(540, 0) };
  private readonly Func<uint, int, bool> _tryApply;
  private bool _capturing;

  public uint HotkeyModifiers
  {
    get
    {
      uint modifiers = 0;
      foreach (var (box, modifier) in _modifiers)
      {
        if (box.Checked)
        {
          modifiers |= modifier;
        }
      }
      return modifiers;
    }
  }

  public int HotkeyKey { get; private set; }

  public HotkeyDialog(uint modifiers, int key, Func<uint, int, bool> tryApply)
  {
    _tryApply = tryApply;

    HotkeyKey = key;
    Text = LocalizationManager.Get("HotkeySettingsTitle");
    FormBorderStyle = FormBorderStyle.FixedDialog;
    MaximizeBox = false;
    MinimizeBox = false;
    ShowInTaskbar = false;
    StartPosition = FormStartPosition.CenterScreen;
    AutoScaleMode = AutoScaleMode.Font;
    AutoSize = true;
    AutoSizeMode = AutoSizeMode.GrowAndShrink;
    KeyPreview = true;

    var layout = new TableLayoutPanel
    {
      AutoSize = true,
      AutoSizeMode = AutoSizeMode.GrowAndShrink,
      Dock = DockStyle.Fill,
      Padding = new Padding(12),
      ColumnCount = 3,
      RowCount = 4
    };
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
    layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

    var keyLabel = new Label
    {
      Text = LocalizationManager.Get("HotkeyKey"),
      AutoSize = true,
      Anchor = AnchorStyles.Left
    };

    _keyBox.Text = HotkeySettings.FormatKey((Keys)HotkeyKey);
    _keyBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;

    var change = new Button { Text = LocalizationManager.Get("HotkeyChange"), AutoSize = true };
    change.Click += (_, _) => BeginCapture();

    layout.Controls.Add(keyLabel, 0, 0);
    layout.Controls.Add(_keyBox, 1, 0);
    layout.Controls.Add(change, 2, 0);

    var modifierLabel = new Label
    {
      Text = LocalizationManager.Get("HotkeyModifiers"),
      AutoSize = true,
      Anchor = AnchorStyles.Left
    };

    var modifierPanel = new FlowLayoutPanel
    {
      AutoSize = true,
      WrapContents = false,
      Margin = new Padding(0, 6, 0, 6)
    };

    AddModifier(modifierPanel, "ModifierCtrl", HotkeySettings.Control);
    AddModifier(modifierPanel, "ModifierAlt", HotkeySettings.Alt);
    AddModifier(modifierPanel, "ModifierShift", HotkeySettings.Shift);
    AddModifier(modifierPanel, "ModifierWin", HotkeySettings.Win);

    ApplyModifiers(modifiers);

    layout.Controls.Add(modifierLabel, 0, 1);
    layout.Controls.Add(modifierPanel, 1, 1);
    layout.SetColumnSpan(modifierPanel, 2);

    SetHint("HotkeyCaptureHint");

    _hint.Margin = new Padding(3, 6, 3, 12);

    layout.Controls.Add(_hint, 0, 2);
    layout.SetColumnSpan(_hint, 3);

    var buttons = new FlowLayoutPanel
    {
      AutoSize = true,
      Dock = DockStyle.Fill,
      FlowDirection = FlowDirection.RightToLeft,
      WrapContents = false
    };

    var cancel = new Button
    {
      Text = LocalizationManager.Get("Cancel"),
      DialogResult = DialogResult.Cancel,
      AutoSize = true
    };

    var ok = new Button { Text = LocalizationManager.Get("OK"), AutoSize = true };
    ok.Click += (_, _) => ApplyShortcut();

    var reset = new Button { Text = LocalizationManager.Get("HotkeyReset"), AutoSize = true };
    reset.Click += (_, _) =>
    {
      _capturing = false;
      HotkeyKey = HotkeySettings.DefaultKey;
      _keyBox.Text = HotkeySettings.FormatKey((Keys)HotkeyKey);
      ApplyModifiers(HotkeySettings.DefaultModifiers);
      SetHint("HotkeyCaptureHint");
    };

    buttons.Controls.AddRange([cancel, ok, reset]);

    layout.Controls.Add(buttons, 0, 3);
    layout.SetColumnSpan(buttons, 3);

    Controls.Add(layout);

    AcceptButton = ok;
    CancelButton = cancel;
  }

  // Capture dialog keys before WinForms uses them for navigation or OK/Cancel
  protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
  {
    if (_capturing)
    {
      CaptureKey(keyData & Keys.KeyCode);

      return true;
    }

    return base.ProcessCmdKey(ref msg, keyData);
  }

  protected override void OnKeyDown(KeyEventArgs e)
  {
    if (_capturing)
    {
      CaptureKey(e.KeyCode);
      e.Handled = true;
      e.SuppressKeyPress = true;
    }

    base.OnKeyDown(e);
  }

  private void BeginCapture()
  {
    _capturing = true;
    _keyBox.Text = LocalizationManager.Get("HotkeyPressKey");
    SetHint("HotkeyCaptureHint");
    _keyBox.Focus();
  }

  private void CaptureKey(Keys key)
  {
    if (key is Keys.ShiftKey or Keys.ControlKey or Keys.Menu or
        Keys.LShiftKey or Keys.RShiftKey or Keys.LControlKey or Keys.RControlKey or
        Keys.LMenu or Keys.RMenu or Keys.LWin or Keys.RWin)
    {
      return;
    }

    if (!HotkeySettings.IsValid(HotkeyModifiers, (int)key))
    {
      SetHint("HotkeyInvalid", true);

      return;
    }

    HotkeyKey = (int)key;
    _keyBox.Text = HotkeySettings.FormatKey(key);
    _capturing = false;

    SetHint("HotkeyCaptureHint");
  }

  private void ApplyShortcut()
  {
    if (_capturing)
    {
      _keyBox.Focus();

      return;
    }

    if (!HotkeySettings.IsValid(HotkeyModifiers, HotkeyKey))
    {
      SetHint("HotkeyInvalid", true);

      return;
    }

    if (_tryApply(HotkeyModifiers, HotkeyKey))
    {
      DialogResult = DialogResult.OK;

      Close();
    }
  }

  private void AddModifier(FlowLayoutPanel panel, string labelKey, uint modifier)
  {
    var box = new CheckBox { Text = LocalizationManager.Get(labelKey), AutoSize = true };
    panel.Controls.Add(box);
    _modifiers.Add((box, modifier));
  }

  private void ApplyModifiers(uint modifiers)
  {
    foreach (var (box, modifier) in _modifiers)
    {
      box.Checked = (modifiers & modifier) != 0;
    }
  }

  private void SetHint(string key, bool error = false)
  {
    _hint.Text = LocalizationManager.Get(key);
    _hint.ForeColor = error ? Color.Firebrick : SystemColors.ControlText;
  }
}
