# AccountApp – Account Token Generation System

A **C# WinForms application** for managing secure account login with two-factor authentication, recovery words, and per-user password hashing.

## 📋 Overview

AccountApp implements a complete authentication system with the following features:

- **User Account Management**: Create accounts, login, and manage user credentials securely
- **Two-Factor Authentication (2FA)**: Time-based authentication keys derived from recovery words and login counters
- **Location-Based Verification**: Challenge users with recovery words when logging in from a different location
- **Password Reset**: Generate and reset passwords via recovery word verification
- **Per-User Password Hashing**: PBKDF2 with random per-user salt (16 bytes, base64-encoded)
- **Login History**: Track login timestamps without storing location data
- **JSON Persistence**: All account data persisted to `accounts.json` with encryption for sensitive fields

---

## 🎯 Key Features

### Account Creation
- User provides username and password
- System generates 8 random recovery words (4–5 characters each)
- Recovery words are hashed and stored (plaintext not persisted for security)
- Login counter initialized to 0

### Login Flow
1. **Username & Password**: User enters credentials
2. **First Login**: No location verification required (allows different location on first login)
3. **Subsequent Logins from Different Location**: User must answer 4 random recovery word slot challenges
4. **2FA Verification**: If enabled, user must enter the 6-digit authentication key
5. **Login History**: Timestamp recorded (date and time only, location stored separately and encrypted)

### Login Counter & 2FA Key Generation
- Increments by **+1** for same-location logins
- Increments by **+2** for different-location logins
- Resets after reaching 64 (modulo 64)
- Authentication key: derived from recovery word hash, login counter, and bitwise transformations

### Password Management
- **Change Password**: No old password required (available in account menu)
- **Forgot Password**: Verify via recovery words → generate new random password
- **Password Hashing**: PBKDF2-SHA256 (10,000 iterations) with per-user salt

### Data Security
- **Passwords**: Hashed with per-user salt (never stored plaintext)
- **Recovery Words**: Only hashes stored; plaintext words shown only at registration
- **Location**: Encrypted with AES-256 at rest
- **Login History**: Stored plaintext (timestamps only, no location)
- **Encryption Key**: Derived dynamically from class metadata (no hardcoded secrets)

---

## 🏗️ Project Structure

```
AccountApp/
├── AccountApp/                          # Main WinForms Application
│   ├── Program.cs                       # Application entry point
│   ├── Models/
│   │   └── Account.cs                   # Account data model
│   ├── Services/
│   │   ├── AuthService.cs               # Authentication logic (login, register, password reset)
│   │   ├── AccountService.cs            # Account persistence and retrieval
│   │   ├── CryptoService.cs             # Password hashing, encryption, recovery word hashing
│   │   ├── JsonStorage.cs               # JSON file I/O and field encryption/decryption
│   │   └── Utils.cs                     # Recovery word generation, 2FA key calculation
│   ├── Forms/
│   │   ├── LoginForm.cs                 # Main login and registration UI
│   │   ├── RegisterForm.cs              # Account registration dialog
│   │   ├── AccountMenuForm.cs           # Post-login user menu (change password, manage 2FA)
│   │   ├── ForgotPasswordForm.cs        # Password recovery dialog
│   │   ├── RecoveryWordsForm.cs         # Recovery word verification UI
│   │   └── AdminForm.cs                 # Admin account management
│   ├── Assets/
│   │   └── wordlist.txt                 # Word pool for recovery word generation (optional)
│   └── bin/Debug/net9.0-windows/
│       └── accounts.json                # Persisted account data
│
├── AuthKeyGenerator/                    # Standalone Console Application
│   └── Program.cs                       # Console tool to generate auth keys from recovery words
│
├── AccountApp.sln                       # Visual Studio solution file
└── README.md                            # This file
```

---

## 🔐 Authentication Key Algorithm

The 2FA key is generated using the following steps:

1. **Input**: Recovery words hash + login counter
2. **Character Processing**: For each character in the hashed string:
   - Convert to ASCII decimal value
   - Apply bit transformations (flip first 7 bits)
   - Add 16 to the result
3. **Summation**: Sum all transformed values
4. **Counter Addition**: Add login counter to the sum
5. **Output**: Convert result to 4-digit PIN (zero-padded if needed)

**Example**:
- Recovery word hash: `"BirdTreeTruck..."`
- Login counter: `5`
- Resulting key: `"1234"` (example output)

