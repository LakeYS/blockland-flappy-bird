exec("./attributes.cs");

if(isFile("Add-Ons/System_ReturnToBlockland/server.cs"))
{
	if(!$RTB::RTBR_ServerControl_Hook) //fix this
		exec("Add-Ons/System_ReturnToBlockland/hooks/serverControl.cs");
	  
	RTB_registerPref("Enable Server Name Score", "Flappy Bird", "$Pref::Server::FlappyServerName", "bool", "GameMode_Flappy_Bird", 0, 0, 1, FlappyUpdateName);
	
	$FlappyRTB = 1;
}

function FlappyUpdateName()
{
	if(!$Pref::Server::FlappyServerName)
		return;
	%score = $FlappyHighScore[1];
	%nameA = strReplace($Pref::Server::Name,strchr($Pref::Server::Name,"("),"");
	%nameB = %nameA @ "(" @ %score @ ")";

	$Server::Name = %nameB;
	$Pref::Server::Name = %nameB;
	
	for(%i=0;%i<ClientGroup.getCount();%i++)
	{
		%cl = ClientGroup.getObject(%i);
		if(%cl.isSuperAdmin && %cl.hasRTB && $FlappyRTB)
			serverCmdRTB_getServerOptions(%cl);

		commandtoclient(%cl,'NewPlayerListGui_UpdateWindowTitle',$Server::Name,$Pref::Server::MaxPlayers);
	}
}

deactivatePackage("GameMode_Flappy_Bird");
datablock PlayerData(PlayerFlappyArmor : PlayerStandardArmor)
{
	runForce = 0;
	runEnergyDrain = 0;
	minRunEnergy = 0;
	maxForwardSpeed = 0;
	maxBackwardSpeed = 0;
	maxSideSpeed = 0;
	horizResistFactor = 1.0;

	maxForwardCrouchSpeed = 0;
	maxBackwardCrouchSpeed = 0;
	maxSideCrouchSpeed = 0;
	
	crouchBoundingBox = PlayerStandardArmor.boundingBox; //remove the crouch bounding box
	
	canJet = 0;

	jumpForce = 0;
	jumpEnergyDrain = 0;
	minJumpEnergy = 0;
	jumpDelay = 0;
	
	thirdPersonOnly = 1;
	firstPersonOnly = 0;

	//maxLookAngle = 0;
	//minLookAngle = 0;
	
	maxSideSpeed = 0;
	
	is2DPlayer = 1;
	cameraDirection2D = 0;
	cameraPitch2D = 0;
	cameraDistance2D = 20;

	minJetEnergy = 0;
	jetEnergyDrain = 0;

	uiName = "Flappy Player";
	showEnergyBar = false;
};

datablock PlayerData(playerFlappyNo2DArmor : PlayerFlappyArmor)
{
	is2DPlayer = 0;
	thirdPersonOnly = 0;
	uiName = "";
};

datablock PlayerData(PlayerFlappyCrouchCheatArmor : PlayerFlappyArmor)
{
	crouchBoundingBox = PlayerStandardArmor.crouchBoundingBox;
	uiName = "Flappy Player Crouch";
};

datablock fxDTSBrickData(brick1x1x8Data)
{
	brickFile = "./bricks/1x1x8.blb";
	category = "Bricks";
	subCategory = "8x Height";
	uiName = "1x1x8";
};

datablock fxDTSBrickData(brick1x1x10Data)
{
	brickFile = "./bricks/1x1x10.blb";
	category = "Bricks";
	subCategory = "10x Height";
	uiName = "1x1x10";
};

function flappySpawnPlant(%brick)
{
	if(!isObject(%group = %brick.getGroup()))
		return;

	if(!%group.spawnBrickCount)
		%group.spawnBrickCount = 0;

	%group.spawnBrick[%group.spawnBrickCount] = %brick;
	%group.spawnbrickCount++;
}

//flap loop
function flapLoop()
{
	if(ClientGroup.getCount() != 0)
	{
		for(%i = 0; %i < ClientGroup.getCount(); %i++) 
		{ 
			%client = clientGroup.getObject(%i); 
			%player = %client.player;
			if(isObject(%player) && !%client.noFlap && !%player.noFlap)
			{
				%player.setVelocity($FlappySpeed SPC getWords(%player.getVelocity(),1,2));
				
				if(getWord(%player.getTransform(),2)>$FlappyCheaterHeight)
				{
					//messageClient(%client,'MsgFlappyCheater',"CHEATER!");
					//echo(%client.name @ " above maximum height, force-killing...");
					%player.kill();
				}
			}
		}
	}
	$FlapLoop = schedule(1000,0,flapLoop);
}

