var coll = document.getElementsByClassName("collapsible");
var i;

for (i = 0; i < coll.length; i++) {
    coll[i].addEventListener("click", function () {
        this.classList.toggle("active");
        var content = this.nextElementSibling;
        if (content.style.display === "block") {
            content.style.display = "none";
        } else {
            content.style.display = "block";
        }
        
    });
}



function BanriderSend(id) {
    
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        var text = this.responseText;
        console.log(text);
        if (text.includes("Applied")) {
            document.location.reload();
        }
    }
    xhttp.open("POST", "/banridersend", true);
    var mins = document.getElementById(id + " bantime").innerText

    xhttp.setRequestHeader("id", id);
    xhttp.setRequestHeader("mins", mins);
    xhttp.send();


}



function GetGarageSetup(id) {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        //var obj = JSON.parse(this.responseText)
        if (this.responseText == "Garage Diabled") {
            return;
        }
        console.log("Updating garage")
        var parser = new DOMParser();
        var xml = parser.parseFromString(this.responseText, "text/xml");
        var root = xml.childNodes[0];
        var data = document.getElementById("garagedata " + id);
        while (data.hasChildNodes()) {
            data.firstChild.remove();
            
        }
        for (var i = 0; i < root.childNodes.length; i++) {
            if (root.childNodes[i].childNodes.length > 0) {
                

                // straight value
                if (root.childNodes[i].childNodes.length == 1) {
                    var node = document.createTextNode(root.childNodes[i].nodeName + " : " + root.childNodes[i].childNodes[0].nodeValue)
                    var el = document.createElement("div");
                    el.appendChild(node);
                    data.appendChild(el);

                }
                else {
                    //partpostion, partmesh etc
                    var objects = root.childNodes[i].childNodes;
                    for (var _i = 0; _i < objects.length; _i++) {

                        
                            if (objects[_i].childNodes.length == 1) {
                               // console.log("subobject: " + objects[_i].nodeName + " : " + objects[_i].firstChild.nodeValue);
                            }
                            else {
                                //console.log("sub-subobject: " + objects[_i].nodeName + " : " + objects[_i].childNodes.length);
                                var structure = objects[_i].childNodes;
                                for (var structval = 0; structval < objects[_i].childNodes.length; structval++) {
                                    if (structure[structval].childNodes.length > 0) {
                                      console.log("Struct value: " + structure[structval].nodeName)

                                    }
                                }
                            }

                        
                        
                    }
                }

               
                
            }

        }
       
    }
    xhttp.open("GET", "/Garagesetup", true);
    xhttp.setRequestHeader("riderid",id )
    xhttp.send();
}