---

## 🛠️ Building & Running

### Prerequisites
- **.NET 9 SDK** (net9.0-windows)
- **Windows** (WinForms is Windows-only)
- Optional: **Visual Studio 2022** or **VS Code** with C# extension

### Build
```powershell
cd C:\Users\gradi\Desktop\AccountApp
dotnet build
```

### Run the Main Application
```powershell
cd AccountApp
dotnet run
```

### Run the Auth Key Generator (Console Tool)
```powershell
cd AuthKeyGenerator
dotnet run
```

---

## 📝 Usage

### Creating an Account
1. Click **Register** on the login form
2. Enter username and password
3. Confirm password
4. Recovery words are displayed — **save them securely**
5. Click **OK** to complete registration

### Logging In
1. Enter username and password
2. If logging in from a **different location**:
   - Answer 4 recovery word slot challenges (e.g., "What is at slot 3?")
3. If **2FA is enabled**:
   - Enter the 6-digit authentication key
4. Upon success, you see the account menu

### Managing Account
- **Change Password**: Click "Change Password" (no old password required)
- **Enable 2FA**: Click "Enable 2FA" and save the generated key
- **Disable 2FA**: Click "Disable 2FA" and enter your current authentication key
- **View Login History**: Displayed in the account info (date & time only)
- **Logout**: Click "Logout"

### Password Recovery
1. From login screen, click **Forgot Password**
2. Enter your username
3. Answer 4 recovery word slot challenges
4. A new random password is generated and copied to clipboard
5. Login with the new password

---

## 🔑 Default Admin Account

An **admin account** is automatically created on first run:
- **Username**: `admin`
- **Password**: `admin`

The admin account can:
- View all accounts and their details (including password hashes)
- See recovery word hashes and login history for all users
- Is exempt from location-based verification

---

## 📊 Data Persistence

### accounts.json Structure
```json
[
  {
    "username": "testuser",
    "password": "<base64-hashed-password>",
    "salt": "<base64-random-salt>",
    "recoveryWordHashes": ["<hash1>", "<hash2>", ...],
    "hashedRecoveryWords": "<combined-hash>",
    "loginCounter": 5,
    "lastLocation": "<encrypted-location>",
    "loginHistory": ["2025-11-26 14:32:15", "2025-11-26 10:20:00"],
    "twoFactorEnabled": true,
    "lockedUntil": null
  }
]
```

**Encrypted Fields**: `lastLocation` (AES-256)  
**Plain Text Fields**: `loginHistory` (timestamps only)  
**Hashed Fields**: `password`, `recoveryWordHashes`, `hashedRecoveryWords`

---

## 🔒 Security Notes

### Strengths
- ✅ Per-user salt for password hashing (PBKDF2 with 10,000 iterations)
- ✅ Recovery words hashed (plaintext not persisted after registration)
- ✅ Location encrypted at rest
- ✅ Account lockout for incorrect recovery words (10 second timeout)
- ✅ Login counter prevents replay attacks in 2FA

### Considerations
- ⚠️ Encryption passphrase derived from class metadata (not a secret; suitable for data-at-rest encryption only)
- ⚠️ No TLS/HTTPS (desktop application; uses local JSON file)
- ⚠️ Recovery words shown only once at registration; no recovery mechanism if lost
- ⚠️ Admin account credentials hardcoded in code (suitable for demo/development only)

---

## 🧪 Testing

### Manual Testing Workflow
1. **Register a new user**:
   - Username: `testuser`
   - Password: `P@ssw0rd`
   - Note recovery words

2. **First login** (same location):
   - No recovery word challenge

3. **Second login** (different location):
   - Click "Change Location" to set a different location
   - Login again → verify recovery word challenge

4. **Enable 2FA**:
   - Login to account
   - Click "Enable 2FA"
   - Save the displayed authentication key
   - Logout and login again → enter the key

5. **Password reset**:
   - Logout
   - Click "Forgot Password"
   - Enter username
   - Answer recovery word challenges
   - New password generated

---

## 📦 Dependencies

- **System.Security.Cryptography** (included in .NET)
- **System.Text.Json** (included in .NET)
- **System.Windows.Forms** (Windows only)

No external NuGet packages required.

---

## 🔗 Related Projects

- **AuthKeyGenerator**: Standalone console tool for computing 2FA keys from recovery words
- **AccountApp**: Main WinForms application

