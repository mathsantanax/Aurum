using System;
using System.Collections.Generic;
using System.Text;

namespace Aurum.Application.Interfaces.EmailInterfaces
{
    public interface IEmailSender
    {
       Task SendAsync(
                string to,
                string subject,
                string htmlBody,
                CancellationToken cancellationToken = default);
    }
}
