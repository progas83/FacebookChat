var login = require("facebook-chat-api");

login({email: "progas83@ukr.net", password: "fynfhr4nblf"}, function callback (err, api) {
    if(err) return console.error(err);

    api.setOptions({listenEvents: true});

    var stopListening = api.listen(function(err, event) {
        if(err) return console.error(err);

        switch(event.type) {
          case "message":
            if(event.body === '/stop') {
              api.sendMessage("Goodbye...", event.threadID);
              return stopListening();
            }
            api.markAsRead(event.threadID, function(err) {
              if(err) console.log(err);
            });
            api.sendMessage("TEST BOT: " + event.body, event.threadID);
            event.threadID("TestSendTestSendTestSendTestSendTestSendTestSendTestSendTestSendTestSend");
			console.log(event.threadID);
			break;
			
          case "event":
            console.log(event);
            break;
        }
    });
});


// login({email: "progas@ukr.net", password: "ghjnbdjht4bt"}, function callback (err, api) {
    // if(err) return console.error(err);

    // api.setOptions({listenEvents: true});

    // // var stopListening = api.listen(function(err, event) {
        // // if(err) return console.error(err);

        // // switch(event.type) {
          // // case "message":
            // // if(event.body === '/stop') {
              // // api.sendMessage("Goodbye...", event.threadID);
              // // return stopListening();
            // // }
            // // api.markAsRead(event.threadID, function(err) {
              // // if(err) console.log(err);
            // // });
            // // api.sendMessage("TEST BOT: " + event.body, event.threadID);
            // // break;
          // // case "event":
            // // console.log(event);
            // // break;
        // // }
    // // });
	
	// console.log("LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!LogOUT!!!!!!!");
		// api.logout(function callback(err, message){
        // console.log(err);
        // console.log(message);   
	// });
// });













// login({email: "progas@ukr.net", password: "ghjnbdjht4bt"}, function callback (err, api) {
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
	
	 // api.getOnlineUsers(function callback(err, message){
        // console.log(err);
        // console.log(message);    
    // });
	
	
	// // console.log("LISTEN FUNCTION");
	// // api.listen(function(err, event)
	// // console.log("GET_ONLINE_USERS");
	 // // api.getOnlineUsers(function callback(err, message){
        // // console.log(err);
        // // console.log(message);    
    // // });
		
	// // console.log("<<GetFriendsList>>");
	//api.logout(done);
	 // // api.logout(function callback(err, message){
        // // console.log(err);
        // // console.log(message);    
    // // });
	
// });