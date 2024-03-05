using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class EmailData
    {
        public string author;
        public string subject;
        public string content;
        public List<string> attachments;

        public EmailData(string author, string subject, string content, List<string> attachments)
        {
            this.author = author;
            this.subject = subject;
            this.content = content;
            this.attachments = attachments;
        }
    }
}
