#convert GLB to TSCN with colliders but this is Godot script
@tool
extends EditorScript

# --- CONFIGURATION ---
# IMPORTANT: Ends with a slash "/"
const INPUT_FOLDER = "res://path/to/your/x_folder/"
const OUTPUT_FOLDER = "res://path/to/your/y_folder/"

func _run():
	# 1. Setup Directory Access
	var dir = DirAccess.open(INPUT_FOLDER)
	if not dir:
		print("Error: Could not open input folder: ", INPUT_FOLDER)
		return

	dir.list_dir_begin()
	var file_name = dir.get_next()

	print("--- Starting Batch Import ---")

	# 2. Loop through all files
	while file_name != "":
		# Check if it's a .glb file (and not a hidden file)
		if not dir.current_is_dir() and file_name.ends_with(".glb"):
			process_file(file_name)
		
		file_name = dir.get_next()
	
	print("--- Batch Import Finished ---")
	# Refresh the editor filesystem so the new files show up immediately
	EditorInterface.get_resource_filesystem().scan()

func process_file(file_name):
	var input_path = INPUT_FOLDER + file_name
	var output_name = file_name.replace(".glb", ".tscn")
	var output_path = OUTPUT_FOLDER + output_name

	# 3. Load the GLB
	var glb_scene = load(input_path)
	if not glb_scene:
		print("Failed to load: ", file_name)
		return

	# 4. Instantiate it (create the nodes in memory)
	var root_node = glb_scene.instantiate()
	
	# 5. Find meshes and add collisions recursively
	add_collisions_recursive(root_node, root_node)

	# 6. Pack into a new .tscn
	var new_scene = PackedScene.new()
	var result = new_scene.pack(root_node)
	
	if result == OK:
		# 7. Save to the Y folder
		var error = ResourceSaver.save(new_scene, output_path)
		if error == OK:
			print("Saved: ", output_name)
		else:
			print("Error saving: ", output_name)
	else:
		print("Error packing scene: ", file_name)
	
	# Clean up memory
	root_node.free()

# Helper function to find all meshes inside the node tree
func add_collisions_recursive(node, root_owner):
	# If this node is a MeshInstance, add collision
	if node is MeshInstance3D:
		# This built-in helper creates the StaticBody and CollisionShape siblings/children
		node.create_trimesh_collision()
		
		# CRITICAL STEP:
		# The new StaticBody and Shape are created, but they don't have an "owner".
		# Nodes without an owner are NOT saved to the .tscn file.
		# We must find them and set their owner to the scene root.
		for child in node.get_children():
			if child is StaticBody3D:
				child.name = node.name + "_StaticBody" # Rename for clarity
				child.owner = root_owner # This makes it save!
				
				# The CollisionShape is usually a child of the StaticBody
				for grandchild in child.get_children():
					grandchild.owner = root_owner # This makes it save!
	
	# Continue checking children (in case the mesh is deep inside)
	for child in node.get_children():
		add_collisions_recursive(child, root_owner)
