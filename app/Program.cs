using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;

public class Program : Form
{
    // Required Mechanics Constants (must match app/mechanics.json exactly)
    public const int REMOTE_PORT = 18080;
    public const string REMOTE_DEFAULT_IP = "192.168.43.1";
    public const int DICE_FACES = 6;
    public const int CURSED_RNG = 0;
    public const int MILD_HUMAN_RNG = 25;
    public const int ROBOTS_RNG = 50;
    public const int LUCKY_HUMAN_RNG = 75;
    public const int GOD_MODE_RNG = 100;
    public const string LOGIN_KEY = "AKIB-06T9";
    public const string BRAND_TITLE = "LUDO CONTROLLER ULTRA";
    public const string BRAND_SUBTITLE = "⚡ Neural Engine v5.0 • Platinum Edition";
    public const string VERSION_TAG = "✦ v5.0";
    public const string LICENSE_STATUS = "✓ ACTIVE • LIFETIME UNLIMITED";

    // Application Interactive State
    private int selectedColor = 0; // 0=Blue, 1=Red, 2=Green, 3=Yellow
    private int selectedTeam = 0;  // 0=Team Blue/Green, 1=Team Red/Yellow
    private int selectedProfile = 2; // 0=Cursed, 1=Mild, 2=Robots(50%), 3=Lucky, 4=God
    private int lockedDice = 0;    // 0=Auto, 1..6 = Locked
    private string lastDice = "-";
    private bool isConnected = true;
    private string roomPin = "849201";
    private string currentKey = LOGIN_KEY;
    private bool isActivated = true;

    // View mode: 0=Dashboard, 1=Activation, 2=ConnectionManager, 3=LiveBoard, 4=Entitlements
    private int currentView = 0;

