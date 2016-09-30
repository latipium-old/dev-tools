const electron = require("electron");
const fs = require("fs");
const readline = require("readline");
const temp = require("temp");

const app = electron.app;
const BrowserWindow = electron.BrowserWindow;

let antiGCCache = [];

function sendRequest(payload) {
    let obj = JSON.parse(payload);
    let req = {
        version: 1,
        session: false,
        queries: obj
    };
    payload = JSON.stringify(req);
    let window = new BrowserWindow({
        width: 600,
        height: 600,
        resizable: false,
        title: "Latipium Development Tools"
    });
    window.webContents.on("dom-ready", function(ev) {
        if (window.webContents.getURL().startsWith("https://apis.latipium.com/callback?state=")) {
            const filename = temp.path({
                suffix: ".json"
            });
            window.webContents.savePage(filename, "HTMLOnly", function(error) {
                if (error) {
                    console.log({
                        successful_queries: [],
                        failed_queries: req.queries
                    });
                    window.close();
                } else {
                    fs.readFile(filename, "utf8", function(error, data) {
                        if (!error) {
                            console.log(data);
                        }
                        fs.unlink(filename, function() {
                            window.close();
                        });
                    });
                }
            });
        }
    });
    window.loadURL(`file://${__dirname}/index.html#${payload}`)
    antiGCCache.push(window);
}

function main () {
    const rl = readline.createInterface({
        input: process.stdin
    });
    rl.on("line", sendRequest);
}

app.on("ready", main);
app.on('window-all-closed', function () {
    app.quit();
});
