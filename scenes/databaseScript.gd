extends Node

const SQLite = preload("res://addons/godot-sqlite/bin/gdsqlite.gdns")
var db
var db_name = "res://DataStore/pacmandatabase"
# Declare member variables here. Examples:
# var a: int = 2
# var b: String = "text"


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	db = SQLite.new()
	db.path = db_name
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta: float) -> void:
#	pass

func queryDB(query : String): #make sure to do var result = queryDB as it returns a value
	db.open_db()
	db.query(query)
	return db.query_result #queryresult is an array of dictionaries
	
func queryDBwithParameters(query : String, parameters : Array): #query needs to include ? and array includes parameters that take the place of those ?
	db.open_db()
	db.query_with_bindings(query,parameters)
	return db.query_result #queryresult is an array of dictionaries
	
func printFromQuery(queryResult): 
	for i in range(0,queryResult.size()):
		print("query results"+str(queryResult[i])) #i think this is an array of dictionaries with key value pairs

#--------------------------------------specific functions for loginscript------------------------------
#as the interoperability between c# and gdscript is quite bad, due to the nature of dynamic vs static typing,
#some of these functions have to be very specific so i can use them in c#

#one of the reasons why ive done it this way is a query returns a array of dictionaries. This array is a Godot.Collections.Array
#which will then have to be casted to a c# array. However a godot array can contain ints, strings etc at the same time so its very annoying#
#and not worth it

func existsInDB(count1_query_result) -> bool: #here query result MUST be a COUNT(1) query, which returns either 0 or 1
	if count1_query_result[0]["COUNT(1)"] == 1: 
		print("exists in db")
		return true
	else:
		print("doesnt exist in db")
		return false;

func queryValue(query_result_with_1_item):
	for i in range (0,query_result_with_1_item.size()):
		for keys in query_result_with_1_item[i].keys():
			return query_result_with_1_item[i][keys]
			 #gets first value in first key of query result

func returnSalt(salt_query_result): #query must be to grab salt for username
	return salt_query_result[0]["Salt"];
