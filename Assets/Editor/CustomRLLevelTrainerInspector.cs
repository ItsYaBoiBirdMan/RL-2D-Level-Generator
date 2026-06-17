using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RLLevelTrainer))]
public class CustomRLLevelTrainerInspector : Editor
{
    // PLAYER PERFORMANCE TRACKER
    SerializedProperty playerPerformanceTracker;

    // BASIC SETTINGS
    SerializedProperty levelWidth;
    SerializedProperty levelHeight;
    SerializedProperty trainingEpisodes;
    SerializedProperty difficulty;

    // PREFABS (Side Scroller)
    SerializedProperty sideScrollerGroundBlock;
    SerializedProperty sideScrollerGroundMiddleBlock;
    SerializedProperty sideScrollerGroundTopLeftCornerBlock;
    SerializedProperty sideScrollerGroundTopRightCornerBlock;
    SerializedProperty sideScrollerGroundTopMiddleBlock;
    SerializedProperty sideScrollerGroundBottomMiddleBlock;
    SerializedProperty sideScrollerGroundBottomLeftCornerBlock;
    SerializedProperty sideScrollerGroundBottomRightCornerBlock;
    SerializedProperty sideScrollerGroundMiddleLeftBlock;
    SerializedProperty sideScrollerGroundMiddleRightBlock;
    SerializedProperty sideScrollerGroundPeakPointBlock;

    // PREFABS (Top Down)
    SerializedProperty topDownGroundBlock;
    SerializedProperty topDownGroundMiddleBlock;
    SerializedProperty topDownGroundTopLeftCornerBlock;
    SerializedProperty topDownGroundTopRightCornerBlock;
    SerializedProperty topDownGroundTopMiddleBlock;
    SerializedProperty topDownGroundBottomMiddleBlock;
    SerializedProperty topDownGroundBottomLeftCornerBlock;
    SerializedProperty topDownGroundBottomRightCornerBlock;
    SerializedProperty topDownGroundMiddleLeftBlock;
    SerializedProperty topDownGroundMiddleRightBlock;
    SerializedProperty topDownGroundPointUpBlock;
    SerializedProperty topDownGroundPointDownBlock;
    SerializedProperty topDownGroundPointLeftBlock;
    SerializedProperty topDownGroundPointRightBlock;
    SerializedProperty topDownGroundAloneBlock;
    SerializedProperty topDownGroundSidewaysBlock;
    SerializedProperty topDownGroundUpwardsBlock;

    // PLATFORM BLOCKS
    SerializedProperty platformBlock;
    SerializedProperty platformMiddleBlock;
    SerializedProperty platformLeftBlock;
    SerializedProperty platformRightBlock;

    // EDGE TRIGGERS
    SerializedProperty leftEdgeTrigger;
    SerializedProperty rightEdgeTrigger;

    // ENEMIES
    SerializedProperty sideScrollingEnemyPrefab;
    SerializedProperty topDownEnemyPrefab;
    SerializedProperty sideScrollingEnemyObjectsList;
    SerializedProperty topDownEnemyObjectsList;

    // LEVEL COMPONENTS
    SerializedProperty coinPrefab;
    SerializedProperty pickupPrefab;
    SerializedProperty hazardPrefab;
    SerializedProperty deathPlane;

    // PARENTS
    SerializedProperty blockParent;
    SerializedProperty edgeParent;
    SerializedProperty enemyParent;
    SerializedProperty coinParent;
    SerializedProperty hazardParent;

    // PLAYER OBJECTS
    SerializedProperty sideScrollerPlayer;
    SerializedProperty topDownPlayer;

    // GENERATION SETTINGS
    SerializedProperty generationMode;
    SerializedProperty haveEnemies;
    SerializedProperty haveEnemiesSpawnBasedOnDifficulty;
    SerializedProperty haveEnemyScaling;
    SerializedProperty havePlatforms;
    SerializedProperty platformDifficultyRelation;
    SerializedProperty minPlatformHeightFromGround;
    SerializedProperty maxPlatformHeightFromGround;
    SerializedProperty havePickups;
    SerializedProperty haveHazards;
    SerializedProperty haveCoins;

