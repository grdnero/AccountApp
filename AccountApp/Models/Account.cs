using System;
using System.Collections.Generic;

namespace AccountApp.Models
{
    public class Account
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        // Per-account salt for password hashing (base64)
        public string Salt { get; set; } = "";
        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> RecoveryWords { get; set; } = new List<string>();
        public List<string> RecoveryWordHashes { get; set; } = new List<string>();
        public DateTime? LockedUntil { get; set; } = null;
        public string HashedRecoveryWords { get; set; } = "";
        public int LoginCounter { get; set; } = 0;
        public string LastLocation { get; set; } = "";
        public bool TwoFactorEnabled { get; set; } = false;
        public List<string> LoginHistory { get; set; } = new List<string>();
    }
}
