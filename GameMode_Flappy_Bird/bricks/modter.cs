// We're going to package the desired bricks rather than loading all of ModTer.
// Doing this makes the game-mode a bit more lightweight; less memory is consumed and loading times improve slightly.
// Will screw up if, for whatever reason, ModTer is removed from the game's directory.

datablock fxDTSBrickData(brick16Cube5Data)
{
	brickFile = "Add-Ons/Brick_ModTer_BasicPack/Bricks/Steep/16cSteep.blb";
	category = "Baseplates";
	subCategory = "ModTer 16x";
	uiName = "16x Cube Steep";
	iconName = "Add-Ons/Brick_ModTer_BasicPack/BrickIcons/Steep/16cSteep";
	CollisionShapeName = "Add-Ons/Brick_ModTer_BasicPack/Shapes/Steep/16cSteepCol.dts";
        hasPrint = 1;
	printAspectRatio = "ModTer";
	isWaterBrick = true;
};

datablock fxDTSBrickData(brick32Cube5Data)
{
	brickFile = "Add-Ons/Brick_ModTer_BasicPack/Bricks/Steep/32cSteep.blb";
	category = "Baseplates";
	subCategory = "ModTer 32x";
	uiName = "32x Cube Steep";
	iconName = "Add-Ons/Brick_ModTer_BasicPack/BrickIcons/Steep/32cSteep";
	CollisionShapeName = "Add-Ons/Brick_ModTer_BasicPack/Shapes/Steep/32cSteepCol.dts";
        hasPrint = 1;
	printAspectRatio = "ModTer";
	isWaterBrick = true;
};

datablock fxDTSBrickData(brick16Cube1Data)
{
	brickFile = "Add-Ons/Brick_ModTer_BasicPack/Bricks/Full/16c.blb";
	category = "Baseplates";
	subCategory = "ModTer 16x";
	uiName = "16x Cube ";
	iconName = "Add-ons/Brick_ModTer_BasicPack/BrickIcons/Full/16c";
	collisionShapeName = "Add-Ons/Brick_ModTer_BasicPack/Shapes/Full/16cCol.dts";
        hasPrint = 1;
	printAspectRatio = "ModTer";
	isWaterBrick = true;
};
