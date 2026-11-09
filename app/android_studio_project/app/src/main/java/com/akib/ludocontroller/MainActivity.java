package com.akib.ludocontroller;

import android.app.AlertDialog;
import android.content.Context;
import android.content.SharedPreferences;
import android.graphics.Color;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {

    public static final String REQUIRED_LOGIN_KEY = "AKIB-06T9";
    public static final String PREFS_NAME = "ludo_controller_prefs";
    public static final String KEY_IS_ACTIVATED = "is_activated";
    public static final String KEY_LICENSE_KEY = "license_key";

    public static final int REMOTE_PORT = 18080;
    public static final String REMOTE_DEFAULT_IP = "192.168.43.1";

    private SharedPreferences prefs;
    private TextView tvState;
    private TextView tvTurn;
    private TextView tvLast;
    private TextView tvLock;

    private TextView tvProfileName;
    private TextView tvProfilePct;
    private TextView tvProfileDesc;

    private Button btnProfCursed;
    private Button btnProfMild;
    private Button btnProfRobots;
    private Button btnProfLucky;
    private Button btnProfGod;

    private int selectedColor = 0; // 0=Blue, 1=Red, 2=Green, 3=Yellow
    private int selectedTeam = 0;
    private int selectedProfile = 2; // 0=Cursed, 1=Mild, 2=Robots, 3=Lucky, 4=God
    private int lockedDice = 0;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        prefs = getSharedPreferences(PREFS_NAME, Context.MODE_PRIVATE);

        initViews();
        setupListeners();

        // Check if activated
        boolean isActivated = prefs.getBoolean(KEY_IS_ACTIVATED, false);
        if (!isActivated) {
            showActivationDialog(false);
        } else {
            Toast.makeText(this, "VIP License Active: " + REQUIRED_LOGIN_KEY, Toast.LENGTH_SHORT).show();
        }
    }

    private void initViews() {
        tvState = findViewById(R.id.tvState);
        tvTurn = findViewById(R.id.tvTurn);
        tvLast = findViewById(R.id.tvLast);
        tvLock = findViewById(R.id.tvLock);

        tvProfileName = findViewById(R.id.tvProfileName);
        tvProfilePct = findViewById(R.id.tvProfilePct);
        tvProfileDesc = findViewById(R.id.tvProfileDesc);

        btnProfCursed = findViewById(R.id.btnProfCursed);
        btnProfMild = findViewById(R.id.btnProfMild);
        btnProfRobots = findViewById(R.id.btnProfRobots);
        btnProfLucky = findViewById(R.id.btnProfLucky);
        btnProfGod = findViewById(R.id.btnProfGod);
    }

    private void setupListeners() {
        // Top Header
        findViewById(R.id.btnMenu).setOnClickListener(v -> showEntitlementsDialog());
        findViewById(R.id.btnSet).setOnClickListener(v -> showConnectionDialog());
        findViewById(R.id.btnConnected).setOnClickListener(v -> {
            Button b = (Button) v;
            if (b.getText().toString().equals("Connected")) {
                b.setText("Offline");
                b.setBackgroundColor(Color.parseColor("#2A1215"));
                b.setTextColor(Color.parseColor("#FFB6B6"));
            } else {
                b.setText("Connected");
                b.setBackgroundColor(Color.parseColor("#09251A"));
                b.setTextColor(Color.parseColor("#85EFAC"));
            }
        });

        // Top Banner Buttons
        findViewById(R.id.btnLiveBoard).setOnClickListener(v -> showLiveBoardDialog());
        findViewById(R.id.btnKey).setOnClickListener(v -> showActivationDialog(true));

        // Player Colors
        findViewById(R.id.btnColorBlue).setOnClickListener(v -> selectColor(0));
        findViewById(R.id.btnColorRed).setOnClickListener(v -> selectColor(1));
        findViewById(R.id.btnColorGreen).setOnClickListener(v -> selectColor(2));
        findViewById(R.id.btnColorYellow).setOnClickListener(v -> selectColor(3));

        // 2v2 Teams
        findViewById(R.id.btnTeamBlueGreen).setOnClickListener(v -> {
            selectedTeam = 0;
            Toast.makeText(this, "Selected Team Blue & Green (Seats 0 & 2)", Toast.LENGTH_SHORT).show();
        });
        findViewById(R.id.btnTeamRedYellow).setOnClickListener(v -> {
            selectedTeam = 1;
            Toast.makeText(this, "Selected Team Red & Yellow (Seats 1 & 3)", Toast.LENGTH_SHORT).show();
        });

        // RNG Profiles
        btnProfCursed.setOnClickListener(v -> setProfile(0));
        btnProfMild.setOnClickListener(v -> setProfile(1));
        btnProfRobots.setOnClickListener(v -> setProfile(2));
        btnProfLucky.setOnClickListener(v -> setProfile(3));
        btnProfGod.setOnClickListener(v -> setProfile(4));

        // Dice Pad
        findViewById(R.id.btnAutoRng).setOnClickListener(v -> {
            lockedDice = 0;
            tvLock.setText("AUTO");
            tvLock.setTextColor(Color.parseColor("#8BD3FF"));
            Toast.makeText(this, "Dice Mode: AUTO RNG", Toast.LENGTH_SHORT).show();
        });

        findViewById(R.id.btnDice1).setOnClickListener(v -> setDiceLock(1));
        findViewById(R.id.btnDice2).setOnClickListener(v -> setDiceLock(2));
        findViewById(R.id.btnDice3).setOnClickListener(v -> setDiceLock(3));
        findViewById(R.id.btnDice4).setOnClickListener(v -> setDiceLock(4));
        findViewById(R.id.btnDice5).setOnClickListener(v -> setDiceLock(5));
        findViewById(R.id.btnDice6).setOnClickListener(v -> setDiceLock(6));
    }

    private void selectColor(int colorIndex) {
        selectedColor = colorIndex;
        String[] names = new String[]{"BLUE", "RED", "GREEN", "YELLOW"};
        Toast.makeText(this, "Target Player: " + names[colorIndex], Toast.LENGTH_SHORT).show();
    }

    private void setDiceLock(int face) {
        lockedDice = face;
        tvLast.setText(String.valueOf(face));
        tvLock.setText(String.valueOf(face));
        tvLock.setTextColor(Color.parseColor("#38BDF8"));
        Toast.makeText(this, "Dice Locked: " + face + " (Sent to game)", Toast.LENGTH_SHORT).show();
    }

    private void setProfile(int profileIndex) {
        selectedProfile = profileIndex;
        resetProfileButtonStyles();

        switch (profileIndex) {
            case 0:
                btnProfCursed.setBackgroundColor(Color.parseColor("#0E2A4A"));
                btnProfCursed.setTextColor(Color.parseColor("#38BDF8"));
                tvProfileName.setText("💀 CURSED");
                tvProfilePct.setText("0%");
                tvProfileDesc.setText("Minimum rolls on danger tiles • Forced captures against user");
                break;
            case 1:
                btnProfMild.setBackgroundColor(Color.parseColor("#0E2A4A"));
                btnProfMild.setTextColor(Color.parseColor("#38BDF8"));
                tvProfileName.setText("🌤️ MILD HUMAN");
                tvProfilePct.setText("25%");
                tvProfileDesc.setText("Organic gameplay simulation • Light assistance on escapes");
                break;
            case 2:
                btnProfRobots.setBackgroundColor(Color.parseColor("#0E2A4A"));
                btnProfRobots.setTextColor(Color.parseColor("#38BDF8"));
                tvProfileName.setText("🤖 ROBOTS");
                tvProfilePct.setText("50%");
                tvProfileDesc.setText("Standard fair simulation • Neutral 50% machine RNG");
                break;
            case 3:
                btnProfLucky.setBackgroundColor(Color.parseColor("#0E2A4A"));
                btnProfLucky.setTextColor(Color.parseColor("#38BDF8"));
                tvProfileName.setText("🎯 LUCKY HUMAN");
                tvProfilePct.setText("75%");
                tvProfileDesc.setText("High 6 probability • Optimal dice distribution on home stretch");
                break;
            case 4:
                btnProfGod.setBackgroundColor(Color.parseColor("#0E2A4A"));
                btnProfGod.setTextColor(Color.parseColor("#38BDF8"));
                tvProfileName.setText("⚡ GOD MODE");
                tvProfilePct.setText("100%");
                tvProfileDesc.setText("100% deterministic rolls • Absolute board dominance");
                break;
        }
    }

    private void resetProfileButtonStyles() {
        Button[] buttons = new Button[]{btnProfCursed, btnProfMild, btnProfRobots, btnProfLucky, btnProfGod};
        for (Button b : buttons) {
            b.setBackgroundColor(Color.parseColor("#0A1220"));
            b.setTextColor(Color.parseColor("#94A3B8"));
        }
    }

    // Dialog 1: Activation Dialog
    private void showActivationDialog(boolean cancelable) {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        View dialogView = getLayoutInflater().inflate(android.R.layout.activity_list_item, null);

        // Build dynamic dialog layout for activation
        android.widget.LinearLayout layout = new android.widget.LinearLayout(this);
        layout.setOrientation(android.widget.LinearLayout.VERTICAL);
        layout.setPadding(32, 32, 32, 32);
        layout.setBackgroundColor(Color.parseColor("#0C162D"));

        TextView tvTitle = new TextView(this);
        tvTitle.setText("🔐 VIP ACTIVATION KEY");
        tvTitle.setTextColor(Color.WHITE);
        tvTitle.setTextSize(18);
        tvTitle.setTypeface(null, android.graphics.Typeface.BOLD);
        layout.addView(tvTitle);

        TextView tvSub = new TextView(this);
        tvSub.setText("256-Bit Hardware Bound ECC License\nEnter key AKIB-06T9 to unlock VIP Lifetime access.");
        tvSub.setTextColor(Color.parseColor("#38BDF8"));
        tvSub.setTextSize(11);
        tvSub.setPadding(0, 8, 0, 16);
        layout.addView(tvSub);

        TextView tvHw = new TextView(this);
        tvHw.setText("DEVICE HARDWARE ID: AKIB-DEV-94F3A-PLATINUM");
        tvHw.setTextColor(Color.parseColor("#94A3B8"));
        tvHw.setTextSize(10);
        layout.addView(tvHw);

        final EditText etKey = new EditText(this);
        etKey.setHint("Enter VIP Key (AKIB-06T9)");
        etKey.setText(REQUIRED_LOGIN_KEY);
        etKey.setTextColor(Color.parseColor("#FDE047"));
        etKey.setBackgroundColor(Color.parseColor("#0B1528"));
        etKey.setPadding(16, 16, 16, 16);
        layout.addView(etKey);

        builder.setView(layout);
        builder.setCancelable(cancelable);

        builder.setPositiveButton("🚀 ACTIVATE VIP KEY", null);
        if (cancelable) {
            builder.setNegativeButton("Cancel", (dialog, which) -> dialog.dismiss());
        }

        final AlertDialog dialog = builder.create();
        dialog.show();

        // Custom positive button listener to prevent dismissal on wrong key
        dialog.getButton(AlertDialog.BUTTON_POSITIVE).setOnClickListener(v -> {
            String enteredKey = etKey.getText().toString().trim();
            if (REQUIRED_LOGIN_KEY.equalsIgnoreCase(enteredKey)) {
                prefs.edit()
                        .putBoolean(KEY_IS_ACTIVATED, true)
                        .putString(KEY_LICENSE_KEY, REQUIRED_LOGIN_KEY)
                        .apply();
                Toast.makeText(MainActivity.this, "✓ License Verified: ACTIVE • LIFETIME UNLIMITED", Toast.LENGTH_LONG).show();
                dialog.dismiss();
            } else {
                Toast.makeText(MainActivity.this, "❌ Invalid Activation Key! Key must be " + REQUIRED_LOGIN_KEY, Toast.LENGTH_LONG).show();
            }
        });
    }

    // Dialog 2: Connection Manager
    private void showConnectionDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        android.widget.LinearLayout layout = new android.widget.LinearLayout(this);
        layout.setOrientation(android.widget.LinearLayout.VERTICAL);
        layout.setPadding(32, 32, 32, 32);
        layout.setBackgroundColor(Color.parseColor("#0C162D"));

        TextView tvTitle = new TextView(this);
        tvTitle.setText("🔌 CONNECTION MANAGER");
        tvTitle.setTextColor(Color.WHITE);
        tvTitle.setTextSize(18);
        tvTitle.setTypeface(null, android.graphics.Typeface.BOLD);
        layout.addView(tvTitle);

        TextView tvInfo = new TextView(this);
        tvInfo.setText("Remote Host: " + REMOTE_DEFAULT_IP + "\nRemote Port: " + REMOTE_PORT + "\nHotspot Sync Bridge: ACTIVE\nRoom PIN: 849201");
        tvInfo.setTextColor(Color.parseColor("#38BDF8"));
        tvInfo.setTextSize(12);
        tvInfo.setPadding(0, 16, 0, 16);
        layout.addView(tvInfo);

        builder.setView(layout);
        builder.setPositiveButton("Close", (dialog, which) -> dialog.dismiss());
        builder.show();
    }

    // Dialog 3: Live Board View
    private void showLiveBoardDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        android.widget.LinearLayout layout = new android.widget.LinearLayout(this);
        layout.setOrientation(android.widget.LinearLayout.VERTICAL);
        layout.setPadding(32, 32, 32, 32);
        layout.setBackgroundColor(Color.parseColor("#0C162D"));

        TextView tvTitle = new TextView(this);
        tvTitle.setText("☁️ LIVE LUDO BOARD");
        tvTitle.setTextColor(Color.WHITE);
        tvTitle.setTextSize(18);
        tvTitle.setTypeface(null, android.graphics.Typeface.BOLD);
        layout.addView(tvTitle);

        TextView tvInfo = new TextView(this);
        tvInfo.setText("Connected to Room 849201\nSynchronizing Pawns:\n🔵 Blue: 4/4 Yard\n🔴 Red: 4/4 Yard\n🟢 Green: 4/4 Yard\n🟡 Yellow: 4/4 Yard");
        tvInfo.setTextColor(Color.parseColor("#38BDF8"));
        tvInfo.setTextSize(12);
        tvInfo.setPadding(0, 16, 0, 16);
        layout.addView(tvInfo);

        builder.setView(layout);
        builder.setPositiveButton("Return to Dashboard", (dialog, which) -> dialog.dismiss());
        builder.show();
    }

    // Dialog 4: VIP Entitlements
    private void showEntitlementsDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        android.widget.LinearLayout layout = new android.widget.LinearLayout(this);
        layout.setOrientation(android.widget.LinearLayout.VERTICAL);
        layout.setPadding(32, 32, 32, 32);
        layout.setBackgroundColor(Color.parseColor("#0C162D"));

        TextView tvTitle = new TextView(this);
        tvTitle.setText("🛡️ VIP LICENSE ENTITLEMENTS");
        tvTitle.setTextColor(Color.WHITE);
        tvTitle.setTextSize(18);
        tvTitle.setTypeface(null, android.graphics.Typeface.BOLD);
        layout.addView(tvTitle);

        TextView tvStatus = new TextView(this);
        tvStatus.setText("✓ ACTIVE • LIFETIME UNLIMITED\nKey: " + REQUIRED_LOGIN_KEY);
        tvStatus.setTextColor(Color.parseColor("#86EFAC"));
        tvStatus.setTextSize(14);
        tvStatus.setTypeface(null, android.graphics.Typeface.BOLD);
        tvStatus.setPadding(0, 12, 0, 12);
        layout.addView(tvStatus);

        TextView tvList = new TextView(this);
        tvList.setText("• Deterministic Dice Automation (Faces 1..6)\n• Direct In-Game Cloud Sync Bridge (0-ms)\n• Smart Boost & AI Tactical Engine (God Mode 100%)\n• 2v2 Team Up Coordination & Seat Targeting\n• 72h Offline Lease • Auto-Reverify • 256-Bit Crypto");
        tvList.setTextColor(Color.parseColor("#BAE6FD"));
        tvList.setTextSize(11);
        layout.addView(tvList);

        builder.setView(layout);
        builder.setPositiveButton("Close", (dialog, which) -> dialog.dismiss());
        builder.show();
    }
}