//distance recorder loop
function distLoop()
{
	for(%i=0;%i<ClientGroup.getCount();%i++)
	{ 
		%client = clientGroup.getObject(%i);
		%player = %client.player;
		if(isObject(%player) && !%client.noFlap && !%player.noFlap)
		{
			%client.distance = mCeil(getWord(%player.getTransform()+%player.flappyPosAdd,0)); 
			%id = %client.getblid();
			%viewdistance = %client.distance;
			%highscore = $FlappyHighScorePersonal[%id];
			
			if(%client.distance < 0)
				%viewdistance = 0;
			
			if(!%client.noPrint)
				%client.bottomPrint("<font:impact:50><color:FFFF00><just:left>Score: " @ %viewdistance @ "<just:right>Best: " @ %highscore,1,1);
			
			if(isObject(%player) && getWord(%player.getTransform(),0)>=$FlappyPosEnd)
			{
				%pos1 = getWord(%player.getTransform(),0);
				%player.setTransform(setWord(%player.getTransform(),0,$FlappyPosStart));
				%pos2 = getWord(%player.getTransform(),0);
				%player.flappyPosAdd += %pos1-%pos2;
			}
			
			//because of the 2d camera's distance, 3d sounds are quieter (which makes 2d sounds too loud to use)
			//i might try playing sounds between the player and camera for balance
			
			if(%client.distance > 200 && !%player.flappyBronze)
			{
				$FlappyHighScoreBronze[%id] = 1;
				$FlappyHighScoreBronzeCount[%id]++;
				%player.flappyBronze = 1;
				%client.centerPrint("<font:impact:40><color:CD7F32>Bronze Medal Awarded",5);
				
				%player.emote(winStarProjectile, 1);
				serverPlay3D(rewardSound,%player.position); 
			}
			
			if(%client.distance > 400 && !%player.flappySilver)
			{
				$FlappyHighScoreSilver[%id] = 1;
				$FlappyHighScoreSilverCount[%id]++;
				%player.flappySilver = 1;
				%client.centerPrint("<font:impact:40><color:afafaf>Silver Medal Awarded",5);
				
				%player.emote(winStarProjectile, 1);
				serverPlay3D(rewardSound,%player.position);
			}
			
			if(%client.distance > 800 && !%player.flappyGold)
			{
				$FlappyHighScoreGold[%id] = 1;
				$FlappyHighScoreGoldCount[%id]++;
				%player.flappyGold = 1;
				%client.centerPrint("<font:impact:40><color:ffd420>Gold Medal Awarded",5);
				
				%player.emote(winStarProjectile, 1);
				serverPlay3D(rewardSound,%player.position);
			}
			
			if(%client.distance > 1600 && !%player.flappyPlatinum)
			{
				$FlappyHighScorePlatinum[%id] = 1;
				$FlappyHighScorePlatinumCount[%id]++;
				%player.flappyPlatinum = 1;
				%client.centerPrint("<font:impact:40><color:ffffff>Platinum Medal Awarded",5);
				
				%player.emote(winStarProjectile, 1);
				serverPlay3D(rewardSound,%player.position);
			}
		} 
	}
	
	$distloop = schedule(32,0,distLoop);
}

//export loop
function exportLoop()
{
	if($FlappyHighScoreUpdate) //only export if something changes (to avoid console spam)
	{
		export("$FlappyHighScore*","config/server/FlappyBirdScores.cs");
		echo("Exporting Flappy Bird high scores...");
		$FlappyHighScoreUpdate = 0;
	}
	$exportLoop = schedule(120000,0,exportLoop);
}

function flap(%player)
{
	if($FlappyJumpControlEnabled || %player.isJumpControlPlayer || %player.dataBlock $= "PlayerFlappyArmor" || %player.dataBlock $= "playerFlappyNo2DArmor")
	{
		if(!%player.isJumpTimeout)
		{
			%player.playThread(0,jump);
			
			%jumpVelocity = getRandom($FlappyJumpVelocity1,$FlappyJumpVelocity2);
			serverPlay3D(jumpSound,%player.position);
			%player.setVelocity(getWords(%player.getVelocity(),0,1) SPC %jumpVelocity);
			
			%player.isJumpTimeout = 1;
			%timeout = getRandom($FlappyJumpTimeout1,$FlappyJumpTimeout2);
			%player.timeout = %player.schedule(%timeout,setAttribute,isJumpTimeout,0);
		}
		
		return 1;
	}
}

