using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Infrastructure.Email.Configuration
{
    public sealed class SmtpOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; } = 587;

        public string User { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public bool EnableSsl { get; set; } = true;

        public string FromEmail { get; set; } = string.Empty;

        public string FromName { get; set; } = "Aurum";
    }
}
