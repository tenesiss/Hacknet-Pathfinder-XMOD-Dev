// Hacknet.Mission.MisisonGoal
using System.Collections.Generic;
using Pathfinder;
using Microsoft.Xna.Framework;
using System;
using BepInEx;
using BepInEx.Hacknet;
using Hacknet;
using Pathfinder.Util;

public class FileCreationGoal : Pathfinder.Mission.PathfinderGoal
{
	[XMLStorage]
	public string target;
	[XMLStorage]
	public string path;
	[XMLStorage]
	public string filename;
	[XMLStorage]
	public string fileContent = null;

	public override bool isComplete(List<string> additionalDetails = null)
	{
		Computer comp = Pathfinder.Util.ComputerLookup.FindById(target);
        if (comp.getFolderFromPath(path).containsFile(filename))
        {
			if(fileContent == null)
            {
				return true;
			} else
            {
                if (comp.getFolderFromPath(path).searchForFile(filename).data == fileContent)
                {
					return true;
                } else
                {
					return false;
                }
            }
			
        } else
        {
			return false;
        }
	}
}