    // DEBUG
    SerializedProperty doNotInstantiate;
    SerializedProperty showTilesetInfo;

    private void OnEnable()
    {
        // Player tracker
        playerPerformanceTracker = serializedObject.FindProperty("PlayerPerformanceTracker");

        // Basic
        levelWidth = serializedObject.FindProperty("LevelWidth");
        levelHeight = serializedObject.FindProperty("LevelHeight");
        trainingEpisodes = serializedObject.FindProperty("TrainingEpisodes");
        difficulty = serializedObject.FindProperty("Difficulty");

        // Side scroller
        sideScrollerGroundBlock = serializedObject.FindProperty("SideScrollerGroundBlock");
        sideScrollerGroundMiddleBlock = serializedObject.FindProperty("SideScrollerGroundMiddleBlock");
        sideScrollerGroundTopLeftCornerBlock = serializedObject.FindProperty("SideScrollerGroundTopLeftCornerBlock");
        sideScrollerGroundTopRightCornerBlock = serializedObject.FindProperty("SideScrollerGroundTopRightCornerBlock");
        sideScrollerGroundTopMiddleBlock = serializedObject.FindProperty("SideScrollerGroundTopMiddleBlock");
        sideScrollerGroundBottomMiddleBlock = serializedObject.FindProperty("SideScrollerGroundBottomMiddleBlock");
        sideScrollerGroundBottomLeftCornerBlock = serializedObject.FindProperty("SideScrollerGroundBottomLeftCornerBlock");
        sideScrollerGroundBottomRightCornerBlock = serializedObject.FindProperty("SideScrollerGroundBottomRightCornerBlock");
        sideScrollerGroundMiddleLeftBlock = serializedObject.FindProperty("SideScrollerGroundMiddleLeftBlock");
        sideScrollerGroundMiddleRightBlock = serializedObject.FindProperty("SideScrollerGroundMiddleRightBlock");
        sideScrollerGroundPeakPointBlock = serializedObject.FindProperty("SideScrollerGroundPeakPointBlock");

        // Top down
        topDownGroundBlock = serializedObject.FindProperty("TopDownGroundBlock");
        topDownGroundMiddleBlock = serializedObject.FindProperty("TopDownGroundMiddleBlock");
        topDownGroundTopLeftCornerBlock = serializedObject.FindProperty("TopDownGroundTopLeftCornerBlock");
        topDownGroundTopRightCornerBlock = serializedObject.FindProperty("TopDownGroundTopRightCornerBlock");
        topDownGroundTopMiddleBlock = serializedObject.FindProperty("TopDownGroundTopMiddleBlock");
        topDownGroundBottomMiddleBlock = serializedObject.FindProperty("TopDownGroundBottomMiddleBlock");
        topDownGroundBottomLeftCornerBlock = serializedObject.FindProperty("TopDownGroundBottomLeftCornerBlock");
        topDownGroundBottomRightCornerBlock = serializedObject.FindProperty("TopDownGroundBottomRightCornerBlock");
        topDownGroundMiddleLeftBlock = serializedObject.FindProperty("TopDownGroundMiddleLeftBlock");
        topDownGroundMiddleRightBlock = serializedObject.FindProperty("TopDownGroundMiddleRightBlock");
        topDownGroundPointUpBlock = serializedObject.FindProperty("TopDownGroundPointUpBlock");
        topDownGroundPointDownBlock = serializedObject.FindProperty("TopDownGroundPointDownBlock");
        topDownGroundPointLeftBlock = serializedObject.FindProperty("TopDownGroundPointLeftBlock");
        topDownGroundPointRightBlock = serializedObject.FindProperty("TopDownGroundPointRightBlock");
        topDownGroundAloneBlock = serializedObject.FindProperty("TopDownGroundAloneBlock");
        topDownGroundSidewaysBlock = serializedObject.FindProperty("TopDownGroundSidewaysBlock");
        topDownGroundUpwardsBlock = serializedObject.FindProperty("TopDownGroundUpwardsBlock");

        // Platform
        platformBlock = serializedObject.FindProperty("PlatformBlock");
        platformMiddleBlock = serializedObject.FindProperty("PlatformMiddleBlock");
        platformLeftBlock = serializedObject.FindProperty("PlatformLeftBlock");
        platformRightBlock = serializedObject.FindProperty("PlatformRightBlock");

        // Edge
        leftEdgeTrigger = serializedObject.FindProperty("LeftEdgeTrigger");
        rightEdgeTrigger = serializedObject.FindProperty("RightEdgeTrigger");

        // Enemies
        sideScrollingEnemyPrefab = serializedObject.FindProperty("SideScrollingEnemyPerfab");
        topDownEnemyPrefab = serializedObject.FindProperty("TopDownEnemyPerfab");
        sideScrollingEnemyObjectsList = serializedObject.FindProperty("sideScrollingEnemyObjectsList");
        topDownEnemyObjectsList = serializedObject.FindProperty("topDownEnemyObjectsList");

        // Level components
        coinPrefab = serializedObject.FindProperty("CoinPrefab");
        pickupPrefab = serializedObject.FindProperty("PickupPrefab");
        hazardPrefab = serializedObject.FindProperty("HazardPrefab");
        deathPlane = serializedObject.FindProperty("DeathPlane");

        // Parents
        blockParent = serializedObject.FindProperty("BlockParent");
        edgeParent = serializedObject.FindProperty("EdgeParent");
        enemyParent = serializedObject.FindProperty("EnemyParent");
        coinParent = serializedObject.FindProperty("CoinParent");
        hazardParent = serializedObject.FindProperty("HazardParent");

        // Players
        sideScrollerPlayer = serializedObject.FindProperty("SideScrollerPlayer");
        topDownPlayer = serializedObject.FindProperty("TopDownPlayer");

        // Generation settings
        generationMode = serializedObject.FindProperty("generationMode");
        haveEnemies = serializedObject.FindProperty("haveEnemies");
        haveEnemiesSpawnBasedOnDifficulty = serializedObject.FindProperty("haveVariousEnemiesSpawnBasedOnDifficulty");
        haveEnemyScaling = serializedObject.FindProperty("haveEnemyScaling");
        havePlatforms = serializedObject.FindProperty("havePlatforms");
        platformDifficultyRelation = serializedObject.FindProperty("platformDifficultyRelation");
        minPlatformHeightFromGround = serializedObject.FindProperty("MinPlatformHeightFromGround");
        maxPlatformHeightFromGround = serializedObject.FindProperty("MaxPlatformHeightFromGround");
        havePickups = serializedObject.FindProperty("havePickups");
        haveHazards = serializedObject.FindProperty("haveHazards");
        haveCoins = serializedObject.FindProperty("haveCoins");

        // Debug
        doNotInstantiate = serializedObject.FindProperty("DoNotInstantiate");
        showTilesetInfo = serializedObject.FindProperty("ShowTilesetInfo");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("PLAYER PERFORMANCE TRACKER", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(playerPerformanceTracker);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("GENERATION SETTINGS", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(generationMode);
        EditorGUILayout.PropertyField(haveEnemies);

        if (haveEnemies.boolValue)
        {
            EditorGUI.indentLevel++;
            
            if(!haveEnemiesSpawnBasedOnDifficulty.boolValue)
            {
                EditorGUILayout.PropertyField(haveEnemyScaling);
            }
            if(!haveEnemyScaling.boolValue)
            {
                EditorGUILayout.PropertyField(haveEnemiesSpawnBasedOnDifficulty);
            }
                
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
        }
        
        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.SideScroller)
        {
            EditorGUILayout.PropertyField(havePlatforms);

            if (havePlatforms.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(platformDifficultyRelation);
                EditorGUILayout.PropertyField(minPlatformHeightFromGround);
                EditorGUILayout.PropertyField(maxPlatformHeightFromGround);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.PropertyField(havePickups);
        EditorGUILayout.PropertyField(haveHazards);
        EditorGUILayout.PropertyField(haveCoins);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("BASIC SETTINGS", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(levelWidth);
        EditorGUILayout.PropertyField(levelHeight);
        EditorGUILayout.PropertyField(trainingEpisodes);
        EditorGUILayout.PropertyField(difficulty);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("PREFABS", EditorStyles.boldLabel);

        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.SideScroller)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Side-Scroller Ground Blocks", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sideScrollerGroundBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundMiddleBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundTopLeftCornerBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundTopRightCornerBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundTopMiddleBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundBottomMiddleBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundBottomLeftCornerBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundBottomRightCornerBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundMiddleLeftBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundMiddleRightBlock);
            EditorGUILayout.PropertyField(sideScrollerGroundPeakPointBlock);
        }

        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.TopDown)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Top-Down Ground Blocks", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(topDownGroundBlock);
            EditorGUILayout.PropertyField(topDownGroundMiddleBlock);
            EditorGUILayout.PropertyField(topDownGroundTopLeftCornerBlock);
            EditorGUILayout.PropertyField(topDownGroundTopRightCornerBlock);
            EditorGUILayout.PropertyField(topDownGroundTopMiddleBlock);
            EditorGUILayout.PropertyField(topDownGroundBottomMiddleBlock);
            EditorGUILayout.PropertyField(topDownGroundBottomLeftCornerBlock);
            EditorGUILayout.PropertyField(topDownGroundBottomRightCornerBlock);
            EditorGUILayout.PropertyField(topDownGroundMiddleLeftBlock);
            EditorGUILayout.PropertyField(topDownGroundMiddleRightBlock);
            EditorGUILayout.PropertyField(topDownGroundPointUpBlock);
            EditorGUILayout.PropertyField(topDownGroundPointDownBlock);
            EditorGUILayout.PropertyField(topDownGroundPointLeftBlock);
            EditorGUILayout.PropertyField(topDownGroundPointRightBlock);
            EditorGUILayout.PropertyField(topDownGroundAloneBlock);
            EditorGUILayout.PropertyField(topDownGroundSidewaysBlock);
            EditorGUILayout.PropertyField(topDownGroundUpwardsBlock);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Platform Blocks", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(platformBlock);
        EditorGUILayout.PropertyField(platformMiddleBlock);
        EditorGUILayout.PropertyField(platformLeftBlock);
        EditorGUILayout.PropertyField(platformRightBlock);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Edge Triggers", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(leftEdgeTrigger);
        EditorGUILayout.PropertyField(rightEdgeTrigger);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Enemy Prefabs", EditorStyles.boldLabel);

        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.SideScroller)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("If only one Enemy object is to be used", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(sideScrollingEnemyPrefab);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("If multiple Enemy objects are to be used", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(sideScrollingEnemyObjectsList);
        }

        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.TopDown)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("If only one Enemy object is to be used (use a prefab here)", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(topDownEnemyPrefab);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("If multiple Enemy objects are to be used (use the Enemy Scriptable Objects here)", EditorStyles.miniBoldLabel);
            EditorGUILayout.PropertyField(topDownEnemyObjectsList);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Level Components Prefabs", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(coinPrefab);
        EditorGUILayout.PropertyField(pickupPrefab);
        EditorGUILayout.PropertyField(hazardPrefab);
        EditorGUILayout.PropertyField(deathPlane);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Object Parents", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(blockParent);
        EditorGUILayout.PropertyField(edgeParent);
        EditorGUILayout.PropertyField(enemyParent);
        EditorGUILayout.PropertyField(coinParent);
        EditorGUILayout.PropertyField(hazardParent);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Player Objects", EditorStyles.boldLabel);
        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.SideScroller) EditorGUILayout.PropertyField(sideScrollerPlayer);
        if ((RLLevelTrainer.GenerationMode)generationMode.enumValueIndex == RLLevelTrainer.GenerationMode.TopDown) EditorGUILayout.PropertyField(topDownPlayer);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("DEBUG", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(doNotInstantiate);
        EditorGUILayout.PropertyField(showTilesetInfo);
        
        serializedObject.ApplyModifiedProperties();
    }
}
