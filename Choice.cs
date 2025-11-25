using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public class Choice
    {
        public string id;
        public string title;
        public string description = "";
        public bool resetChoicesAtChoose = true;

        public Choice(string id, string title, string description, bool resetChoicesAtChoose)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.resetChoicesAtChoose = resetChoicesAtChoose;
        }

        public Choice(string id, string title, string description)
        {
            this.id = id;
            this.title = title;
            this.description = description;
        }

        public Choice(string id, string title)
        {
            this.id = id;
            this.title = title;
        }

        public Choice(string id, string title, bool resetChoicesAtChoose)
        {
            this.id = id;
            this.title = title;
            this.resetChoicesAtChoose = resetChoicesAtChoose;
        }
    }
}
