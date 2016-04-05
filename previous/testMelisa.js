var login = require("facebook-chat-api");

login({email: "Sabinebraun1304@gmail.com", password: "Sabine1qaz"}, function callback (err, api) {
    // if(err) return console.error(err);

    // api.setOptions({listenEvents: true});

    // var stopListening = api.listen(function(err, event) {
        // if(err) return console.error(err);
// console.log("LISTEN ===================================================================================== FUNCTION");
// setTimeout(function(){alert("hi")}, 1000);
// console.log(event);
        // switch(event.type) {
          // case "message":
            // if(event.body === '/stop') {
              // api.sendMessage("Goodbye...", event.threadID);
              // return stopListening();
            // }
            // api.markAsRead(event.threadID, function(err) {
              // if(err) console.log(err);
            // });
            // api.sendMessage("TEST BOT: " + event.body, event.threadID);
            // break;
          // case "event":
            // console.log(event);
            // break;
        // } 
    // });
	
	// console.log("LISTEN FUNCTION");
	// api.listen(function(err, event)
	// console.log("GET_ONLINE_USERS");
	 // api.getOnlineUsers(function callback(err, message){
        // console.log(err);
        // console.log(message);    
    // });
		
	// console.log("<<GetFriendsList>>");
	
	 // api.getFriendsList(function callback(err, message){
        // console.log(err);
        // console.log(message);    
    // });
	
});