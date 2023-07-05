// Creating functions for the Unity
mergeInto(LibraryManager.library, {    

   // Function example
    CallFunction: function () {
      // Show a message as an alert
      window.alert("You called a function from this plugin!");
   },
   // Function with the text param
   PassTextParam: function (text) {
      // Convert bytes to the text
      var convertedText = UTF8ToString(text);
      // Show a message as an alert
      window.alert("You've passed the text: " + convertedText);
   }  
   /*
   // Function with the number param
   PassNumberParam: function (number) {
      // Show a message as an alert
      window.alert("The number is: " + number);
   },
   // Function returning text value
   GetTextValue: function () {
      // Define text value
      var textToPass = "You got this text from the plugin";
      // Create a buffer to convert text to bytes
      var bufferSize = lengthBytesUTF8(textToPass) + 1;
      var buffer = _malloc(bufferSize);
      // Convert text
      stringToUTF8(textToPass, buffer, bufferSize);
      // Return text value
      window.alert("buffer value is : " + buffer);   
       //return buffer;
   },
   // Function returning number value
   GetNumberValue: function () {
      // Return number value
      return 2020;
   }     */
 } 
 );  


  var GetURL = {
    
    GetURLFromPage: function () {
        var returnStr = window.top.location.href;
        var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);
        writeStringToMemory(returnStr, buffer);
        return buffer;
    },

    /*
    SetBrowserData: function() {
    const id = localStorage.SetItem("token");
    const fname = JSON.parse(localStorage.SetItem("userData")).first_name;
    const lname = JSON.parse(localStorage.SetItem("userData")).last_name;
    return [id, fname, lname].toString();
  }    
  */
      getBrowserData: function() {  
      const id = localStorage.getItem("token");    
      const fname = localStorage.getItem("first_name");    
      const lname = localStorage.getItem("last_name");      
   //  window.alert("toke is : " + id);      
  //  var returnStr =[id, fname, lname].toString();
    
    var returnStr = JSON.stringify({ "token": id.toString(), first_name: fname.toString() , last_name: lname.toString() }).toString();
     var bufferSize = lengthBytesUTF8(returnStr) + 1;  
     var buffer = _malloc(bufferSize);
    stringToUTF8(returnStr, buffer, bufferSize); 
    return buffer;     
     //  return id.toString();      
    // const fname = JSON.parse(localStorage.getItem("userData")).first_name;
   // const lname = JSON.parse(localStorage.getItem("userData")).last_name;
  //     var returnStr = id.src;  
  //  var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);  
  //   writeStringToMemory(returnStr, buffer); 
   //   return buffer;    
   } ,

  
   GetURLFromIframe: function (text) {
       var convertedText = UTF8ToString(text);  
      // Show a message as an alert
      window.alert("You've passed the text: " + convertedText);
    console.log('id is:', convertedText);    
       // var iframe = window.parent.document.getElementById('game_iframe');
        var iframe = window.parent.document.getElementById(convertedText);    
   console.log('id is:', iframe);  
     if (iframe) {
       var returnStr = iframe.src;
      var buffer = _malloc(lengthBytesUTF8(returnStr) + 1);  
      writeStringToMemory(returnStr, buffer);
      return buffer;
    } else {
      return null;
    }  
  }   
};
 mergeInto(LibraryManager.library, GetURL);
