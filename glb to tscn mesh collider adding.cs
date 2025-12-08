//This script imports GLB files, adds collision shapes, and saves them as TSCN files.
#if TOOLS
using Godot;
using System;

[Tool]
public partial class BatchImporter : EditorScript
{
    // --- CONFIGURATION ---
    // Ensure these end with a slash "/"
    const string InputFolder = "res://Assets/RawMeshes/";
    const string OutputFolder = "res://Assets/Prefabs/";

    public override void _Run()
    {
        // 1. Setup Directory Access
        using var dir = DirAccess.Open(InputFolder);
        if (dir == null)
        {
            GD.PrintErr($"Error: Could not open input folder: {InputFolder}");
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        GD.Print("--- Starting C# Batch Import ---");

        // 2. Loop through all files
        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".glb"))
            {
                ProcessFile(fileName);
            }
            fileName = dir.GetNext();
        }

        GD.Print("--- Batch Import Finished ---");
        // Refresh the editor filesystem
        EditorInterface.Singleton.GetResourceFilesystem().Scan();
    }

    private void ProcessFile(string fileName)
    {
        string inputPath = InputFolder + fileName;
        string outputName = fileName.Replace(".glb", ".tscn");
        string outputPath = OutputFolder + outputName;

        // 3. Load the GLB
        var glbScene = GD.Load<PackedScene>(inputPath);
        if (glbScene == null)
        {
            GD.PrintErr($"Failed to load: {fileName}");
            return;
        }

        // 4. Instantiate
        Node rootNode = glbScene.Instantiate();

        // 5. Add collisions recursively
        AddCollisionsRecursive(rootNode, rootNode);

        // 6. Pack into a new .tscn
        var newScene = new PackedScene();
        Error result = newScene.Pack(rootNode);

        if (result == Error.Ok)
        {
            Error saveErr = ResourceSaver.Save(newScene, outputPath);
            if (saveErr == Error.Ok)
                GD.Print($"Saved: {outputName}");
            else
                GD.PrintErr($"Error saving: {outputName}");
        }
        else
        {
            GD.PrintErr($"Error packing scene: {fileName}");
        }

        // Clean up memory
        rootNode.QueueFree();
    }

    private void AddCollisionsRecursive(Node node, Node rootOwner)
    {
        // If this node is a MeshInstance, add collision
        if (node is MeshInstance3D meshInst)
        {
            // Create the StaticBody and CollisionShape
            meshInst.CreateTrimeshCollision();

            // CRITICAL STEP: Fix the "Owner" so it saves to the file
            foreach (Node child in node.GetChildren())
            {
                if (child is StaticBody3D)
                {
                    child.Name = node.Name + "_StaticBody";
                    child.Owner = rootOwner;

                    foreach (Node grandchild in child.GetChildren())
                    {
                        grandchild.Owner = rootOwner;
                    }
                }
            }
        }

        // Continue checking children
        foreach (Node child in node.GetChildren())
        {
            AddCollisionsRecursive(child, rootOwner);
        }
    }
}
#endif