    public Program()
    {
        this.Text = "Ludo Controller Ultra v5.0";
        this.DoubleBuffered = true;
        this.ClientSize = new Size(710, 1601);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = ColorTranslator.FromHtml("#050912");
        this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        int x = e.X;
        int y = e.Y;

        // Header Buttons
        if (y >= 105 && y <= 147)
        {
            if (x >= 430 && x <= 495) { currentView = (currentView == 4) ? 0 : 4; Invalidate(); return; } // MENU
            if (x >= 505 && x <= 620) { isConnected = !isConnected; Invalidate(); return; } // Connected
            if (x >= 630 && x <= 685) { currentView = (currentView == 2) ? 0 : 2; Invalidate(); return; } // SET
        }

        // Top Banner Buttons
        if (y >= 295 && y <= 331)
        {
            if (x >= 200 && x <= 300) { currentView = (currentView == 3) ? 0 : 3; Invalidate(); return; } // LIVE
            if (x >= 315 && x <= 405) { currentView = (currentView == 1) ? 0 : 1; Invalidate(); return; } // KEY
        }

        // Target Colors (Y=485..560)
        if (y >= 485 && y <= 560)
        {
            if (x >= 155 && x <= 225) { selectedColor = 0; Invalidate(); return; }
            if (x >= 270 && x <= 340) { selectedColor = 1; Invalidate(); return; }
            if (x >= 380 && x <= 450) { selectedColor = 2; Invalidate(); return; }
            if (x >= 485 && x <= 555) { selectedColor = 3; Invalidate(); return; }
        }

        // Teams (Y=610..705)
        if (y >= 610 && y <= 705)
        {
            if (x >= 40 && x <= 345) { selectedTeam = 0; Invalidate(); return; }
            if (x >= 365 && x <= 670) { selectedTeam = 1; Invalidate(); return; }
        }

        // RNG Profile Buttons (Y=795..835)
        if (y >= 795 && y <= 835)
        {
            if (x >= 40 && x <= 150) { selectedProfile = 0; Invalidate(); return; }
            if (x >= 160 && x <= 280) { selectedProfile = 1; Invalidate(); return; }
            if (x >= 290 && x <= 410) { selectedProfile = 2; Invalidate(); return; }
            if (x >= 420 && x <= 540) { selectedProfile = 3; Invalidate(); return; }
            if (x >= 550 && x <= 670) { selectedProfile = 4; Invalidate(); return; }
        }

        // Dice Pad: Auto RNG (X=510..660, Y=1155..1195)
        if (x >= 510 && x <= 660 && y >= 1155 && y <= 1195)
        {
            lockedDice = 0;
            Invalidate();
            return;
        }

        // Dice Buttons (Row 1: Y=1220..1310, Row 2: Y=1330..1420, Row 3: Y=1440..1530)
        if (y >= 1220 && y <= 1310)
        {
            if (x >= 40 && x <= 345) { lockedDice = 1; lastDice = "1"; Invalidate(); return; }
            if (x >= 365 && x <= 670) { lockedDice = 2; lastDice = "2"; Invalidate(); return; }
        }
        else if (y >= 1330 && y <= 1420)
        {
            if (x >= 40 && x <= 345) { lockedDice = 3; lastDice = "3"; Invalidate(); return; }
            if (x >= 365 && x <= 670) { lockedDice = 4; lastDice = "4"; Invalidate(); return; }
        }
        else if (y >= 1440 && y <= 1530)
        {
            if (x >= 40 && x <= 345) { lockedDice = 5; lastDice = "5"; Invalidate(); return; }
            if (x >= 365 && x <= 670) { lockedDice = 6; lastDice = "6"; Invalidate(); return; }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Render Dashboard UI
        RenderDashboard(g);

        // Render Modals if opened
        if (currentView == 1) RenderActivationModal(g);
        else if (currentView == 2) RenderConnectionModal(g);
        else if (currentView == 3) RenderLiveBoardModal(g);
        else if (currentView == 4) RenderEntitlementsModal(g);
    }

    private void RenderDashboard(Graphics g)
    {
        // 1. Android Status Bar (0..55)
        using (SolidBrush sb = new SolidBrush(ColorTranslator.FromHtml("#1B1B1B")))
        {
            g.FillRectangle(sb, 0, 0, 710, 55);
        }
        using (Font fTime = new Font("Segoe UI", 13, FontStyle.Bold))
        using (SolidBrush sbTime = new SolidBrush(ColorTranslator.FromHtml("#E2E8F0")))
        {
            g.DrawString("12:00", fTime, sbTime, 35, 16);
            g.DrawString("📶 5G  🔋 100%", new Font("Segoe UI", 11), sbTime, 560, 18);
        }

        // 2. App Header (55..185)
        DrawAppLogo(g, 30, 85, 65, 65);

        // Title and Subtitle
        using (Font fTitle = new Font("Segoe UI", 16, FontStyle.Bold))
        using (Font fSub = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbWhite = new SolidBrush(Color.White))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#38BDF8")))
        {
            g.DrawString(BRAND_TITLE, fTitle, sbWhite, 110, 92);
            g.DrawString(BRAND_SUBTITLE, fSub, sbCyan, 110, 134);
        }

        // Header Action Buttons
        DrawRoundedPill(g, 430, 105, 65, 40, 8, "#092433", "#1F7AA8", "MENU", "#9DE7FF", 10, FontStyle.Bold);
        string connText = isConnected ? "Connected" : "Offline";
        string connBg = isConnected ? "#09251A" : "#2A1215";
        string connBorder = isConnected ? "#137A4A" : "#B53B45";
        string connFg = isConnected ? "#85EFAC" : "#FFB6B6";
        DrawRoundedPill(g, 505, 105, 115, 40, 8, connBg, connBorder, connText, connFg, 10, FontStyle.Bold);
        DrawRoundedPill(g, 630, 105, 55, 40, 8, "#0C2D48", "#0284C7", "SET", "#38BDF8", 10, FontStyle.Bold);

        // 3. Top Banner / Status Card (Y=195..415)
        DrawCard(g, 20, 195, 670, 220, 16, "#0B1D38", "#0A2342", "#071528", "#0EA5E9", 2);
        
        // Version badge & headline
        using (Font fVer = new Font("Segoe UI", 12, FontStyle.Bold))
        using (Font fHead = new Font("Segoe UI", 10, FontStyle.Bold))
        using (SolidBrush sbYellow = new SolidBrush(ColorTranslator.FromHtml("#FDE047")))
        using (SolidBrush sbLightBlue = new SolidBrush(ColorTranslator.FromHtml("#BAE6FD")))
        {
            g.DrawString(VERSION_TAG, fVer, sbYellow, 45, 218);
            g.DrawString("CONTROLLER 5.0 • NEURAL TACTICS • ANTI-JURA • 0-MS SYNC", fHead, sbLightBlue, 45, 255);
        }

        // Action pills
        DrawRoundedPill(g, 45, 295, 140, 36, 8, "#091013", "#17272B", "SYNC LOCKED", "#556B70", 9, FontStyle.Bold);
        DrawRoundedPill(g, 195, 295, 105, 36, 8, "#0C2D48", "#0284C7", "🎮 LIVE", "#38BDF8", 10, FontStyle.Bold);
        DrawRoundedPill(g, 310, 295, 95, 36, 8, "#171F2C", "#B45309", "KEY", "#FBBF24", 10, FontStyle.Bold);

        // Card divider
        using (Pen pLine = new Pen(ColorTranslator.FromHtml("#1E2C45"), 1))
        {
            g.DrawLine(pLine, 35, 350, 675, 350);
        }

        // Stats row
        using (Font fStatLabel = new Font("Segoe UI", 9, FontStyle.Bold))
        using (Font fStatVal = new Font("Segoe UI", 11, FontStyle.Bold))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        using (SolidBrush sbWhite = new SolidBrush(Color.White))
        using (SolidBrush sbGold = new SolidBrush(ColorTranslator.FromHtml("#F9D36B")))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#8BD3FF")))
        {
            // STATE: MENU
            g.DrawString("STATE", fStatLabel, sbGray, 45, 360);
            g.DrawString("MENU", fStatVal, sbWhite, 45, 380);

            // TURN: -
            g.DrawString("TURN", fStatLabel, sbGray, 210, 360);
            g.DrawString("-", fStatVal, sbWhite, 210, 380);

            // LAST: -
            g.DrawString("LAST", fStatLabel, sbGray, 380, 360);
            g.DrawString(lastDice, fStatVal, sbGold, 380, 380);

            // LOCK: AUTO / 1..6
            g.DrawString("LOCK", fStatLabel, sbGray, 535, 360);
            string lockStr = (lockedDice == 0) ? "AUTO" : lockedDice.ToString();
            g.DrawString(lockStr, fStatVal, sbCyan, 535, 380);
        }

        // 4. Target Player & Team Selection Card (Y=430..730)
        DrawCard(g, 20, 430, 670, 300, 16, "#0C1629", "#0A1220", "#080F1D", "#1E3150", 2);
        using (Font fHeader = new Font("Segoe UI", 11, FontStyle.Bold))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        {
            g.DrawString("TARGET PLAYER:", fHeader, sbGray, 45, 450);
        }

        // 4 Color Circles
        DrawColorCircle(g, 190, 515, 35, "#2464E8", selectedColor == 0);
        DrawColorCircle(g, 305, 515, 32, "#DD2724", selectedColor == 1);
        DrawColorCircle(g, 415, 515, 32, "#17A34A", selectedColor == 2);
        DrawColorCircle(g, 520, 515, 32, "#C98B04", selectedColor == 3);

        using (Font fColorLabel = new Font("Segoe UI", 9, FontStyle.Bold))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        using (SolidBrush sbActive = new SolidBrush(Color.White))
        {
            g.DrawString("BLUE", fColorLabel, (selectedColor == 0) ? sbActive : sbGray, 175, 560);
            g.DrawString("RED", fColorLabel, (selectedColor == 1) ? sbActive : sbGray, 295, 560);
            g.DrawString("GREEN", fColorLabel, (selectedColor == 2) ? sbActive : sbGray, 395, 560);
            g.DrawString("YELLOW", fColorLabel, (selectedColor == 3) ? sbActive : sbGray, 495, 560);
        }

        // 2v2 Teams Selection (Y=605..705)
        DrawTeamBox(g, 40, 605, 305, 95, selectedTeam == 0,
            "🛡️ TEAM BLUE & GREEN", "🔵 Blue  •  🟢 Green", "Teammates (Seats 0 & 2)",
            "#091D30", "#1E3A5F", "#38BDF8", "#7DD3FC");

        DrawTeamBox(g, 365, 605, 305, 95, selectedTeam == 1,
            "⚔️ TEAM RED & YELLOW", "🔴 Red  •  🟡 Yellow", "Teammates (Seats 1 & 3)",
            "#2A1215", "#4A1D24", "#FB7185", "#FCA5A5");

        // 5. Tactics & RNG Profile Card (Y=745..1125)
        DrawCard(g, 20, 745, 670, 375, 16, "#0C1528", "#0A1020", "#09101F", "#1E293B", 2);
        using (Font fHeader = new Font("Segoe UI", 11, FontStyle.Bold))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        {
            g.DrawString("SMART MODE & TACTICS:", fHeader, sbGray, 45, 765);
        }

        // 5 Profile Pills (Cursed 0%, Mild 25%, Robots 50%, Lucky 75%, God Mode 100%)
        string[] profileNames = new string[] { "💀 Cursed", "🌤️ Mild", "🤖 Robots", "🎯 Lucky", "⚡ God Mode" };
        int[] profileRng = new int[] { CURSED_RNG, MILD_HUMAN_RNG, ROBOTS_RNG, LUCKY_HUMAN_RNG, GOD_MODE_RNG };
        int pillW = 118;
        int pillH = 38;
        int pillGap = 10;
        int pillStartX = 40;
        for (int i = 0; i < 5; i++)
        {
            int px = pillStartX + i * (pillW + pillGap);
            bool isSel = (selectedProfile == i);
            string pBg = isSel ? "#0E2A4A" : "#0A1220";
            string pBorder = isSel ? "#38BDF8" : "#1E293B";
            string pFg = isSel ? "#38BDF8" : "#94A3B8";
            DrawRoundedPill(g, px, 800, pillW, pillH, 8, pBg, pBorder, profileNames[i] + " " + profileRng[i] + "%", pFg, 9, isSel ? FontStyle.Bold : FontStyle.Regular);
        }

        // Highlight Active Profile Card
        DrawCard(g, 40, 855, 630, 160, 12, "#0E2238", "#0C1D30", "#0A1828", "#38BDF8", 2);
        using (Font fBigName = new Font("Segoe UI", 16, FontStyle.Bold))
        using (Font fBigPct = new Font("Segoe UI", 20, FontStyle.Bold))
        using (Font fDesc = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#38BDF8")))
        using (SolidBrush sbLight = new SolidBrush(ColorTranslator.FromHtml("#BAE6FD")))
        using (SolidBrush sbDesc = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        {
            string curName = "🤖 ROBOTS";
            string curDesc = "Standard fair simulation • Neutral 50% machine RNG";
            int curRng = ROBOTS_RNG;
            if (selectedProfile == 0) { curName = "💀 CURSED"; curDesc = "Minimum rolls on danger tiles • Forced captures against user"; curRng = CURSED_RNG; }
            else if (selectedProfile == 1) { curName = "🌤️ MILD HUMAN"; curDesc = "Organic gameplay simulation • Light assistance on escapes"; curRng = MILD_HUMAN_RNG; }
            else if (selectedProfile == 2) { curName = "🤖 ROBOTS"; curDesc = "Standard fair simulation • Neutral 50% machine RNG"; curRng = ROBOTS_RNG; }
            else if (selectedProfile == 3) { curName = "🎯 LUCKY HUMAN"; curDesc = "High 6 probability • Optimal dice distribution on home stretch"; curRng = LUCKY_HUMAN_RNG; }
            else if (selectedProfile == 4) { curName = "⚡ GOD MODE"; curDesc = "100% deterministic rolls • Absolute board dominance"; curRng = GOD_MODE_RNG; }

            g.DrawString(curName, fBigName, sbLight, 65, 875);
            g.DrawString(curRng + "%", fBigPct, sbCyan, 550, 870);
            g.DrawString(curDesc, fDesc, sbDesc, 65, 920);

            // Progress bar
            using (SolidBrush sbBarBg = new SolidBrush(ColorTranslator.FromHtml("#0B1323")))
            {
                g.FillRectangle(sbBarBg, 65, 960, 570, 12);
            }
            int fillW = (570 * curRng) / 100;
            if (fillW > 0)
            {
                using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(65, 960, fillW, 12), ColorTranslator.FromHtml("#0284C7"), ColorTranslator.FromHtml("#38BDF8"), LinearGradientMode.Horizontal))
                {
                    g.FillRectangle(lgb, 65, 960, fillW, 12);
                }
            }
        }

        // Tactical Status Footer
        using (Font fStatus = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbStatus = new SolidBrush(ColorTranslator.FromHtml("#64748B")))
        {
            g.DrawString("● Controller v5.0 • Neural Tactical Engine Active", fStatus, sbStatus, 45, 1070);
        }

        // 6. 1-Shot Dice Pad Card (Y=1140..1585)
        DrawCard(g, 20, 1140, 670, 435, 16, "#0E1A30", "#0B1528", "#080F1D", "#2B4875", 2);

        // Header: Title and AUTO RNG
        using (Font fPadTitle = new Font("Segoe UI", 13, FontStyle.Bold))
        using (SolidBrush sbPad = new SolidBrush(ColorTranslator.FromHtml("#E2E8F0")))
        {
            g.DrawString("🎲 1-SHOT DICE PAD • v5.0", fPadTitle, sbPad, 45, 1165);
        }
        DrawRoundedPill(g, 510, 1155, 155, 40, 8, "#0C243C", "#0284C7", "↺ AUTO RNG", "#7DD3FC", 11, FontStyle.Bold);

        // 6 Dice Buttons in 3 rows × 2 columns
        int btnW = 305;
        int btnH = 92;
        int leftColX = 40;
        int rightColX = 365;
        int row1Y = 1220;
        int row2Y = 1330;
        int row3Y = 1440;

        DrawDiceButton(g, leftColX, row1Y, btnW, btnH, 1, lockedDice == 1);
        DrawDiceButton(g, rightColX, row1Y, btnW, btnH, 2, lockedDice == 2);
        DrawDiceButton(g, leftColX, row2Y, btnW, btnH, 3, lockedDice == 3);
        DrawDiceButton(g, rightColX, row2Y, btnW, btnH, 4, lockedDice == 4);
        DrawDiceButton(g, leftColX, row3Y, btnW, btnH, 5, lockedDice == 5);
        DrawDiceButton(g, rightColX, row3Y, btnW, btnH, 6, lockedDice == 6);
    }

    private void DrawCard(Graphics g, int x, int y, int w, int h, int radius, string topHex, string midHex, string botHex, string borderHex, int borderWidth)
    {
        using (GraphicsPath path = GetRoundedRect(new Rectangle(x, y, w, h), radius))
        {
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(x, y, w, h), ColorTranslator.FromHtml(topHex), ColorTranslator.FromHtml(botHex), LinearGradientMode.Vertical))
            {
                ColorBlend blend = new ColorBlend(3);
                blend.Colors = new Color[] { ColorTranslator.FromHtml(topHex), ColorTranslator.FromHtml(midHex), ColorTranslator.FromHtml(botHex) };
                blend.Positions = new float[] { 0.0f, 0.5f, 1.0f };
                lgb.InterpolationColors = blend;
                g.FillPath(lgb, path);
            }
            using (Pen p = new Pen(ColorTranslator.FromHtml(borderHex), borderWidth))
            {
                g.DrawPath(p, path);
            }
        }
    }

    private void DrawRoundedPill(Graphics g, int x, int y, int w, int h, int radius, string bgHex, string borderHex, string text, string fgHex, float fontSize, FontStyle style)
    {
        using (GraphicsPath path = GetRoundedRect(new Rectangle(x, y, w, h), radius))
        {
            using (SolidBrush sbBg = new SolidBrush(ColorTranslator.FromHtml(bgHex)))
            {
                g.FillPath(sbBg, path);
            }
            if (!string.IsNullOrEmpty(borderHex))
            {
                using (Pen p = new Pen(ColorTranslator.FromHtml(borderHex), 1.5f))
                {
                    g.DrawPath(p, path);
                }
            }
            using (Font f = new Font("Segoe UI", fontSize, style))
            using (SolidBrush sbFg = new SolidBrush(ColorTranslator.FromHtml(fgHex)))
            {
                SizeF size = g.MeasureString(text, f);
                float tx = x + (w - size.Width) / 2.0f;
                float ty = y + (h - size.Height) / 2.0f;
                g.DrawString(text, f, sbFg, tx, ty);
            }
        }
    }

    private void DrawAppLogo(Graphics g, int x, int y, int w, int h)
    {
        string iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "icon.png");
        if (!File.Exists(iconPath))
        {
            iconPath = "icon.png";
        }
        if (File.Exists(iconPath))
        {
            try
            {
                using (Image icon = Image.FromFile(iconPath))
                {
                    using (GraphicsPath path = GetRoundedRect(new Rectangle(x, y, w, h), 12))
                    {
                        GraphicsState state = g.Save();
                        g.SetClip(path);
                        g.DrawImage(icon, x, y, w, h);
                        g.Restore(state);

                        using (Pen p = new Pen(ColorTranslator.FromHtml("#FDE68A"), 2))
                        {
                            g.DrawPath(p, path);
                        }
                    }
                    return;
                }
            }
            catch {}
        }

        // Stylized Controller/Crown Emblem Fallback
        using (GraphicsPath path = GetRoundedRect(new Rectangle(x, y, w, h), 14))
        {
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(x, y, w, h), ColorTranslator.FromHtml("#F59E0B"), ColorTranslator.FromHtml("#D97706"), LinearGradientMode.Vertical))
            {
                g.FillPath(lgb, path);
            }
            using (Pen p = new Pen(ColorTranslator.FromHtml("#FDE68A"), 2))
            {
                g.DrawPath(p, path);
            }
        }
        using (Font f = new Font("Segoe UI Emoji", 24, FontStyle.Bold))
        using (SolidBrush sb = new SolidBrush(Color.White))
        {
            g.DrawString("👑", f, sb, x + 8, y + 8);
        }
    }

    private void DrawColorCircle(Graphics g, int cx, int cy, int radius, string colorHex, bool isSelected)
    {
        int d = radius * 2;
        Rectangle rect = new Rectangle(cx - radius, cy - radius, d, d);
        if (isSelected)
        {
            // Outer glowing ring
            using (Pen pGlow = new Pen(ColorTranslator.FromHtml("#38BDF8"), 4))
            {
                g.DrawEllipse(pGlow, cx - radius - 6, cy - radius - 6, d + 12, d + 12);
            }
            using (Pen pInner = new Pen(Color.White, 2))
            {
                g.DrawEllipse(pInner, cx - radius - 2, cy - radius - 2, d + 4, d + 4);
            }
        }
        using (SolidBrush sb = new SolidBrush(ColorTranslator.FromHtml(colorHex)))
        {
            g.FillEllipse(sb, rect);
        }
        // Inner shadow/gradient shine
        using (Pen p = new Pen(Color.FromArgb(80, Color.White), 2))
        {
            g.DrawEllipse(p, rect);
        }
    }

    private void DrawTeamBox(Graphics g, int x, int y, int w, int h, bool isSelected, string title, string players, string seats, string bgHex, string borderHex, string titleHex, string subHex)
    {
        using (GraphicsPath path = GetRoundedRect(new Rectangle(x, y, w, h), 10))
        {
            using (SolidBrush sbBg = new SolidBrush(ColorTranslator.FromHtml(bgHex)))
            {
                g.FillPath(sbBg, path);
            }
            using (Pen pBorder = new Pen(isSelected ? Color.White : ColorTranslator.FromHtml(borderHex), isSelected ? 2.5f : 1.5f))
            {
                g.DrawPath(pBorder, path);
            }
            using (Font fTitle = new Font("Segoe UI", 10, FontStyle.Bold))
            using (Font fPlayers = new Font("Segoe UI", 10, FontStyle.Regular))
            using (Font fSeats = new Font("Segoe UI", 9, FontStyle.Regular))
            using (SolidBrush sbTitle = new SolidBrush(ColorTranslator.FromHtml(titleHex)))
            using (SolidBrush sbPlayers = new SolidBrush(Color.White))
            using (SolidBrush sbSeats = new SolidBrush(ColorTranslator.FromHtml(subHex)))
            {
                g.DrawString(title, fTitle, sbTitle, x + 14, y + 12);
                g.DrawString(players, fPlayers, sbPlayers, x + 14, y + 40);
                g.DrawString(seats, fSeats, sbSeats, x + 14, y + 66);
            }
        }
    }

    private void DrawDiceButton(Graphics g, int x, int y, int w, int h, int num, bool isLocked)
    {
        string bgHex = isLocked ? "#152E52" : "#0B1425";
        string borderHex = isLocked ? "#38BDF8" : "#1E3150";
        float borderWidth = isLocked ? 2.5f : 1.5f;

        using (GraphicsPath path = GetRoundedRect(new Rectangle(x, y, w, h), 12))
        {
            using (SolidBrush sbBg = new SolidBrush(ColorTranslator.FromHtml(bgHex)))
            {
                g.FillPath(sbBg, path);
            }
            using (Pen p = new Pen(ColorTranslator.FromHtml(borderHex), borderWidth))
            {
                g.DrawPath(p, path);
            }

            // Dice number display
            using (Font fNum = new Font("Segoe UI", 30, FontStyle.Bold))
            using (Font fSub = new Font("Segoe UI", 10, FontStyle.Bold))
            using (SolidBrush sbNum = new SolidBrush(isLocked ? ColorTranslator.FromHtml("#38BDF8") : Color.White))
            using (SolidBrush sbSub = new SolidBrush(isLocked ? ColorTranslator.FromHtml("#8BD3FF") : ColorTranslator.FromHtml("#94A3B8")))
            {
                g.DrawString(num.ToString(), fNum, sbNum, x + 35, y + 18);
                string rollLabel = isLocked ? "LOCKED " + num : "ROLL " + num;
                g.DrawString(rollLabel, fSub, sbSub, x + 115, y + 28);
                g.DrawString("1-SHOT INSTANT", new Font("Segoe UI", 9), new SolidBrush(ColorTranslator.FromHtml("#64748B")), x + 115, y + 50);
            }

            // Small dice pip preview
            DrawMiniDicePip(g, x + w - 55, y + 28, 35, num, isLocked);
        }
    }

    private void DrawMiniDicePip(Graphics g, int px, int py, int size, int num, bool isLocked)
    {
        using (GraphicsPath path = GetRoundedRect(new Rectangle(px, py, size, size), 6))
        {
            using (SolidBrush sb = new SolidBrush(ColorTranslator.FromHtml(isLocked ? "#0284C7" : "#1E293B")))
            {
                g.FillPath(sb, path);
            }
            using (Pen p = new Pen(ColorTranslator.FromHtml(isLocked ? "#38BDF8" : "#475569"), 1))
            {
                g.DrawPath(p, path);
            }
        }
        using (SolidBrush sbDot = new SolidBrush(isLocked ? Color.White : ColorTranslator.FromHtml("#CBD5E1")))
        {
            int rad = 3;
            int midX = px + size / 2;
            int midY = py + size / 2;
            int leftX = px + 9;
            int rightX = px + size - 9;
            int topY = py + 9;
            int botY = py + size - 9;

            if (num == 1 || num == 3 || num == 5) g.FillEllipse(sbDot, midX - rad, midY - rad, rad * 2, rad * 2);
            if (num >= 2)
            {
                g.FillEllipse(sbDot, leftX - rad, topY - rad, rad * 2, rad * 2);
                g.FillEllipse(sbDot, rightX - rad, botY - rad, rad * 2, rad * 2);
            }
            if (num >= 4)
            {
                g.FillEllipse(sbDot, rightX - rad, topY - rad, rad * 2, rad * 2);
                g.FillEllipse(sbDot, leftX - rad, botY - rad, rad * 2, rad * 2);
            }
            if (num == 6)
            {
                g.FillEllipse(sbDot, leftX - rad, midY - rad, rad * 2, rad * 2);
                g.FillEllipse(sbDot, rightX - rad, midY - rad, rad * 2, rad * 2);
            }
        }
    }

    private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
    {
        int diameter = radius * 2;
        Size size = new Size(diameter, diameter);
        Rectangle arc = new Rectangle(bounds.Location, size);
        GraphicsPath path = new GraphicsPath();

        if (radius == 0)
        {
            path.AddRectangle(bounds);
            return path;
        }

        // top left arc  
        path.AddArc(arc, 180, 90);

        // top right arc  
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);

        // bottom right arc  
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);

        // bottom left arc 
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);

        path.CloseFigure();
        return path;
    }

    // Modal Dialogs
    private void RenderActivationModal(Graphics g)
    {
        using (SolidBrush sbDim = new SolidBrush(Color.FromArgb(220, 5, 9, 18)))
        {
            g.FillRectangle(sbDim, 0, 0, 710, 1601);
        }
        DrawCard(g, 40, 300, 630, 800, 20, "#0C162D", "#0A1225", "#060A14", "#38BDF8", 2);

        using (Font fHeader = new Font("Segoe UI", 18, FontStyle.Bold))
        using (Font fSub = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbWhite = new SolidBrush(Color.White))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#38BDF8")))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        {
            g.DrawString("🔐 VIP ACTIVATION KEY", fHeader, sbWhite, 65, 340);
            g.DrawString("256-Bit Hardware Bound ECC License", fSub, sbCyan, 65, 385);

            g.DrawString("STEP 1: YOUR DEVICE HARDWARE ID", new Font("Segoe UI", 10, FontStyle.Bold), sbGray, 65, 440);
            DrawCard(g, 65, 470, 580, 60, 8, "#0B1528", "#080F1D", "#080F1D", "#1E3150", 1);
            g.DrawString("AKIB-DEVICE-8F3E92B4-PLATINUM", new Font("Consolas", 12, FontStyle.Bold), sbCyan, 80, 488);
            DrawRoundedPill(g, 480, 480, 150, 40, 6, "#0C2D48", "#0284C7", "📋 Copy ID", "#38BDF8", 10, FontStyle.Bold);

            g.DrawString("STEP 2: ENTER ACTIVATION CODE", new Font("Segoe UI", 10, FontStyle.Bold), sbGray, 65, 560);
            DrawCard(g, 65, 590, 580, 65, 8, "#0B1528", "#080F1D", "#080F1D", "#38BDF8", 2);
            g.DrawString(LOGIN_KEY, new Font("Consolas", 16, FontStyle.Bold), new SolidBrush(ColorTranslator.FromHtml("#FDE047")), 85, 608);

            DrawRoundedPill(g, 65, 680, 580, 55, 10, "#16A34A", "#22C55E", "🚀 ACTIVATE VIP KEY (" + LOGIN_KEY + ")", "#FFFFFF", 13, FontStyle.Bold);
            DrawRoundedPill(g, 65, 755, 580, 50, 10, "#0C243C", "#0284C7", "✓ ACTIVE • LIFETIME UNLIMITED", "#38BDF8", 11, FontStyle.Bold);

            g.DrawString("Telegram Support: @AkibSupport", fSub, sbGray, 65, 830);
            g.DrawString("Official VIP License authenticated for AKIB-06T9", fSub, sbCyan, 65, 860);

            DrawRoundedPill(g, 220, 920, 270, 45, 8, "#1E293B", "#475569", "← Return to Dashboard", "#E2E8F0", 11, FontStyle.Bold);
        }
    }

    private void RenderConnectionModal(Graphics g)
    {
        using (SolidBrush sbDim = new SolidBrush(Color.FromArgb(220, 5, 9, 18)))
        {
            g.FillRectangle(sbDim, 0, 0, 710, 1601);
        }
        DrawCard(g, 40, 350, 630, 700, 20, "#0C162D", "#0A1225", "#060A14", "#0EA5E9", 2);

        using (Font fHeader = new Font("Segoe UI", 18, FontStyle.Bold))
        using (Font fSub = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbWhite = new SolidBrush(Color.White))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#38BDF8")))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        {
            g.DrawString("🔌 CONNECTION MANAGER", fHeader, sbWhite, 65, 390);
            g.DrawString("Configure Remote Mode & Hotspot IP", fSub, sbCyan, 65, 435);

            g.DrawString("REMOTE HOST IP:", new Font("Segoe UI", 10, FontStyle.Bold), sbGray, 65, 490);
            DrawCard(g, 65, 515, 580, 50, 8, "#0B1528", "#080F1D", "#080F1D", "#1E3150", 1);
            g.DrawString(REMOTE_DEFAULT_IP, new Font("Consolas", 14, FontStyle.Bold), sbCyan, 85, 528);

            g.DrawString("REMOTE PORT:", new Font("Segoe UI", 10, FontStyle.Bold), sbGray, 65, 585);
            DrawCard(g, 65, 610, 580, 50, 8, "#0B1528", "#080F1D", "#080F1D", "#1E3150", 1);
            g.DrawString(REMOTE_PORT.ToString(), new Font("Consolas", 14, FontStyle.Bold), sbCyan, 85, 623);

            DrawRoundedPill(g, 65, 690, 580, 50, 10, "#0C2D48", "#0284C7", "⚡ Auto Detect Phone A IP", "#38BDF8", 12, FontStyle.Bold);
            DrawRoundedPill(g, 65, 760, 580, 50, 10, "#16A34A", "#22C55E", "● Connected to Ludo King (Room: " + roomPin + ")", "#FFFFFF", 12, FontStyle.Bold);

            DrawRoundedPill(g, 220, 850, 270, 45, 8, "#1E293B", "#475569", "← Return to Dashboard", "#E2E8F0", 11, FontStyle.Bold);
        }
    }

    private void RenderLiveBoardModal(Graphics g)
    {
        using (SolidBrush sbDim = new SolidBrush(Color.FromArgb(235, 5, 9, 18)))
        {
            g.FillRectangle(sbDim, 0, 0, 710, 1601);
        }
        DrawCard(g, 30, 200, 650, 1000, 20, "#0C162D", "#0A1225", "#060A14", "#38BDF8", 2);

        using (Font fHeader = new Font("Segoe UI", 18, FontStyle.Bold))
        using (Font fSub = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbWhite = new SolidBrush(Color.White))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#38BDF8")))
        {
            g.DrawString("☁️ LIVE LUDO BOARD", fHeader, sbWhite, 65, 230);
            g.DrawString("Real-Time Topology Sync • PIN: " + roomPin, fSub, sbCyan, 65, 275);

            // Draw Interactive 4-Color Board Representation
            int bX = 65;
            int bY = 320;
            int bSize = 580;

            // Board Outline
            using (SolidBrush sbBoard = new SolidBrush(ColorTranslator.FromHtml("#050912")))
            using (Pen pBoard = new Pen(ColorTranslator.FromHtml("#1E3150"), 3))
            {
                g.FillRectangle(sbBoard, bX, bY, bSize, bSize);
                g.DrawRectangle(pBoard, bX, bY, bSize, bSize);
            }

            // 4 Corner Yards (Green=TopLeft, Yellow=TopRight, Red=BotLeft, Blue=BotRight)
            int yardSize = 230;
            using (SolidBrush sbG = new SolidBrush(ColorTranslator.FromHtml("#17A34A")))
            using (SolidBrush sbY = new SolidBrush(ColorTranslator.FromHtml("#C98B04")))
            using (SolidBrush sbR = new SolidBrush(ColorTranslator.FromHtml("#DD2724")))
            using (SolidBrush sbB = new SolidBrush(ColorTranslator.FromHtml("#2464E8")))
            {
                g.FillRectangle(sbG, bX, bY, yardSize, yardSize);
                g.FillRectangle(sbY, bX + bSize - yardSize, bY, yardSize, yardSize);
                g.FillRectangle(sbR, bX, bY + bSize - yardSize, yardSize, yardSize);
                g.FillRectangle(sbB, bX + bSize - yardSize, bY + bSize - yardSize, yardSize, yardSize);
            }

            // Center Triangle Home
            using (SolidBrush sbHome = new SolidBrush(ColorTranslator.FromHtml("#0B1528")))
            {
                g.FillRectangle(sbHome, bX + yardSize, bY + yardSize, bSize - 2 * yardSize, bSize - 2 * yardSize);
            }

            // Return Button
            DrawRoundedPill(g, 220, 980, 270, 45, 8, "#1E293B", "#475569", "← Return to Dashboard", "#E2E8F0", 11, FontStyle.Bold);
        }
    }

    private void RenderEntitlementsModal(Graphics g)
    {
        using (SolidBrush sbDim = new SolidBrush(Color.FromArgb(220, 5, 9, 18)))
        {
            g.FillRectangle(sbDim, 0, 0, 710, 1601);
        }
        DrawCard(g, 40, 300, 630, 820, 20, "#0C162D", "#0A1225", "#060A14", "#FBBF24", 2);

        using (Font fHeader = new Font("Segoe UI", 18, FontStyle.Bold))
        using (Font fSub = new Font("Segoe UI", 11, FontStyle.Regular))
        using (SolidBrush sbWhite = new SolidBrush(Color.White))
        using (SolidBrush sbGold = new SolidBrush(ColorTranslator.FromHtml("#FBBF24")))
        using (SolidBrush sbCyan = new SolidBrush(ColorTranslator.FromHtml("#38BDF8")))
        using (SolidBrush sbGray = new SolidBrush(ColorTranslator.FromHtml("#94A3B8")))
        {
            g.DrawString("🛡️ VIP LICENSE ENTITLEMENTS", fHeader, sbWhite, 65, 340);
            g.DrawString("Official Enterprise Tier • Key: " + LOGIN_KEY, fSub, sbGold, 65, 385);

            DrawRoundedPill(g, 65, 430, 580, 50, 10, "#052E16", "#16A34A", LICENSE_STATUS, "#86EFAC", 12, FontStyle.Bold);

            string[] features = new string[] {
                "✓ Deterministic Dice Automation (Faces 1..6)",
                "✓ Direct In-Game Cloud Sync Bridge (0-ms)",
                "✓ Smart Boost & AI Tactical Engine (God Mode 100%)",
                "✓ 2v2 Team Up Coordination & Seat Targeting",
                "✓ Real-Time Mode & Topology Detection",
                "✓ 72h Offline Lease • Auto-Reverify • 256-Bit Crypto"
            };

            for (int i = 0; i < features.Length; i++)
            {
                g.DrawString(features[i], new Font("Segoe UI", 11, FontStyle.Regular), sbCyan, 70, 510 + i * 45);
            }

            DrawRoundedPill(g, 220, 880, 270, 45, 8, "#1E293B", "#475569", "← Return to Dashboard", "#E2E8F0", 11, FontStyle.Bold);
        }
    }

    public static void Main(string[] args)
    {
        string screenshotPath = null;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--screenshot" && i + 1 < args.Length)
            {
                screenshotPath = args[i + 1];
            }
        }

        Program form = new Program();

        if (screenshotPath != null)
        {
            // Direct rendering to exact 710x1601 bitmap matching reference
            using (Bitmap bmp = new Bitmap(710, 1601))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(ColorTranslator.FromHtml("#050912"));
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    form.RenderDashboard(g);
                }
                string dir = Path.GetDirectoryName(screenshotPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                bmp.Save(screenshotPath, System.Drawing.Imaging.ImageFormat.Png);
            }
            return;
        }

        Application.Run(form);
    }
}
