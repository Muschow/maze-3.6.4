extends Node

const SQLite = preload("res://addons/godot-sqlite/bin/gdsqlite.gdns")
var db
var db_name = "res://DataStore/pacmandatabase"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	db = SQLite.new()
	db.path = db_name
	

func queryDB(query : String): #make sure to do var result = queryDB as it returns a value
	db.open_db()
	db.query(query)
	return db.query_result #queryresult is an array of dictionaries
	
#query needs to include ? and array includes parameters that take the place of those ?
func queryDBwithParameters(query : String, parameters : Array): 
	db.open_db()
	db.query_with_bindings(query,parameters)
	return db.query_result #queryresult is an array of dictionaries
	
func printFromQuery(queryResult): 
	for i in range(0,queryResult.size()):
		print("query results"+str(queryResult[i])) #just for debug purposes

func queryValue(query_result_with_1_item) -> Array: 
	var valueArray = []
	for i in range (0,query_result_with_1_item.size()):
		for keys in query_result_with_1_item[i].keys():
			valueArray.append(query_result_with_1_item[i][keys])
	
	return valueArray
			 #returns the value of those key:value pairs into an array so i can use it easier in c#


