using Hacknet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMOD
{
    public static class ChoiceManager
    {
        public static List<Choice> choices = new List<Choice>();

        public static void showChoices(OS os)
        {
            os.write("\n");
            for(int i = 0; i < choices.Count; i++)
            {
                Choice c = choices[i];
                os.write("#" + (i+1) + " " + c.title);
                os.write(c.description);
                os.write("\n");
            }
        }

        public static Choice choose(int id)
        {
            if(id <= 0 || id > choices.Count)
            {
                return null;
            } else
            {
                Choice c = choices[id - 1];
                if (c.resetChoicesAtChoose)
                {
                    choices.Clear();
                }
                return c;
            }
        }
    }
}