function FlappyBirdDeath(%client)
{
	if($DistLoop==0 || %client.noFlap || %client.player.noFlap)
		return; 
	
	$FlappyHighScoreDeaths[%client.bl_id]++;
	
	//echo(%client.name @ " DIED; final score is " @ %client.distance); 
	%score = %client.distance;
	%oldScore = $FlappyHighScorePersonal[%client.bl_id];
	
	if(%score > $FlappyHighScorePersonal[%client.bl_id])
	{
		if($FlappyHighScorePersonal[%client.bl_id])
			%message = "\c0 (A new personal record!)";
		%leaderboard = 1; //doesn't count if the score isn't above their personal best (lan clients excepted)
		$FlappyHighScorePersonal[%client.bl_id] = %score;
		$FlappyHighScoreUpdate = 1;
		//messageClient(%client,'MsgFlappyBirdPersonalScoreWin',"Your new personal record is \c6" @ %score @ "\c0!");
	}
	
	if(%client.bl_id==999999)
		%leaderboard = 1;
	
	messageClient(%client,'MsgFlappyBirdDead',"You died! Distance: \c6" @ %client.distance @ %message);
	
	//leaderboard check
	if(%score > $FlappyHighScore[20] && %leaderboard)
	{
		for(%i = 1; %i <= 20; %i++)
		{
			if(%score > $FlappyHighScore[%i])
			{
				%newpos = %i;
				
				//if they already have a lower slot, we'll remove it and push down scores to fill it.
				if(%client.bl_id != 999999)
				{
					for(%iB = 1; %iB <= 20; %iB++)
					{
						if(%client.getblid() == $FlappyHighScoreID[%iB])
						{
							if(%emptySlot)
							{
								error("Flappy Bird - Multiple empty slots! First slot will be filled. (i: " @ %i @ "; iB: " @ %iB @ "; newpos: " @ %newpos @ "; emptyslot: " @ %emptyslot @ "; score: " @ %score @ "; player: " @ %client.bl_id SPC %client.name @ ")");
								break;
							}
							else
							{
								%emptySlot = %iB; //empty slot will be filled
								$FlappyHighScoreName[%iB] = "\c5This slot should be filled!";
							}
						}
					}
				}
				
				if(!%emptySlot)
					%emptySlot = 20;
				
				if(%emptyslot == %newpos)
				{
					if(%newpos <= 10) //leaderboard scores above 10 are recorded but hidden (for now)
						messageClient(%client,'',"Congratulations, your score is \c6#" @ %newpos @ "\c0 on the leaderboard! Type /leaderboard to view it.");
					
					$FlappyHighScore[%newpos] = %score;
					$FlappyHighScoreName[%newpos] = %client.name;
					$FlappyHighScoreID[%newpos] = %client.bl_id;
					
					if(%client.bl_id==999999)
						$FlappyHighScoreID[%newpos] = "LAN";
					
					$FlappyHighScoreUpdate = 1;
					break;
				}
				
				if(%emptySlot < %newpos)
					error("Flappy Bird - Empty slot < new position! (i: " @ %i @ "; iB: " @ %iB @ "; newpos: " @ %newpos @ "; emptyslot: " @ %emptyslot @ "; score: " @ %score @ "; player: " @ %client.bl_id SPC %client.name @ ")");
				
				//%emptySlot is the slot to be filled. %emptySlot-1 is the last slot to be pushed down.
				
				for(%iC = %emptySlot-1; %iC >= %newpos; %iC--)
				{
					$FlappyHighScore[%iC+1] = $FlappyHighScore[%iC];
					$FlappyHighScoreName[%iC+1] = $FlappyHighScoreName[%iC];
					$FlappyHighScoreID[%iC+1] = $FlappyHighScoreID[%iC];
					
					if(%iC == %newpos)
					{
						//echo("filling score #" @ %newpos @ " with the new values (" @ %client.name @ ", " @ %client.bl_id @ ", " @ %score @ ")");
						
						if(%newpos <= 10) //leaderboard scores above 10 are recorded but hidden (for now)
							messageClient(%client,'',"Congratulations! Your score is \c6#" @ %newpos @ "\c0 on the leaderboard!");
						
						$FlappyHighScore[%newpos] = %score;
						$FlappyHighScoreName[%newpos] = %client.name;
						$FlappyHighScoreID[%newpos] = %client.bl_id;
						
						if(%client.bl_id==999999)
							$FlappyHighScoreID[%newpos] = "LAN";
					}
				}
				$FlappyHighScoreUpdate = 1;
				break;
			}
		}
		
		if(%newpos == 1)
		{
			FlappyUpdateName(); //set server name
			echo("New top score: " @ $FlappyHighScore[1]);
			if(!$Server::LAN)
				WebCom_PostServer();
		}
	}
	
	if(!%client.noPrint)
		%client.bottomPrint("<font:impact:50><color:FFFF00><just:left>Score: "@%client.distance @ "<just:right>Best: " @ $FlappyHighScorePersonal[%client.bl_id],0,1);
	%client.score = $FlappyHighScorePersonal[%client.bl_id];
}

