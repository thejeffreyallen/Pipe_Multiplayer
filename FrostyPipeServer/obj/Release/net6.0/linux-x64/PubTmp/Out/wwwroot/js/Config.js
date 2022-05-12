
//tell server to reload
function SendReloadConfig() {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        GetServerConfig();
    }
    xhttp.open("GET", "/reloadconfig", true);
    xhttp.send();
}
//get current config
function GetServerConfig() {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        var obj = JSON.parse(this.responseText)
        document.getElementById("mplayers").innerText = obj["Maxplayers"];
        document.getElementById("trMax").innerText = obj["TickrateMax"];
        document.getElementById("trMin").innerText = obj["TickrateMin"];
        document.getElementById("port").innerText = obj["Port"];
        document.getElementById("adpass").innerText = obj["Adminpassword"];
        document.getElementById("POST").innerText = obj["POST"];
        document.getElementById("PUT").innerText = obj["PUT"];
        document.getElementById("KEY").innerText = obj["KEY"];
        document.getElementById("Servername").innerText = obj["Servername"];
        document.getElementById("public").checked = obj["Publicise"]
        document.getElementById("portal").innerText = obj["PortalUrl"]
    }
    xhttp.open("GET", "/serverconfig", true);
    xhttp.send();

}

function ApplyConfigChanges() {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        var text = this.responseText;
        console.log(text);
        if (text.includes("Applied")) {
         SendReloadConfig();
        }
    }
    xhttp.open("POST", "/applyconfig", true);

    
    var jsonconfig = {
        "Maxplayers": document.getElementById("mplayers").innerText,
        "TickrateMax": document.getElementById("trMax").innerText,
        "TickrateMin": document.getElementById("trMin").innerText,
        "Port": document.getElementById("port").innerText,
        "Adminpassword": document.getElementById("adpass").innerText,
        "POST": document.getElementById("POST").innerText,
        "PUT": document.getElementById("PUT").innerText,
        "KEY": document.getElementById("KEY").innerText,
        "Servername": document.getElementById("Servername").innerText,
        "Publicise": document.getElementById("public").checked,
        "PortalUrl": document.getElementById("portal").innerText
    }
    var js = JSON.stringify(jsonconfig);
    xhttp.setRequestHeader("jsonconfig", js);
    xhttp.send();
}

function GetServerStats() {
    const xhttp = new XMLHttpRequest();
    xhttp.onload = function () {
        var obj = JSON.parse(this.responseText)
        document.getElementById("ramused").innerText = obj["ramused"];
        document.getElementById("cpuused").innerText = obj["cpuused"];
        document.getElementById("trnow").innerText = obj["trnow"];
        document.getElementById("players").innerText = obj["players"];
        document.getElementById("lastlooprec").innerText = obj["lastlooprec"];
        document.getElementById("lastloopsen").innerText = obj["lastloopsen"];
        document.getElementById("lastloopsentimeout").innerText = obj["lastloopsentimeout"];
        document.getElementById("lastlooprectimeout").innerText = obj["lastlooprectimeout"];
        document.getElementById("bytesout").innerText = obj["Kbytesout"];
        document.getElementById("bytesin").innerText = obj["Kbytesin"];
        document.getElementById("avping").innerText = obj["avping"];
        document.getElementById("pendingrel").innerText = obj["pendingrel"];
        document.getElementById("pendingunrel").innerText = obj["pendingunrel"];


    }
    xhttp.open("GET", "/serverstats", true);
    xhttp.send();

}

setInterval(GetServerStats, 5000);
GetServerConfig();