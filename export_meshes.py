# Writen by Parthkumar Rathod (Wardragon3399)
# Script for mass exprort mesh fromm Unreal project that as ussassts to GLB
# You must need GLTFExporter and Paython runner script plugin in your project mostly avalivble defult in new unreal project

import unreal
import os

# Root folder in Unreal's Content Browser
ROOT_FOLDER = "/Game/Content/Fab or whater pack folder/meshes" # use your path of directory that have static mesh in project
# Output directory on disk
EXPORT_PATH = "C:\Exports\GLB" # use your path of output export file folder.

# Ensure export folder exists
os.makedirs(EXPORT_PATH, exist_ok=True)

asset_registry = unreal.AssetRegistryHelpers.get_asset_registry()
assets = asset_registry.get_assets_by_path(ROOT_FOLDER, recursive=True)

for asset_data in assets:
    # Use asset_class_path instead of deprecated asset_class
    if asset_data.asset_class_path.asset_name == "StaticMesh":
        mesh = asset_data.get_asset()
        asset_name = asset_data.asset_name

        filename = os.path.join(EXPORT_PATH, f"{asset_name}.glb")

        # Create export task
        task = unreal.AssetExportTask()
        task.object = mesh
        task.filename = filename
        task.automated = True
        task.replace_identical = True
        task.prompt = False

        # IMPORTANT: don't set exporter manually, Unreal will pick GLTFExporter
        unreal.Exporter.run_asset_export_task(task)

print("Export complete!")
# below is command for execute python file in Unreal Engine mostly change your path where this script is store in side of unreal project 
#exec(open(r"C:\Users\XYZ\Documents\Unreal Projects\MyProject\Content\Python\export_meshes.py").read())