package GameMode_Flappy_Bird
{
	//flap by jumping (onTrigger slot 2)
	function Armor::onTrigger(%data,%obj,%slot,%val) 
	{
		Parent::onTrigger(%data,%obj,%slot,%val); 
		//%obj = player
		if(%slot == 2 && %val) 
			flap(%obj);
	}
	
	//flap by planting
	function serverCmdPlantBrick(%client)
	{
		if(isObject(%client.player) && !%client.noPlantFlap)
		{
			%flap = flap(%client.player);
			
			if(%flap)
				return;
		}
		Parent::serverCmdPlantBrick(%client); 
	}
	
	//onDrop
	function GameConnection::onDrop(%client,%a)
	{
		FlappyBirdDeath(%client);
		Parent::onDrop(%client,%a);
	}

	//onDeath
	function GameConnection::onDeath(%client) 
	{
		FlappyBirdDeath(%client);
		parent::onDeath(%client); 
	}

	////CLEANUP////
	function destroyServer()
	{
		echo("Exporting Flappy Bird high scores (server closing)");
		
		export("$FlappyHighScore*","config/server/FlappyBirdScores.cs");
		deleteVariables("$Flappy*");
		cancel($exportLoop);
		cancel($flapLoop);
		cancel($distLoop);
		parent::destroyServer();
	}

	////CLEANUP////
	function onExit()
	{		
		echo("Exporting Flappy Bird high scores (server closing/quit)");
		
		export("$FlappyHighScore*","config/server/FlappyBirdScores.cs");
		deleteVariables("$Flappy*");
		cancel($exportLoop);
		cancel($flapLoop);
		cancel($distLoop);
		
		parent::onExit();
	}
	
	////prevent admin cheaters////
	function serverCmdDropCameraAtPlayer(%client)
	{
		FlappyBirdDeath(%client);
		
		if(%client.isAdmin)
		{
			%client.player.noFlap = 1;
			%client.player.flappyOrb = 1;
		}
		
		Parent::ServerCmdDropCameraAtPlayer(%client);
	}
	function serverCmdDropPlayerAtCamera(%client)
	{
		if(!%client.player.flappyOrb && %client.player.dataBlock $= "PlayerFlappyArmor")
			%client.player.setDatablock("playerFlappyNo2DArmor");
		else if(!%client.player.flappyOrb && %client.player.dataBlock $= "playerFlappyNo2DArmor")
			%client.player.setDatablock("PlayerFlappyArmor");
		else
		{
			FlappyBirdDeath(%client);
			
			if(%client.isAdmin)
				%client.player.noFlap = 1;
			
			Parent::serverCmdDropPlayerAtCamera(%client);
		}
	}
	function serverCmdWarp(%client)
	{
		FlappyBirdDeath(%client);
		
		if(%client.isAdmin)
			%client.player.noFlap = 1;
		
		Parent::serverCmdWarp(%client);
	}
	function serverCmdFetch(%client,%this)
	{
		if($GameModeArg !$= "Add-Ons/GameMode_Flappy_Bird/gamemode.txt")
			Parent::serverCmdFetch(%client,%this);
	}
	function serverCmdFind(%client,%this)
	{
		FlappyBirdDeath(%client);
		
		if(%client.isAdmin)
			%client.player.noFlap = 1;
		
		Parent::serverCmdFind(%client,%this);
	}

	//SetJCVelocity
	function serverCmdSetJCVelocity(%client,%val1,%val2)
	{
		if(%client.isSuperAdmin)
		{
			if(!%val1 || !%val2)
			{
				%output1 = 8;
				%output2 = 12;
			}
			else
			{
				%output1 = %val1;
				%output2 = %val2;
			}
			
			$FlappyJumpVelocity1 = %output1;
			$FlappyJumpVelocity2 = %output2;
			
			messageAll('MsgJumpControl',%client.name @ " changed the jump velocity to (" @ %output1 @ "," @ %output2 @ ")");
		}
	}

	//SetJCTimeout
	function serverCmdSetJCTimeout(%client,%val1,%val2)
	{
		if(%client.isSuperAdmin)
		{
			if(!%val1 || !%val2)
			{
				%output1 = 500;
				%output2 = 750;
			}
			else
			{
				%output1 = %val1;
				%output2 = %val2;
			}
			
			$FlappyJumpTimeout1 = %output1;
			$FlappyJumpTimeout2 = %output2;
			
			messageAll('MsgJumpControl',%client.name @ " changed the jump timeout to (" @ %output1 @ "," @ %output2 @ ")");
		}
	}

	function serverCmdToggleFlapLoop(%client)
	{
		if(!%client.isSuperAdmin)
			return;
		
		if(!$FlapLoop)
		{
			echo("Flap loop enabled by " @ %client.name);
			messageAll('MsgFlapLoopEnable',%client.name @ " enabled the flap loop");
			flapLoop();
		}
		else
		{
			echo("Flap loop disabled by " @ %client.name);
			messageAll('MsgFlapLoopDisable',%client.name @ " disabled the flap loop");
			cancel($FlapLoop);
			$FlapLoop = 0;
		}
	}

	function serverCmdToggleDistLoop(%client)
	{
		if(!%client.isSuperAdmin)
			return;
		
		if(!$DistLoop)
		{
			echo("Distance loop enabled by " @ %client.name);
			messageAll('MsgDistLoopEnable',%client.name @ " enabled the distance loop");
			distLoop();
		}
		else
		{
			echo("Distance loop disabled by " @ %client.name);
			messageAll('MsgDistLoopDisable',%client.name @ " disabled the distance loop");
			cancel($DistLoop);
			$DistLoop = 0;
		}
	}

	function serverCmdSetFlappyDefaults(%client)
	{
		if(!%client.isSuperAdmin)
			return;

		echo(%client.name SPC "is resetting the default settings for Flappy Bird...");
		messageAll('MsgFlappyReset',%client.name SPC "is resetting the default settings for Flappy Bird...");
		
		$FlappyJumpVelocity1 = 9.85;
		$FlappyJumpVelocity2 = 9.85;

		$FlappyJumpTimeout1 = 1;
		$FlappyJumpTimeout2 = 1;

		$FlappyJumpAirAnim = 1;

		//old
		//$FlappyPosStart = 334.75; 
		//$FlappyPosEnd = 590.75;
		
		$FlappyPosStart = 95.75;
		$FlappyPosEnd = 831.75;

		$FlappyCheaterHeight = 90;

		$FlappySpeed = 10;
	}

	function serverCmdSetFlappySpeed(%client,%speed)
	{
		if(!%client.isSuperAdmin)
			return;
		
		if(%speed < 1 || %speed > 100 || !%speed)
		{
			$FlappySpeed = 10;
			echo(%client.name @ " changed the flap speed to 10");
			messageAll('MsgFlapSpeed',%client.name @ " changed the flap speed to 10");
			return;
		}

		$FlappySpeed = %speed;
		echo(%client.name @ " changed the flap speed to " @ %speed);
		messageAll('MsgFlapSpeed',%client.name @ " changed the flap speed to " @ %speed);
	}

	function serverCmdSetFlappyCheaterHeight(%client,%height)
	{
		if(!%client.isSuperAdmin)
			return;
		
		if(%height < 0 || !%height)
		{
			$FlappyCheaterHeight = 90;
			echo(%client.name @ " changed the flap height to 90");
			messageAll('MsgFlapHeight',%client.name @ " changed the flap height to 90");
			return;
		}

		$FlappyCheaterHeight = %height;
		echo(%client.name @ " changed the flap height to " @ %height);
		messageAll('MsgFlapHeight',%client.name @ " changed the flap height to " @ %height);
	}

	function serverCmdResetFlapLoop(%client)
	{
		if(!%client.isSuperAdmin)
			return;
		
		cancel($FlapLoop);
		flapLoop();
		
		echo(%client.name @ " reset the flap loop");
	}

	function serverCmdLeaderboard(%client)
	{
		for(%i = 10; %i >= 1; %i--)
			messageClient(%client,'',"\c0#:\c6 " @ %i @ " \c7|\c0 Distance:\c6 " @ $FlappyHighScore[%i] @ " \c7|\c0 ID:\c6 " @ $FlappyHighScoreID[%i] @ " \c7|\c0 Name:\c6 " @ $FlappyHighScoreName[%i]);
		
		messageClient(%client,'FlappyHighScoreEnd',"Press PageUp to see more. Tip: You can also see high scores in the player list!");
	}
	
	function leaderboard() //Copied from above
	{
		for(%i = 10; %i >= 1; %i--)
			echo("\c0#:\c6 " @ %i @ " \c7|\c0 Distance:\c6 " @ $FlappyHighScore[%i] @ " \c7|\c0 ID:\c6 " @ $FlappyHighScoreID[%i] @ " \c7|\c0 Name:\c6 " @ $FlappyHighScoreName[%i]);
	}
	
	function serverCmdHighScores(%client)
	{
		serverCmdLeaderboard(%client);
	}

	function serverCmdStats(%client,%target)
	{
		%id = %client.bl_id;
		%findID = findClientByBL_ID(%target);
		
		if(%target !$= "")
		{
			if($FlappyHighScorePersonal[%target]) // They specified a valid ID
				%id = %target;
			else if(findClientByName(%target)) // They specified the name of a player in the server
				%id = findClientByName(%target).bl_id;
			else // Something else...
			{
				for(%i = 1; %i <= 10; %i++)
				{
					if(strstr(strupr($FlappyHighScoreName[%i]),strupr(%target)) >= 0) // They specified a name from the leaderboard
					{
						%id = $FlappyHighScoreID[%i];
						%name = $FlappyHighScoreName[%i] SPC "\c0(ID\c6" SPC %id @ "\c0)"; // (Show their BLID if they aren't in the server)
						break;
					}
				}
				
				for(%i = 2; %i <= mainBrickGroup.getCount()-1; %i++) // They specified the name of someone that was previously on the server
				{
					%brickgroup = mainBrickGroup.getObject(%i);
					%search = %brickgroup.name;
					if(strstr(strupr(%search),strupr(%target)) >= 0)
					{
						%id = %brickgroup.bl_id;
						%name = %search SPC "\c0(ID\c6" SPC %id @ "\c0)"; // (Show their BLID if they aren't in the server)
						break;
					}
				}
			}
		}
		
		if(%name $= "")
			%name = findClientByBL_ID(%id).name;
		
		if(%id == %client.bl_id)
		{
			%playerWordA = "Your";
			%playerWordB = "You have";
			%playerWordC = "You have";
		}
		else
		{
			%playerWordA = "This person's";
			%playerWordB = "This person has";
			%playerWordC = "They have";
		}
		
		if($FlappyHighScoreBronzeCount[%id] == 1)
			%bronzeWord = "medal";
		else
			%bronzeWord = "medals";
		
		if($FlappyHighScoreSilverCount[%id] == 1)
			%silverWord = "medal";
		else
			%silverWord = "medals";
		
		if($FlappyHighScoreGoldCount[%id] == 1)
			%goldWord = "medal";
		else
			%goldWord = "medals";
		
		if($FlappyHighScorePlatinumCount[%id] == 1)
			%platinumWord = "medal";
		else
			%platinumWord = "medals";
			
		//scores from older versions won't have certain values so we need to fill them
		if($FlappyHighScoreBronzeCount[%id])
			%bronzeCount = $FlappyHighScoreBronzeCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 200)
				%bronzeCount = 1;
			else
				%bronzeCount = 0;
		}
		
		if($FlappyHighScoreSilverCount[%id])
			%silverCount = $FlappyHighScoreSilverCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 400)
				%silverCount = 1;
			else
				%silverCount = 0;
		}
		
		if($FlappyHighScoreGoldCount[%id])
			%goldCount = $FlappyHighScoreGoldCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 800)
				%goldCount = 1;
			else
				%goldCount = 0;
		}
		
		if($FlappyHighScorePlatinumCount[%id])
			%platinumCount = $FlappyHighScorePlatinumCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 1600)
				%platinumCount = 1;
			else
				%platinumCount = 0;
		}
		
		if($FlappyHighScoreDeaths[%id])
			%deathCount = $FlappyHighScoreDeaths[%id];
		else if(!$FlappyHighScorePersonal[%id]) // They don't have a high score either which means they're new. Display the count as 0.
			%deathCount = 0;
		else
			%deathCount = "?"; // Death count is blank because the score is from an older version. Display question mark.
			
		
		if(%id != %client.bl_id)
		{
			if(%name $= "")
				messageClient(%client,'FlappyStats',"\c0Viewing stats for BL_ID \c6" @ %id @ "\c0.");
			else
				messageClient(%client,'FlappyStats',"\c0Viewing stats for player \c6" @ %name @ "\c0.");
		}
		
		messageClient(%client,'FlappyStats',"\c0" @ %playerWordA @ " high score is \c6" @ $FlappyHighScorePersonal[%id] @ "\c0. " @ %playerWordC @ " died \c6" @ %deathCount @ "\c0 times.");
		messageClient(%client,'FlappyStats',"\c0" @ %playerWordB @ " \c6" @ %bronzeCount @ "\c0 bronze " @ %bronzeWord @ ", \c6" @ %silverCount @ "\c0 silver " @ %silverWord @ ", \c6" @ %goldCount @ "\c0 gold " @ %goldWord @ ", and \c6" @ %platinumCount @ "\c0 platinum " @ %platinumWord @ ".");
		
		if(%id == %client.bl_id)
			messageClient(%client,'FlappyStats',"\c0To view top scores, type \c6/leaderboard\c0. To view other's scores, type \c6/stats (Name or ID)\c0.");
	}
	
	function stats(%target) // Copied from above. Removed uses of %client.
	{
		%findID = findClientByBL_ID(%target);
		
		if(%target !$= "")
		{
			if($FlappyHighScorePersonal[%target]) // They specified a valid ID
				%id = %target;
			else if(findClientByName(%target)) // They specified the name of a player in the server
				%id = findClientByName(%target).bl_id;
			else // Something else...
			{
				for(%i = 1; %i <= 10; %i++)
				{
					if(strstr(strupr($FlappyHighScoreName[%i]),strupr(%target)) >= 0) // They specified a name from the leaderboard
					{
						%id = $FlappyHighScoreID[%i];
						%name = $FlappyHighScoreName[%i] SPC "\c0(ID\c6" SPC %id @ "\c0)"; // (Show their BLID if they aren't in the server)
						break;
					}
				}
				
				for(%i = 2; %i <= mainBrickGroup.getCount()-1; %i++) // They specified the name of someone that was previously on the server
				{
					%brickgroup = mainBrickGroup.getObject(%i);
					%search = %brickgroup.name;
					if(strstr(strupr(%search),strupr(%target)) >= 0)
					{
						%id = %brickgroup.bl_id;
						%name = %search SPC "\c0(ID\c6" SPC %id @ "\c0)"; // (Show their BLID if they aren't in the server)
						break;
					}
				}
			}
		}
		
		if(!%id)
		{
			warn("Couldn't find the specified user.");
			return;
		}
		
		if(%name $= "")
			%name = findClientByBL_ID(%id).name;
		
		%playerWordA = "This person's";
		%playerWordB = "This person has";
		%playerWordC = "They have";
		
		if($FlappyHighScoreBronzeCount[%id] == 1)
			%bronzeWord = "medal";
		else
			%bronzeWord = "medals";
		
		if($FlappyHighScoreSilverCount[%id] == 1)
			%silverWord = "medal";
		else
			%silverWord = "medals";
		
		if($FlappyHighScoreGoldCount[%id] == 1)
			%goldWord = "medal";
		else
			%goldWord = "medals";
		
		if($FlappyHighScorePlatinumCount[%id] == 1)
			%platinumWord = "medal";
		else
			%platinumWord = "medals";
			
		//scores from older versions won't have certain values so we need to fill them
		if($FlappyHighScoreBronzeCount[%id])
			%bronzeCount = $FlappyHighScoreBronzeCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 200)
				%bronzeCount = 1;
			else
				%bronzeCount = 0;
		}
		
		if($FlappyHighScoreSilverCount[%id])
			%silverCount = $FlappyHighScoreSilverCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 400)
				%silverCount = 1;
			else
				%silverCount = 0;
		}
		
		if($FlappyHighScoreGoldCount[%id])
			%goldCount = $FlappyHighScoreGoldCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 800)
				%goldCount = 1;
			else
				%goldCount = 0;
		}
		
		if($FlappyHighScorePlatinumCount[%id])
			%platinumCount = $FlappyHighScorePlatinumCount[%id];
		else
		{
			if($FlappyHighScorePersonal[%id] > 1600)
				%platinumCount = 1;
			else
				%platinumCount = 0;
		}
		
		if($FlappyHighScoreDeaths[%id])
			%deathCount = $FlappyHighScoreDeaths[%id];
		else if(!$FlappyHighScorePersonal[%id]) // They don't have a high score either which means they're new. Display the count as 0.
			%deathCount = 0;
		else
			%deathCount = "?"; // Death count is blank because the score is from an older version. Display question mark.
			
		if(%name $= "")
			echo("\c0Viewing stats for BL_ID \c6" @ %id @ "\c0.");
		else
			echo("\c0Viewing stats for player \c6" @ %name @ "\c0.");
		
		echo("\c0" @ %playerWordA @ " high score is \c6" @ $FlappyHighScorePersonal[%id] @ "\c0. " @ %playerWordC @ " died \c6" @ %deathCount @ "\c0 times.");
		echo("\c0" @ %playerWordB @ " \c6" @ %bronzeCount @ "\c0 bronze " @ %bronzeWord @ ", \c6" @ %silverCount @ "\c0 silver " @ %silverWord @ ", \c6" @ %goldCount @ "\c0 gold " @ %goldWord @ ", and \c6" @ %platinumCount @ "\c0 platinum " @ %platinumWord @ ".");
	}
	
	function serverCmdMyHighScore(%client,%target)
	{
		serverCmdStats(%client,%target);
	}
	
	function GameConnection::autoAdminCheck(%client)
	{
		%id = %client.bl_id;
		//initialize their high score variables (this is necessary)
		if(!$FlappyHighScorePersonal[%id])
			$FlappyHighScorePersonal[%id] = 0;
		
		if(!$FlappyHighScoreDeaths[%id])
			$FlappyHighScoreDeaths[%id] = 0;
		
		if(!$FlappyHighScoreBronzeCount[%id])
			$FlappyHighScoreBronzeCount[%id] = 0;
		
		if(!$FlappyHighScoreSilverCount[%id])
			$FlappyHighScoreSilverCount[%id] = 0;
		
		if(!$FlappyHighScoreGoldCount[%id])
			$FlappyHighScoreGoldCount[%id] = 0;
		
		if(!$FlappyHighScorePlatinumCount[%id])
			$FlappyHighScorePlatinumCount[%id] = 0;
		
		//add medals to their count if they earned them previously
		if($FlappyHighScoreBronze[%id] && !$FlappyHighScoreBronzeCount[%id])
			$FlappyHighScoreBronzeCount[%id] = 1;
		
		if($FlappyHighScoreSilver[%id] && !$FlappyHighScoreSilverCount[%id])
			$FlappyHighScoreSilverCount[%id] = 1;
		
		if($FlappyHighScoreGold[%id] && !$FlappyHighScoreGoldCount[%id])
			$FlappyHighScoreGoldCount[%id] = 1;
		
		if($FlappyHighScorePersonal[%id] > 1600 && !$FlappyHighScorePlatinumCount[%id]) //give them the platinum medal if they have the score
			$FlappyHighScorePlatinumCount[%id] = 1;
		
		//check for a leaderboard name update
		for(%i = 1; %i < 11; %i++)
		{
			if(%id == $FlappyHighScoreID[%i] && %client.name !$= $FlappyHighScoreName[%i])
			{
				//echo("Updated leaderboard name" SPC $FlappyHighScoreName[%i] SPC "to" SPC %client.name);
				$FlappyHighScoreName[%i] = %client.name;
			}
		}
		
		Parent::autoAdminCheck(%client);
	}
};

