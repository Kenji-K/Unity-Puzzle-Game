Parse.Cloud.define("PeriodicHash", function(request, response) {
	var seed = GetSeed();
	
	response.success(seed);
});

function GetSeed() {
	var today = new Date();
	var dd = today.getDate() - today.getDay() + 1; //First day is Monday!
	var mm = today.getMonth()+1; //January is 0!
	var yyyy = today.getFullYear();

	if(dd<10) {
		dd='0'+dd
	} 

	if(mm<10) {
		mm='0'+mm
	}

	today = mm+'/'+dd+'/'+yyyy+'heyoooo';
	return today;
}

Parse.Cloud.define("GlobalTopScores", function(request, response) {
	if (request.params.gameModes.length < 1) {
		response.error("No game modes specified, aborting.");
		return;
	}
	var globalTopArray = new Array();
	queryGlobalHighScores(request, response, 0, globalTopArray);
});

function queryGlobalHighScores(request, response, index, globalTopArray) {
	var gameMode = request.params.gameModes[index];
	var query = new Parse.Query("HighScore");
	query.equalTo("gameVersion", request.params.gameVersion);
	query.equalTo("gameMode", gameMode);
	if (gameMode == "PeriodicMode") {
		query.equalTo("rawSeedUsed", GetSeed());
	}
	query.descending("score");
	query.limit(1);
	query.find({
		success: function(results) {
			globalTopArray = globalTopArray.concat(results[0]);
			if (index == request.params.gameModes.length - 1) {
				response.success(globalTopArray);
			} else {
				queryGlobalHighScores(request, response, index + 1, globalTopArray);
			}
		},
		error: function(error) {
			response.error("Error: " + error.code + " " + error.message);
		}
	});
}

Parse.Cloud.define("FriendsHighScores", function(request, response) {
	if (request.params.gameModes.length < 1) {
		response.error("No game modes specified, aborting.");
		return;
	}
	
	var friendsScores = new Array();
	
	queryHighScores(request, response, 0, friendsScores);
});

function queryHighScores(request, response, index, fullList) {
	var gameMode = request.params.gameModes[index];
	var query = new Parse.Query("HighScore");
	query.equalTo("gameVersion", request.params.gameVersion);
	query.equalTo("gameMode", gameMode);
	if (gameMode == "PeriodicMode") {
		query.equalTo("rawSeedUsed", GetSeed());
	}
	query.containedIn("facebookUserID", request.params.friendsIDs);
	query.limit(10);
	query.find({
		success: function(results) {
			fullList = fullList.concat(results);
			if (index == request.params.gameModes.length - 1) {
				response.success(fullList);
			} else {
				queryHighScores(request, response, index + 1, fullList);
			}
		},
		error: function(error) {
			response.error("Error: " + error.code + " " + error.message);
		}
	});
}

Parse.Cloud.define("PlayerScoresAndRanks", function(request, response) {
	var compilation = new Object();
	queryScoresAndRanks(request, response, 0, compilation);
});

function queryScoresAndRanks(request, response, index, compilation) {
	var gameVersion = request.params.gameVersion;
	var facebookUserID = request.params.userID;
	var friendsIDs = request.params.friendsIDs;
	var gameMode = request.params.gameModes[index];

	var userScoreQuery = new Parse.Query("HighScore");
	userScoreQuery.equalTo("gameVersion", gameVersion);
	userScoreQuery.equalTo("gameMode", gameMode);
	userScoreQuery.equalTo("facebookUserID", facebookUserID);
	userScoreQuery.first({
		success: function(userScoreParseObj) {
			var userScore = 0;
			
			if (userScoreParseObj != undefined && userScoreParseObj.get("rawSeedUsed") == GetSeed()) {
				userScore = userScoreParseObj.get("score");
			} else {
				userScore = 0;
			}
			
			var query = new Parse.Query("HighScore");
			query.equalTo("gameVersion", gameVersion);
			query.equalTo("gameMode", gameMode);
			if (gameMode == "PeriodicMode") {
				query.equalTo("rawSeedUsed", GetSeed());
			}
			query.containedIn("facebookUserID", friendsIDs);
			query.greaterThan("score", userScore);
			query.count({
				success: function(result) {
					compilation[gameMode + "Rank"] = result + 1;
					compilation[gameMode + "Score"] = userScore;
					if (index == request.params.gameModes.length - 1) {
						response.success(compilation);
					} else {
						queryScoresAndRanks(request, response, index + 1, compilation);
					}
				},
				error: function(error) {
					response.error("Error: " + error.code + " " + error.message);
				}
			});
		},
		error: function(error) {
			response.error("Error: " + error.code + " " + error.message);
		}
	});
}