activatePackage("GameMode_Flappy_Bird");

//jump control defaults
$FlappyJumpVelocity1 = 9.85;
$FlappyJumpVelocity2 = 9.85;

$FlappyJumpTimeout1 = 125;
$FlappyJumpTimeout2 = 125;

//old
//$FlappyPosStart = 334.75; 
//$FlappyPosEnd = 590.75;

$FlappyPosStart = 95.75;
$FlappyPosEnd = 831.75;

$FlappyCheaterHeight = 90;

$FlappySpeed = 10;

if(!$FlappyLoaded)
{
	if(!isFile("config/server/FlappyBirdScores.cs"))
	{
		for(%i = 1; %i <= 20; %i++)
		{
			$FlappyHighScore[%i] = -%i+21;
			$FlappyHighScoreName[%i] = "Flappy Bird";
			$FlappyHighScoreID[%i] = 50;
		}
		
		echo("Exporting Flappy Bird high scores...");
		export("$FlappyHighScore*","config/server/FlappyBirdScores.cs");
	}
	else
		exec("config/server/FlappyBirdScores.cs");
	
	if($GameModeArg $= "Add-Ons/GameMode_Flappy_Bird/gamemode.txt")
	{
		if(isFile("Add-Ons/Brick_WedgePlus/server.cs"))
			exec("Add-Ons/Brick_WedgePlus/server.cs");
		
		flapLoop();
		distLoop();
	}
	
	schedule(120000,0,exportloop);
	$FlappyLoaded = 1;
}